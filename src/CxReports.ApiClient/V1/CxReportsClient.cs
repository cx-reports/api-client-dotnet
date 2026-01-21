using CxReports.ApiClient.Exceptions;
using CxReports.ApiClient.Utilities;
using CxReports.ApiClient.V1.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace CxReports.ApiClient.V1
{
    public class WorkspaceId
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
    }

    public class ReportId
    {
        public int? Id { get; set; }
        public string? TypeCode { get; set; }
    }

    public class ReportQueryParams
    {
        public JsonObject? Params { get; set; }
        public JsonObject? Data { get; set; }
        public int? TempDataId { get; set; }
        public string? Nonce { get; set; }
        public string? Timezone { get; set; }
    }

    public class CxReportsClient : ApiClientBase, ICxReportsClient
    {
        private readonly CxReportsClientConfig _config;

        public CxReportsClient(CxReportsClientConfig config, HttpClient httpClient)
            : base(httpClient)
        {
            if (config.BaseUrl.EndsWith("/"))
                config.BaseUrl = config.BaseUrl[..^1];
            _config = config;
        }

        protected string ResolveEndpointUrl(
            string endpointPath,
            Dictionary<string, object?>? query = null
        )
        {
            return ResolveEndpointURLWithApiPath("/api/v1/", endpointPath, query);
        }

        protected string ResolveEndpointURLWithApiPath(
            string apiPath,
            string endpointPath,
            Dictionary<string, object?>? query = null
        )
        {
            return BuildUrl(_config.BaseUrl, apiPath, endpointPath, query);
        }

        protected override void OnPrepareRequest(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _config.AuthToken
            );
        }

        protected string GetDefaultWorkspaceId()
        {
            if (_config.DefaultWorkspaceId != null)
                return _config.DefaultWorkspaceId.ToString();

            if (!string.IsNullOrEmpty(_config.DefaultWorkspaceCode))
                return _config.DefaultWorkspaceCode;

            throw new MissingWorkspaceIdException();
        }

        protected string GetWorkspaceId(WorkspaceId? workspace)
        {
            return workspace?.Id?.ToString() ?? workspace?.Code ?? GetDefaultWorkspaceId();
        }

        protected string GetJobId(JobKey? job)
        {
            return job?.Id?.ToString() ??
                   job?.Code ?? throw new InvalidOperationException("Could not find Job Id or Code");
        }

        public async Task<IList<Report>> GetReportsAsync(
            WorkspaceId? workspace,
            string? type = null,
            int? limit = null,
            int? offset = null,
            CancellationToken cancellationToken = default
        )
        {
            var query = new Dictionary<string, object?>()
            {
                ["type"] = type,
                ["limit"] = limit,
                ["offset"] = offset
            };
            string workspaceId = GetWorkspaceId(workspace);
            return await GET<IList<Report>>(
                ResolveEndpointUrl($"ws/{Uri.EscapeDataString(workspaceId)}/reports", query),
                cancellationToken
            );
        }

        protected string GetReportId(ReportId? report)
        {
            string? reportId = report?.Id?.ToString() ?? report?.TypeCode;
            return reportId
                   ?? throw new CxReportsException(
                       "Invalid report identification. Missing either reportId or reportType."
                   );
        }

        protected Dictionary<string, object?>? EncodeReportQueryParams(ReportQueryParams? query)
        {
            var result = new Dictionary<string, object?>();
            if (query == null)
                return null;

            if (query.Params != null)
                result["params"] = JsonSerializer.Serialize(query.Params);
            if (query.Data != null)
                result["data"] = JsonSerializer.Serialize(query.Data);
            if (query.Nonce != null)
                result["nonce"] = query.Nonce;
            if (query.TempDataId != null)
                result["tempDataId"] = query.TempDataId.ToString();
            if (query.Timezone != null)
                result["timezone"] = query.Timezone;
            else if (_config.DefaultTimezone != null)
                result["timezone"] = _config.DefaultTimezone;

            return result.Count > 0 ? result : null;
        }

        public string GetReportPreviewUrl(ReportParams reportParams)
        {
            string workspaceId = GetWorkspaceId(reportParams.Workspace);
            string reportId = GetReportId(reportParams.Report);
            var query = EncodeReportQueryParams(reportParams.QueryParams);
            return ResolveEndpointURLWithApiPath(
                "/",
                $"ws/{Uri.EscapeDataString(workspaceId)}/reports/{Uri.EscapeDataString(reportId)}/preview",
                query
            );
        }

        public string GetReportPdfDownloadUrl(ReportParams reportParams)
        {
            string workspaceId = GetWorkspaceId(reportParams.Workspace);
            string reportId = GetReportId(reportParams.Report);
            var query = EncodeReportQueryParams(reportParams.QueryParams);
            return ResolveEndpointUrl(
                $"ws/{Uri.EscapeDataString(workspaceId)}/reports/{Uri.EscapeDataString(reportId)}/pdf",
                query
            );
        }

        public async Task<HttpResponseMessage> DownloadPdfAsync(
            ReportParams reportParams,
            CancellationToken cancellationToken = default
        )
        {
            string workspaceId = GetWorkspaceId(reportParams.Workspace);
            string reportId = GetReportId(reportParams.Report);
            var query = EncodeReportQueryParams(reportParams.QueryParams);
            var url = ResolveEndpointUrl(
                $"ws/{Uri.EscapeDataString(workspaceId)}/reports/{Uri.EscapeDataString(reportId)}/pdf",
                query
            );
            return await Send(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken);
        }

        public async Task<List<Workspace>> GetWorkspacesAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await GET<List<Workspace>>(ResolveEndpointUrl("workspaces"), cancellationToken);
        }

        public async Task<NonceToken> CreateNonceAuthToken(
            CancellationToken cancellationToken = default
        )
        {
            return await POST<NonceToken>(
                ResolveEndpointUrl("nonce-tokens"),
                null,
                cancellationToken
            );
        }

        public async Task<TemporaryData> PushTemporaryData(
            JsonObject content,
            DateTimeOffset? expires = null,
            WorkspaceId? workspace = null,
            CancellationToken cancellationToken = default
        )
        {
            string workspaceId = GetWorkspaceId(workspace);
            var data = new { content, expiryDate = expires };
            return await POST<TemporaryData>(
                ResolveEndpointUrl($"ws/{Uri.EscapeDataString(workspaceId)}/temporary-data"),
                JsonContent.Create(data),
                cancellationToken
            );
        }

        public async Task<IList<ReportType>> GetReportTypesAsync(
            WorkspaceId? workspace, CancellationToken cancellationToken = default
        )
        {
            var workspaceId = GetWorkspaceId(workspace);
            return await GET<IList<ReportType>>(
                ResolveEndpointUrl($"ws/{Uri.EscapeDataString(workspaceId)}/report-types"), cancellationToken);
        }

        public async Task<IList<ReportPage>> GetReportPagesAsync(
            WorkspaceId? workspace,
            ReportId report,
            CancellationToken cancellationToken = default
        )
        {
            var workspaceId = GetWorkspaceId(workspace);
            var reportId = GetReportId(report);
            return await GET<IList<ReportPage>>(
                ResolveEndpointUrl(
                    $"ws/{Uri.EscapeDataString(workspaceId)}/reports/{Uri.EscapeDataString(reportId)}/pages"),
                cancellationToken);
        }

        public async Task<AsyncReportGenerationResponse> StartReportExportAsync(
            WorkspaceId? workspace,
            ReportId report,
            AsyncReportGenerationRequest parameters,
            CancellationToken cancellationToken = default
        )
        {
            var workspaceId = GetWorkspaceId(workspace);
            var reportId = GetReportId(report);
            var body = JsonContent.Create(parameters);
            return await POST<AsyncReportGenerationResponse>(
                ResolveEndpointUrl(
                    $"ws/{Uri.EscapeDataString(workspaceId)}/reports/{Uri.EscapeDataString(reportId)}/export"),
                body, cancellationToken);
        }

        public async Task<ReportExportStatusResponse> GetReportExportStatusAsync(
            WorkspaceId? workspace,
            int tempFileId,
            CancellationToken cancellationToken = default
        )
        {
            var workspaceId = GetWorkspaceId(workspace);
            return await GET<ReportExportStatusResponse>(
                ResolveEndpointUrl($"ws/{Uri.EscapeDataString(workspaceId)}/exports/{tempFileId}/status"),
                cancellationToken
            );
        }

        public async Task<HttpResponseMessage> DownloadExportedFileAsync(
            WorkspaceId? workspace,
            int tempFileId,
            CancellationToken cancellationToken = default
        )
        {
            var workspaceId = GetWorkspaceId(workspace);
            var url = ResolveEndpointUrl($"ws/{Uri.EscapeDataString(workspaceId)}/exports/{tempFileId}/content");
            return await Send(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken);
        }

        public async Task<IList<Job>> GetAllJobsAsync(
            WorkspaceId? workspace,
            CancellationToken cancellationToken = default
        )
        {
            var workspaceId = GetWorkspaceId(workspace);
            return await GET<IList<Job>>(
                ResolveEndpointUrl(
                    $"ws/{Uri.EscapeDataString(workspaceId)}/jobs"
                ),
                cancellationToken
            );
        }

        public async Task<JobRun> StartJobRunAsync(
            WorkspaceId? workspace,
            JobKey job,
            JobRunRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var workspaceId = GetWorkspaceId(workspace);
            var jobId = GetJobId(job);
            return await POST<JobRun>(
                ResolveEndpointUrl(
                    $"ws/{Uri.EscapeDataString(workspaceId)}/jobs/{Uri.EscapeDataString(jobId)}/runs"
                ),
                JsonContent.Create(request),
                cancellationToken
            );
        }

        public async Task<JobRunStatus> GetJobRunStatusAsync(
            WorkspaceId? workspace,
            JobKey job,
            int jobRunId,
            CancellationToken cancellationToken = default
        )
        {
            var workspaceId = GetWorkspaceId(workspace);
            var jobId = GetJobId(job);
            return await GET<JobRunStatus>(
                ResolveEndpointUrl(
                    $"ws/{Uri.EscapeDataString(workspaceId)}/jobs/{Uri.EscapeDataString(jobId)}/runs/{jobRunId}/status"
                ), cancellationToken
            );
        }

        public async Task<AsyncReportGenerationResponse> GetJobReviewDocumentAsync(
            WorkspaceId? workspace,
            JobKey job,
            int jobRunId,
            CancellationToken cancellationToken = default
        )
        {
            var workspaceId = GetWorkspaceId(workspace);
            var jobId = GetJobId(job);
            return await POST<AsyncReportGenerationResponse>(
                ResolveEndpointUrl(
                    $"ws/{Uri.EscapeDataString(workspaceId)}/jobs/{Uri.EscapeDataString(jobId)}/runs/{jobRunId}/generate-review-document"
                ),
                null,
                cancellationToken
            );
        }

        public async Task<HttpResponseMessage> DeliverJobRunAsync(
            WorkspaceId? workspace,
            JobKey job,
            int jobRunId,
            CancellationToken cancellationToken = default
        )
        {
            var workspaceId = GetWorkspaceId(workspace);
            var jobId = GetJobId(job);
            var url = ResolveEndpointUrl(
                $"ws/{Uri.EscapeDataString(workspaceId)}/jobs/{Uri.EscapeDataString(jobId)}/runs/{jobRunId}/deliver"
            );
            return await Send(new HttpRequestMessage(HttpMethod.Post, url), cancellationToken);
        }
    }
}

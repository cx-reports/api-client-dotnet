using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using CxReports.ApiClient.Exceptions;
using CxReports.ApiClient.Utilities;
using CxReports.ApiClient.V1.Models;

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
            if (string.IsNullOrEmpty(_config.DefaultWorkspaceId))
                throw new MissingWorkspaceIdException();
            return _config.DefaultWorkspaceId;
        }

        protected string GetWorkspaceId(WorkspaceId? workspace = null)
        {
            return workspace?.Id?.ToString() ?? workspace?.Code ?? GetDefaultWorkspaceId();
        }

        public async Task<IList<Report>> GetReportsAsync(
            WorkspaceId? workspace,
            Dictionary<string, object?>? query = null,
            CancellationToken cancellationToken = default
        )
        {
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
            return await Send(
                new HttpRequestMessage(
                    HttpMethod.Get,
                    ResolveEndpointUrl(
                        $"ws/{Uri.EscapeDataString(workspaceId)}/reports/{Uri.EscapeDataString(reportId)}/pdf",
                        query
                    )
                ),
                cancellationToken
            );
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
    }
}

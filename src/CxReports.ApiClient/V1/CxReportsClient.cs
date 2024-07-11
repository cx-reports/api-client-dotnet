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
    public class WorkspaceIdParams
    {
        public int? WorkspaceId { get; set; }
        public string? WorkspaceCode { get; set; }
    }

    public class ReportIdParams
    {
        public int? ReportId { get; set; }
        public string? ReportTypeCode { get; set; }
    }

    public class ReportPreviewParams
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

        protected string ResolveEndpointURL(
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

        protected string GetWorkspaceId(WorkspaceIdParams? workspace = null)
        {
            return workspace?.WorkspaceId?.ToString()
                ?? workspace?.WorkspaceCode
                ?? GetDefaultWorkspaceId();
        }

        public async Task<IList<Report>> GetReports(
            WorkspaceIdParams? workspace,
            Dictionary<string, object?>? query = null,
            CancellationToken cancellationToken = default
        )
        {
            string workspaceId = GetWorkspaceId(workspace);
            return await GET<IList<Report>>(
                ResolveEndpointURL($"ws/{Uri.EscapeDataString(workspaceId)}/reports", query),
                cancellationToken
            );
        }

        protected string GetReportId(ReportIdParams? report)
        {
            string? reportId = report?.ReportId?.ToString() ?? report?.ReportTypeCode;
            if (reportId == null)
                throw new CxReportsException(
                    "Invalid report identification. Missing either reportId or reportType."
                );
            return reportId;
        }

        protected Dictionary<string, object?> EncodeReportPreviewParams(ReportPreviewParams @params)
        {
            return new Dictionary<string, object?>
            {
                {
                    "params",
                    @params.Params != null ? JsonSerializer.Serialize(@params.Params) : null
                },
                { "data", @params.Data != null ? JsonSerializer.Serialize(@params.Data) : null },
                { "nonce", @params.Nonce },
                { "tempDataId", @params.TempDataId?.ToString() }
            };
        }

        public string GetReportPreviewURL(
            WorkspaceIdParams workspaceParams,
            ReportIdParams reportParams,
            ReportPreviewParams previewParams
        )
        {
            string workspaceId = GetWorkspaceId(workspaceParams);
            string reportId = GetReportId(reportParams);
            var query = EncodeReportPreviewParams(previewParams);
            return ResolveEndpointURLWithApiPath(
                "/",
                $"ws/{Uri.EscapeDataString(workspaceId)}/reports/{Uri.EscapeDataString(reportId)}/preview",
                query
            );
        }

        public string GetReportPdfDownloadURL(
            WorkspaceIdParams workspaceParams,
            ReportIdParams reportParams,
            ReportPreviewParams previewParams
        )
        {
            string workspaceId = GetWorkspaceId(workspaceParams);
            string reportId = GetReportId(reportParams);
            var query = EncodeReportPreviewParams(previewParams);
            return ResolveEndpointURL(
                $"ws/{Uri.EscapeDataString(workspaceId)}/reports/{Uri.EscapeDataString(reportId)}/pdf",
                query
            );
        }

        public async Task<HttpResponseMessage> DownloadPDF(
            WorkspaceIdParams workspaceParams,
            ReportIdParams reportParams,
            ReportPreviewParams previewParams,
            CancellationToken cancellationToken = default
        )
        {
            string workspaceId = GetWorkspaceId(workspaceParams);
            string reportId = GetReportId(reportParams);
            var query = EncodeReportPreviewParams(previewParams);
            return await Send(
                new HttpRequestMessage(
                    HttpMethod.Get,
                    ResolveEndpointURLWithApiPath(
                        "/",
                        $"ws/{Uri.EscapeDataString(workspaceId)}/reports/{Uri.EscapeDataString(reportId)}/pdf",
                        query
                    )
                ),
                cancellationToken
            );
        }

        public async Task<List<Workspace>> GetWorkspaces(
            CancellationToken cancellationToken = default
        )
        {
            return await GET<List<Workspace>>("workspaces", cancellationToken);
        }

        public async Task<NonceToken> CreateNonceAuthToken(
            CancellationToken cancellationToken = default
        )
        {
            return await POST<NonceToken>("nonce-tokens", null, cancellationToken);
        }

        public async Task<TemporaryData> PushTemporaryData(
            WorkspaceIdParams workspaceParams,
            JsonObject content,
            DateTimeOffset? expires = null,
            CancellationToken cancellationToken = default
        )
        {
            string workspaceId = GetWorkspaceId(workspaceParams);
            var data = new { content, expiryDate = expires };
            return await POST<TemporaryData>(
                $"ws/{Uri.EscapeDataString(workspaceId)}/temporary-data",
                JsonContent.Create(data),
                cancellationToken
            );
        }

        private static string BuildUrl(
            string baseUrl,
            string apiPath,
            string endpointPath,
            Dictionary<string, object?>? query = null
        )
        {
            var uriBuilder = new UriBuilder(baseUrl)
            {
                Path = apiPath.TrimEnd('/') + "/" + endpointPath.TrimStart('/')
            };

            if (query != null)
            {
                var queryString = new List<string>();
                foreach (var kvp in query)
                {
                    if (kvp.Value != null)
                    {
                        queryString.Add(
                            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString())}"
                        );
                    }
                }
                uriBuilder.Query = string.Join("&", queryString);
            }

            return uriBuilder.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using CxReports.ApiClient.V1.Models;

namespace CxReports.ApiClient.V1
{
    public class ReportParams
    {
        public WorkspaceId? Workspace { get; set; }
        public ReportId Report { get; set; } = null!;
        public ReportQueryParams? QueryParams { get; set; }
    }

    public interface ICxReportsClient
    {
        Task<NonceToken> CreateNonceAuthToken(CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> DownloadPdfAsync(
            ReportParams reportParams,
            CancellationToken cancellationToken = default
        );

        string GetReportPdfDownloadUrl(ReportParams reportParams);

        string GetReportPreviewUrl(ReportParams reportParam);

        Task<IList<Report>> GetReportsAsync(
            WorkspaceId? workspace = null,
            string? type = null,
            int? limit = null,
            int? offset = null,
            CancellationToken cancellationToken = default
        );

        Task<List<Workspace>> GetWorkspacesAsync(CancellationToken cancellationToken = default);

        Task<TemporaryData> PushTemporaryData(
            JsonObject content,
            DateTimeOffset? expires = null,
            WorkspaceId? workspace = null,
            CancellationToken cancellationToken = default
        );
    }
}

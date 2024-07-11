using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using CxReports.ApiClient.V1.Models;

namespace CxReports.ApiClient.V1
{
    public interface ICxReportsClient
    {
        Task<NonceToken> CreateNonceAuthToken(CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> DownloadPDF(
            WorkspaceIdParams workspaceParams,
            ReportIdParams reportParams,
            ReportPreviewParams previewParams,
            CancellationToken cancellationToken = default
        );

        string GetReportPdfDownloadURL(
            WorkspaceIdParams workspaceParams,
            ReportIdParams reportParams,
            ReportPreviewParams previewParams
        );

        string GetReportPreviewURL(
            WorkspaceIdParams workspaceParams,
            ReportIdParams reportParams,
            ReportPreviewParams previewParams
        );

        Task<IList<Report>> GetReports(
            WorkspaceIdParams? workspace = null,
            Dictionary<string, object?>? query = null,
            CancellationToken cancellationToken = default
        );

        Task<List<Workspace>> GetWorkspaces(CancellationToken cancellationToken = default);

        Task<TemporaryData> PushTemporaryData(
            WorkspaceIdParams workspaceParams,
            JsonObject content,
            DateTimeOffset? expires = null,
            CancellationToken cancellationToken = default
        );
    }
}

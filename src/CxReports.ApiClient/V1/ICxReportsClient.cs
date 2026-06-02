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

        Task<HttpResponseMessage> ExportPdfAsync(
            WorkspaceId? workspace,
            ReportId report,
            ReportExportRequest request,
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

        Task<IList<ReportType>> GetReportTypesAsync(
            WorkspaceId? workspace,
            CancellationToken cancellationToken = default
        );

        Task<IList<ReportPage>> GetReportPagesAsync(
            WorkspaceId? workspace,
            ReportId report,
            CancellationToken cancellationToken = default
        );

        Task<AsyncReportGenerationResponse> StartReportExportAsync(
            WorkspaceId? workspace,
            ReportId report,
            AsyncReportGenerationRequest parameters,
            CancellationToken cancellationToken = default
        );

        Task<ReportExportStatusResponse> GetReportExportStatusAsync(
            WorkspaceId? workspace,
            int tempFileId,
            CancellationToken cancellationToken = default
        );

        Task<HttpResponseMessage> DownloadExportedFileAsync(
            WorkspaceId? workspace,
            int tempFileId,
            CancellationToken cancellationToken = default
        );

        Task<IList<Job>> GetAllJobsAsync(
            WorkspaceId? workspace,
            CancellationToken cancellationToken = default
        );

        Task<JobRun> StartJobRunAsync(
            WorkspaceId? workspace,
            JobKey job,
            JobRunRequest request,
            CancellationToken cancellationToken = default
        );

        Task<JobRunStatus> GetJobRunStatusAsync(
            WorkspaceId? workspace,
            JobKey job,
            int jobRunId,
            CancellationToken cancellationToken = default
        );

        Task<AsyncReportGenerationResponse> GetJobReviewDocumentAsync(
            WorkspaceId? workspace,
            JobKey job,
            int jobRunId,
            CancellationToken cancellationToken = default
        );

        Task<HttpResponseMessage> DeliverJobRunAsync(
            WorkspaceId? workspace,
            JobKey job,
            int jobRunId,
            CancellationToken cancellationToken = default
        );

        Task<IList<ThemeItem>> GetThemesAsync(
            WorkspaceId? workspace,
            CancellationToken cancellationToken = default
        );

        Task<IList<TemplateItem>> GetReportTemplatesAsync(
            WorkspaceId? workspace,
            CancellationToken cancellationToken = default
        );
    }
}

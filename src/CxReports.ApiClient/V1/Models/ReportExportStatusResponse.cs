
using System;

namespace CxReports.ApiClient.V1.Models
{
    public class ReportExportStatusResponse
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public bool IsReady { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTimeOffset? ExpiryTime { get; set; }
        public string? Name { get; set; }
        public long ContentSize { get; set; }
        public string? ContentType { get; set; }
    }
}

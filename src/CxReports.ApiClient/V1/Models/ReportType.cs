
namespace CxReports.ApiClient.V1.Models
{
    public class ReportType
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Code { get; set; }
        public int? DefaultReportId { get; set; }
        public string? DefaultReportName { get; set; }
    }
}

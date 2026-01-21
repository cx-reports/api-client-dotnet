
namespace CxReports.ApiClient.V1.Models
{
    public class ReportType
    {
        public  int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int DefaultReportId { get; set; }
        public string DefaultReportName { get; set; } = null!;
    }
}

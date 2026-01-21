
namespace CxReports.ApiClient.V1.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Code { get; set; }
        public bool? ReviewRequired { get; set; }
        public bool? IsActive { get; set; }
    }
}

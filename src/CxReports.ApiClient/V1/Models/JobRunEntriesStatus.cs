namespace CxReports.ApiClient.V1.Models
{
    public class JobRunEntriesStatus
    {
        public int? Queued { get; set; }
        public int? Review { get; set; }
        public int? Completed { get; set; }
        public int? Errors { get; set; }
    }
}

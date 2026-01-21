
namespace CxReports.ApiClient.V1.Models
{
    public class JobRunStatus
    {
        public  bool Finished { get; set; }
        public int Entries { get; set; }
        public JobRunEntriesStatus Status { get; set; }
    }

}

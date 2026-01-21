using System.Text.Json;

namespace CxReports.ApiClient.V1.Models
{
    public class JobRunRequest
    {
        public JsonDocument? Params { get; set; }
        public JsonDocument? Data { get; set; }
    }
}

using System.Text.Json.Nodes;

namespace CxReports.ApiClient.V1.Models
{
    public class JobRunRequest
    {
        public JsonObject? Params { get; set; }
        public JsonObject? Data { get; set; }
    }
}

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CxReports.ApiClient.V1.Models
{
    public class ReportExportRequest
    {
        public JsonObject? Params { get; set; }
        public JsonObject? Data { get; set; }
        public string? Lang { get; set; }
        public string? Timezone { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DocumentFileFormat? Format { get; set; }

        public bool IncludeAttachments { get; set; }
    }
}

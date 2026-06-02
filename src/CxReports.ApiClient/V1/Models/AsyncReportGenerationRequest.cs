using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CxReports.ApiClient.V1.Models
{
    public class AsyncReportGenerationRequest
    {
        public JsonObject? Params { get; set; }
        public JsonObject? Data { get; set; }
        public string? Lang { get; set; }
        public string? Timezone { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DocumentFileFormat? Format { get; set; }

        public bool IncludeAttachments { get; set; }
        public List<int>? ExcludePages { get; set; }
        public int? TempDataId { get; set; }
        public string? Theme { get; set; }
        public string? Template { get; set; }
    }
}

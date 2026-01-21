using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CxReports.ApiClient.V1.Models
{
    public class AsyncReportGenerationRequest
    {
        public JsonDocument? Params { get; set; }
        public JsonDocument? Data { get; set; }
        public string? Lang { get; set; }
        public string? Timezone { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DocumentFileFormat? Format { get; set; }

        public bool IncludeAttachments { get; set; }
        public List<int>? ExcludePages { get; set; }
        public int? TempDataId { get; set; }
    }
}

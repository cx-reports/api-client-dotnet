using System;
using System.Collections.Generic;
using System.Text;

namespace CxReports.ApiClient.V1
{
    public class CxReportsClientConfig
    {
        public string BaseUrl { get; set; } = null!;
        public string AuthToken { get; set; } = null!;
        public int? DefaultWorkspaceId { get; set; }
        public string? DefaultWorkspaceCode { get; set; }
        public string? DefaultTimezone { get; set; }
    }
}

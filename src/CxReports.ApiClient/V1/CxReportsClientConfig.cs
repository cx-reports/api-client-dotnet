using System;
using System.Collections.Generic;
using System.Text;

namespace CxReports.ApiClient.V1
{
    public class CxReportsClientConfig
    {
        public string BaseUrl { get; set; } = null!;
        public string AuthToken { get; set; } = null!;
        public string? DefaultWorkspaceId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CxReports.ApiClient.V1.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int ReportTypeId { get; set; }
        public string ReportTypeName { get; set; } = null!;
        public string ReportTemplateName { get; set; } = null!;
        public string? PreviewImage { get; set; }
        public string? ThemeName { get; set; }
        public bool IsDefault { get; set; }
    }
}

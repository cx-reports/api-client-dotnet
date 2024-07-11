using System;
using System.Collections.Generic;
using System.Text;

namespace CxReports.ApiClient.V1.Models
{
    public class Workspace
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Code { get; set; } = null!;
    }
}

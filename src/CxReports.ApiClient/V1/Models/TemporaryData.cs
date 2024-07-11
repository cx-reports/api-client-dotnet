using System;
using System.Collections.Generic;
using System.Text;

namespace CxReports.ApiClient.V1.Models
{
    public class TemporaryData
    {
        public int TempDataId { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }
    }
}

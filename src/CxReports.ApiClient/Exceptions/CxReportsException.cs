using System;
using System.Collections.Generic;
using System.Text;

namespace CxReports.ApiClient.Exceptions
{
    public class CxReportsException : Exception
    {
        public CxReportsException(string message)
            : base(message) { }
    }
}

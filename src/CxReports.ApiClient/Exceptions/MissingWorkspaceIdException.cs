using System;
using System.Collections.Generic;
using System.Text;

namespace CxReports.ApiClient.Exceptions
{
    public class MissingWorkspaceIdException : CxReportsException
    {
        public MissingWorkspaceIdException()
            : base(
                "WorkspaceId is missing. Please provide the workspaceId argument or  set the default value in the client configuration."
            ) { }
    }
}

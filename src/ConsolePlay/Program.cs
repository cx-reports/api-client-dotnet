using System.Text.Json;
using System.Text.Json.Nodes;
using CxReports.ApiClient.V1;
using CxReports.ApiClient.V1.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// copy appSettings.json.sample to appSettings.json and fill in the values
var configuration = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();

var serviceCollection = new ServiceCollection();
serviceCollection.AddCxReportsApiClient(configuration);

var serviceProvider = serviceCollection.BuildServiceProvider();

var cxReports = serviceProvider.GetRequiredService<ICxReportsClient>();

// Playground settings (see appSettings.json.sample -> "Playground" section)
var playground = configuration.GetSection("Playground");
int reportId = playground.GetValue("ReportId", 18623);
bool runMutatingJobTests = playground.GetValue("RunMutatingJobTests", false);
int? jobId = playground.GetValue<int?>("JobId");
string? jobCode = playground.GetValue<string?>("JobCode");

var reports = await cxReports.GetReportsAsync();
Console.WriteLine(JsonSerializer.Serialize(reports));

var workspaces = await cxReports.GetWorkspacesAsync();
Console.WriteLine(JsonSerializer.Serialize(workspaces));

var pdfResponse = await cxReports.DownloadPdfAsync(new() { Report = new() { Id = reportId } });
using var stream = await pdfResponse.Content.ReadAsStreamAsync();
using var output = File.Create("report.pdf");
stream.CopyTo(output);

var json = """
{
  "invoice": {
      "invoiceNumber": "12345",
      "dateIssued": "2024-01-27",
      "dueDate": "2024-02-10",
      "issuer": {
        "name": "Test Corporation",
        "address": "123 Business Rd, Business City, BC 12345",
        "phone": "123-456-7890",
        "email": "contact@xyzcorporation.com"
      },
      "recipient": {
        "name": "TEST Enterprises",
        "address": "456 Enterprise Blvd, Commerce City, CC 67890",
        "phone": "987-654-3210",
        "email": "info@abcenterprises.com"
      },
      "items": [
        {
          "description": "Product 1",
          "quantity": 10,
          "unitPrice": 29.99,
          "total": 299.90
        },
        {
        "description": "Product 2",
          "quantity": 5,
          "unitPrice": 49.99,
          "total": 249.95
        }
      ],
      "subTotal": 549.85,
      "taxRate": 0.07,
      "taxAmount": 38.49,
      "total": 588.34,
      "notes": "Thank you for your business. Passing data works!"
   }
}
""";

var url = cxReports.GetReportPreviewUrl(
    new()
    {
        Report = new() { Id = reportId },
        QueryParams = new() { Data = (JsonObject)JsonNode.Parse(json)! }
    }
);

Console.WriteLine($"PREVIEW URL: {url}");

var pdfResponse2 = await cxReports.DownloadPdfAsync(
    new()
    {
        Report = new() { Id = reportId },
        QueryParams = new() { Data = (JsonObject)JsonNode.Parse(json)! }
    }
);

using var stream2 = await pdfResponse2.Content.ReadAsStreamAsync();
using var output2 = File.Create("report2.pdf");
stream2.CopyTo(output2);

var tempData = await cxReports.PushTemporaryData((JsonObject)JsonNode.Parse(json)!);

var pdfResponse3 = await cxReports.DownloadPdfAsync(
    new()
    {
        Report = new() { Id = reportId },
        QueryParams = new() { TempDataId = tempData.TempDataId }
    }
);

using var stream3 = await pdfResponse3.Content.ReadAsStreamAsync();
using var output3 = File.Create("report3.pdf");
stream3.CopyTo(output3);

// ---------------------------------------------------------------------------
// API expansion exercises
// ---------------------------------------------------------------------------

void Section(string title) => Console.WriteLine($"\n===== {title} =====");

// --- Report types --------------------------------------------------------
Section("GetReportTypesAsync");
var reportTypes = await cxReports.GetReportTypesAsync(null);
Console.WriteLine(JsonSerializer.Serialize(reportTypes));

// --- Report pages --------------------------------------------------------
Section("GetReportPagesAsync");
var reportPages = await cxReports.GetReportPagesAsync(null, new() { Id = reportId });
Console.WriteLine(JsonSerializer.Serialize(reportPages));

// --- Themes --------------------------------------------------------------
Section("GetThemesAsync");
var themes = await cxReports.GetThemesAsync(null);
Console.WriteLine(JsonSerializer.Serialize(themes));

// --- Templates -----------------------------------------------------------
Section("GetReportTemplatesAsync");
var templates = await cxReports.GetReportTemplatesAsync(null);
Console.WriteLine(JsonSerializer.Serialize(templates));

// --- Synchronous PDF export via POST (ReportExportRequest body) -----------
Section("ExportPdfAsync (POST /pdf)");
var exportPdfResponse = await cxReports.ExportPdfAsync(
    null,
    new() { Id = reportId },
    new ReportExportRequest
    {
        Data = JsonSerializer.SerializeToDocument(JsonNode.Parse(json)),
        Format = DocumentFileFormat.PDF,
        IncludeAttachments = false
    }
);
using (var exportStream = await exportPdfResponse.Content.ReadAsStreamAsync())
using (var exportOutput = File.Create("report-export-post.pdf"))
{
    exportStream.CopyTo(exportOutput);
}
Console.WriteLine("Saved report-export-post.pdf");

// --- Asynchronous export flow: start -> poll status -> download -----------
Section("StartReportExportAsync -> poll -> download");
var asyncExport = await cxReports.StartReportExportAsync(
    null,
    new() { Id = reportId },
    new AsyncReportGenerationRequest
    {
        Data = JsonSerializer.SerializeToDocument(JsonNode.Parse(json)),
        Format = DocumentFileFormat.PDF,
        IncludeAttachments = false
    }
);
Console.WriteLine($"Temporary file id: {asyncExport.TemporaryFileId}");

ReportExportStatusResponse status;
do
{
    await Task.Delay(1000);
    status = await cxReports.GetReportExportStatusAsync(null, asyncExport.TemporaryFileId);
    Console.WriteLine($"Status: {status.Status} (ready: {status.IsReady})");
}
while (!status.IsReady && status.Status != "Failed");

if (status.IsReady)
{
    var downloadResponse = await cxReports.DownloadExportedFileAsync(
        null,
        asyncExport.TemporaryFileId
    );
    using var asyncStream = await downloadResponse.Content.ReadAsStreamAsync();
    using var asyncOutput = File.Create("report-export-async.pdf");
    asyncStream.CopyTo(asyncOutput);
    Console.WriteLine("Saved report-export-async.pdf");
}
else
{
    Console.WriteLine($"Export failed: {status.ErrorMessage}");
}

// --- Jobs (read-only) ----------------------------------------------------
Section("GetAllJobsAsync");
var jobs = await cxReports.GetAllJobsAsync(null);
Console.WriteLine(JsonSerializer.Serialize(jobs));

// --- Jobs (mutating) -----------------------------------------------------
// These start a job run, generate a review document, and DELIVER all
// entries (an outward-facing, hard-to-reverse action). Disabled by default;
// set Playground:RunMutatingJobTests=true and a Playground:JobId/JobCode in
// appSettings.json to exercise them. Falls back to the first listed job.
if (runMutatingJobTests && (jobId != null || jobCode != null || jobs.Count > 0))
{
    var jobKey = new JobKey { Id = jobId ?? (jobCode == null ? jobs[0].Id : null), Code = jobCode };

    Section("StartJobRunAsync");
    var jobRun = await cxReports.StartJobRunAsync(
        null,
        jobKey,
        new JobRunRequest { Data = JsonSerializer.SerializeToDocument(JsonNode.Parse(json)) }
    );
    Console.WriteLine($"Job run id: {jobRun.JobRunId}");

    Section("GetJobRunStatusAsync");
    JobRunStatus jobRunStatus;
    do
    {
        await Task.Delay(1000);
        jobRunStatus = await cxReports.GetJobRunStatusAsync(null, jobKey, jobRun.JobRunId);
        Console.WriteLine(JsonSerializer.Serialize(jobRunStatus));
    }
    while (!jobRunStatus.Finished);

    Section("GetJobReviewDocumentAsync");
    var reviewDoc = await cxReports.GetJobReviewDocumentAsync(null, jobKey, jobRun.JobRunId);
    Console.WriteLine($"Review document temp file id: {reviewDoc.TemporaryFileId}");

    Section("DeliverJobRunAsync");
    var deliverResponse = await cxReports.DeliverJobRunAsync(null, jobKey, jobRun.JobRunId);
    Console.WriteLine($"Deliver status: {deliverResponse.StatusCode}");
}

using System.Text.Json;
using System.Text.Json.Nodes;
using CxReports.ApiClient.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var serviceCollection = new ServiceCollection();
serviceCollection.AddCxReportsApiClient(configuration);

var serviceProvider = serviceCollection.BuildServiceProvider();

var cxReports = serviceProvider.GetRequiredService<ICxReportsClient>();

var reports = await cxReports.GetReportsAsync();
Console.WriteLine(JsonSerializer.Serialize(reports));

var workspaces = await cxReports.GetWorkspacesAsync();
Console.WriteLine(JsonSerializer.Serialize(workspaces));

var pdfResponse = await cxReports.DownloadPdfAsync(new() { Report = new() { Id = 18623 } });
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
        Report = new() { Id = 18623 },
        QueryParams = new() { Data = (JsonObject)JsonNode.Parse(json)! }
    }
);

Console.WriteLine($"PREVIEW URL: {url}");

var pdfResponse2 = await cxReports.DownloadPdfAsync(
    new()
    {
        Report = new() { Id = 18623 },
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
        Report = new() { Id = 18623 },
        QueryParams = new() { TempDataId = tempData.TempDataId }
    }
);

using var stream3 = await pdfResponse3.Content.ReadAsStreamAsync();
using var output3 = File.Create("report3.pdf");
stream3.CopyTo(output3);

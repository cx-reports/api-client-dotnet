# .NET API Client for CxReports

This library allows you to connect to the CxReports API from your applications.

## Installation

```bash
dotnet add package CxReports.ApiClient
```

## Example Usage

First, register the `ICxReportsApiClient` service at stratup:

```csharp
serviceCollection.AddCxReportsApiClient(configuration);
```

Then, resolve the `ICxReportsApiClient` or inject it into your class:

```csharp
var cxReports = serviceProvider.GetRequiredService<ICxReportsClient>();
```

Now you can interact with the CxReports API:

```csharp
// list all reports for a default workspace
var reports = await cxReports.GetReportsAsync();

// list all workspaces
var workspaces = await cxReports.GetWorkspacesAsync();

// download a PDF report
var pdfResponse = await cxReports.DownloadPdfAsync(new() { 
	Report = new() { TypeCode = "invoice" } 
});

using var stream = await pdfResponse.Content.ReadAsStreamAsync();
using var output = File.Create("invoice.pdf");
stream.CopyTo(output);
```

## Playground Project

The `ConsolePlay` project provides the usage examples.

To set up your access variables:

1. Copy the `appSettings.json.sample` file to `appSettings.json`.
2. Open the file in your text editor.
3. Replace the placeholder values with your actual values.

## License

[MIT License](./License.txt)
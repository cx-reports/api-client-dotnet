using System.Text.Json;
using CxReports.ApiClient.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var serviceCollection = new ServiceCollection();
serviceCollection.AddCxReportsApiClient(configuration);

var serviceProvider = serviceCollection.BuildServiceProvider();

var cxReportsClient = serviceProvider.GetRequiredService<ICxReportsClient>();

var reports = await cxReportsClient.GetReports();

Console.WriteLine(JsonSerializer.Serialize(reports));

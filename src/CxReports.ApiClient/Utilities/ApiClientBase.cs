using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CxReports.ApiClient.Exceptions;
using CxReports.ApiClient.V1.Models;

namespace CxReports.ApiClient.Utilities
{
    public class ApiClientBase
    {
        private readonly HttpClient _httpClient;

        public ApiClientBase(HttpClient httpClient)
        {
            _httpClient = httpClient;
            JsonSerializerOptions = CreateJsonSerializerOptions();
        }

        protected virtual void OnPrepareRequest(HttpRequestMessage request) { }

        protected JsonSerializerOptions JsonSerializerOptions { get; set; }

        protected virtual JsonSerializerOptions CreateJsonSerializerOptions()
        {
            return new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        protected async Task<HttpResponseMessage> Send(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            OnPrepareRequest(request);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                await ProcessError(response);
            return response;
        }

        private async Task ProcessError(HttpResponseMessage response)
        {
            try
            {
                var errorData = await ParseJsonResponse<ErrorData>(response);
                if (errorData?.Error != null)
                    throw new CxReportsException(errorData.Error);
            }
            catch { }

            throw new CxReportsException(response.ReasonPhrase);
        }

        protected async Task<T> GET<T>(string url, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await Send(request, cancellationToken);
            return await ParseJsonResponse<T>(response);
        }

        protected async Task<T> POST<T>(
            string url,
            HttpContent? content,
            CancellationToken cancellationToken
        )
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            var response = await Send(request, cancellationToken);
            return await ParseJsonResponse<T>(response);
        }

        protected async Task<T> ParseJsonResponse<T>(HttpResponseMessage response)
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            return JsonSerializer.Deserialize<T>(stream, JsonSerializerOptions)!;
        }

        protected static string BuildUrl(
            string baseUrl,
            string apiPath,
            string endpointPath,
            Dictionary<string, object?>? query = null
        )
        {
            var uriBuilder = new UriBuilder(baseUrl)
            {
                Path = apiPath.TrimEnd('/') + "/" + endpointPath.TrimStart('/')
            };

            if (query != null)
            {
                var queryString = new List<string>();
                foreach (var kvp in query)
                {
                    if (kvp.Value == null)
                        continue;

                    queryString.Add(
                        $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString())}"
                    );
                }
                uriBuilder.Query = string.Join("&", queryString);
            }
            return uriBuilder.ToString();
        }
    }
}

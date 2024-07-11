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
        }

        protected virtual void OnPrepareRequest(HttpRequestMessage request) { }

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
            return JsonSerializer.Deserialize<T>(stream)!;
        }
    }
}

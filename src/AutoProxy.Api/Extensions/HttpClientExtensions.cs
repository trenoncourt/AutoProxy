using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace AutoProxy.Api.Extensions
{
    public static class HttpClientExtensions
    {
        public static void AddHeaders(this HttpClient httpClient, IHeaderDictionary headers)
        {
            httpClient.DefaultRequestHeaders.Clear();
            foreach (var header in headers)
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value.ToString());
            }
        }
    }
}
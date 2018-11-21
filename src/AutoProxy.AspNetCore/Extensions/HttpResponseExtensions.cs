using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AutoProxy.AspNetCore.Extensions
{
    public static class HttpResponseExtensions
    {
        public static async Task SetResponse(this HttpResponse httpResponse, HttpResponseMessage httpResponseMessage)
        {
            foreach (var header in httpResponseMessage.Content.Headers)
            {
                httpResponse.Headers[header.Key] = new StringValues(header.Value.ToArray());
            }
            httpResponse.StatusCode = (int)httpResponseMessage.StatusCode;
            Stream responseBufferStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            await responseBufferStream.CopyToAsync(httpResponse.Body);
        }
    }
}
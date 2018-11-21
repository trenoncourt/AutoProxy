﻿using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace AutoProxy.AspNetCore.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string GetPath(this HttpRequest request)
        {
            string path = "/";
            if (request.Path.HasValue)
            {
                path = request.Path.Value;
            }

            return path;
        }
        
        public static bool IsInWhiteList(this HttpRequest request, ProxySettings settings)
        {
            return (settings.WhiteList != null 
                    && !settings.WhiteList.Split(';')
                        .Any(template => Regex.IsMatch(request.GetPath(), template)));
        }
        
        public static HttpContent GetContent(this HttpRequest request)
        {
            if (request.Body.CanSeek)
            {
                request.EnableRewind();
                var streamContent = new StreamContent(request.Body);
                streamContent.Headers.ContentLength = request.ContentLength;
            }

            return null;
        }
        
        public static HttpRequestMessage ToHttpRequestMessage(this HttpRequest request, ProxySettings settings)
        {
            var fullPath = new Uri(new Uri(settings.BaseUrl), request.GetPath());

            // Prepare builder.
            var url = fullPath.AbsoluteUri;
            if (request.QueryString.HasValue)
            {
                url += request.QueryString.Value;
            }
            
            var httpRequestMesage = new HttpRequestMessage(new HttpMethod(request.Method), url);

            httpRequestMesage.Headers.Clear();
            foreach (var header in request.Headers)
            {
                if (settings?.Headers?.Remove != null &&
                    settings.Headers.Remove.Contains(header.Key))
                    continue;
                var replaceHeader = settings?.Headers?.Replace?.FirstOrDefault(h => h.Key == header.Key);
                if (replaceHeader != null)
                {
                    httpRequestMesage.Headers.TryAddWithoutValidation(header.Key, replaceHeader.Value.ToString());
                    continue;
                }
                httpRequestMesage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToString());
            }

            if (request.ContentLength != null && request.ContentLength > 0)
            {
                request.EnableRewind();
                httpRequestMesage.Content = new StreamContent(request.Body);
                httpRequestMesage.Content.Headers.ContentLength = request.ContentLength;
                if (!string.IsNullOrEmpty(request.ContentType))
                {
                    httpRequestMesage.Content.Headers.TryAddWithoutValidation("Content-Type", request.ContentType);
                }
            }

            return httpRequestMesage;
        }
    }
}
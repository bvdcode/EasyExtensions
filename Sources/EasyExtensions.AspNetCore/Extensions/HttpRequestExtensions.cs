using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace EasyExtensions.AspNetCore.Extensions
{
    /// <summary>
    /// <see cref="HttpRequest"/> extensions.
    /// </summary>
    public static class HttpRequestExtensions
    {
        private static readonly ConcurrentDictionary<string, List<DateTime>> _accesses = new();

        /// <summary>
        /// Get remote host IP address using proxy "X-Real-IP", "CF-Connecting-IP", "X-Forwarded-For" headers, or connection remote IP address.
        /// </summary>
        /// <returns> IP address, or "Unknown" by default. </returns>
        public static string GetRemoteAddress(this HttpRequest request)
        {
            const string defaultResponce = "Unknown";
            string[] addressHeaders = ["CF-Connecting-IP", "X-Real-IP", "X-Forwarded-For"];
            if (request == null || request.HttpContext == null || request.HttpContext.Request == null)
            {
                return defaultResponce;
            }

            foreach (var addressHeader in addressHeaders)
            {
                if (request.HttpContext.Request.Headers.TryGetValue(addressHeader, out var value))
                {
                    return value!;
                }
            }

            if (request.HttpContext.Connection?.RemoteIpAddress != null)
            {
                return request.HttpContext.Connection.RemoteIpAddress.ToString().Replace("::ffff:", string.Empty);
            }

            return defaultResponce;
        }

        /// <summary>
        /// Check if there are too many requests from the same IP address, using in-memory cache.
        /// </summary>
        /// <param name="request">HTTP request.</param>
        /// <param name="atTimeSeconds">Time period to check, default is 60 seconds.</param>
        /// <param name="maxRepeats">Maximum number of requests for the specified time period, default is 1.</param>
        /// <returns>True if there are too many requests, otherwise false.</returns>
        public static bool TooManyRequests(this HttpRequest request, int atTimeSeconds = 60, int maxRepeats = 1)
        {
            string ip = request.GetRemoteAddress();
            return TooManyRequests(ip, TimeSpan.FromSeconds(atTimeSeconds), maxRepeats);
        }

        /// <summary>
        /// Check if there are too many requests from the same IP address, using in-memory cache.
        /// </summary>
        /// <param name="request">HTTP request.</param>
        /// <param name="atTime">Time period to check, default is 60 seconds.</param>
        /// <param name="maxRepeats">Maximum number of requests for the specified time period, default is 1.</param>
        /// <returns>True if there are too many requests, otherwise false.</returns>
        public static bool TooManyRequests(this HttpRequest request, TimeSpan atTime = default, int maxRepeats = 1)
        {
            string ip = request.GetRemoteAddress();
            return TooManyRequests(ip, atTime, maxRepeats);
        }

        /// <summary>
        /// Check if there are too many requests from the same IP address, using in-memory cache.
        /// </summary>
        /// <param name="ip">IP address.</param>
        /// <param name="atTime">Time period to check.</param>
        /// <param name="maxRepeats">Maximum number of requests for the specified time period, default is 1.</param>
        /// <returns>True if there are too many requests, otherwise false.</returns>
        public static bool TooManyRequests(string ip, TimeSpan atTime, int maxRepeats = 1)
        {
            const int requestCount = 10;
            const int periodInSeconds = 60;

            if (maxRepeats < 1)
            {
                maxRepeats = requestCount;
            }
            if (atTime <= TimeSpan.Zero)
            {
                atTime = TimeSpan.FromSeconds(periodInSeconds);
            }

            if (!_accesses.TryGetValue(ip, out var dateTimes))
            {
                _accesses[ip] = [ DateTime.UtcNow ];
                return false;
            }
            dateTimes.Add(DateTime.UtcNow);
            DateTime threshold = DateTime.UtcNow.Add(-atTime);
            var currentRequests = dateTimes.Where(x => x > threshold);
            _accesses[ip] = [.. currentRequests];
            int currentCount = currentRequests.Count();
            // reduce list with old requests
            if (currentCount > requestCount)
            {
                _accesses[ip] = [.. currentRequests.Skip(currentCount - requestCount)];
            }
            return currentCount > maxRepeats;
        }
    }
}
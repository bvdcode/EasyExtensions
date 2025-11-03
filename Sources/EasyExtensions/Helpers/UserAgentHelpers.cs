using System;
using System.Text.RegularExpressions;

namespace EasyExtensions.Helpers
{
    /// <summary>
    /// Provides helper methods for identifying device types from user agent strings.
    /// </summary>
    /// <remarks>This class offers static methods to extract a compact, human-readable device label from a
    /// user agent string without relying on external user agent parsing libraries. It is intended for lightweight
    /// device detection scenarios where performance and simplicity are prioritized over exhaustive user agent
    /// analysis.</remarks>
    public static class UserAgentHelpers
    {
        /// <summary>
        /// Determines the type of device based on the provided user agent string.
        /// </summary>
        /// <remarks>The method attempts to classify the device as a phone, tablet, desktop, smart TV,
        /// game console, bot, or script based on common patterns in the user agent string. The returned value is a
        /// general category and may include additional details, such as the Android device model if available. The
        /// detection is heuristic and may not be accurate for all user agents.</remarks>
        /// <param name="userAgent">The user agent string to analyze for device identification. Cannot be null or whitespace.</param>
        /// <returns>A string representing the detected device type, such as "iPhone", "Android Phone", "Windows PC", or "Bot".
        /// Returns "Unknown" if the user agent is null or whitespace.</returns>
        public static string GetDevice(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return "Unknown";
            }
            var ua = userAgent;
            var ual = ua.ToLowerInvariant();

            // bots / scripts
            if (ContainsAny(ual, "bot", "crawler", "spider", "slurp", "bingpreview", "facebookexternalhit", "monitoring", "uptime"))
            {
                return "Bot";
            }
            if (ContainsAny(ual, "curl/", "wget/", "httpclient", "libwww", "okhttp", "java/"))
            {
                return "Script";
            }

            // TVs / consoles
            if (ContainsAny(ual, "smart-tv", "smarttv", "hbbtv", "appletv", "googletv"))
            {
                return "Smart TV";
            }
            if (ContainsAny(ual, "playstation", "xbox", "nintendo switch", "steam deck"))
            {
                return "Game Console";
            }

            // iOS
            if (ual.Contains("ipod"))
            {
                return "iPod";
            }
            if (ual.Contains("ipad"))
            {
                return "iPad";
            }
            if (ual.Contains("iphone"))
            {
                return "iPhone";
            }

            // Android (try to extract model between "; ... Build/")
            if (ual.Contains("android"))
            {
                var m = Regex.Match(ua, @";\s*([^;]+?)\s+Build/", RegexOptions.IgnoreCase);
                var model = m.Success ? SanitizeModel(m.Groups[1].Value) : null;
                var isMobile = ual.Contains("mobile"); // tablets often lack "mobile"
                if (isMobile)
                {
                    return model?.Length > 0 ? $"Android Phone ({model})" : "Android Phone";
                }
                return model?.Length > 0 ? $"Android Tablet ({model})" : "Android Tablet";
            }

            // ChromeOS
            if (ual.Contains("cros"))
            {
                return "Chromebook";
            }

            // Desktop OS
            if (ual.Contains("windows nt"))
            {
                return "Windows PC";
            }
            if (ual.Contains("macintosh") || ual.Contains("mac os x"))
            {
                return "Mac";
            }
            if (ual.Contains("linux"))
            {
                return "Linux PC";
            }

            // Fallbacks (some rare UAs only say "Mobile")
            if (ual.Contains("mobile"))
            {
                return "Mobile";
            }
            return "Desktop";
        }

        // small helpers
        private static bool ContainsAny(string haystack, params string[] needles)
        {
            foreach (var n in needles)
            {
                if (haystack.Contains(n)) return true;
            }
            return false;
        }

        private static string SanitizeModel(string raw)
        {
            // remove common noise like locale or build channel remnants
            var s = raw.Trim();
            s = Regex.Replace(s, @"\s+", " ");
            // cut vendor prefixes that are too verbose (optional)
            s = s.Replace("Build", "", StringComparison.OrdinalIgnoreCase).Trim();
            return s;
        }
    }
}

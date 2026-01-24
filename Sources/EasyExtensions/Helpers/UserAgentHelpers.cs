// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;
using System.Collections.Generic;
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
        private static readonly Func<string, UserAgentDeviceInfo?>[] _parsers = new Func<string, UserAgentDeviceInfo?>[]
        {
            TryParseBot,
            TryParseScript,
            TryParseTv,
            TryParseConsole,
            TryParseKnownDeviceCode,
            TryParseIos,
            TryParseAndroid,
            TryParseChromeOs,
            TryParseDesktop,
            TryParseMobileFallback,
            TryParseServerFallback,
        };

        private static readonly Regex _samsungModelRegex = new Regex(@"\bSM-[A-Z]\d{3}[A-Z0-9]{0,4}\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex _appleMachineRegex = new Regex(@"\b(iPhone|iPad)\d{1,2},\d{1,2}\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _onePlusRegex = new Regex(@"\bCPH\d{4}\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _xiaomiSkuRegex = new Regex(@"\b\d{5}[A-Z0-9]{5,8}\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);

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
        public static string GetDevice(string? userAgent)
        {
            return GetDeviceInfo(userAgent).FriendlyName ?? "Unknown";
        }

        /// <summary>
        /// Attempts to identify device information from the specified user agent string.
        /// </summary>
        /// <param name="userAgent">The user agent string to analyze. Cannot be null, but may be empty or whitespace.</param>
        /// <returns>A <see cref="UserAgentDeviceInfo"/> object containing the detected device information. If the device type
        /// cannot be determined, the returned object will have <see cref="UserAgentDeviceType.Unknown"/> as the device
        /// type.</returns>
        public static UserAgentDeviceInfo GetDeviceInfo(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return new UserAgentDeviceInfo(UserAgentDeviceType.Unknown, null, "Unknown");
            }

            foreach (var parser in _parsers)
            {
                var info = parser(userAgent);
                if (info != null)
                {
                    return info;
                }
            }

            return new UserAgentDeviceInfo(UserAgentDeviceType.Unknown, null, "Unknown");
        }

        private static UserAgentDeviceInfo? TryParseKnownDeviceCode(string ua)
        {
            return TryGetFirstKnownDeviceCode(ua);
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

        private static UserAgentDeviceInfo? TryParseBot(string ua)
        {
            var ual = ua.ToLowerInvariant();
            if (ContainsAny(ual, "bot", "crawler", "spider", "slurp", "bingpreview", "facebookexternalhit", "monitoring", "uptime"))
            {
                return new UserAgentDeviceInfo(UserAgentDeviceType.Bot, null, "Bot");
            }
            return null;
        }

        private static UserAgentDeviceInfo? TryParseScript(string ua)
        {
            var ual = ua.ToLowerInvariant();
            if (ContainsAny(ual, "curl/", "wget/", "httpclient", "libwww", "java/"))
            {
                return new UserAgentDeviceInfo(UserAgentDeviceType.Script, null, "Script");
            }
            return null;
        }

        private static UserAgentDeviceInfo? TryParseTv(string ua)
        {
            var ual = ua.ToLowerInvariant();
            if (ContainsAny(ual, "smart-tv", "smarttv", "hbbtv", "appletv", "googletv"))
            {
                return new UserAgentDeviceInfo(UserAgentDeviceType.SmartTv, null, "Smart TV");
            }
            return null;
        }

        private static UserAgentDeviceInfo? TryParseConsole(string ua)
        {
            var ual = ua.ToLowerInvariant();
            if (ContainsAny(ual, "playstation", "xbox", "nintendo switch", "steam deck"))
            {
                return new UserAgentDeviceInfo(UserAgentDeviceType.GameConsole, null, "Game Console");
            }
            return null;
        }

        private static UserAgentDeviceInfo? TryParseIos(string ua)
        {
            var ual = ua.ToLowerInvariant();
            if (ual.Contains("ipod")) return new UserAgentDeviceInfo(UserAgentDeviceType.IPod, null, "iPod");
            if (ual.Contains("ipad")) return new UserAgentDeviceInfo(UserAgentDeviceType.IPad, null, "iPad");
            if (ual.Contains("iphone")) return new UserAgentDeviceInfo(UserAgentDeviceType.IPhone, null, "iPhone");
            return null;
        }

        private static UserAgentDeviceInfo? TryParseAndroid(string ua)
        {
            var ual = ua.ToLowerInvariant();
            if (!ual.Contains("android")) return null;

            var isMobile = ual.Contains("mobile");
            var model = TryExtractAndroidModel(ua);

            var knownCode = model != null ? TryGetFirstKnownDeviceCode(model) : TryGetFirstKnownDeviceCode(ua);
            if (knownCode != null)
            {
                return knownCode;
            }

            // Samsung detection (SM-*) fallback
            var samsungCode = model != null ? GetFirstSamsungModelCode(model) : null;
            if (samsungCode != null)
            {
                var samsungType = ResolveSamsungType(samsungCode);
                return new UserAgentDeviceInfo(samsungType, samsungCode, "Samsung " + samsungCode);
            }

            if (isMobile)
            {
                return model?.Length > 0
                    ? new UserAgentDeviceInfo(UserAgentDeviceType.AndroidPhone, model, $"Android Phone ({model})")
                    : new UserAgentDeviceInfo(UserAgentDeviceType.AndroidPhone, null, "Android Phone");
            }

            return model?.Length > 0
                ? new UserAgentDeviceInfo(UserAgentDeviceType.AndroidTablet, model, $"Android Tablet ({model})")
                : new UserAgentDeviceInfo(UserAgentDeviceType.AndroidTablet, null, "Android Tablet");
        }

        private static string? GetFirstSamsungModelCode(string value)
        {
            var m = _samsungModelRegex.Match(value);
            return m.Success ? m.Value : null;
        }

        private static UserAgentDeviceInfo? TryGetFirstKnownDeviceCode(string value)
        {
            // Lookup-only: we only return a match if the extracted code exists in KnownDeviceCodes.Map.

            // 1) Samsung
            var samsungCode = GetFirstSamsungModelCode(value);
            if (samsungCode != null && KnownDeviceCodes.Map.TryGetValue(samsungCode, out var knownSamsung))
            {
                return knownSamsung;
            }

            // 2) Apple machine ids (useful in SDK/logs, typically not present in Safari UA)
            var am = _appleMachineRegex.Match(value);
            if (am.Success && KnownDeviceCodes.Map.TryGetValue(am.Value, out var knownApple))
            {
                return knownApple;
            }

            // 3) OnePlus CPH
            var op = _onePlusRegex.Match(value);
            if (op.Success && KnownDeviceCodes.Map.TryGetValue(op.Value, out var knownOp))
            {
                return knownOp;
            }

            // 4) Xiaomi-ish SKU (rough; match then lookup)
            var xi = _xiaomiSkuRegex.Match(value);
            if (xi.Success && KnownDeviceCodes.Map.TryGetValue(xi.Value, out var knownXi))
            {
                return knownXi;
            }
            return null;
        }

        private static UserAgentDeviceType ResolveSamsungType(string samsungModelCode)
        {
            // Heuristics based on common Samsung model code families.
            // This is intentionally simple and can be extended with more rules.
            if (samsungModelCode.StartsWith("SM-T", StringComparison.OrdinalIgnoreCase) || samsungModelCode.StartsWith("SM-X", StringComparison.OrdinalIgnoreCase))
            {
                return UserAgentDeviceType.SamsungTablet;
            }

            if (samsungModelCode.StartsWith("SM-R", StringComparison.OrdinalIgnoreCase)
                || samsungModelCode.StartsWith("SM-W", StringComparison.OrdinalIgnoreCase)
                || samsungModelCode.StartsWith("SM-L", StringComparison.OrdinalIgnoreCase))
            {
                return UserAgentDeviceType.SamsungWatch;
            }

            // Default to phone (most SM-* are phones).
            return UserAgentDeviceType.SamsungPhone;
        }

        private static string[]? TryGetParenthesesParts(string ua)
        {
            var p = Regex.Match(ua, @"\(([^)]*)\)");
            return p.Success ? p.Groups[1].Value.Split(';') : null;
        }

        private static bool HasTabletToken(string[] parts)
        {
            foreach (var part in parts)
            {
                if (part.Trim().Equals("tablet", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAndroidToken(string token)
        {
            return token.StartsWith("Android", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsNoiseModelToken(string token, bool hasTabletToken)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return true;
            }

            if (token.Equals("wv", StringComparison.OrdinalIgnoreCase)
                || token.Equals("mobile", StringComparison.OrdinalIgnoreCase)
                || token.Equals("tablet", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return hasTabletToken && token.StartsWith("rv:", StringComparison.OrdinalIgnoreCase);
        }

        private static string? TryExtractAndroidModel(string ua)
        {
            // Pattern 1: ...; <model> Build/...
            var m = Regex.Match(ua, @";\s*([^;]+?)\s+Build/", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                var model = SanitizeModel(m.Groups[1].Value);
                return model.Length > 0 ? model : null;
            }

            // Pattern 2: (Linux; Android 13; SM-G981B)  => take token after Android version
            var parts = TryGetParenthesesParts(ua);
            if (parts == null)
            {
                return null;
            }

            var hasTabletToken = HasTabletToken(parts);
            for (var i = 0; i < parts.Length; i++)
            {
                var token = parts[i].Trim();
                if (!IsAndroidToken(token))
                {
                    continue;
                }

                for (var j = i + 1; j < parts.Length; j++)
                {
                    var cand = parts[j].Trim();
                    if (IsNoiseModelToken(cand, hasTabletToken))
                    {
                        continue;
                    }

                    var model = SanitizeModel(cand);
                    return model.Length > 0 ? model : null;
                }

                return null;
            }

            return null;
        }

        private static UserAgentDeviceInfo? TryParseChromeOs(string ua)
        {
            if (ua.IndexOf("CrOS", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new UserAgentDeviceInfo(UserAgentDeviceType.Chromebook, null, "Chromebook");
            }
            return null;
        }

        private static UserAgentDeviceInfo? TryParseDesktop(string ua)
        {
            var ual = ua.ToLowerInvariant();

            if (ual.Contains("windows nt")) return new UserAgentDeviceInfo(UserAgentDeviceType.WindowsPc, null, "Windows PC");
            if (ual.Contains("macintosh") || ual.Contains("mac os x")) return new UserAgentDeviceInfo(UserAgentDeviceType.Mac, null, "Mac");

            // must be after android match to avoid classifying Android as Linux
            if (ual.Contains("linux")) return new UserAgentDeviceInfo(UserAgentDeviceType.LinuxPc, null, "Linux PC");

            return null;
        }

        private static UserAgentDeviceInfo? TryParseMobileFallback(string ua)
        {
            if (ua.IndexOf("Mobile", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new UserAgentDeviceInfo(UserAgentDeviceType.Mobile, null, "Mobile");
            }
            return null;
        }

        private static UserAgentDeviceInfo? TryParseServerFallback(string ua)
        {
            if (ua.IndexOf("server", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new UserAgentDeviceInfo(UserAgentDeviceType.Server, null, "Server");
            }
            return null;
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

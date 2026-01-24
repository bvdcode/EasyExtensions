// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;
using System.Collections.Generic;

namespace EasyExtensions.Helpers
{
    internal static class KnownDeviceCodes
    {
        internal static readonly Dictionary<string, UserAgentDeviceInfo> Map = new Dictionary<string, UserAgentDeviceInfo>(StringComparer.OrdinalIgnoreCase)
        {
            // =========================
            // Samsung phones (flagships)
            // =========================
            ["SM-G981B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-G981B", "Samsung Galaxy S20"),
            ["SM-G991B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-G991B", "Samsung Galaxy S21"),
            ["SM-G996B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-G996B", "Samsung Galaxy S21+"),
            ["SM-G998B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-G998B", "Samsung Galaxy S21 Ultra"),

            ["SM-S901B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-S901B", "Samsung Galaxy S22"),
            ["SM-S906B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-S906B", "Samsung Galaxy S22+"),
            ["SM-S908B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-S908B", "Samsung Galaxy S22 Ultra"),

            ["SM-S911B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-S911B", "Samsung Galaxy S23"),
            ["SM-S916B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-S916B", "Samsung Galaxy S23+"),
            ["SM-S918B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-S918B", "Samsung Galaxy S23 Ultra"),

            ["SM-S921B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-S921B", "Samsung Galaxy S24"),
            ["SM-S926B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-S926B", "Samsung Galaxy S24+"),
            ["SM-S928B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-S928B", "Samsung Galaxy S24 Ultra"),
            ["SM-S928U1"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-S928U1", "Samsung Galaxy S24 Ultra (US Unlocked)"),

            // S25 family (????? ??????????? ? ?????/?????????; ???????????? ???????? ???? ??????)
            ["SM-S931B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-S931B", "Samsung Galaxy S25"),
            ["SM-S936B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-S936B", "Samsung Galaxy S25+"),
            ["SM-S938B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-S938B", "Samsung Galaxy S25 Ultra"),

            // =========================
            // Samsung foldables
            // =========================
            ["SM-F936B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-F936B", "Samsung Galaxy Z Fold4"),
            ["SM-F731B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-F731B", "Samsung Galaxy Z Flip5"),
            ["SM-F946B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungPhone, "SM-F946B", "Samsung Galaxy Z Fold5"),

            // =========================
            // Samsung tablets (Tab S-series)
            // =========================
            ["SM-X710"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungTablet, "SM-X710", "Samsung Galaxy Tab S9 (Wi-Fi)"),
            ["SM-X716B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungTablet, "SM-X716B", "Samsung Galaxy Tab S9 (5G)"),

            ["SM-X910"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungTablet, "SM-X910", "Samsung Galaxy Tab S9 Ultra (Wi-Fi)"),
            ["SM-X916B"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungTablet, "SM-X916B", "Samsung Galaxy Tab S9 Ultra (5G)"),

            ["SM-X920"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungTablet, "SM-X920", "Samsung Galaxy Tab S10 Ultra"),

            // =========================
            // Samsung watches
            // =========================
            ["SM-R930"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungWatch, "SM-R930", "Samsung Galaxy Watch6 40mm"),
            ["SM-R940"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungWatch, "SM-R940", "Samsung Galaxy Watch6 44mm"),
            ["SM-R960"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungWatch, "SM-R960", "Samsung Galaxy Watch6 Classic 47mm"),

            ["SM-L310"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungWatch, "SM-L310", "Samsung Galaxy Watch7 (Bluetooth)"),
            ["SM-L315"] = new UserAgentDeviceInfo(UserAgentDeviceType.SamsungWatch, "SM-L315", "Samsung Galaxy Watch7 (LTE)"),

            // ============================================================
            // Google Pixel (????? ??????????? ??? device/sku codes ? UA/app)
            // ============================================================
            ["G9BQD"] = new UserAgentDeviceInfo(UserAgentDeviceType.GooglePhone, "G9BQD", "Google Pixel 8"),
            ["GKWS6"] = new UserAgentDeviceInfo(UserAgentDeviceType.GooglePhone, "GKWS6", "Google Pixel 8 (variant)"),

            // =========================
            // OnePlus (????? CPH****)
            // =========================
            ["CPH2581"] = new UserAgentDeviceInfo(UserAgentDeviceType.OnePlusPhone, "CPH2581", "OnePlus 12 (Global)"),
            ["CPH2609"] = new UserAgentDeviceInfo(UserAgentDeviceType.OnePlusPhone, "CPH2609", "OnePlus 12R (Global)"),

            // =========================
            // Xiaomi / Redmi / POCO (????? "M****" ??? "231***")
            // =========================
            ["23127PN0CG"] = new UserAgentDeviceInfo(UserAgentDeviceType.XiaomiPhone, "23127PN0CG", "Xiaomi 14 (Global)"),
            ["23090RA98G"] = new UserAgentDeviceInfo(UserAgentDeviceType.XiaomiPhone, "23090RA98G", "POCO X6 Pro (Global)"),

            // ============================================================
            // Apple iPhone / iPad / Watch (?????? machine identifiers)
            //   ?????: ? Safari UA ????? ?????? ???. ??? ??????? ??? SDK/?????.
            // ============================================================
            ["iPhone15,2"] = new UserAgentDeviceInfo(UserAgentDeviceType.ApplePhone, "iPhone15,2", "iPhone 14 Pro"),
            ["iPhone15,3"] = new UserAgentDeviceInfo(UserAgentDeviceType.ApplePhone, "iPhone15,3", "iPhone 14 Pro Max"),
            ["iPhone16,1"] = new UserAgentDeviceInfo(UserAgentDeviceType.ApplePhone, "iPhone16,1", "iPhone 15 Pro"),
            ["iPhone16,2"] = new UserAgentDeviceInfo(UserAgentDeviceType.ApplePhone, "iPhone16,2", "iPhone 15 Pro Max"),
            ["iPhone17,1"] = new UserAgentDeviceInfo(UserAgentDeviceType.ApplePhone, "iPhone17,1", "iPhone 16 Pro"),
            ["iPhone17,2"] = new UserAgentDeviceInfo(UserAgentDeviceType.ApplePhone, "iPhone17,2", "iPhone 16 Pro Max"),
            ["iPhone17,3"] = new UserAgentDeviceInfo(UserAgentDeviceType.ApplePhone, "iPhone17,3", "iPhone 16"),
            ["iPhone17,4"] = new UserAgentDeviceInfo(UserAgentDeviceType.ApplePhone, "iPhone17,4", "iPhone 16 Plus"),
        };
    }
}

using EasyExtensions.Helpers;

namespace EasyExtensions.Tests
{
    public class UserAgentHelpersTests
    {
        /*
        PSEUDOCODE PLAN:
        - Test null/whitespace:
          - Inputs: null, "", "   " -> Expect "Unknown".
        - Test bots and scripts:
          - Bot UAs: Googlebot, BingPreview, FacebookExternalHit -> Expect "Bot".
          - Script UAs: curl, Java, okhttp -> Expect "Script".
        - Test TVs and consoles:
          - SMART-TV, AppleTV, HbbTV -> Expect "Smart TV".
          - PlayStation, Xbox, Nintendo Switch, Steam Deck -> Expect "Game Console".
        - Test iOS devices:
          - iPhone UA -> "iPhone"
          - iPad UA -> "iPad"
          - iPod UA -> "iPod"
        - Test Android classification:
          - Phone with model (contains "Mobile" and "; <model> Build/") -> "Android Phone (<model>)"
          - Phone without model (no "Build/") -> "Android Phone"
          - Tablet with model (no "Mobile", has "Build/") -> "Android Tablet (<model>)"
          - Tablet without model (no "Mobile", no "Build/") -> "Android Tablet"
        - Test ChromeOS:
          - CrOS UA -> "Chromebook"
        - Test desktop OS:
          - Windows NT -> "Windows PC"
          - Macintosh/Mac OS X -> "Mac"
          - Linux (not Android) -> "Linux PC"
        - Test fallbacks:
          - UA with "Mobile" only -> "Mobile"
          - Generic/unknown UA -> "Desktop"
        - For each case, assert: Assert.That(UserAgentHelpers.GetDevice(ua), Is.EqualTo(expected));
        */

        [TestCase(null, "Unknown")]
        [TestCase("", "Unknown")]
        [TestCase("   ", "Unknown")]
        public void GetDevice_Returns_Unknown_For_NullOrWhitespace(string? ua, string expected)
        {
            Assert.That(UserAgentHelpers.GetDevice(ua!), Is.EqualTo(expected));
        }

        [TestCase("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)", "Bot")]
        [TestCase("Mozilla/5.0 (Linux; Android 12; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/88.0.4324.93 Mobile Safari/537.36 (BingPreview)", "Bot")]
        [TestCase("facebookexternalhit/1.1 (+http://www.facebook.com/externalhit_uatext.php)", "Bot")]
        [TestCase("curl/7.64.1", "Script")]
        [TestCase("Java/11.0.2", "Script")]
        [TestCase("okhttp/3.12.1", "Script")]
        public void GetDevice_Identifies_Bots_And_Scripts(string ua, string expected)
        {
            Assert.That(UserAgentHelpers.GetDevice(ua), Is.EqualTo(expected));
        }

        [TestCase("Mozilla/5.0 (SMART-TV; Linux; Tizen 2.4.0) AppleWebKit/538.1", "Smart TV")]
        [TestCase("AppleTV11,1/11.1", "Smart TV")]
        [TestCase("HbbTV/1.2.1 (+DRM;SomeVendor)", "Smart TV")]
        [TestCase("Mozilla/5.0 (PlayStation 4 3.11) AppleWebKit/537.73 (KHTML, like Gecko)", "Game Console")]
        [TestCase("Mozilla/5.0 (Xbox; Xbox One; rv:10.0) like Gecko", "Game Console")]
        [TestCase("Mozilla/5.0 (Nintendo Switch; WifiWebAuthApplet)", "Game Console")]
        [TestCase("Mozilla/5.0 (Steam Deck) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0 Safari/537.36", "Game Console")]
        public void GetDevice_Identifies_TVs_And_Consoles(string ua, string expected)
        {
            Assert.That(UserAgentHelpers.GetDevice(ua), Is.EqualTo(expected));
        }

        [TestCase("Mozilla/5.0 (iPhone; CPU iPhone OS 14_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1", "iPhone")]
        [TestCase("Mozilla/5.0 (iPad; CPU OS 14_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1", "iPad")]
        [TestCase("Mozilla/5.0 (iPod touch; CPU iPhone OS 12_4 like Mac OS X) AppleWebKit/600.1.4 (KHTML, like Gecko) Version/8.0 Mobile/12H143 Safari/600.1.4", "iPod")]
        public void GetDevice_Identifies_iOS(string ua, string expected)
        {
            Assert.That(UserAgentHelpers.GetDevice(ua), Is.EqualTo(expected));
        }

        [TestCase("Mozilla/5.0 (Linux; Android 12; Pixel 6 Build/SP1A.210812.016; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/94.0.4606.61 Mobile Safari/537.36", "Android Phone (Pixel 6)")]
        [TestCase("Mozilla/5.0 (Linux; Android 10; SM-G973F Build/QP1A.190711.020) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Mobile Safari/537.36", "Android Phone (SM-G973F)")]
        [TestCase("Mozilla/5.0 (Linux; Android 10; SM-G973F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Mobile Safari/537.36", "Android Phone")]
        [TestCase("Mozilla/5.0 (Linux; Android 11; SM-T970 Build/RP1A.200720.012) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.93 Safari/537.36", "Android Tablet (SM-T970)")]
        [TestCase("Mozilla/5.0 (Linux; Android 13; Tablet; rv:102.0) Gecko/102.0 Firefox/102.0", "Android Tablet")]
        public void GetDevice_Identifies_Android(string ua, string expected)
        {
            Assert.That(UserAgentHelpers.GetDevice(ua), Is.EqualTo(expected));
        }

        [TestCase("Mozilla/5.0 (X11; CrOS x86_64 14541.0.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.85 Safari/537.36", "Chromebook")]
        public void GetDevice_Identifies_ChromeOS(string ua, string expected)
        {
            Assert.That(UserAgentHelpers.GetDevice(ua), Is.EqualTo(expected));
        }

        [TestCase("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36 Edge/92.0.902.78", "Windows PC")]
        [TestCase("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.2 Safari/605.1.15", "Mac")]
        [TestCase("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36", "Linux PC")]
        public void GetDevice_Identifies_DesktopOS(string ua, string expected)
        {
            Assert.That(UserAgentHelpers.GetDevice(ua), Is.EqualTo(expected));
        }

        [TestCase("Mozilla/5.0 (Mobile; rv:47.0) Gecko/47.0 Firefox/47.0", "Mobile")]
        [TestCase("Mozilla/5.0 (compatible; FooBar/1.0)", "Desktop")]
        public void GetDevice_Fallbacks(string ua, string expected)
        {
            Assert.That(UserAgentHelpers.GetDevice(ua), Is.EqualTo(expected));
        }
    }
}

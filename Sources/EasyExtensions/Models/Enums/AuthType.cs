// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Models.Enums
{
    /// <summary>
    /// Specifies the authentication mechanism that was used to establish the current session
    /// or to access a protected resource.
    /// </summary>
    /// <remarks>
    /// The enumeration covers a broad range of authentication approaches: local credentials,
    /// federated consumer identity providers (OAuth 2.0 / OpenID Connect), regional providers,
    /// enterprise single sign-on platforms, protocol-level standards (SAML, LDAP, Kerberos),
    /// passwordless and multi-factor schemes (Passkey, WebAuthn, OTP, magic links),
    /// and machine-to-machine credentials (API key, client certificate, bearer token).
    /// Persist the most specific value that describes how the principal actually proved their
    /// identity - it is useful for audit logs, telemetry, risk scoring, and conditional access.
    /// </remarks>
    public enum AuthType
    {
        /// <summary>
        /// The authentication method is unknown or has not been specified.
        /// </summary>
        /// <remarks>
        /// Use this value when the original authentication source cannot be determined - for example,
        /// when silently refreshing a session without re-authenticating, when reading legacy records
        /// that predate this enumeration, or when the value is not yet available. This is the default
        /// value of the enumeration.
        /// </remarks>
        Unknown = 0,

        /// <summary>
        /// Local credentials managed by the application itself, typically a username or email
        /// combined with a password (or password hash) stored in the application's own user store.
        /// </summary>
        /// <remarks>
        /// This represents the classic first-party "log in with email and password" flow.
        /// It does NOT include third-party identity providers - those have their own dedicated
        /// values. Prefer a more specific value (for example <see cref="MagicLink"/> or
        /// <see cref="OneTimePassword"/>) when the actual mechanism is passwordless.
        /// </remarks>
        Credentials = 1,

        /// <summary>
        /// Google identity platform - Google Sign-In for consumer accounts, Google Workspace,
        /// or Google Cloud Identity, performed over OAuth 2.0 / OpenID Connect.
        /// </summary>
        Google = 2,

        /// <summary>
        /// Clover platform sign-in, used by merchants and integrators of the Clover point-of-sale
        /// ecosystem (clover.com) over OAuth 2.0.
        /// </summary>
        Clover = 3,

        // -------- Major consumer OAuth 2.0 / OpenID Connect providers --------

        /// <summary>
        /// Microsoft identity platform for personal Microsoft accounts (Outlook, Hotmail, Live,
        /// Xbox). For organizational tenants in Microsoft Entra ID use <see cref="AzureAd"/> instead.
        /// </summary>
        Microsoft = 4,

        /// <summary>
        /// "Sign in with Apple" - Apple's privacy-focused identity provider over OAuth 2.0 / OIDC,
        /// commonly required by iOS and macOS App Store applications that offer third-party sign-in.
        /// </summary>
        Apple = 5,

        /// <summary>
        /// Meta / Facebook Login over OAuth 2.0. Also used as the back-end for "Continue with
        /// Facebook" on Instagram, WhatsApp Business, and other Meta properties.
        /// </summary>
        Facebook = 6,

        /// <summary>
        /// GitHub OAuth Apps or GitHub Apps - commonly used by developer tools, CI/CD systems,
        /// and forge integrations.
        /// </summary>
        GitHub = 7,

        /// <summary>
        /// GitLab OAuth provider, both for gitlab.com and self-hosted GitLab instances.
        /// </summary>
        GitLab = 8,

        /// <summary>
        /// Atlassian Bitbucket OAuth, for Bitbucket Cloud and Bitbucket Data Center.
        /// </summary>
        Bitbucket = 9,

        /// <summary>
        /// X (formerly Twitter) OAuth 2.0 / OAuth 1.0a "Log in with X".
        /// </summary>
        Twitter = 10,

        /// <summary>
        /// LinkedIn "Sign In with LinkedIn" over OAuth 2.0 / OpenID Connect.
        /// </summary>
        LinkedIn = 11,

        /// <summary>
        /// Discord OAuth 2.0 - commonly used by gaming communities, bots, and developer tooling.
        /// </summary>
        Discord = 12,

        /// <summary>
        /// Twitch OAuth 2.0 - used for streamer tooling, overlays, and viewer-facing integrations.
        /// </summary>
        Twitch = 13,

        /// <summary>
        /// Reddit OAuth 2.0.
        /// </summary>
        Reddit = 14,

        /// <summary>
        /// Yahoo OAuth 2.0 / OpenID Connect.
        /// </summary>
        Yahoo = 15,

        /// <summary>
        /// "Login with Amazon" - Amazon's consumer OAuth 2.0 provider (distinct from AWS IAM /
        /// Cognito enterprise authentication).
        /// </summary>
        Amazon = 16,

        /// <summary>
        /// Steam OpenID 2.0 - the legacy OpenID flow exposed by Valve's Steam platform,
        /// commonly used by gaming-related sites.
        /// </summary>
        Steam = 17,

        /// <summary>
        /// Spotify "Log in with Spotify" over OAuth 2.0.
        /// </summary>
        Spotify = 18,

        /// <summary>
        /// Slack OAuth 2.0 - both "Sign in with Slack" for user identity and the workspace
        /// installation flow for Slack apps.
        /// </summary>
        Slack = 19,

        /// <summary>
        /// Dropbox OAuth 2.0.
        /// </summary>
        Dropbox = 20,

        /// <summary>
        /// "Log In with PayPal" - PayPal's OpenID Connect provider used to verify a user's PayPal
        /// account identity (separate from accepting a payment).
        /// </summary>
        PayPal = 21,

        /// <summary>
        /// Stripe Connect OAuth 2.0 - used by platforms to onboard merchants and access their
        /// Stripe accounts.
        /// </summary>
        Stripe = 22,

        // -------- Regional providers: Russia / CIS --------

        /// <summary>
        /// Yandex ID OAuth - the unified identity provider behind Yandex services
        /// (Mail, Disk, Music, Maps, Taxi, etc.).
        /// </summary>
        Yandex = 23,

        /// <summary>
        /// VK ID / VKontakte OAuth - the dominant social-network sign-in across Russia and CIS.
        /// </summary>
        Vk = 24,

        /// <summary>
        /// Mail.ru OAuth provider.
        /// </summary>
        MailRu = 25,

        /// <summary>
        /// Odnoklassniki (OK.ru) OAuth - the second major Russian social-network sign-in,
        /// part of the VK Group ecosystem.
        /// </summary>
        Odnoklassniki = 26,

        /// <summary>
        /// Telegram Login Widget / "Log in with Telegram" - identity is conveyed by a signed
        /// payload from the Telegram bot platform rather than a classic OAuth code exchange.
        /// </summary>
        Telegram = 27,

        /// <summary>
        /// Gosuslugi (ESIA) - the Russian Federation's unified state identification and
        /// authentication system used by government and regulated services.
        /// </summary>
        Gosuslugi = 28,

        // -------- Regional providers: Asia --------

        /// <summary>
        /// WeChat (微信) OAuth 2.0 - dominant Chinese consumer sign-in by Tencent.
        /// </summary>
        WeChat = 29,

        /// <summary>
        /// QQ Connect OAuth - Tencent's QQ-based consumer sign-in.
        /// </summary>
        QQ = 30,

        /// <summary>
        /// Sina Weibo (新浪微博) OAuth - widely used Chinese microblogging identity.
        /// </summary>
        Weibo = 31,

        /// <summary>
        /// Alipay (支付宝) OAuth - Ant Group's identity provider, often used alongside payment.
        /// </summary>
        Alipay = 32,

        /// <summary>
        /// Baidu (百度) OAuth.
        /// </summary>
        Baidu = 33,

        /// <summary>
        /// LINE Login - widely used in Japan, Taiwan, and Thailand.
        /// </summary>
        Line = 34,

        /// <summary>
        /// Naver Login (네이버 로그인) - one of the two dominant Korean consumer sign-ins.
        /// </summary>
        Naver = 35,

        /// <summary>
        /// Kakao Login (카카오 로그인) - the other dominant Korean consumer sign-in,
        /// tied to KakaoTalk.
        /// </summary>
        Kakao = 36,

        // -------- Other social / consumer providers --------

        /// <summary>
        /// Instagram Basic Display / Instagram Login (Meta-owned, but exposed as a distinct
        /// product surface from <see cref="Facebook"/>).
        /// </summary>
        Instagram = 37,

        /// <summary>
        /// TikTok Login Kit OAuth 2.0.
        /// </summary>
        TikTok = 38,

        /// <summary>
        /// Pinterest OAuth 2.0.
        /// </summary>
        Pinterest = 39,

        /// <summary>
        /// Snap (Snapchat) Login Kit.
        /// </summary>
        Snapchat = 40,

        // -------- Enterprise SSO / Identity-as-a-Service --------

        /// <summary>
        /// Microsoft Entra ID (formerly Azure Active Directory) - work or school accounts in a
        /// Microsoft 365 / Azure tenant. For personal Microsoft accounts use <see cref="Microsoft"/>.
        /// </summary>
        AzureAd = 41,

        /// <summary>
        /// Okta workforce or customer identity (Okta CIAM / Customer Identity Cloud).
        /// </summary>
        Okta = 42,

        /// <summary>
        /// Auth0 - Okta's developer-oriented identity platform, treated as a separate product
        /// surface from classic Okta tenants.
        /// </summary>
        Auth0 = 43,

        /// <summary>
        /// OneLogin by One Identity.
        /// </summary>
        OneLogin = 44,

        /// <summary>
        /// Ping Identity (PingOne, PingFederate, PingID).
        /// </summary>
        PingIdentity = 45,

        /// <summary>
        /// JumpCloud directory platform.
        /// </summary>
        JumpCloud = 46,

        /// <summary>
        /// Keycloak - open-source identity and access management server (Red Hat SSO upstream).
        /// </summary>
        Keycloak = 47,

        /// <summary>
        /// Amazon Cognito user pools / identity pools.
        /// </summary>
        AwsCognito = 48,

        /// <summary>
        /// Firebase Authentication (Google Cloud / Firebase).
        /// </summary>
        FirebaseAuth = 49,

        /// <summary>
        /// IBM Security Verify (formerly IBM Cloud Identity).
        /// </summary>
        IbmSecurityVerify = 50,

        /// <summary>
        /// Cisco Duo - typically used as a second factor on top of another primary authenticator.
        /// </summary>
        Duo = 51,

        // -------- Protocol-level / standards-based authentication --------

        /// <summary>
        /// On-premises Microsoft Active Directory authentication (typically via Integrated Windows
        /// Authentication / NTLM / Kerberos against a domain controller). For cloud Entra ID
        /// tenants use <see cref="AzureAd"/>.
        /// </summary>
        ActiveDirectory = 52,

        /// <summary>
        /// Generic LDAP / LDAPS bind against a directory server (OpenLDAP, 389-DS, Apple Open
        /// Directory, etc.).
        /// </summary>
        Ldap = 53,

        /// <summary>
        /// Kerberos ticket-based authentication (SPNEGO / GSSAPI), used by both on-premises AD
        /// and Unix realms.
        /// </summary>
        Kerberos = 54,

        /// <summary>
        /// SAML 2.0 federation - typical enterprise B2B SSO assertion flow.
        /// </summary>
        Saml = 55,

        /// <summary>
        /// Generic OAuth 2.0 provider that does not have a dedicated value in this enumeration.
        /// </summary>
        OAuth = 56,

        /// <summary>
        /// Generic OpenID Connect provider that does not have a dedicated value in this enumeration.
        /// </summary>
        OpenIdConnect = 57,

        /// <summary>
        /// Central Authentication Service (CAS) - protocol common in higher-education environments.
        /// </summary>
        Cas = 58,

        /// <summary>
        /// Shibboleth - SAML-based federated identity widely deployed across research and
        /// education federations such as InCommon and eduGAIN.
        /// </summary>
        Shibboleth = 59,

        /// <summary>
        /// WS-Federation - legacy Microsoft federation protocol (ADFS, classic SharePoint).
        /// </summary>
        WsFederation = 60,

        /// <summary>
        /// NTLM challenge-response authentication. Considered legacy; prefer <see cref="Kerberos"/>
        /// or modern OIDC where possible.
        /// </summary>
        Ntlm = 61,

        // -------- Passwordless and multi-factor schemes --------

        /// <summary>
        /// Magic link - a single-use, time-limited URL delivered to the user's verified email
        /// (or other channel) that authenticates them on click.
        /// </summary>
        MagicLink = 62,

        /// <summary>
        /// Generic one-time password not tied to a specific delivery channel
        /// (printed backup codes, recovery codes, generic OTP).
        /// </summary>
        OneTimePassword = 63,

        /// <summary>
        /// Time-based one-time password (RFC 6238) generated by an authenticator app such as
        /// Google Authenticator, Microsoft Authenticator, Authy, or 1Password.
        /// </summary>
        TimeBasedOneTimePassword = 64,

        /// <summary>
        /// One-time code delivered to the user by SMS.
        /// </summary>
        SmsOtp = 65,

        /// <summary>
        /// One-time code delivered to the user by email.
        /// </summary>
        EmailOtp = 66,

        /// <summary>
        /// One-time code delivered to the user by an automated voice call.
        /// </summary>
        VoiceOtp = 67,

        /// <summary>
        /// Passkey - a synced or device-bound WebAuthn credential branded as a "passkey" in
        /// modern OS-level password managers (iCloud Keychain, Google Password Manager, Windows
        /// Hello). Wraps <see cref="WebAuthn"/> at the protocol level but is recorded distinctly
        /// because the user experience and recoverability differ.
        /// </summary>
        Passkey = 68,

        /// <summary>
        /// W3C WebAuthn assertion (browser-side API of the <see cref="Fido2"/> standard) using
        /// either a platform authenticator or a roaming authenticator.
        /// </summary>
        WebAuthn = 69,

        /// <summary>
        /// FIDO2 authentication (WebAuthn + CTAP2) - typically a hardware security key or
        /// platform authenticator acting as a sole factor.
        /// </summary>
        Fido2 = 70,

        /// <summary>
        /// Biometric authentication on the local device (Face ID, Touch ID, Windows Hello face
        /// or fingerprint) when not bridged through WebAuthn / Passkey.
        /// </summary>
        Biometric = 71,

        /// <summary>
        /// Hardware security key used through a non-FIDO2 path - for example, a YubiKey acting
        /// as a smart card / PIV token, or U2F-only legacy keys.
        /// </summary>
        HardwareSecurityKey = 72,

        /// <summary>
        /// Out-of-band approval delivered as a push notification to a trusted device
        /// (Microsoft Authenticator push, Duo Push, Okta Verify push, etc.).
        /// </summary>
        PushNotification = 73,

        /// <summary>
        /// QR-code based sign-in, where the user scans a code on one device to authenticate a
        /// session on another (cross-device authentication).
        /// </summary>
        QrCode = 74,

        // -------- Token-based and machine-to-machine authentication --------

        /// <summary>
        /// Static API key, typically issued to a service account or developer integration
        /// and transmitted in a header or query string.
        /// </summary>
        ApiKey = 75,

        /// <summary>
        /// Mutual TLS (mTLS) - the caller is identified by a client X.509 certificate presented
        /// during the TLS handshake.
        /// </summary>
        ClientCertificate = 76,

        /// <summary>
        /// HTTP Basic authentication (RFC 7617) - base64-encoded username:password in the
        /// Authorization header. Considered legacy; prefer stronger schemes where possible.
        /// </summary>
        BasicAuth = 77,

        /// <summary>
        /// HTTP Bearer token (RFC 6750) whose internal format is opaque or unspecified.
        /// Use <see cref="JsonWebToken"/> when the bearer is specifically a JWT.
        /// </summary>
        BearerToken = 78,

        /// <summary>
        /// JSON Web Token (RFC 7519) presented as a bearer credential and validated by signature
        /// against the issuer.
        /// </summary>
        JsonWebToken = 79,

        /// <summary>
        /// Personal access token issued by a developer-facing platform (GitHub PAT, GitLab PAT,
        /// Atlassian API token, etc.) and used in place of a password.
        /// </summary>
        PersonalAccessToken = 80,

        // -------- Other / specialty --------

        /// <summary>
        /// Anonymous access - no identifying credential was presented, and the principal is not
        /// associated with any account.
        /// </summary>
        Anonymous = 81,

        /// <summary>
        /// Guest session - a short-lived, non-account identity (for example, a guest checkout
        /// or a temporary share link).
        /// </summary>
        Guest = 82,

        /// <summary>
        /// National BankID schemes used in the Nordics and Baltics - Swedish BankID, Norwegian
        /// BankID, Danish MitID, Finnish Trust Network, etc.
        /// </summary>
        BankId = 83,

        /// <summary>
        /// National electronic ID schemes (eIDAS-compliant national eIDs, smart-card national
        /// IDs, e-residency cards).
        /// </summary>
        NationalEid = 84,

        /// <summary>
        /// ID.me - identity-verification provider used by U.S. federal and state agencies and
        /// some private organizations.
        /// </summary>
        IdMe = 85,
    }
}

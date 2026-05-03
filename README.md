[![License](https://img.shields.io/github/license/bvdcode/EasyExtensions)](https://github.com/bvdcode/EasyExtensions/blob/main/LICENSE.md)
[![NuGet](https://img.shields.io/nuget/v/EasyExtensions.svg?label=EasyExtensions)](https://www.nuget.org/packages/EasyExtensions/)
[![NuGet downloads](https://img.shields.io/nuget/dt/EasyExtensions?color=9100ff)](https://www.nuget.org/packages/EasyExtensions/)
[![FuGet](https://img.shields.io/badge/fuget-f88445?logo=readme&logoColor=white)](https://www.fuget.org/packages/EasyExtensions)
[![Build](https://img.shields.io/github/actions/workflow/status/bvdcode/EasyExtensions/publish-release.yml?branch=main)](https://github.com/bvdcode/EasyExtensions/actions/workflows/publish-release.yml)
[![CodeFactor](https://www.codefactor.io/repository/github/bvdcode/EasyExtensions/badge)](https://www.codefactor.io/repository/github/bvdcode/EasyExtensions)
![GitHub repo size](https://img.shields.io/github/repo-size/bvdcode/EasyExtensions)

# EasyExtensions

EasyExtensions is a modular set of .NET packages for application code that tends to repeat across projects: core BCL extensions, ASP.NET Core helpers, PostgreSQL/EF Core setup, Quartz job registration, WebDAV clients, ImageSharp utilities, embedded fonts, streaming AES-GCM encryption, and a lightweight mediator.

The repository is intentionally split into small NuGet packages. Install only the package you need, keep your application dependencies narrow, and use the XML documentation in your IDE or [FuGet](https://www.fuget.org/packages/EasyExtensions) when you need a full API reference.

## Contents

- [Packages](#packages)
- [Installation](#installation)
- [Quick Examples](#quick-examples)
- [Configuration Notes](#configuration-notes)
- [Build and Test](#build-and-test)
- [Releases](#releases)
- [Contributing](#contributing)
- [License](#license)

## Packages

| Package | Target | Use when you need |
| --- | --- | --- |
| [EasyExtensions](https://www.nuget.org/packages/EasyExtensions/) | `netstandard2.1` | Core extensions and helpers for strings, hashes, streams, enums, dates, IP/networking, claims, random strings, queues, password hashing abstractions, Brotli HTTP helpers, and stream cipher abstractions. |
| [EasyExtensions.AspNetCore](https://www.nuget.org/packages/EasyExtensions.AspNetCore/) | `net10.0` | ASP.NET Core helpers for exception responses, health checks, CORS, request metadata, rate limiting helpers, console logging, controllers, form files, CPU usage, and PBKDF2 password hashing registration. |
| [EasyExtensions.AspNetCore.Authorization](https://www.nuget.org/packages/EasyExtensions.AspNetCore.Authorization/) | `net10.0` | JWT authentication setup, token creation, claim building, development authorization bypass, and a base auth controller with login, refresh, logout, password, and Google login hooks. |
| [EasyExtensions.AspNetCore.Sentry](https://www.nuget.org/packages/EasyExtensions.AspNetCore.Sentry/) | `net10.0` | Sentry ASP.NET Core integration with user capture. |
| [EasyExtensions.AspNetCore.Stack](https://www.nuget.org/packages/EasyExtensions.AspNetCore.Stack/) | `net10.0` | One-call application setup for controllers, logging, compression, Quartz, SignalR, CORS, health checks, optional PostgreSQL, optional authorization, and optional EasyVault secrets. |
| [EasyExtensions.Clients](https://www.nuget.org/packages/EasyExtensions.Clients/) | `net10.0` | Cached IP lookup helpers backed by ipapi.co. |
| [EasyExtensions.Crypto](https://www.nuget.org/packages/EasyExtensions.Crypto/) | `net10.0` | Streaming AES-GCM encryption/decryption, per-chunk authentication, HKDF subkeys, secure random bytes, and hash helpers. |
| [EasyExtensions.Drawing](https://www.nuget.org/packages/EasyExtensions.Drawing/) | `net10.0` | ImageSharp helpers for JPEG conversion, drawing text, blurred backgrounds, automatic brightness adjustment, and font integration. |
| [EasyExtensions.EntityFrameworkCore](https://www.nuget.org/packages/EasyExtensions.EntityFrameworkCore/) | `net10.0` | Audited entities and DbContext base types, Gridify mapper registration, database health checks, and migration helpers. |
| [EasyExtensions.EntityFrameworkCore.Npgsql](https://www.nuget.org/packages/EasyExtensions.EntityFrameworkCore.Npgsql/) | `net10.0` | PostgreSQL DbContext registration, connection string construction from configuration, lazy loading options, and design-time factory registration. |
| [EasyExtensions.Fonts](https://www.nuget.org/packages/EasyExtensions.Fonts/) | `netstandard2.1` | Embedded fonts: Arial, Consola, FreeMonospaced, RetroGaming, and UbuntuMono. |
| [EasyExtensions.Mediator](https://www.nuget.org/packages/EasyExtensions.Mediator/) | `netstandard2.1` | A MediatR v12.5.0 based mediator package with request, notification, stream request, pipeline behavior, pre/post processor, and exception processor support. |
| [EasyExtensions.Quartz](https://www.nuget.org/packages/EasyExtensions.Quartz/) | `netstandard2.1` | Reflection-based Quartz job registration with `JobTriggerAttribute`, hosted service setup, and optional PostgreSQL persistent store. |
| [EasyExtensions.WebDav](https://www.nuget.org/packages/EasyExtensions.WebDav/) | `netstandard2.1` | WebDAV and Nextcloud file operations: folders, existence checks, uploads, listings, downloads, and deletes. |
| [EasyExtensions.Windows](https://www.nuget.org/packages/EasyExtensions.Windows/) | `netstandard2.1` | Windows-specific helpers for shortcuts and moving files or directories to the recycle bin. |

## Installation

Install packages individually:

```bash
dotnet add package EasyExtensions
dotnet add package EasyExtensions.AspNetCore
dotnet add package EasyExtensions.Crypto
dotnet add package EasyExtensions.EntityFrameworkCore.Npgsql
```

For an opinionated ASP.NET Core setup, start with:

```bash
dotnet add package EasyExtensions.AspNetCore.Stack
```

## Quick Examples

### Core Helpers

```csharp
using EasyExtensions.Extensions;
using EasyExtensions.Helpers;
using System.Net;

string digest = "hello".Sha512();
IPAddress network = IPAddress.Parse("192.168.10.25").GetNetwork(24);
string maskedEmail = StringHelpers.HideEmail("vadim@example.com");
```

### ASP.NET Core

```csharp
using EasyExtensions.AspNetCore.Extensions;

builder.Logging.AddSimpleConsoleLogging();

builder.Services
    .AddDefaultHealthChecks()
    .AddDefaultCorsWithOrigins("https://app.example.com")
    .AddExceptionHandler()
    .AddPbkdf2PasswordHashService();
```

### JWT Authorization

```csharp
using EasyExtensions.AspNetCore.Authorization.Extensions;

builder.Services.AddJwt(useCookies: true);
```

```json
{
  "JwtSettings": {
    "Key": "0123456789abcdef0123456789abcdef",
    "Issuer": "my-api",
    "Audience": "my-clients",
    "LifetimeMinutes": 60
  }
}
```

### EasyStack

```csharp
using EasyExtensions.AspNetCore.Stack.Extensions;

builder.AddEasyStack(stack => stack
    .WithPostgres<AppDbContext>(useLazyLoadingProxies: false)
    .AddAuthorization()
    .UseSecrets(useSecrets: true));
```

### PostgreSQL and EF Core

```csharp
using EasyExtensions.EntityFrameworkCore.Npgsql.Extensions;

builder.Services.AddPostgresDbContext<AppDbContext>(postgres =>
{
    postgres.ConfigurationSection = "DatabaseSettings";
    postgres.UseLazyLoadingProxies = false;
});
```

```json
{
  "DatabaseSettings": {
    "Host": "localhost",
    "Port": "5432",
    "Username": "postgres",
    "Password": "postgres",
    "Database": "app"
  }
}
```

### Quartz Jobs

```csharp
using EasyExtensions.Quartz.Attributes;
using EasyExtensions.Quartz.Extensions;
using Quartz;

[JobTrigger(minutes: 5, startNow: true)]
public sealed class CleanupJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        return Task.CompletedTask;
    }
}

builder.Services.AddQuartzJobs();
```

### Streaming Encryption

```csharp
using EasyExtensions.Crypto;
using System.Security.Cryptography;

byte[] masterKey = RandomNumberGenerator.GetBytes(AesGcmStreamCipher.KeySize);
using var cipher = new AesGcmStreamCipher(masterKey, memoryLimitBytes: 256L * 1024 * 1024);

await using var input = File.OpenRead("plain.bin");
await using var encrypted = File.Create("plain.bin.eegcm");
await cipher.EncryptAsync(input, encrypted);

await using var cipherText = File.OpenRead("plain.bin.eegcm");
await using var plainText = File.Create("plain-restored.bin");
await cipher.DecryptAsync(cipherText, plainText);
```

### WebDAV and Nextcloud

```csharp
using EasyExtensions.WebDav;

using var client = WebDavCloudClient.CreateNextcloudClient(
    "https://cloud.example.com",
    username: "user",
    password: "app-password");

await client.CreateFolderAsync("backups");
using var backup = File.OpenRead("backup.zip");
await client.UploadFileAsync(backup, "backups/backup.zip");
```

## Configuration Notes

- The full solution currently uses the .NET 10 SDK. Some packages still target `netstandard2.1`; see the package table before choosing a package for older applications.
- `EasyExtensions.AspNetCore.Authorization` accepts either a `JwtSettings` section or flat `JwtKey`, `JwtIssuer`, `JwtAudience`, and `JwtLifetimeMinutes` keys. Configure a persistent signing key in production.
- `EasyExtensions.AspNetCore.Stack` allows all CORS origins when `CorsOrigins` is missing. Set `CorsOrigins` explicitly for production services.
- `EasyExtensions.AspNetCore.Sentry` enables `SendDefaultPii` when Sentry is active. Review this against your data handling policy.
- `EasyExtensions.Crypto` expects you to own key storage and rotation. Do not hard-code master keys in source control.
- `EasyExtensions.WebDav` currently changes `ServicePointManager.ServerCertificateValidationCallback`; review this before production use.

## Build and Test

```bash
git clone https://github.com/bvdcode/EasyExtensions.git
cd EasyExtensions/Sources
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

The test projects use NUnit and target `net10.0`.

## Releases

Releases are produced by GitHub Actions from `main` when files under `Sources/` change. The workflow builds the solution, packs NuGet packages, publishes artifacts, pushes packages to GitHub Packages and NuGet.org, and creates a GitHub release. Versioning is driven by GitVersion; the current `next-version` is configured in [GitVersion.yml](GitVersion.yml).

## Contributing

Contributions are welcome. For a clean pull request:

1. Fork the repository.
2. Create a focused feature branch.
3. Add or update tests for behavior changes.
4. Run `dotnet test` from `Sources/`.
5. Update this README when package scope, setup, or public examples change.
6. Open a pull request with a clear description of the change.

Issues and feature requests are also welcome. Small, well-scoped proposals are easiest to review.

## License

Most packages are distributed under the MIT License. See [LICENSE.md](LICENSE.md).

`EasyExtensions.Mediator` is based on MediatR v12.5.0 and uses Apache-2.0 package licensing. See [Sources/EasyExtensions.Mediator/LICENSE.md](Sources/EasyExtensions.Mediator/LICENSE.md).

## Contact

Created and maintained by [Vadim Belov](mailto:github-easy-extensions@belov.us).

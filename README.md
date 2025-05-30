[![GitHub](https://img.shields.io/github/license/bvdcode/EasyExtensions)](https://github.com/bvdcode/EasyExtensions/blob/main/LICENSE)
[![Nuget](https://img.shields.io/nuget/dt/EasyExtensions?color=%239100ff)](https://www.nuget.org/packages/EasyExtensions/)
[![Static Badge](https://img.shields.io/badge/fuget-f88445?logo=readme&logoColor=white)](https://www.fuget.org/packages/EasyExtensions)
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/bvdcode/EasyExtensions/.github%2Fworkflows%2Fpublish-release.yml)](https://github.com/bvdcode/EasyExtensions/actions)
[![NuGet version (EasyExtensions)](https://img.shields.io/nuget/v/EasyExtensions.svg?label=stable)](https://www.nuget.org/packages/EasyExtensions/)
[![CodeFactor](https://www.codefactor.io/repository/github/bvdcode/EasyExtensions/badge)](https://www.codefactor.io/repository/github/bvdcode/EasyExtensions)
![GitHub repo size](https://img.shields.io/github/repo-size/bvdcode/EasyExtensions)


# EasyExtensions

Lightweight, extensible library built on .NET Standard.

Focused on clean, composable code and real-world .NET development scenarios.

> EasyExtensions is a modular, open-source extension library for .NET developers, designed to reduce boilerplate and improve clarity across backend, infrastructure, and application-level codebases.  
> Actively maintained and used in production environments.  
> Created and maintained by [Vadim Belov](mailto:github-easy-extensions@belov.us).

# Purposes

- **Easy to use** - just add a few lines of code to start working with the library.
- **Flexible** - use the library as a base for your project.
- **Fast** - add new features and commands in a few minutes.
- **Modern** - use the latest technologies and approaches.
- **Secure** - protect your data and users.
- **Open Source** - contribute to the project and make it better.
- **Free** - use the library for free.
- **Cross-platform** - run the library on any platform.
- **Lightweight** - use only necessary features.
- **Documented** - read the documentation and start using the library.

# Getting Started

- Start by importing the library into your project

```bash
# Basic package with BCL extensions
dotnet add package EasyExtensions

# AspNetCore package with additional features
dotnet add package EasyExtensions.AspNetCore
dotnet add package EasyExtensions.AspNetCore.Authorization
dotnet add package EasyExtensions.AspNetCore.Sentry

# Drawing package with native SixLabors.ImageSharp support
# This package is .NET Standard 2.1 and not using System.Drawing, so it can be used in any project.
dotnet add package EasyExtensions.Drawing

# Entity Framework Core package with absractions and ORM features
dotnet add package EasyExtensions.EntityFrameworkCore
dotnet add package EasyExtensions.EntityFrameworkCore.Npgsql

# Quartz package - scheduler integration and convenient reflection-based job registration
dotnet add package EasyExtensions.Quartz

# WebDav package - WebDav client with NextCloud support
dotnet add package EasyExtensions.WebDav

# Windows package - Windows-specific features with WinAPI
dotnet add package EasyExtensions.Windows

```


# Contents

- [Extensions](#extensions)
  * [Byte Array Extensions](#byte-array-extensions)
  * [Claims Principal Extensions](#claims-principal-extensions)
  * [DateTime Extensions](#datetime-extensions)
  * [Enumerable Extensions](#enumerable-extensions)
  * [Exception Extensions](#exception-extensions)
  * [HttpRequest Extensions](#httprequest-extensions)
  * [Math Extensions](#math-extensions)
  * [Object Extensions](#object-extensions)
  * [ServiceCollection Extensions](#servicecollection-extensions)
  * [Stream Extensions](#stream-extensions)
  * [String Extensions](#string-extensions)
  * [IP Address Extensions](#ip-address-extensions)

- [Helpers](#helpers)
  * [DateTime Helpers](#datetime-helpers)
  * [Reflection Helpers](#reflection-helpers)
  * [String Helpers](#string-helpers)
  * [IP Address Helpers](#ip-address-helpers)

- [Quartz Extensions](#quartz-extensions)
  * [ServiceCollection Extensions](#servicecollection-extensions)
  * [Attributes](#attributes)
	+ [JobTriggerAttribute](#jobtriggerattribute)

- [Authorization Extensions](#authorization-extensions)
  * [ServiceCollection Extensions](#servicecollection-extensions)

- [Drawing Extensions](#drawing-extensions)
  * [Image Extensions](#image-extensions)

- [Entity Framework Extensions](#entity-framework-extensions)
  * [ServiceCollection Extensions](#servicecollection-extensions)
  * [Host Extensions](#host-extensions)

- [Sentry Extensions](#sentry-extensions)
  * [WebHostBuilder Extensions](#webhostbuilder-extensions)


# Extensions

## Byte Array Extensions


```csharp
/// <summary>
/// Calculate SHA512 hash of byte array.
/// </summary>
/// <param name="bytes"> Data to calculate hash. </param>
/// <returns> SHA512 hash of byte array. </returns>
string SHA512(this byte[] bytes);
```

## Claims Principal Extensions


```csharp
/// <summary>
/// Get user id.
/// </summary>
/// <param name="user"> User instance. </param>
/// <returns> User id. </returns>
/// <exception cref="KeyNotFoundException"> Throws when claim not found. </exception>
int GetId(this ClaimsPrincipal? user);
```

---

```csharp
/// <summary>
/// Try get user id.
/// </summary>
/// <param name="user"> User instance. </param>
/// <returns> User id, or 0 if not found. </returns>
int TryGetId(this ClaimsPrincipal? user);
```

---

```csharp
/// <summary>
/// Get user roles.
/// </summary>
/// <param name="user"> User instance. </param>
/// <param name="rolePrefix"> Role prefix, for example: "user-group-" prefix returns group like "user-group-admins" </param>
/// <returns> User roles. </returns>
IEnumerable<string> GetRoles(this ClaimsPrincipal user, string rolePrefix = "");
```

## DateTime Extensions


```csharp
/// <summary>
/// Remove microseconds from <see cref="DateTime"/>.
/// </summary>
/// <returns> DateTime without microseconds. </returns>
DateTime DropMicroseconds(this DateTime value);
```

---

```csharp
/// <summary>
/// Remove microseconds from <see cref="DateTime"/>.
/// </summary>
/// <returns> DateTime without microseconds. </returns>
DateTimeOffset DropMicroseconds(this DateTimeOffset value);
```

---

```csharp
/// <summary>
/// Create new datetime with same values but <see cref="DateTimeKind.Utc"/>.
/// </summary>
/// <returns> New datetime. </returns>
DateTime ToUniversalTimeWithoutOffset(this DateTime value);
```

---

```csharp
/// <summary>
/// Convert datetime value to nullable datetime type.
/// </summary>
/// <returns> Wrapped datetime value. </returns>
DateTime? ToNullable(this DateTime value);
```


# Enumerable Extensions


```csharp
/// <summary>
/// Shuffles the elements of the <see cref="IEnumerable{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the <see cref="IEnumerable{T}"/>.</typeparam>
/// <param name="source">The <see cref="IEnumerable{T}"/> to shuffle.</param>
/// <returns>A shuffled <see cref="IEnumerable{T}"/>.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
IEnumerable<T> Shuffle<T>(this IEnumerable<T> source);
```

---

```csharp
/// <summary>
/// Gets a random element from the <see cref="IEnumerable{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the <see cref="IEnumerable{T}"/>.</typeparam>
/// <param name="enumerable">The <see cref="IEnumerable{T}"/> to get a random element from.</param>
/// <returns>A random element from the <see cref="IEnumerable{T}"/>.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="enumerable"/> is null.</exception>
T Random<T>(this IEnumerable<T> enumerable);
```


## Exception Extensions


```csharp
/// <summary>
/// Create string with error message from all inner exceptions if exists.
/// </summary>
/// <returns> Error message. </returns>
string ToStringWithInner(this Exception exception);
```


## HttpRequest Extensions


```csharp
/// <summary>
/// Get remote host IP address using proxy "X-Real-IP", "CF-Connecting-IP", "X-Forwarded-For" headers, or connection remote IP address.
/// </summary>
/// <returns> IP address, or "Unknown" by default. </returns>
string GetRemoteAddress(this HttpRequest request);
```


## Math Extensions


```csharp
/// <summary>
/// Pow specified foundation to exponent.
/// </summary>
/// <param name="number"> Foundation. </param>
/// <param name="exponent"> Exponent of pow. </param>
/// <returns> Calculation result. </returns>
/// <exception cref="OverflowException"> Throws when calculation result is too big. </exception>
int Pow(this int number, int exponent);
```


## Object Extensions


```csharp
/// <summary>
/// Clone object with MemberwiseClone.
/// </summary>
/// <typeparam name="TObj"> Type of object. </typeparam>
/// <param name="obj"> Object to clone. </param>
/// <returns> Cloned object. </returns>
TObj MemberwiseClone<TObj>(this TObj obj);
```


## ServiceCollection Extensions


```csharp
/// <summary>
/// Adds <see cref="CpuUsageService"/> to the <see cref="IServiceCollection"/>.
/// </summary>
/// <param name="services"> Current <see cref="IServiceCollection"/> instance. </param>
/// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
IServiceCollection AddCpuUsageService(this IServiceCollection services);
```

---

```csharp
/// <summary>
/// Add all types inherited from TInterface.
/// </summary>
/// <param name="services"> Current <see cref="IServiceCollection"/> instance. </param>
/// <param name="serviceLifetime"> Service lifetime, default is Scoped. </param>
/// <typeparam name="TInterface"> Interface type. </typeparam>
/// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
IServiceCollection AddTypesOfInterface<TInterface>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) where TInterface : class
```


## Stream Extensions


```csharp
/// <summary>
/// Reads the bytes from the current stream and writes them to the byte array.
/// </summary>
/// <returns> Received byte array. </returns>
/// <exception cref="IOException"> An I/O error occurred. </exception>
/// <exception cref="ArgumentNullException"> Destination is null. </exception>
/// <exception cref="ObjectDisposedException"> Either the current stream or the destination stream is disposed. </exception>
/// <exception cref="NotSupportedException"> The current stream does not support reading, or the destination stream does not support writing. </exception>
byte[] ReadToEnd(this Stream stream);
```

---

```csharp
/// <summary>
/// Asynchronously reads the bytes from the current stream and writes them to the byte array.
/// </summary>
/// <returns> Received byte array. </returns>
/// <exception cref="ArgumentNullException"> Destination is null. </exception>
/// <exception cref="ObjectDisposedException"> Either the current stream or the destination stream is disposed. </exception>
/// <exception cref="NotSupportedException"> The current stream does not support reading, or the destination stream does not support writing. </exception>
public static async Task<byte[]> ReadToEndAsync(this Stream stream)
```

---

```csharp
/// <summary>
/// Calculate SHA512 hash of byte stream.
/// </summary>
/// <param name="stream"> Data to calculate hash. </param>
/// <returns> SHA512 hash of byte stream. </returns>
string SHA512(this Stream stream);
```


## String Extensions


```csharp
/// <summary>
/// Make first letter as lower case. If text is null or whitespace - returns <see cref="string.Empty"/>
/// </summary>
/// <returns> Text with lower case first letter. </returns>
string ToLowerFirstLetter(this string text);
```

---

```csharp
/// <summary>
/// Make first letter as upper case. If text is null or whitespace - returns <see cref="string.Empty"/>
/// </summary>
/// <returns> Text with upper case first letter. </returns>
string ToUpperFirstLetter(this string text);
```

---

```csharp
/// <summary>
/// Create SHA512 hash of specified text string.
/// </summary>
/// <returns> SHA512 hash. </returns>
string SHA512(this string str);
```

---

```csharp
/// <summary>
/// Read only numbers from specified string.
/// </summary>
/// <returns> Parsed number, or -1 by default. </returns>
long ReadOnlyNumbers(this string str);
```


## IP Address Extensions


```csharp
/// <summary>
/// Get network address.
/// </summary>
/// <param name="address"> IP address. </param>
/// <param name="subnetMask"> Subnet mask. </param>
/// <returns> Network address. </returns>
IPAddress GetNetwork(this IPAddress address, IPAddress subnetMask);
```

---

```csharp
/// <summary>
/// Get broadcast address.
/// </summary>
/// <param name="address"> IP address. </param>
/// <param name="subnetMask"> Subnet mask. </param>
/// <returns> Broadcast address. </returns>
IPAddress GetBroadcast(this IPAddress address, IPAddress subnetMask);
```

---

```csharp
/// <summary>
/// Get network address.
/// </summary>
/// <param name="address"> IP address. </param>
/// <param name="subnetMask"> Subnet mask. </param>
/// <returns> Network address. </returns>
/// <exception cref="ArgumentOutOfRangeException"> Thrown when subnet mask is invalid. </exception>
IPAddress GetNetwork(this IPAddress address, int subnetMask);
```

---

```csharp
/// <summary>
/// Get broadcast address.
/// </summary>
/// <param name="address"> IP address. </param>
/// <param name="subnetMask"> Subnet mask. </param>
/// <returns> Broadcast address. </returns>
/// <exception cref="ArgumentOutOfRangeException"> Thrown when subnet mask is invalid. </exception>
IPAddress GetBroadcast(this IPAddress address, int subnetMask);
```


# Helpers


## DateTime Helpers


```csharp
/// <summary>
/// Parse DateTime from JSON format ISO 8601.
/// </summary>
/// <returns> Parsed datetime with UTC kind. </returns>
/// <exception cref="ArgumentException"></exception>
/// <exception cref="FormatException"></exception>
/// <exception cref="NotSupportedException"></exception>
DateTime ParseDateTime(string datetime);
```

---

```csharp
/// <summary>
/// Parse DateTimeOffset from JSON format ISO 8601.
/// </summary>
/// <returns> Parsed datetime offset. </returns>
/// <exception cref="ArgumentException"></exception>
/// <exception cref="FormatException"></exception>
DateTimeOffset ParseDateTimeOffset(string datetime);
```


## Reflection Helpers


```csharp
/// <summary>
/// Get all types inherited from interface.
/// </summary>
/// <typeparam name="TInterface"> Interface type. </typeparam>
/// <returns> All types inherited from interface. </returns>
IEnumerable<Type> GetTypesOfInterface<TInterface>() where TInterface : class
```

---

```csharp
/// <summary>
/// Copy matching properties from source to destination.
/// </summary>
/// <param name="source"> Source object. </param>
/// <param name="destination"> Destination object. </param>
void CopyMatchingProperties(object source, object destination);
```

## String Helpers


```csharp
/// <summary>
/// Fast generate pseudo random string with <see cref="DefaultCharset"/> and string length.
/// </summary>
/// <returns> Pseudo-random string. </returns>
string CreatePseudoRandomString();
```

---

```csharp
/// <summary>
/// Fast generate pseudo random string with <see cref="DefaultCharset"/> and specified string length.
/// </summary>
/// <param name="length"> Result string length. </param>
/// <returns> Pseudo-random string. </returns>
string CreatePseudoRandomString(int length);
```

---

```csharp
/// <summary>
/// Fast generate pseudo random string with specified charset and string length.
/// </summary>
/// <param name="charset"> Generator charset. </param>
/// <param name="length"> Result string length. </param>
/// <returns> Pseudo-random string. </returns>
string CreatePseudoRandomString(int length, string charset);
```

---

```csharp
/// <summary>
/// Generate random string with <see cref="DefaultCharset"/> and string length.
/// </summary>
/// <returns> Really random string. </returns>
string CreateRandomString();
```

---

```csharp
/// <summary>
/// Generate random string with <see cref="DefaultCharset"/> and specified string length.
/// </summary>
/// <param name="length"> Result string length. </param>
/// <returns> Really random string. </returns>
string CreateRandomString(int length);
```

---

```csharp
/// <summary>
/// Generate random string with specified charset and string length.
/// </summary>
/// <param name="charset"> Generator charset. </param>
/// <param name="length"> Result string length. </param>
/// <returns> Really random string. </returns>
string CreateRandomString(int length, string charset);
```

---

```csharp
/// <summary>
/// Remove spaces from string - trim, replace new lines and multiple spaces.
/// </summary>
/// <param name="comment"></param>
/// <returns></returns>
string RemoveSpaces(string? comment);
```


## IP Address Helpers


```csharp
/// <summary>
/// Convert IP address to number.
/// </summary>
/// <param name="ipAddress"> IP address. </param>
/// <returns> IP address as number. </returns>
BigInteger IpToNumber(string ipAddress);
```

---

```csharp
/// <summary>
/// Convert number to IP address.
/// </summary>
/// <param name="ipNumber"> IP address as number. </param>
/// <returns> IP address. </returns>
IPAddress NumberToIp(BigInteger ipNumber);
```

---

```csharp
/// <summary>
/// Get subnet mask address.
/// </summary>
/// <param name="subnetMask"> Subnet mask. </param>
/// <returns> Subnet address. </returns>
/// <exception cref="ArgumentOutOfRangeException"> Thrown when subnet mask is invalid. </exception>
IPAddress GetMaskAddress(int subnetMask);
```

---

```csharp
/// <summary>
/// Extract subnet mask from IP address.
/// </summary>
/// <param name="ip"> IP address. </param>
/// <returns> Subnet mask, or null if not found. </returns>
int? ExtractMask(string ip);
```


# Quartz Extensions

Usually, Quartz integration requires a lot of boilerplate code. This library simplifies the process of adding Quartz jobs to the project.

## ServiceCollection Extensions


```csharp
/// <summary>
/// Adds Quartz jobs with <see cref="JobTriggerAttribute"/> to the <see cref="IServiceCollection"/>.
/// </summary>
/// <param name="services">IServiceCollection instance.</param>
/// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
IServiceCollection AddQuartzJobs(this IServiceCollection services);
```

example of usage:

```csharp
builder.Services.AddQuartzJobs();
```

---

## Attributes

### JobTriggerAttribute

Example of usage:

```csharp
[JobTrigger(minutes: 1)]
[DisallowConcurrentExecution]
public class TestJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Test job executed.");
        return Task.CompletedTask;
    }
}
```


# Authorization Extensions

## ServiceCollection Extensions

```csharp
/// <summary>
/// Adds CORS policy with origins.
/// </summary>
/// <param name="services"> <see cref="IServiceCollection"/> instance. </param>
/// <param name="policyName"> Name of the policy. </param>
/// <param name="origins"> Origins to add to the policy. </param>
/// <returns></returns>
IServiceCollection AddCorsWithOrigins(this IServiceCollection services, string policyName, params string[] origins);
```

---

```csharp
/// <summary>
/// Adds JWT authentication from JwtSettings section or Jwt[Key] configuration values.
/// </summary>
/// <param name="services"> <see cref="IServiceCollection"/> instance. </param>
/// <param name="configuration"> Configuration from which to get JWT settings. </param>
/// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
/// <exception cref="KeyNotFoundException"> When JwtSettings section is not set. </exception>
IServiceCollection AddJwt(this IServiceCollection services, IConfiguration configuration);
```

---

```csharp
/// <summary>
/// Ignore <see cref="AuthorizeAttribute"/> when application is on development environment.
/// </summary>
/// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
IServiceCollection AllowAnonymousOnDevelopment(this IServiceCollection services);
```


# Drawing Extensions

## Image Extensions

```csharp
/// <summary>
/// Fit image to target size and copy and blur it to background.
/// </summary>
/// <param name="image">Target image.</param>
/// <param name="targetWidth">Target width.</param>
/// <param name="targetHeight">Target height.</param>
/// <param name="gaussianBlurLevel">Gaussian blur level (optional).</param>
/// <returns cref="Image">Image with blured background.</returns>
Image FitBluredBackground(this Image image, int targetWidth, int targetHeight, float gaussianBlurLevel = 8F);
```

---

```csharp
/// <summary>
/// Save image as JPEG to byte array.
/// </summary>
/// <param name="image">Target image.</param>
/// <returns>Byte array of image.</returns>
byte[] SaveAsJpegToArray(this Image image);
```


# Entity Framework Extensions


## ServiceCollection Extensions

```csharp
/// <summary>
/// Adds exception handler for EasyExtensions.EntityFrameworkCore.Exceptions to the <see cref="IServiceCollection"/>.
/// </summary>
/// <param name="services"> The <see cref="IServiceCollection"/> instance. </param>
/// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
IServiceCollection AddExceptionHandler(this IServiceCollection services);
```

---

```csharp
/// <summary>
/// Sets up Gridify.
/// </summary>
IServiceCollection AddGridifyMappers(this IServiceCollection services);
```

---

```csharp
/// <summary>
/// Adds a <see cref="DbContext"/> to the <see cref="IServiceCollection"/> using the 
/// <see cref="IConfigurationRoot"/> to build the connection string from DatabaseSettings section.
/// </summary>
/// <typeparam name="TContext"> The type of <see cref="DbContext"/> to add. </typeparam>
/// <param name="services"> The <see cref="IServiceCollection"/> instance. </param>
/// <param name="configuration"> The <see cref="IConfigurationRoot"/> instance. </param>
/// <param name="maxPoolSize"> The maximum pool size, default is 100. </param>
/// <param name="timeout_s"> The connection timeout in seconds, default is 60. </param>
/// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
/// <exception cref="KeyNotFoundException"> When DatabaseSettings section is not set. </exception>
IServiceCollection AddDbContext<TContext>(this IServiceCollection services, IConfigurationRoot configuration, int maxPoolSize = 100, int timeout_s = 60) where TContext : DbContext
```

## Host Extensions

```csharp
/// <summary>
/// Applies migrations to the database.
/// </summary>
/// <returns> Current <see cref="IHost"/> instance. </returns>
IHost ApplyMigrations<TContext>(this IHost host) where TContext : DbContext
```


# Sentry Extensions

## WebHostBuilder Extensions

```csharp
/// <summary>
/// Use Sentry integration with specified DSN.
/// </summary>
/// <param name="builder"> Current <see cref="IWebHostBuilder"/> instance. </param>
/// <param name="dsn"> Sentry DSN. </param>
/// <param name="forceUseInDevelopment"> Force use in development environment. </param>
/// <returns> Current <see cref="IWebHostBuilder"/> instance. </returns>
IWebHostBuilder UseSentryWithUserCapturing(this IWebHostBuilder builder, string dsn, bool forceUseInDevelopment = false);
```

# Contributing

This library is maintained as part of my effort to support the .NET developer community with practical, production-ready tools.  
It is actively used in enterprise and internal systems, and continuously improved based on real-world needs.

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

# License

Distributed under the MIT License. See LICENSE.md for more information.

# Contact

[E-Mail](mailto:github-easy-extensions@belov.us)

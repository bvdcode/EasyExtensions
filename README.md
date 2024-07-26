[![GitHub](https://img.shields.io/github/license/bvdcode/EasyExtensions)](https://github.com/bvdcode/EasyExtensions/blob/main/LICENSE.md)
[![Nuget](https://img.shields.io/nuget/dt/EasyExtensions?color=%239100ff)](https://www.nuget.org/packages/EasyExtensions/)
[![Static Badge](https://img.shields.io/badge/fuget-f88445?logo=readme&logoColor=white)](https://www.fuget.org/packages/EasyExtensions)
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/bvdcode/EasyExtensions/.github%2Fworkflows%2Fpublish-release.yml)](https://github.com/bvdcode/EasyExtensions/actions)
[![NuGet version (EasyExtensions)](https://img.shields.io/nuget/vpre/EasyExtensions.svg?style=flat-square&label=latest&color=yellowgreen)](https://www.nuget.org/packages/EasyExtensions/)
[![NuGet version (EasyExtensions)](https://img.shields.io/nuget/v/EasyExtensions.svg?style=flat-square&label=stable)](https://www.nuget.org/packages/EasyExtensions/)


# EasyExtensions

Ready-to-use **.NET Standard** library for convenient development.

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

Start by importing the library into your project

```bash
dotnet add package EasyExtensions --version 0.1.7
```

Add Quartz package if you want to use Quartz extensions

```bash
dotnet add package EasyExtensions.Quartz --version 0.1.7
```

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


# Quartz Extensions

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

# Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right"><a href="#readme-top">back to top</a></p>

# License

Distributed under the MIT License. See LICENSE.md for more information.

# Contact

[E-Mail](mailto:github-easy-extensions@belov.us)

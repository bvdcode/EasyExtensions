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

`dotnet add package EasyExtensions --version 0.1.3`

# Extensions

## Byte Array Extensions


`string SHA512(this byte[] bytes)` - Calculate SHA512 hash of byte array.

`bytes` - Data to calculate hash.

`returns` - SHA512 hash of byte array.

## Claims Principal Extensions


`int GetId(this ClaimsPrincipal? user)` - Get user id.

`user` - User instance.

`returns` - User id.

---

`int TryGetId(this ClaimsPrincipal? user)` - Try get user id.

`user` - User instance.

`returns` - User id, or 0 if not found.

---

`IEnumerable<string> GetRoles(this ClaimsPrincipal user, string rolePrefix = "")` - Get user roles.

`user` - User instance.

`rolePrefix` - Role prefix, for example: "user-group-" prefix returns group like "user-group-admins" </param>

`returns` - User roles.

## DateTime Extensions


`DateTime DropMicroseconds(this DateTime value)` - Remove microseconds from DateTime.

`value` - DateTime value.

`returns` - DateTime without microseconds.

---

`DateTimeOffset DropMicroseconds(this DateTimeOffset value)` - Remove microseconds from DateTimeOffset.

`value` - DateTimeOffset value.

`returns` - DateTimeOffset without microseconds.

---

`DateTime ToUniversalTimeWithoutOffset(this DateTime value)` - Create new datetime with same values but DateTimeKind.Utc.

`value` - DateTime value.

`returns` - New datetime.

---

`DateTime? ToNullable(this DateTime value)` - Convert datetime value to nullable datetime type.

`value` - DateTime value.

`returns` - Wrapped datetime value.


## Exception Extensions


`string ToStringWithInner(this Exception ex)` - Create string with error message from all inner exceptions if exists.

`exception` - Exception instance.

`returns` - Error message.


## HttpRequest Extensions


`string GetRemoteAddress(this HttpRequest request)` - Get remote host IP address using proxy "X-Real-IP", "CF-Connecting-IP", "X-Forwarded-For" headers, or connection remote IP address.

`request` - HttpRequest instance.

`returns` - IP address, or "Unknown" by default.


## Math Extensions


`int Pow(this int number, int exponent)` - Pow specified foundation to exponent.

`number` - Foundation.

`exponent` - Exponent of pow.

`returns` - Calculation result.


## Object Extensions


`TObj MemberwiseClone<TObj>(this TObj obj)` - Clone object with MemberwiseClone.

`obj` - Object to clone.

`returns` - Cloned object.


## ServiceCollection Extensions


`IServiceCollection AddCpuUsageService(this IServiceCollection services)` - Adds CpuUsageService to the IServiceCollection.

`services` - IServiceCollection instance.

`returns` - Current IServiceCollection instance.

---

`IServiceCollection AddRepositories(this IServiceCollection services)` - Add all types inherited from IRepository.

`services` - IServiceCollection instance.

`returns` - Current IServiceCollection instance.


## Stream Extensions


`byte[] ReadToEnd(this Stream stream)` - Reads the bytes from the current stream and writes them to the byte array.

`stream` - Stream instance.

`returns` - Received byte array.

---

`Task<byte[]> ReadToEndAsync(this Stream stream)` - Asynchronously reads the bytes from the current stream and writes them to the byte array.

`stream` - Stream instance.

`returns` - Received byte array.

---

`string SHA512(this Stream stream)` - Calculate SHA512 hash of byte stream.

`stream` - Data to calculate hash.

`returns` - SHA512 hash of byte stream.


## String Extensions


`string SHA512(this string str)` - Create SHA512 hash of specified text string.

`str` - Text string.

`returns` - SHA512 hash.

---

`long ReadOnlyNumbers(this string str)` - Read only numbers from specified string.

`str` - Text string.

`returns` - Parsed number, or -1 by default.

---

`string ToLowerFirstLetter(this string text)` - Make first letter as lower case. If text is null or whitespace - returns string.Empty.

`text` - Text string.

`returns` - Text with lower case first letter.

---

`string ToUpperFirstLetter(this string text)` - Make first letter as upper case. If text is null or whitespace - returns string.Empty.

`text` - Text string.

`returns` - Text with upper case first letter.


# Helpers

## DateTime Helpers


`DateTime ParseDateTimeOffset(string date)` - Parse DateTimeOffset from JSON format ISO 8601.

`date` - Date string.

`returns` - Parsed DateTimeOffset.

---

`DateTime ParseDateTime(string time)` - Parse DateTime from JSON format ISO 8601.

`datetime` - Date string.

`returns` - Parsed DateTime.


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

// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Clients;
using System.Net;
using System.Net.Http;
using System.Text;

namespace EasyExtensions.Tests
{
    [NonParallelizable]
    public class ClientsTests
    {
        private const int DelayBetweenTests = 1000; // 1 second delay

        [Test]
        [Order(1)]
        public async Task IpApiCoClient_LookupAsync_ValidIpAddress_ValidOutput()
        {
            // Arrange
            const string ip = "8.8.8.8";

            // Act
            var result = await IpApiCoClient.LookupAsync(ip);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Ip, Is.EqualTo(ip));
                Assert.That(result.Country, Is.Not.Empty);
                Assert.That(result.Latitude, Is.Not.Null);
                Assert.That(result.Longitude, Is.Not.Null);
            }
        }

        [Test]
        [Order(2)]
        public async Task IpApiCoClient_LookupAsync_InvalidIpAddress_ThrowsArgumentException()
        {
            // Arrange
            await Task.Delay(DelayBetweenTests);
            const string invalidIp = "999.999.999.999";

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await IpApiCoClient.LookupAsync(invalidIp));
        }

        [Test]
        [Order(3)]
        public async Task IpApiCoClient_LookupAsync_NullOrWhitespace_ThrowsArgumentException()
        {
            // Arrange
            await Task.Delay(DelayBetweenTests);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await IpApiCoClient.LookupAsync(null!));
            Assert.ThrowsAsync<ArgumentException>(async () => await IpApiCoClient.LookupAsync(""));
            Assert.ThrowsAsync<ArgumentException>(async () => await IpApiCoClient.LookupAsync("   "));
        }

        [Test]
        [Order(4)]
        public async Task IpApiCoClient_LookupCurrentHostAsync_ReturnsValidResponse()
        {
            // Arrange
            await Task.Delay(DelayBetweenTests);

            // Act
            var result = await IpApiCoClient.LookupCurrentHostAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Ip, Is.Not.Empty);
                Assert.That(result.Country, Is.Not.Empty);
            }
        }

        [Test]
        [Order(5)]
        public async Task GeoIpClient_LookupAsync_ValidIpAddress_ValidOutput()
        {
            // Arrange
            await Task.Delay(DelayBetweenTests);
            const string ip = "8.8.8.8";

            // Act
            var result = await GeoIpClient.Shared.LookupAsync(ip);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Ip, Is.EqualTo(ip));
                Assert.That(result.Country, Is.Not.Null.And.Not.Empty);
                Assert.That(result.Latitude, Is.Not.Null);
                Assert.That(result.Longitude, Is.Not.Null);
            }
        }

        [Test]
        [Order(6)]
        public async Task GeoIpClient_LookupAsync_InvalidIpAddress_ThrowsArgumentException()
        {
            // Arrange
            await Task.Delay(DelayBetweenTests);
            const string invalidIp = "999.999.999.999";

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await GeoIpClient.Shared.LookupAsync(invalidIp));
        }

        [Test]
        [Order(7)]
        public async Task GeoIpClient_TryLookupAsync_InvalidIpAddress_ReturnsNull()
        {
            // Arrange
            await Task.Delay(DelayBetweenTests);
            const string invalidIp = "999.999.999.999";

            // Act
            var result = await GeoIpClient.Shared.TryLookupAsync(invalidIp);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [Order(8)]
        public async Task GeoIpClient_TryLookupAsync_ValidIpAddress_ReturnsValidOutput()
        {
            // Arrange
            await Task.Delay(DelayBetweenTests);
            const string ip = "1.1.1.1";

            // Act
            var result = await GeoIpClient.Shared.TryLookupAsync(ip);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Ip, Is.EqualTo(ip));
                Assert.That(result.Country ?? result.Continent ?? result.AsnOrganization, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        [Order(9)]
        public async Task GeoIpClient_TryLookupAsync_CurrentHost_DoesNotThrow()
        {
            // Arrange
            await Task.Delay(DelayBetweenTests);

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await GeoIpClient.Shared.TryLookupAsync());
        }

        [Test]
        [Order(10)]
        public async Task GeoIpClient_LookupAsync_CustomEndpoint_AppendsIpAfterEndpointPath()
        {
            // Arrange
            var handler = new CapturingGeoIpHandler();
            using var httpClient = new HttpClient(handler);
            var client = new GeoIpClient("https://bridge.cottoncloud.dev/api/v1/lookup", httpClient);

            // Act
            var result = await client.LookupAsync("8.8.8.8");

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Ip, Is.EqualTo("8.8.8.8"));
                Assert.That(handler.RequestUris.Single().ToString(), Is.EqualTo("https://bridge.cottoncloud.dev/api/v1/lookup/8.8.8.8"));
            }
        }

        [Test]
        [Order(11)]
        public async Task GeoIpClient_LookupAsync_CustomEndpointWithoutIp_UsesEndpointAddressAsIs()
        {
            // Arrange
            var handler = new CapturingGeoIpHandler();
            using var httpClient = new HttpClient(handler);
            var client = new GeoIpClient("https://bridge.cottoncloud.dev/api/v1/lookup", httpClient);

            // Act
            var result = await client.LookupAsync();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Ip, Is.EqualTo("8.8.8.8"));
                Assert.That(handler.RequestUris.Single().ToString(), Is.EqualTo("https://bridge.cottoncloud.dev/api/v1/lookup"));
            }
        }

        private sealed class CapturingGeoIpHandler : HttpMessageHandler
        {
            public List<Uri> RequestUris { get; } = [];

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestUris.Add(request.RequestUri!);

                const string json = """
                    {
                        "ip": "8.8.8.8",
                        "country": "United States",
                        "latitude": 37.751,
                        "longitude": -97.822
                    }
                    """;

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }
        }
    }
}

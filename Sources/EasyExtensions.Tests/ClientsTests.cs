// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Clients;

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
            var result = await GeoIpClient.LookupAsync(ip);

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
            Assert.ThrowsAsync<ArgumentException>(async () => await GeoIpClient.LookupAsync(invalidIp));
        }

        [Test]
        [Order(7)]
        public async Task GeoIpClient_TryLookupAsync_InvalidIpAddress_ReturnsNull()
        {
            // Arrange
            await Task.Delay(DelayBetweenTests);
            const string invalidIp = "999.999.999.999";

            // Act
            var result = await GeoIpClient.TryLookupAsync(invalidIp);

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
            var result = await GeoIpClient.TryLookupAsync(ip);

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
            Assert.DoesNotThrowAsync(async () => await GeoIpClient.TryLookupAsync());
        }
    }
}

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
    }
}

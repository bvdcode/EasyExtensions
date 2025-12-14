using EasyExtensions.Helpers;
using EasyExtensions.Models.Enums;
using System.Net;
using System.Net.Sockets;

namespace EasyExtensions.Tests
{
    public class NetworkHelpersTests
    {
        [Test]
        public void PingAsync_NullAddress_ThrowsArgumentNullException()
        {
            // Arrange
            IPAddress? ipAddress = null;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await NetworkHelpers.PingAsync(ipAddress!));
        }

        [Test]
        public async Task PingAsync_LoopbackAddress_ReturnsSuccessStatus()
        {
            // Arrange
            var ipAddress = IPAddress.Loopback;

            // Act
            var result = await NetworkHelpers.PingAsync(ipAddress);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.Status, Is.EqualTo(IcmpStatus.Success));
                Assert.That(result.RoundtripTime, Is.GreaterThanOrEqualTo(0));
            }
        }

        [Test]
        public async Task TryPingAsync_LoopbackAddress_ReturnsTrue()
        {
            // Arrange
            var ipAddress = IPAddress.Loopback;

            // Act
            var result = await NetworkHelpers.TryPingAsync(ipAddress);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public async Task PingAsync_IPv6LoopbackAddress_ReturnsSuccessStatus()
        {
            // Arrange
            var ipAddress = IPAddress.IPv6Loopback;

            // Act
            var result = await NetworkHelpers.PingAsync(ipAddress);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.Status, Is.EqualTo(IcmpStatus.Success));
                Assert.That(result.RoundtripTime, Is.GreaterThanOrEqualTo(0));
            }
        }

        [Test]
        public async Task TryPingAsync_IPv6LoopbackAddress_ReturnsTrue()
        {
            // Arrange
            var ipAddress = IPAddress.IPv6Loopback;

            // Act
            var result = await NetworkHelpers.TryPingAsync(ipAddress);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public async Task PingAsync_UnreachableAddress_ReturnsFailedOrTimeoutStatus()
        {
            // Arrange
            // This IP is reserved for documentation and should never be reachable
            var ipAddress = IPAddress.Parse("192.0.2.1");

            // Act
            var result = await NetworkHelpers.PingAsync(ipAddress, timeout: 1000);

            // Assert
            Assert.That(result.Status, Is.AnyOf(IcmpStatus.Failed, IcmpStatus.Timeout));
        }

        [Test]
        public async Task TryPingAsync_UnreachableAddress_ReturnsFalse()
        {
            // Arrange
            // This IP is reserved for documentation and should never be reachable
            var ipAddress = IPAddress.Parse("192.0.2.1");

            // Act
            var result = await NetworkHelpers.TryPingAsync(ipAddress, timeout: 1000);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public async Task PingAsync_WithCustomTimeout_RespectsTimeout()
        {
            // Arrange
            // This IP is reserved for documentation and should never be reachable
            var ipAddress = IPAddress.Parse("192.0.2.1");
            var shortTimeout = 200;
            var startTime = DateTime.Now;

            // Act
            await NetworkHelpers.PingAsync(ipAddress, timeout: shortTimeout);
            var elapsedTime = (DateTime.Now - startTime).TotalMilliseconds;

            // Assert
            // Allow some leeway for processing time (2x the timeout)
            Assert.That(elapsedTime, Is.LessThan(shortTimeout * 2));
        }

        [Test]
        public void PingAsync_WithCancellation_RespectsToken()
        {
            // Arrange
            // This IP is reserved for documentation and should never be reachable
            var ipAddress = IPAddress.Parse("192.0.2.1");
            var cts = new CancellationTokenSource();
            var longTimeout = 5000;

            // Act - cancel immediately
            cts.Cancel();

            // Act & Assert
            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await NetworkHelpers.PingAsync(ipAddress, timeout: longTimeout, cancellationToken: cts.Token);
            });
        }

        [Test]
        public void CalculateIcmpChecksum_ValidPacket_ReturnsCorrectChecksum()
        {
            // Arrange
            var packet = new byte[] { 8, 0, 0, 0, 0, 0, 0, 0 };

            // Act
            ushort checksum = NetworkHelpers.CalculateIcmpChecksum(packet);

            // Assert
            // Expected checksum for this simple packet is 0xF7FF
            Assert.That(checksum, Is.EqualTo(0xF7FF));
        }

        [Test]
        public void CreateIcmpEchoPacket_IPv4_ReturnsValidPacket()
        {
            // Arrange
            var addressFamily = AddressFamily.InterNetwork;

            // Act
            byte[] packet = NetworkHelpers.CreateIcmpEchoPacket(addressFamily);

            // Assert
            Assert.That(packet, Is.Not.Null);
            Assert.That(packet, Has.Length.EqualTo(16));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(packet[0], Is.EqualTo(8)); // Echo Request type
                Assert.That(packet[1], Is.Zero); // Code 0
                                                 // Don't check checksum as it varies
                Assert.That(packet[6], Is.Zero); // Sequence MSB
                Assert.That(packet[7], Is.EqualTo(1)); // Sequence LSB
            }
        }

        [Test]
        public void CreateIcmpEchoPacket_IPv6_ReturnsValidPacket()
        {
            // Arrange
            var addressFamily = AddressFamily.InterNetworkV6;

            // Act
            byte[] packet = NetworkHelpers.CreateIcmpEchoPacket(addressFamily);

            // Assert
            Assert.That(packet, Is.Not.Null);
            Assert.That(packet, Has.Length.EqualTo(16));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(packet[0], Is.EqualTo(128)); // Echo Request type for IPv6
                Assert.That(packet[1], Is.Zero); // Code 0
                Assert.That(packet[6], Is.Zero); // Sequence MSB
                Assert.That(packet[7], Is.EqualTo(1)); // Sequence LSB
            }
        }

        [Test]
        public void IsIcmpEchoReply_ValidIPv4Reply_ReturnsTrue()
        {
            // Arrange
            var buffer = new byte[30]; // 20 bytes IP header + 8 bytes ICMP header + 2 bytes data
            buffer[20] = 0; // Type 0 (Echo Reply)
            var bytesReceived = buffer.Length;

            // Act
            bool result = NetworkHelpers.IsIcmpEchoReply(buffer, bytesReceived, AddressFamily.InterNetwork);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsIcmpEchoReply_InvalidIPv4Reply_ReturnsFalse()
        {
            // Arrange
            var buffer = new byte[30]; // 20 bytes IP header + 8 bytes ICMP header + 2 bytes data
            buffer[20] = 8; // Type 8 (Echo Request, not Reply)
            var bytesReceived = buffer.Length;

            // Act
            bool result = NetworkHelpers.IsIcmpEchoReply(buffer, bytesReceived, AddressFamily.InterNetwork);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsIcmpEchoReply_ValidIPv6Reply_ReturnsTrue()
        {
            // Arrange
            var buffer = new byte[10]; // 8 bytes ICMP header + 2 bytes data
            buffer[0] = 129; // Type 129 (Echo Reply for IPv6)
            var bytesReceived = buffer.Length;

            // Act
            bool result = NetworkHelpers.IsIcmpEchoReply(buffer, bytesReceived, AddressFamily.InterNetworkV6);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsIcmpEchoReply_InvalidIPv6Reply_ReturnsFalse()
        {
            // Arrange
            var buffer = new byte[10]; // 8 bytes ICMP header + 2 bytes data
            buffer[0] = 128; // Type 128 (Echo Request for IPv6, not Reply)
            var bytesReceived = buffer.Length;

            // Act
            bool result = NetworkHelpers.IsIcmpEchoReply(buffer, bytesReceived, AddressFamily.InterNetworkV6);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsIcmpEchoReply_BufferTooSmall_ReturnsFalse()
        {
            // Arrange
            var buffer = new byte[5]; // Too small for any valid ICMP packet
            var bytesReceived = buffer.Length;

            // Act
            bool result = NetworkHelpers.IsIcmpEchoReply(buffer, bytesReceived, AddressFamily.InterNetwork);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task PingAsync_GoogleDnsServer_ReturnsSuccessStatusIfNetworkAvailable()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("8.8.8.8");

            try
            {
                // Act
                var result = await NetworkHelpers.PingAsync(ipAddress, timeout: 3000);

                // Assert
                // This test may be flaky depending on network conditions
                // We use Ignore.FailureException as this is more of an integration test
                // that depends on external resources
                Assert.That(result.Status, Is.EqualTo(IcmpStatus.Success),
                    "Test is running in an environment without internet access");
            }
            catch (Exception)
            {
                // Ignore test if network access is unavailable
                Assert.Ignore("Test skipped due to lack of network connectivity");
            }
        }

        [Test]
        [Explicit("This test is for manual verification only as it uses real DNS lookup")]
        public async Task PingAsync_RealDomainName_ReturnsSuccessStatusIfNetworkAvailable()
        {
            // Arrange - we need to resolve the hostname first
            IPAddress? ipAddress = null;
            try
            {
                var hostEntry = await Dns.GetHostEntryAsync("google.com");
                ipAddress = hostEntry.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                if (ipAddress == null)
                {
                    Assert.Ignore("Could not resolve hostname to IPv4 address");
                    return;
                }

                // Act
                var result = await NetworkHelpers.PingAsync(ipAddress, timeout: 3000);

                // Assert
                Assert.That(result.Status, Is.EqualTo(IcmpStatus.Success),
                    "Test is running in an environment without internet access");
            }
            catch (Exception)
            {
                // Ignore test if network access is unavailable
                Assert.Ignore("Test skipped due to lack of network connectivity");
            }
        }
    }
}

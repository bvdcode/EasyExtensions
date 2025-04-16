using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using EasyExtensions.Models;
using System.Threading.Tasks;
using EasyExtensions.Models.Enums;

namespace EasyExtensions.Helpers
{
    /// <summary>
    /// Network related helper methods.
    /// </summary>
    public static class NetworkHelpers
    {
        /// <summary>
        /// Sends an ICMP Echo Request (ping) to the specified IP address.
        /// </summary>
        /// <param name="address">The IP address to ping.</param>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result of the ping operation.</returns>
        public static async Task<IcmpResult> PingAsync(IPAddress address, int timeout = 5000, CancellationToken cancellationToken = default)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            cancellationToken.ThrowIfCancellationRequested();
            DateTime startTime = DateTime.UtcNow;
            try
            {
                using var socket = new Socket(address.AddressFamily, SocketType.Raw,
                    address.AddressFamily == AddressFamily.InterNetwork ? ProtocolType.Icmp : ProtocolType.IcmpV6);
                socket.ReceiveTimeout = timeout;
                socket.SendTimeout = timeout;

                await socket.ConnectAsync(new IPEndPoint(address, 0));
                byte[] packet = CreateIcmpEchoPacket(address.AddressFamily);
                await socket.SendAsync(new ArraySegment<byte>(packet), SocketFlags.None, cancellationToken);

                var receiveBuffer = new byte[1024];
                using var timeoutCts = new CancellationTokenSource(timeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var received = await socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), SocketFlags.None, linkedCts.Token);
                    var elapsedMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                    if (IsIcmpEchoReply(receiveBuffer, received, address.AddressFamily))
                    {
                        return new IcmpResult(IcmpStatus.Success, elapsedMs);
                    }
                    return new IcmpResult(IcmpStatus.Failed, elapsedMs);
                }
                catch (OperationCanceledException)
                {
                    return new IcmpResult(IcmpStatus.Timeout, timeout);
                }
            }
            catch (SocketException)
            {
                long elapsedMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                return new IcmpResult(IcmpStatus.Failed, elapsedMs);
            }
            catch (Exception)
            {
                var elapsedMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                return new IcmpResult(IcmpStatus.Failed, elapsedMs);
            }
        }

        /// <summary>
        /// Simplified ping method that returns whether the ping was successful.
        /// </summary>
        /// <param name="address">The IP address to ping.</param>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if ping was successful, false otherwise.</returns>
        public static async Task<bool> TryPingAsync(IPAddress address, int timeout = 5000, CancellationToken cancellationToken = default)
        {
            var result = await PingAsync(address, timeout, cancellationToken);
            return result.Status == IcmpStatus.Success;
        }

        /// <summary>
        /// Creates an ICMP Echo Request packet.
        /// </summary>
        /// <param name="addressFamily">Address family (IPv4 or IPv6).</param>
        /// <returns>ICMP packet bytes.</returns>
        public static byte[] CreateIcmpEchoPacket(AddressFamily addressFamily)
        {
            // Create a simple ICMP Echo Request packet with minimal data
            var isIpv4 = addressFamily == AddressFamily.InterNetwork;

            // 8-byte ICMP header + 8-byte data
            var packet = new byte[16];

            // Type: 8 for IPv4 Echo Request, 128 for IPv6 Echo Request
            packet[0] = (byte)(isIpv4 ? 8 : 128);

            // Code: 0 for Echo Request
            packet[1] = 0;

            // Checksum (bytes 2-3): Will be filled later for IPv4
            packet[2] = 0;
            packet[3] = 0;

            // Identifier (bytes 4-5): Use process ID
            ushort processId = (ushort)Environment.CurrentManagedThreadId;
            packet[4] = (byte)(processId >> 8);
            packet[5] = (byte)(processId & 0xFF);

            // Sequence Number (bytes 6-7): Use 1
            packet[6] = 0;
            packet[7] = 1;

            // Data (bytes 8-15): Fill with some pattern
            for (int i = 8; i < 16; i++)
            {
                packet[i] = (byte)(i - 8);
            }

            // Calculate and set checksum for IPv4
            // IPv6 checksums are computed by the IPv6 stack
            if (isIpv4)
            {
                var checksum = CalculateIcmpChecksum(packet);
                packet[2] = (byte)(checksum >> 8);
                packet[3] = (byte)(checksum & 0xFF);
            }

            return packet;
        }

        /// <summary>
        /// Checks if a received packet is an ICMP Echo Reply.
        /// </summary>
        /// <param name="buffer">Received data buffer.</param>
        /// <param name="bytesReceived">Number of bytes received.</param>
        /// <param name="addressFamily">Address family (IPv4 or IPv6).</param>
        /// <returns>True if the packet is a valid Echo Reply.</returns>
        public static bool IsIcmpEchoReply(byte[] buffer, int bytesReceived, AddressFamily addressFamily)
        {
            if (bytesReceived < 8)
            {
                return false;
            }

            // For IPv4, ICMP header starts at offset 20 (after IP header)
            // For IPv6, ICMP header is directly accessible
            int icmpOffset = addressFamily == AddressFamily.InterNetwork ? 20 : 0;

            if (bytesReceived < icmpOffset + 8)
            {
                return false;
            }

            byte type = buffer[icmpOffset];

            // Type 0 for IPv4 Echo Reply, 129 for IPv6 Echo Reply
            return (addressFamily == AddressFamily.InterNetwork && type == 0) ||
                   (addressFamily == AddressFamily.InterNetworkV6 && type == 129);
        }

        /// <summary>
        /// Calculates the ICMP checksum.
        /// </summary>
        /// <param name="buffer">ICMP packet buffer.</param>
        /// <returns>Checksum value.</returns>
        public static ushort CalculateIcmpChecksum(byte[] buffer)
        {
            int length = buffer.Length;
            int i = 0;
            uint sum = 0;

            // Sum all 16-bit words
            while (length > 1)
            {
                sum += (ushort)((buffer[i] << 8) | buffer[i + 1]);
                i += 2;
                length -= 2;
            }

            // If there's an odd byte left, add it
            if (length == 1)
            {
                sum += buffer[i];
            }

            // Fold 32-bit sum into 16 bits
            while (sum >> 16 != 0)
            {
                sum = (sum & 0xFFFF) + (sum >> 16);
            }

            // Take one's complement
            return (ushort)~sum;
        }
    }
}

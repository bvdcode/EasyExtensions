using EasyExtensions.Helpers;
using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace EasyExtensions
{
    /// <summary>
    /// <see cref="IPAddress"/> extensions.
    /// </summary>
    public static class IpAddressExtensions
    {
        /// <summary>
        /// Get network address.
        /// </summary>
        /// <param name="address"> IP address. </param>
        /// <param name="subnetMask"> Subnet mask. </param>
        /// <returns> Network address. </returns>
        public static IPAddress GetNetwork(this IPAddress address, IPAddress subnetMask)
        {
            if (address.AddressFamily != subnetMask.AddressFamily)
            {
                throw new ArgumentException("IPv6 mask cannot be used with IPv4 address.");
            }
            byte[] ipBytes = address.GetAddressBytes();
            byte[] maskBytes = subnetMask.GetAddressBytes();
            if (ipBytes.Length != maskBytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(subnetMask), "Invalid subnet mask length.");
            }
            byte[] networkBytes = new byte[ipBytes.Length];
            for (int i = 0; i < ipBytes.Length; i++)
            {
                networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
            }
            return new IPAddress(networkBytes);
        }

        /// <summary>
        /// Get broadcast address.
        /// </summary>
        /// <param name="address"> IP address. </param>
        /// <param name="subnetMask"> Subnet mask. </param>
        /// <returns> Broadcast address. </returns>
        public static IPAddress GetBroadcast(this IPAddress address, IPAddress subnetMask)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork && subnetMask.AddressFamily == AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException("IPv6 mask cannot be used with IPv4 address.");
            }
            byte[] ipBytes = address.GetAddressBytes();
            byte[] maskBytes = subnetMask.GetAddressBytes();
            byte[] broadcastBytes = new byte[ipBytes.Length];
            for (int i = 0; i < ipBytes.Length; i++)
            {
                broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
            }
            return new IPAddress(broadcastBytes);
        }

        /// <summary>
        /// Get network address.
        /// </summary>
        /// <param name="address"> IP address. </param>
        /// <param name="subnetMask"> Subnet mask. </param>
        /// <returns> Network address. </returns>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when subnet mask is invalid. </exception>
        public static IPAddress GetNetwork(this IPAddress address, int subnetMask)
        {
            return GetNetwork(address, IpAddressHelpers.GetMaskAddress(subnetMask, address.AddressFamily));
        }

        /// <summary>
        /// Get broadcast address.
        /// </summary>
        /// <param name="address"> IP address. </param>
        /// <param name="subnetMask"> Subnet mask. </param>
        /// <returns> Broadcast address. </returns>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when subnet mask is invalid. </exception>
        public static IPAddress GetBroadcast(this IPAddress address, int subnetMask)
        {
            return GetBroadcast(address, IpAddressHelpers.GetMaskAddress(subnetMask, address.AddressFamily));
        }

        /// <summary>
        /// Convert IP address to number.
        /// </summary>
        /// <param name="ipAddress"> IP address. </param>
        /// <returns> IP address as number. </returns>
        /// <exception cref="ArgumentException"> Invalid IP address family. </exception>
        public static BigInteger ToNumber(this IPAddress ipAddress)
        {
            byte[] ipBytes = ipAddress.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(ipBytes);
            }
            if (ipBytes.Length != 4 && ipBytes.Length != 16)
            {
                throw new ArgumentOutOfRangeException(nameof(ipAddress), "Invalid IP address length: " + ipBytes.Length);
            }
            return ipAddress.AddressFamily switch
            {
                AddressFamily.InterNetwork => BitConverter.ToUInt32(ipBytes, 0),
                AddressFamily.InterNetworkV6 => new BigInteger(ipBytes),
                _ => throw new ArgumentException("Invalid IP address family: " + ipAddress.AddressFamily),
            };
        }
    }
}

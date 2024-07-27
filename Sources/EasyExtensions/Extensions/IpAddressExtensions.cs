using System;
using System.Net;

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
            byte[] ipBytes = address.GetAddressBytes();
            byte[] maskBytes = subnetMask.GetAddressBytes();
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
            return GetNetwork(address, GetMaskAddress(subnetMask));
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
            return GetBroadcast(address, GetMaskAddress(subnetMask));
        }

        /// <summary>
        /// Convert IP address to number.
        /// </summary>
        /// <param name="ipAddress"> IP address. </param>
        /// <returns> IP address as number. </returns>
        /// <exception cref="ArgumentException"> Invalid IP address family. </exception>
        public static ulong ToNumber(this IPAddress ipAddress)
        {
            // convert the IP address to bytes - ipv4 or ipv6
            byte[] ipBytes = ipAddress.GetAddressBytes();
            // ensure correct endianness
            if (BitConverter.IsLittleEndian)
            {
                // reverse the bytes
                Array.Reverse(ipBytes);
            }
            return ipAddress.AddressFamily switch
            {
                System.Net.Sockets.AddressFamily.InterNetwork => BitConverter.ToUInt32(ipBytes, 0),
                System.Net.Sockets.AddressFamily.InterNetworkV6 => BitConverter.ToUInt64(ipBytes, 0),
                _ => throw new ArgumentException("Invalid IP address family: " + ipAddress.AddressFamily),
            };
        }

        /// <summary>
        /// Get subnet mask address.
        /// </summary>
        /// <param name="subnetMask"> Subnet mask. </param>
        /// <returns> Subnet address. </returns>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when subnet mask is invalid. </exception>

        private static IPAddress GetMaskAddress(int subnetMask)
        {
            if (subnetMask < 0 || subnetMask > 128)
            {
                throw new ArgumentOutOfRangeException(nameof(subnetMask), "Invalid subnet mask.");
            }
            bool is64 = subnetMask > 32;
            byte[] maskBytes = new byte[is64 ? 16 : 4];
            for (int i = 0; i < maskBytes.Length; i++)
            {
                if (subnetMask >= 8)
                {
                    maskBytes[i] = 0xFF;
                    subnetMask -= 8;
                }
                else
                {
                    maskBytes[i] = (byte)(0xFF << (8 - subnetMask));
                    subnetMask = 0;
                }
            }
            return new IPAddress(maskBytes);
        }
    }
}

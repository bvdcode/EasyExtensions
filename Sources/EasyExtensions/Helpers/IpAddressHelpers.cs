using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace EasyExtensions.Helpers
{
    /// <summary>
    /// IP address helpers.
    /// </summary>
    public static class IpAddressHelpers
    {
        /// <summary>
        /// Convert IP address to number.
        /// </summary>
        /// <param name="ipAddress"> IP address. </param>
        /// <returns> IP address as number. </returns>
        public static BigInteger IpToNumber(string ipAddress)
        {
            return IPAddress.Parse(ipAddress).ToNumber();
        }

        /// <summary>
        /// Convert number to IP address.
        /// </summary>
        /// <param name="ipNumber"> IP address as number. </param>
        /// <param name="addressFamily"> Address family. </param>
        /// <returns> IP address. </returns>
        public static IPAddress NumberToIp(BigInteger ipNumber, AddressFamily addressFamily)
        {
            if (ipNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ipNumber), "IP number cannot be negative.");
            }
            if (addressFamily == AddressFamily.InterNetwork && ipNumber > uint.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(ipNumber), "Invalid IP number for IPv4.");
            }
            byte[] ipBytes = addressFamily switch
            {
                AddressFamily.InterNetwork => BitConverter.GetBytes((uint)ipNumber),
                AddressFamily.InterNetworkV6 => ipNumber.ToByteArray(),
                _ => throw new ArgumentOutOfRangeException(nameof(addressFamily), "Invalid address family."),
            };
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(ipBytes);
            }
            if (ipBytes.Length != 4 && ipBytes.Length != 16)
            {
                throw new ArgumentOutOfRangeException(nameof(ipNumber), "Invalid IP address length: " + ipBytes.Length);
            }
            return new IPAddress(ipBytes);
        }

        /// <summary>
        /// Get subnet mask address.
        /// </summary>
        /// <param name="subnetMask"> Subnet mask. </param>
        /// <param name="addressFamily"> Address family. </param>
        /// <returns> Subnet address. </returns>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when subnet mask is invalid. </exception>
        public static IPAddress GetMaskAddress(int subnetMask, AddressFamily addressFamily)
        {
            switch (addressFamily)
            {
                case AddressFamily.InterNetwork:
                    if (subnetMask < 0 || subnetMask > 32)
                    {
                        throw new ArgumentOutOfRangeException(nameof(subnetMask), "Invalid subnet mask.");
                    }
                    break;
                case AddressFamily.InterNetworkV6:
                    if (subnetMask < 0 || subnetMask > 128)
                    {
                        throw new ArgumentOutOfRangeException(nameof(subnetMask), "Invalid subnet mask.");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(addressFamily), "Invalid address family.");
            }

            byte[] maskBytes = new byte[addressFamily == AddressFamily.InterNetworkV6 ? 16 : 4];
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

        /// <summary>
        /// Extract subnet mask from IP address.
        /// </summary>
        /// <param name="ip"> IP address. </param>
        /// <returns> Subnet mask, or null if not found. </returns>
        public static IPAddress? ExtractMask(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                return null;
            }
            string[] parts = ip.Split('/');
            if (parts.Length != 2)
            {
                return null;
            }
            if (!int.TryParse(parts[1], out int mask))
            {
                return null;
            }
            bool parsed = IPAddress.TryParse(parts[0], out IPAddress? ipAddress);
            if (!parsed)
            {
                return null;
            }
            return GetMaskAddress(mask, ipAddress.AddressFamily);
        }
    }
}

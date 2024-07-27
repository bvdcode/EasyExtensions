using System;
using System.Net;

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
        public static ulong IpToNumber(string ipAddress)
        {
            return IPAddress.Parse(ipAddress).ToNumber();
        }

        /// <summary>
        /// Convert number to IP address.
        /// </summary>
        /// <param name="ipNumber"> IP address as number. </param>
        /// <returns> IP address. </returns>
        public static IPAddress NumberToIp(ulong ipNumber)
        {
            if (ipNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ipNumber), "IP number cannot be negative.");
            }
            byte[] ipBytes = ipNumber <= uint.MaxValue ?
                BitConverter.GetBytes((uint)ipNumber) :
                BitConverter.GetBytes(ipNumber);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(ipBytes);
            }
            return new IPAddress(ipBytes);
        }

        /// <summary>
        /// Get subnet mask address.
        /// </summary>
        /// <param name="subnetMask"> Subnet mask. </param>
        /// <returns> Subnet address. </returns>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when subnet mask is invalid. </exception>
        public static IPAddress GetMaskAddress(int subnetMask)
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

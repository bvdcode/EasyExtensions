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
    }
}

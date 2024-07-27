using System.Net;
using EasyExtensions.Helpers;

namespace EasyExtensions.Tests
{
    public class IpAddressHelpersTests
    {
        [Test]
        public void ConvertIpToNumber_ValidInput_ValidOutput()
        {
            string ip = "70.95.76.0";
            ulong expected = 1180650496;
            ulong actual = IpAddressHelpers.IpToNumber(ip);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertNumberToIp_ValidInput_ValidOutput()
        {
            ulong ipNumber = 1180650496;
            string expected = "70.95.76.0";
            string actual = IpAddressHelpers.NumberToIp(ipNumber).ToString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertIpToNumber_InvalidInput_ThrowFormatException()
        {
            string ip = "1234.5678.90.12";
            Assert.Throws(typeof(FormatException), () => IpAddressHelpers.IpToNumber(ip));
        }

        [Test]
        public void ConvertIpV6ToNumber_ValidInput_ValidOutput()
        {
            string ip = "2607:fb90:7328:47bf:3dfe:3f80:a256:8f65";
            ulong expected = 4467077702110056293;
            ulong actual = IpAddressHelpers.IpToNumber(ip);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetNetworkAddress_ValidInput_ValidOutput()
        {
            string ip = "10.0.0.15";
            IPAddress iPAddress = IPAddress.Parse(ip);
            int subnetMask = 24;
            string expected = "10.0.0.0";
            string actual = iPAddress.GetNetwork(subnetMask).ToString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetBroadcastAddress_ValidInput_ValidOutput()
        {
            string ip = "10.0.0.62";
            IPAddress iPAddress = IPAddress.Parse(ip);
            int subnetMask = 24;
            string expected = "10.0.0.255";
            string actual = iPAddress.GetBroadcast(subnetMask).ToString();
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}

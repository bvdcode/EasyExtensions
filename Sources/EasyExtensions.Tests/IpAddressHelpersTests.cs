using System.Net;
using System.Numerics;
using System.Net.Sockets;
using EasyExtensions.Helpers;

namespace EasyExtensions.Tests
{
    public class IpAddressHelpersTests
    {
        private readonly BigInteger ipv6 = BigInteger.Parse("50552112247141937565140153591141928805");

        [Test]
        public void ConvertIpToNumber_ValidInput_ValidOutput()
        {
            string ip = "70.95.76.0";
            BigInteger expected = 1180650496;
            BigInteger actual = IpAddressHelpers.IpToNumber(ip);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertNumberToIp_ValidInput_ValidOutput()
        {
            uint ipNumber = 1180650496;
            string expected = "70.95.76.0";
            string actual = IpAddressHelpers.NumberToIp(ipNumber, AddressFamily.InterNetwork).ToString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertIpToNumber_InvalidInput_ThrowFormatException()
        {
            string ip = "1234.5678.90.12";
            Assert.Throws(typeof(FormatException), () => IpAddressHelpers.IpToNumber(ip));
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

        [Test]
        public void GetNetwork_ReturnsCorrectNetworkAddress()
        {
            // Arrange
            IPAddress ipAddress = IPAddress.Parse("192.168.0.100");
            IPAddress subnetMask = IPAddress.Parse("255.255.255.0");
            IPAddress expectedNetworkAddress = IPAddress.Parse("192.168.0.0");

            // Act
            IPAddress actualNetworkAddress = ipAddress.GetNetwork(subnetMask);

            // Assert
            Assert.That(actualNetworkAddress, Is.EqualTo(expectedNetworkAddress));
        }

        [Test]
        public void GetBroadcast_ReturnsCorrectBroadcastAddress()
        {
            // Arrange
            IPAddress ipAddress = IPAddress.Parse("192.168.0.100");
            IPAddress subnetMask = IPAddress.Parse("255.255.255.0");
            IPAddress expectedBroadcastAddress = IPAddress.Parse("192.168.0.255");

            // Act
            IPAddress actualBroadcastAddress = ipAddress.GetBroadcast(subnetMask);

            // Assert
            Assert.That(actualBroadcastAddress, Is.EqualTo(expectedBroadcastAddress));
        }

        [Test]
        public void GetNetwork_WithInvalidSubnetMask_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            IPAddress ipAddress = IPAddress.Parse("192.168.0.100");
            int invalidSubnetMask = 33;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => ipAddress.GetNetwork(invalidSubnetMask));
        }

        [Test]
        public void GetBroadcast_WithInvalidSubnetMask_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            IPAddress ipAddress = IPAddress.Parse("192.168.0.100");
            int invalidSubnetMask = 33;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => ipAddress.GetBroadcast(invalidSubnetMask));
        }

        [Test]
        public void ToNumber_ReturnsCorrectNumber()
        {
            // Arrange
            IPAddress ipAddress = IPAddress.Parse("192.168.0.100");
            BigInteger expectedNumber = 3232235620;

            // Act
            BigInteger actualNumber = ipAddress.ToNumber();

            // Assert
            Assert.That(actualNumber, Is.EqualTo(expectedNumber));
        }

        [Test]
        public void GetMaskAddress_WithInvalidSubnetMask_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int invalidSubnetMask = 129;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => IpAddressHelpers.GetMaskAddress(invalidSubnetMask, AddressFamily.InterNetwork));
        }



        // IPv6 tests


        [Test]
        public void ConvertIpV6ToNumber_ValidInput_ValidOutput()
        {
            string ip = "2607:fb90:7328:47bf:3dfe:3f80:a256:8f65";
            BigInteger expected = ipv6;
            BigInteger actual = IpAddressHelpers.IpToNumber(ip);
            Assert.That(actual, Is.EqualTo(expected));
        }


        // 2607:fb90:7328:47bf:3dfe:3f80:a256:8f65
        // network: 2607:fb90:7300::/40
        // broadcast: 2607:fb90:73ff:ffff:ffff:ffff:ffff:ffff

        [Test]
        public void GetNetworkV6Address_ValidInput_ValidOutput()
        {
            string ip = "2607:fb90:7328:47bf:3dfe:3f80:a256:8f65";
            IPAddress iPAddress = IPAddress.Parse(ip);
            int subnetMask = 40;
            string expected = "2607:fb90:7300::";
            string actual = iPAddress.GetNetwork(subnetMask).ToString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetBroadcastV6Address_ValidInput_ValidOutput()
        {
            string ip = "2607:fb90:7328:47bf:3dfe:3f80:a256:8f65";
            IPAddress iPAddress = IPAddress.Parse(ip);
            int subnetMask = 40;
            string expected = "2607:fb90:73ff:ffff:ffff:ffff:ffff:ffff";
            string actual = iPAddress.GetBroadcast(subnetMask).ToString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetMaskAddressV6_WithInvalidSubnetMask_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int invalidSubnetMask = 129;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => IpAddressHelpers.GetMaskAddress(invalidSubnetMask, AddressFamily.InterNetworkV6));
        }

        [Test]
        public void GetNetworkV6_ReturnsCorrectNetworkAddress()
        {
            // Arrange
            IPAddress ipAddress = IPAddress.Parse("2607:fb90:7328:47bf:3dfe:3f80:a256:8f65");
            IPAddress subnetMask = IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff00");
            IPAddress expectedNetworkAddress = IPAddress.Parse("2607:fb90:7328:47bf:3dfe:3f80:a256:8f00");

            // Act
            IPAddress actualNetworkAddress = ipAddress.GetNetwork(subnetMask);

            // Assert
            Assert.That(actualNetworkAddress, Is.EqualTo(expectedNetworkAddress));
        }

        [Test]
        public void GetBroadcastV6_ReturnsCorrectBroadcastAddress()
        {
            // Arrange
            IPAddress ipAddress = IPAddress.Parse("2607:fb90:7328:47bf:3dfe:3f80:a256:8f65");
            IPAddress subnetMask = IPAddress.Parse("ffff:ffff:ff00:0000:0000:0000:0000:0000");
            IPAddress expectedBroadcastAddress = IPAddress.Parse("2607:fb90:73ff:ffff:ffff:ffff:ffff:ffff");

            // Act
            IPAddress actualBroadcastAddress = ipAddress.GetBroadcast(subnetMask);

            // Assert
            Assert.That(actualBroadcastAddress, Is.EqualTo(expectedBroadcastAddress));
        }

        [Test]
        public void GetNetworkV6_WithInvalidSubnetMask_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            IPAddress ipAddress = IPAddress.Parse("2607:fb90:7328:47bf:3dfe:3f80:a256:8f65");
            int invalidSubnetMask = 129;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => ipAddress.GetNetwork(invalidSubnetMask));
        }

        [Test]
        public void GetBroadcastV6_WithInvalidSubnetMask_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            IPAddress ipAddress = IPAddress.Parse("2607:fb90:7328:47bf:3dfe:3f80:a256:8f65");
            int invalidSubnetMask = 129;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => ipAddress.GetBroadcast(invalidSubnetMask));
        }

        [Test]
        public void ToNumberV6_ReturnsCorrectNumber()
        {
            // Arrange
            IPAddress ipAddress = IPAddress.Parse("2607:fb90:7328:47bf:3dfe:3f80:a256:8f65");
            BigInteger expectedNumber = ipv6;

            // Act
            BigInteger actualNumber = ipAddress.ToNumber();

            // Assert
            Assert.That(actualNumber, Is.EqualTo(expectedNumber));
        }

        [Test]
        public void ConvertNumberToIpV6_ValidInput_ValidOutput()
        {
            BigInteger ipNumber = ipv6;
            string expected = "2607:fb90:7328:47bf:3dfe:3f80:a256:8f65";
            string actual = IpAddressHelpers.NumberToIp(ipNumber, AddressFamily.InterNetworkV6).ToString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertNumberToIpV6_InvalidInput_ThrowArgumentOutOfRangeException()
        {
            ulong ipNumber = ulong.MaxValue;
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => IpAddressHelpers.NumberToIp(ipNumber, AddressFamily.InterNetworkV6));
        }

        [Test]
        public void ConvertIPv6_ValidInput_ValidOutput()
        {
            string expected = "2607:fb90:7328:47bf:3dfe:3f80:a256:8f65";
            BigInteger num = IpAddressHelpers.IpToNumber(expected);
            BigInteger expectedNumber = ipv6;
            Assert.That(num, Is.EqualTo(expectedNumber));
            string actual = IpAddressHelpers.NumberToIp(num, AddressFamily.InterNetworkV6).ToString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CalculateV4_WithValidInput_ValidOutput()
        {
            string ip = "2.125.53.1";
            IPAddress iPAddress = IPAddress.Parse(ip);
            string network = "2.125.52.0/22";
            BigInteger expectedNetwork = 41759744;
            BigInteger expectedBroadcast = 41760767;

            string networkAddress = "2.125.52.0";
            BigInteger forceParsed = IpAddressHelpers.IpToNumber(networkAddress);
            Assert.That(forceParsed, Is.EqualTo(expectedNetwork));

            string broadcastAddress = "2.125.55.255";
            forceParsed = IpAddressHelpers.IpToNumber(broadcastAddress);
            Assert.That(forceParsed, Is.EqualTo(expectedBroadcast));


            var subnet = IpAddressHelpers.ExtractMask(network);
            Assert.That(subnet?.ToString(), Is.EqualTo("255.255.252.0"));
            var actualNetwork = iPAddress.GetNetwork(subnet).ToNumber();
            Assert.That(actualNetwork, Is.EqualTo(expectedNetwork));
            var actualBroadcast = iPAddress.GetBroadcast(subnet).ToNumber();
            Assert.That(actualBroadcast, Is.EqualTo(expectedBroadcast));
        }
    }
}

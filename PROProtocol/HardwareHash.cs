using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace PROProtocol
{
    public static class HardwareHash
    {
        public static string Empty => "MAC";

        public static string GenerateRandom()
        {
            var mac = new StringBuilder();
            using (var rng = new RNGCryptoServiceProvider())
            {
                var random = new byte[16];
                rng.GetBytes(random);
                var count = random[0] % 5 == 0 ? 6 : 12;
                for (var i = 0; i < count; ++i)
                    mac.Append((random[i + 1] % 254 + 1).ToString("X2"));
            }
            return mac.ToString();
        }

        public static string RetrieveMac()
        {
            var result = string.Empty;
            var allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            if (allNetworkInterfaces.Length < 1)
                return Empty;
            result = AddressToString(allNetworkInterfaces[0].GetPhysicalAddress());
            if (allNetworkInterfaces.Length > 1)
                result += AddressToString(allNetworkInterfaces[1].GetPhysicalAddress());
            return result;
        }

        private static string AddressToString(PhysicalAddress physicalAddress)
        {
            var result = string.Empty;
            var addressBytes = physicalAddress.GetAddressBytes();
            for (var i = 0; i < addressBytes.Length; i++)
                result += string.Format("{0}", addressBytes[i].ToString("X2"));
            return result;
        }
    }
}
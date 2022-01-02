using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PROProtocol
{
    public static class Hardware
    {
        private static readonly Random Random = new Random();
        public static Guid GenerateRandomHash()
        {
            return Guid.NewGuid();
        }

        public static Guid RetrieveRealHash()
        {
            // TODO: find a way to retrieve the real Device ID from Unity.
            throw new NotImplementedException();
        }

        public static string GenerateRandomOsInfo()
        {
            string[] osVer = {"Windows 10  (10.0.18363) 64bit",
                              "Windows 10  (10.0.10240) 64bit",
                              "Windows 10  (10.0.10586) 64bit",
                              "Windows 10  (10.0.14393) 64bit",
                              "Windows 10  (10.0.15063) 64bit",
                              "Windows 10  (10.0.16299) 64bit",
                              "Windows 10  (10.0.17134) 64bit",
                              "Windows 10  (10.0.17763) 64bit",
                              "Windows 10  (10.0.18362) 64bit",
                              "Windows 10  (10.0.19041) 64bit",
                              "Windows 10  (10.0.19042) 64bit",
                              "Windows 10  (10.0.19043) 64bit",
                              "Windows 8.1 (6.3.9600) 64 bit",
                              "Windows 7 (6.1.7601) 64bit",
                              //"Mac OS X 10.10.4",
                              //"iPhone OS 8.4",
                              //"Android OS API-22",
                              //"Android OS API-23",
                              //"Android OS API-24",
                              //"Android OS API-25",
                              //"Android OS API-26",
                              //"Android OS API-27",
                              //"Android OS API-28",
                              //"Android OS API-29",
                              };
            int vRandom = Random.Next(osVer.Length);

            return osVer[vRandom];
        }

        public static string RetrieveRealOsInfo()
        {
            // TODO: find a way to retrieve the real Os Info from Unity.
            throw new NotImplementedException();
        }
    }
}

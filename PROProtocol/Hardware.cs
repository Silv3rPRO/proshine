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
            return "Windows 10  (10.0.19042) 64bit";
        }

        public static string RetrieveRealOsInfo()
        {
            // TODO: find a way to retrieve the real Os Info from Unity.
            throw new NotImplementedException();
        }
    }
}

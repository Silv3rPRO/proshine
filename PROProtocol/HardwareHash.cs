using System;

namespace PROProtocol
{
    public static class HardwareHash
    {
        public static Guid GenerateRandom()
        {
            return Guid.NewGuid();
        }

        public static Guid RetrieveReal()
        {
            // TODO: find a way to retrieve the real Device ID from Unity.
            throw new NotImplementedException();
        }
    }
}

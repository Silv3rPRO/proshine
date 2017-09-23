using System;

namespace PROProtocol
{
    public static class XorEncryption
    {
        public static string Encrypt(string input)
        {
            var output = string.Empty;
            for (var i = 0; i < input.Length; i++)
                output += Convert.ToChar((byte)(input[i] ^ '\u0001')).ToString();
            return output;
        }
    }
}
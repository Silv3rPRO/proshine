namespace PROBot
{
    public enum SocksVersion
    {
        None = 0,
        Socks4 = 4,
        Socks5 = 5
    };

    public class Socks
    {
        public SocksVersion Version { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public Socks()
        {
            Version = SocksVersion.None;
            Port = -1;
        }
    }
}

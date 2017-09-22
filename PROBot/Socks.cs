namespace PROBot
{
    public class Socks
    {
        public Socks()
        {
            Version = SocksVersion.None;
            Port = -1;
        }

        public SocksVersion Version { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
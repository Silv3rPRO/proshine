using BrightNetwork;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PROProtocol
{
    public class GameConnection : SimpleTextClient
    {
        public enum Server
        {
            Red,
            Blue
        }
        
        private const string RedAddress = "46.28.205.35";
        private const string BlueAddress = "46.28.207.53";
        protected const int ServerPort = 800;

        private Server _server;
        private bool _useSocks;
        private int _socksVersion;
        private string _socksHost;
        private int _socksPort;
        private string _socksUser;
        private string _socksPass;

        public GameConnection(Server server)
            : base(new BrightClient())
        {
            PacketDelimiter = "|.\\\r\n";
            TextEncoding = Encoding.GetEncoding(1252);

            _server = server;
        }

        public GameConnection(Server server, int socksVersion, string socksHost, int socksPort, string socksUser, string socksPass)
            : this(server)
        {
            _useSocks = true;
            _socksVersion = socksVersion;
            _socksHost = socksHost;
            _socksPort = socksPort;
            _socksUser = socksUser;
            _socksPass = socksPass;
        }

        public async void Connect()
        {
            string host = _server == Server.Blue ? BlueAddress : RedAddress;
            
            if (!_useSocks)
            {
                Connect(IPAddress.Parse(host), ServerPort);
            }
            else
            {
                try
                {
                    Socket socket = await SocksConnection.OpenConnection(_socksVersion, host, ServerPort, _socksHost, _socksPort, _socksUser, _socksPass);
                    Initialize(socket);
                }
                catch (Exception ex)
                {
                    Close(ex);
                }
            }
        }

        protected override string ProcessDataBeforeSending(string data)
        {
            return XorEncryption.Encrypt(data);
        }

        protected override string ProcessDataBeforeReceiving(string data)
        {
            return XorEncryption.Encrypt(data);
        }
    }
}

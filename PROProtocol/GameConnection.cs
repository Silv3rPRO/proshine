using BrightNetwork;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PROProtocol
{
    public class GameConnection : SimpleTextClient
    {
        private const int ServerPort = 800;

        public GameServer Server;

        private bool _useSocks;
        private int _socksVersion;
        private string _socksHost;
        private int _socksPort;
        private string _socksUser;
        private string _socksPass;

        public GameConnection(GameServer server)
            : base(new BrightClient())
        {
            PacketDelimiter = "|.\\\r\n";
            TextEncoding = Encoding.GetEncoding(1252);

            Server = server;
        }

        public GameConnection(GameServer server, int socksVersion, string socksHost, int socksPort, string socksUser, string socksPass)
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
            string host = Server.GetAddress();
            
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

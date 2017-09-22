using System;
using System.Net;
using System.Text;
using BrightNetwork;

namespace PROProtocol
{
    public class GameConnection : SimpleTextClient
    {
        private const int ServerPort = 800;
        private readonly string _socksHost;
        private readonly string _socksPass;
        private readonly int _socksPort;
        private readonly string _socksUser;
        private readonly int _socksVersion;

        private readonly bool _useSocks;

        public GameServer Server;

        public GameConnection(GameServer server)
            : base(new BrightClient())
        {
            PacketDelimiter = "|.\\\r\n";
            TextEncoding = Encoding.GetEncoding(1252);

            Server = server;
        }

        public GameConnection(GameServer server, int socksVersion, string socksHost, int socksPort, string socksUser,
            string socksPass)
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
            var host = Server.GetAddress();

            if (!_useSocks)
                Connect(IPAddress.Parse(host), ServerPort);
            else
                try
                {
                    var socket = await SocksConnection.OpenConnection(_socksVersion, host, ServerPort, _socksHost,
                        _socksPort, _socksUser, _socksPass);
                    Initialize(socket);
                }
                catch (Exception ex)
                {
                    Close(ex);
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
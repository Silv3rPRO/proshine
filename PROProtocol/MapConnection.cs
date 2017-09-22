using System;
using System.Net;
using BrightNetwork;

namespace PROProtocol
{
    public class MapConnection : SimpleBinaryClient
    {
        // TODO: Remove the duplication with GameConnection by creating an abstract class?

        private const string ServerAddress = "95.183.48.126";
        private const int ServerPort = 803;
        private readonly string _socksHost;
        private readonly string _socksPass;
        private readonly int _socksPort;
        private readonly string _socksUser;
        private readonly int _socksVersion;

        private readonly bool _useSocks;

        public MapConnection()
            : base(new BrightClient())
        {
            MaxPacketLength = 0xFFFFFF;
            HeaderSize = 4;
            IsHeaderSizeIncluded = true;
        }

        public MapConnection(int socksVersion, string socksHost, int socksPort, string socksUser, string socksPass)
            : this()
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
            if (!_useSocks)
                Connect(IPAddress.Parse(ServerAddress), ServerPort);
            else
                try
                {
                    var socket = await SocksConnection.OpenConnection(_socksVersion, ServerAddress, ServerPort,
                        _socksHost, _socksPort, _socksUser, _socksPass);
                    Initialize(socket);
                }
                catch (Exception ex)
                {
                    Close(ex);
                }
        }
    }
}
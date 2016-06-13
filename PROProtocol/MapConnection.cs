using BrightNetwork;
using System;
using System.Net;
using System.Net.Sockets;

namespace PROProtocol
{
    public class MapConnection : SimpleBinaryClient
    {
        // TODO: Remove the duplication with GameConnection by creating an abstract class?

        private const string ServerAddress = "46.28.207.193";
        private const int ServerPort = 803;

        private bool _useSocks;
        private int _socksVersion;
        private string _socksHost;
        private int _socksPort;
        private string _socksUser;
        private string _socksPass;

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
            {
                Connect(IPAddress.Parse(ServerAddress), ServerPort);
            }
            else
            {
                try
                {
                    Socket socket = await SocksConnection.OpenConnection(_socksVersion, ServerAddress, ServerPort, _socksHost, _socksPort, _socksUser, _socksPass);
                    Initialize(socket);
                }
                catch (Exception ex)
                {
                    Close(ex);
                }
            }
        }
    }
}

﻿using BrightNetwork;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace PROProtocol
{
    public class GameConnection : SimpleTextClient
    {
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
            var serverHost = Server.GetAddress();
            
            if (!_useSocks)
            {
                Connect(serverHost.Address, serverHost.Port);
            }
            else
            {
                try
                {
                    Socket socket = await SocksConnection.OpenConnection(_socksVersion, serverHost.Address, serverHost.Port, _socksHost, _socksPort, _socksUser, _socksPass);
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
            var input_bytes = TextEncoding.GetBytes(data);
            var output_bytes = Encryption.Encrypt(input_bytes);
            return TextEncoding.GetString(output_bytes);
        }

        protected override string ProcessDataBeforeReceiving(string data)
        {
            var data_bytes = TextEncoding.GetBytes(data);
            if (!Encryption.StateReady)
            {
                Encryption.Decrypt(data_bytes, 16);
                Encryption.Encrypt(data_bytes.Skip(16).ToArray(), 16);
                Encryption.StateReady = true;
                return null;
            }
            return TextEncoding.GetString(Encryption.Decrypt(data_bytes));
        }
    }
}

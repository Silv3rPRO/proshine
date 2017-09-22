using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace BrightNetwork
{
    public class SimpleBinaryClient
    {
        private readonly BrightClient _client;
        private Exception _closingException;
        private readonly byte[] _lengthBuffer = new byte[16];

        private int _pendingLength;
        private readonly Queue<byte[]> _pendingPackets = new Queue<byte[]>();

        private readonly List<byte> _receiveBuffer = new List<byte>();
        private bool _wasConnected;
        private bool _wasDisconnected;
        protected int HeaderSize = 2;
        protected bool IsHeaderSizeIncluded = false;

        protected int MaxPacketLength = 0xFFFF;

        public SimpleBinaryClient(BrightClient client)
        {
            _client = client;

            client.Connected += Client_Connected;
            client.Disconnected += Client_Disconnected;
            client.DataReceived += Client_DataReceived;
        }

        public bool IsConnected => _client.IsConnected;

        public IPAddress RemoteIpAddress => _client.RemoteIpAddress;

        public event Action Connected;
        public event Action<Exception> Disconnected;
        public event Action<BinaryReader> PacketReceived;

        public void Connect(IPAddress address, int port)
        {
            _client.BeginConnect(address, port);
        }

        public void Initialize(Socket socket)
        {
            _client.Initialize(socket);
        }

        public void Update()
        {
            if (_wasConnected)
            {
                _wasConnected = false;
                Connected?.Invoke();
            }
            ReceivePendingPackets();
            if (_wasDisconnected)
            {
                _wasDisconnected = false;
                Disconnected?.Invoke(_closingException);
            }
        }

        public void Send(byte[] packet)
        {
            if (packet.Length > MaxPacketLength)
                throw new Exception("Tried to send a too large packet");

            var packetLength = packet.Length;
            if (IsHeaderSizeIncluded) packetLength += HeaderSize;

            byte[] header;
            if (HeaderSize == 2)
                header = BitConverter.GetBytes((ushort) packetLength);
            else if (HeaderSize == 4)
                header = BitConverter.GetBytes(packetLength);
            else
                throw new Exception("Unsupported header size: " + HeaderSize);
            var data = new byte[packet.Length + HeaderSize];
            Array.Copy(header, 0, data, 0, header.Length);
            Array.Copy(packet, 0, data, header.Length, packet.Length);
            _client.BeginSend(data);
        }

        public void Close(Exception error = null)
        {
            _client.Close(error);
        }

        private void ReceivePendingPackets()
        {
            bool hasReceived;
            do
            {
                byte[] packet = null;
                lock (_pendingPackets)
                {
                    if (_pendingPackets.Count > 0)
                        packet = _pendingPackets.Dequeue();
                }
                hasReceived = false;
                if (packet != null)
                {
                    hasReceived = true;
                    using (var stream = new MemoryStream(packet, false))
                    {
                        using (var reader = new BinaryReader(stream))
                        {
                            PacketReceived?.Invoke(reader);
                        }
                    }
                }
            } while (hasReceived);
        }

        private void Client_Connected()
        {
            _wasConnected = true;
        }

        private void Client_Disconnected(Exception ex)
        {
            _wasDisconnected = true;
            _closingException = ex;
        }

        private void Client_DataReceived(byte[] data)
        {
            _receiveBuffer.AddRange(data);
            ExtractPackets();
        }

        private void ExtractPackets()
        {
            bool hasExtracted;
            do
            {
                if (_pendingLength == 0)
                    hasExtracted = ExtractPendingLength();
                else
                    hasExtracted = ExtractPendingPacket();
            } while (hasExtracted);
        }

        private bool ExtractPendingLength()
        {
            if (_receiveBuffer.Count >= HeaderSize)
            {
                _receiveBuffer.CopyTo(0, _lengthBuffer, 0, HeaderSize);
                if (HeaderSize == 2)
                    _pendingLength = BitConverter.ToUInt16(_lengthBuffer, 0);
                else if (HeaderSize == 4)
                    _pendingLength = BitConverter.ToInt32(_lengthBuffer, 0);
                else
                    throw new Exception("Unsupported header size: " + HeaderSize);
                _receiveBuffer.RemoveRange(0, HeaderSize);

                if (IsHeaderSizeIncluded) _pendingLength -= HeaderSize;

                if (_pendingLength < 0 || _pendingLength > MaxPacketLength)
                {
                    _client.Close(new Exception("Tried to receive a too large packet"));
                    return false;
                }

                return true;
            }
            return false;
        }

        private bool ExtractPendingPacket()
        {
            if (_receiveBuffer.Count >= _pendingLength)
            {
                var packet = new byte[_pendingLength];

                _receiveBuffer.CopyTo(0, packet, 0, _pendingLength);
                _receiveBuffer.RemoveRange(0, _pendingLength);
                _pendingLength = 0;

                lock (_pendingPackets)
                {
                    _pendingPackets.Enqueue(packet);
                }

                return true;
            }
            return false;
        }
    }
}
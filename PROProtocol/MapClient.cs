using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace PROProtocol
{
    public class MapClient
    {
        public bool IsConnected { get; private set; }

        public event Action ConnectionOpened;
        public event Action<Exception> ConnectionFailed;
        public event Action<Exception> ConnectionClosed;
        public event Action<string, Map> MapLoaded;

        private const string MapExtension = ".pm";

        private MapConnection _connection;
        private byte[] _compressionBuffer = new byte[4096];
        private Dictionary<string, Map> _cache = new Dictionary<string, Map>();

        public MapClient(MapConnection connection)
        {
            _connection = connection;
            _connection.PacketReceived += OnPacketReceived;
            _connection.Connected += OnConnected;
            _connection.Disconnected += OnDisconnected;
        }

        public void Open(IPAddress ip, int port)
        {
#if DEBUG
            Console.WriteLine("[+++] Connecting to the map server");
#endif
            _connection.Connect(ip, port);
        }

        public void Update()
        {
            _connection.Update();
        }

        public void Close()
        {
            _connection.Close();
        }

        public void DownloadMap(string mapName)
        {
            mapName += MapExtension;

            if (_cache.ContainsKey(mapName))
            {
#if DEBUG
                Console.WriteLine("[Map] Loaded from cache: " + mapName);
#endif
                MapLoaded?.Invoke(RemoveExtension(mapName), _cache[mapName]);
                return;
            }

#if DEBUG
            Console.WriteLine("[Map] Requested: " + mapName);
#endif

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)0x72);
                    writer.Write(Encoding.ASCII.GetBytes(mapName));
                }
                _connection.Send(stream.ToArray());
            }
        }

        private void OnPacketReceived(BinaryReader reader)
        {
            char type = (char)reader.ReadByte();

            if (type == 'm')
            {
                int mapNameSize = reader.ReadByte();
                string name = Encoding.ASCII.GetString(reader.ReadBytes(mapNameSize));

                int mapLength = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
                byte[] content = reader.ReadBytes(mapLength);

                content = DecompressContent(content);

                Map map = new Map(content);
                if (!_cache.ContainsKey(name))
                {
                    _cache.Add(name, map);
                }

#if DEBUG
                Console.WriteLine("[Map] Received: " + name);
#endif

                MapLoaded?.Invoke(RemoveExtension(name), map);
            }
        }

        private byte[] DecompressContent(byte[] content)
        {
            using (MemoryStream mapStream = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(new MemoryStream(content), CompressionMode.Decompress))
                {
                    int bytesCount;
                    do
                    {
                        bytesCount = gzip.Read(_compressionBuffer, 0, _compressionBuffer.Length);
                        if (bytesCount > 0)
                        {
                            mapStream.Write(_compressionBuffer, 0, bytesCount);
                        }
                    }
                    while (bytesCount > 0);
                }

                return mapStream.ToArray();
            }
        }

        private void OnConnected()
        {
            IsConnected = true;
#if DEBUG
            Console.WriteLine("[+++] Map connection opened");
#endif
            ConnectionOpened?.Invoke();
        }

        private void OnDisconnected(Exception ex)
        {
            if (!IsConnected)
            {
#if DEBUG
                Console.WriteLine("[---] Map connection failed");
#endif
                ConnectionFailed?.Invoke(ex);
            }
            else
            {
                IsConnected = false;
#if DEBUG
                Console.WriteLine("[---] Map connection closed");
#endif
                ConnectionClosed?.Invoke(ex);
            }
        }

        public static string RemoveExtension(string name)
        {
            return name.Replace(MapExtension, "");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PROProtocol
{
    public class MapClient
    {
        private const string MapExtension = ".pm";
        private readonly Dictionary<string, Map> _cache = new Dictionary<string, Map>();
        private readonly byte[] _compressionBuffer = new byte[4096];

        private readonly MapConnection _connection;

        public MapClient(MapConnection connection)
        {
            _connection = connection;
            _connection.PacketReceived += OnPacketReceived;
            _connection.Connected += OnConnected;
            _connection.Disconnected += OnDisconnected;
        }

        public bool IsConnected { get; private set; }

        public event Action ConnectionOpened;

        public event Action<Exception> ConnectionFailed;

        public event Action<Exception> ConnectionClosed;

        public event Action<string, Map> MapLoaded;

        public void Open()
        {
#if DEBUG
            Console.WriteLine("[+++] Connecting to the map server");
#endif
            _connection.Connect();
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

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)0x72);
                    writer.Write(Encoding.ASCII.GetBytes(mapName));
                }
                _connection.Send(stream.ToArray());
            }
        }

        private void OnPacketReceived(BinaryReader reader)
        {
            var type = (char)reader.ReadByte();

            if (type == 'm')
            {
                int mapNameSize = reader.ReadByte();
                var name = Encoding.ASCII.GetString(reader.ReadBytes(mapNameSize));

                var mapLength = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
                var content = reader.ReadBytes(mapLength);

                content = DecompressContent(content);

                var map = new Map(content);
                if (!_cache.ContainsKey(name))
                    _cache.Add(name, map);

#if DEBUG
                Console.WriteLine("[Map] Received: " + name);
#endif

                MapLoaded?.Invoke(RemoveExtension(name), map);
            }
        }

        private byte[] DecompressContent(byte[] content)
        {
            using (var mapStream = new MemoryStream())
            {
                using (var gzip = new GZipStream(new MemoryStream(content), CompressionMode.Decompress))
                {
                    int bytesCount;
                    do
                    {
                        bytesCount = gzip.Read(_compressionBuffer, 0, _compressionBuffer.Length);
                        if (bytesCount > 0)
                            mapStream.Write(_compressionBuffer, 0, bytesCount);
                    } while (bytesCount > 0);
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
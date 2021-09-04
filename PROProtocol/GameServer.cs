using System;
using System.Collections.Generic;
using System.Net;

namespace PROProtocol
{
    public enum GameServer
    {
        Silver,
        Gold
    }

    public static class GameServerExtensions
    {
        private static Dictionary<GameServer, IPAddress> _cachedIpAddresses = new Dictionary<GameServer, IPAddress>();

        public static IPEndPoint GetAddress(this GameServer server)
        {
            if (!_cachedIpAddresses.ContainsKey(server))
                _cachedIpAddresses.Add(server, GetAddressFromDns(server + ".pokemonrevolution.net"));
            else if (_cachedIpAddresses[server] is null)
                _cachedIpAddresses[server] = GetAddressFromDns(server + ".pokemonrevolution.net");

            switch (server)
            {
                case GameServer.Silver:
                    return new IPEndPoint(_cachedIpAddresses[server], 800);
                case GameServer.Gold:
                    return new IPEndPoint(_cachedIpAddresses[server], 801);
            }
            return null;
        }

        public static GameServer FromName(string name)
        {
            switch (name.ToUpperInvariant())
            {
                case "SILVER":
                    return GameServer.Silver;
                case "GOLD":
                    return GameServer.Gold;
            }
            throw new Exception("The server " + name + " does not exist");
        }

        public static IPAddress GetMapAddress(this GameServer server)
        {
            return _cachedIpAddresses[server];
        }

        private static Random Random = new Random();
        private static IPAddress GetAddressFromDns(string dns_host)
        {
            var addresses = Dns.GetHostAddresses(dns_host);
            return addresses[Random.Next(0, addresses.Length - 1)];
        }
    }
}

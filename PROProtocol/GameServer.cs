using System;
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
        public static IPEndPoint GetAddress(this GameServer server)
        {
            switch (server)
            {
                case GameServer.Silver:
                    return new IPEndPoint(GetAddressFromDns(server + ".pokemonrevolution.net"), 800);
                case GameServer.Gold:
                    return new IPEndPoint(GetAddressFromDns(server + ".pokemonrevolution.net"), 801);
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
            return GetAddressFromDns(server + ".pokemonrevolution.net");
        }

        private static Random Random = new Random();
        private static IPAddress GetAddressFromDns(string dns_host)
        {
            var addresses = Dns.GetHostAddresses(dns_host);
            return addresses[Random.Next(0, addresses.Length - 1)];
        }
    }
}

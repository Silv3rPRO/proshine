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
                    return new IPEndPoint(IPAddress.Parse("185.212.131.104"), 800);
                case GameServer.Gold:
                    return new IPEndPoint(IPAddress.Parse("185.212.131.104"), 801);
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
            var rand = new Random();
            var addresses = Dns.GetHostAddresses(server + ".pokemonrevolution.net");
            return addresses[rand.Next(0, addresses.Length - 1)];
        }
    }
}

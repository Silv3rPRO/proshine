using System;

namespace PROProtocol
{
    public enum GameServer
    {
        Silver,
        Gold
    }

    public static class GameServerExtensions
    {
        public static string GetAddress(this GameServer server)
        {
            switch (server)
            {
                case GameServer.Silver:
                    return "95.183.48.126";
                case GameServer.Gold:
                    return "46.28.205.63";
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
    }
}

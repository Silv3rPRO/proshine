using System;

namespace PROProtocol
{
    public enum GameServer
    {
        Red,
        Blue,
        Yellow
    }

    public static class GameServerExtensions
    {
        public static string GetAddress(this GameServer server)
        {
            switch (server)
            {
                case GameServer.Red:
                    return "95.183.48.67";

                case GameServer.Blue:
                    return "46.28.207.53";

                case GameServer.Yellow:
                    return "46.28.205.63";
            }
            return null;
        }

        public static GameServer FromName(string name)
        {
            switch (name.ToUpperInvariant())
            {
                case "RED":
                    return GameServer.Red;

                case "BLUE":
                    return GameServer.Blue;

                case "YELLOW":
                    return GameServer.Yellow;
            }
            throw new Exception("The server " + name + " does not exist");
        }
    }
}
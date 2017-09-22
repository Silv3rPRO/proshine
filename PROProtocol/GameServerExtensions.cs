// Decompiled with JetBrains decompiler
// Type: PROProtocol.GameServerExtensions
// Assembly: PROProtocol, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 661A1E55-CDC5-415A-AB7E-E148E47E0F5C
// Assembly location: C:\Users\Derex\Desktop\PROShine-2.6.0.0\PROShine-2.6.1.0 - Copy - Copy\PROProtocol.dll

using System;

namespace PROProtocol
{
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
        default:
          return (string) null;
      }
    }

    public static GameServer FromName(string name)
    {
      string upperInvariant = name.ToUpperInvariant();
      if (upperInvariant == "RED")
        return GameServer.Red;
      if (upperInvariant == "BLUE")
        return GameServer.Blue;
      if (upperInvariant == "YELLOW")
        return GameServer.Yellow;
      throw new Exception("The server " + name + " does not exist");
    }
  }
}

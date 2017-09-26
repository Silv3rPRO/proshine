// Decompiled with JetBrains decompiler
// Type: PROProtocol.DirectionExtensions
// Assembly: PROProtocol, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 661A1E55-CDC5-415A-AB7E-E148E47E0F5C
// Assembly location: C:\Users\Derex\Desktop\PROShine-2.6.0.0\PROShine-2.6.1.0 - Copy - Copy\PROProtocol.dll

using System;

namespace PROProtocol
{
  public static class DirectionExtensions
  {
    public static string AsChar(this Direction direction)
    {
      switch (direction)
      {
        case Direction.Up:
          return "u";
        case Direction.Down:
          return "d";
        case Direction.Left:
          return "l";
        case Direction.Right:
          return "r";
        default:
          return (string) null;
      }
    }

    public static Direction GetOpposite(this Direction direction)
    {
      switch (direction)
      {
        case Direction.Up:
          return Direction.Down;
        case Direction.Down:
          return Direction.Up;
        case Direction.Left:
          return Direction.Right;
        default:
          return Direction.Left;
      }
    }

    public static void ApplyToCoordinates(this Direction direction, ref int x, ref int y)
    {
      switch (direction)
      {
        case Direction.Up:
          y = y - 1;
          break;
        case Direction.Down:
          y = y + 1;
          break;
        case Direction.Left:
          x = x - 1;
          break;
        case Direction.Right:
          x = x + 1;
          break;
      }
    }

    public static Direction FromChar(char c)
    {
      if ((uint) c <= 108U)
      {
        if ((int) c == 100)
          return Direction.Down;
        if ((int) c == 108)
          return Direction.Left;
      }
      else
      {
        if ((int) c == 114)
          return Direction.Right;
        if ((int) c == 117)
          return Direction.Up;
      }
      throw new Exception("The direction '" + c.ToString() + "' does not exist");
    }
  }
}

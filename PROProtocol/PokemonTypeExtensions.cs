// Decompiled with JetBrains decompiler
// Type: PROProtocol.PokemonTypeExtensions
// Assembly: PROProtocol, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 661A1E55-CDC5-415A-AB7E-E148E47E0F5C
// Assembly location: C:\Users\Derex\Desktop\PROShine-2.6.0.0\PROShine-2.6.1.0 - Copy - Copy\PROProtocol.dll

using System;

namespace PROProtocol
{
  public static class PokemonTypeExtensions
  {
    public static PokemonType FromName(string typeName)
    {
      string lowerInvariant = typeName.ToLowerInvariant();
      // ISSUE: reference to a compiler-generated method
      uint stringHash = \u003CPrivateImplementationDetails\u003E.ComputeStringHash(lowerInvariant);
      if (stringHash <= 2627848062U)
      {
        if (stringHash <= 1927346304U)
        {
          if (stringHash <= 974867124U)
          {
            if ((int) stringHash != 218211588)
            {
              if ((int) stringHash == 974867124 && lowerInvariant == "rock")
                return PokemonType.Rock;
            }
            else if (lowerInvariant == "steel")
              return PokemonType.Steel;
          }
          else if ((int) stringHash != 1237752336)
          {
            if ((int) stringHash == 1927346304 && lowerInvariant == "ice")
              return PokemonType.Ice;
          }
          else if (lowerInvariant == "water")
            return PokemonType.Water;
        }
        else if (stringHash <= 2017461200U)
        {
          if ((int) stringHash != 1943375221)
          {
            if ((int) stringHash == 2017461200 && lowerInvariant == "ghost")
              return PokemonType.Ghost;
          }
          else if (lowerInvariant == "bug")
            return PokemonType.Bug;
        }
        else if ((int) stringHash != -2128831035)
        {
          if ((int) stringHash != -1901390119)
          {
            if ((int) stringHash == -1667119234 && lowerInvariant == "ground")
              return PokemonType.Ground;
          }
          else if (lowerInvariant == "fire")
            return PokemonType.Fire;
        }
        else if (lowerInvariant != null && lowerInvariant.Length == 0)
          return PokemonType.None;
      }
      else if (stringHash <= 3562700964U)
      {
        if (stringHash <= 2993663101U)
        {
          if ((int) stringHash != -1487625730)
          {
            if ((int) stringHash == -1301304195 && lowerInvariant == "grass")
              return PokemonType.Grass;
          }
          else if (lowerInvariant == "flying")
            return PokemonType.Flying;
        }
        else if ((int) stringHash != -852231042)
        {
          if ((int) stringHash != -823974991)
          {
            if ((int) stringHash == -732266332 && lowerInvariant == "psychic")
              return PokemonType.Psychic;
          }
          else if (lowerInvariant == "poison")
            return PokemonType.Poison;
        }
        else if (lowerInvariant == "fairy")
          return PokemonType.Fairy;
      }
      else if (stringHash <= 3732367685U)
      {
        if ((int) stringHash != -574180418)
        {
          if ((int) stringHash == -562599611 && lowerInvariant == "dark")
            return PokemonType.Dark;
        }
        else if (lowerInvariant == "electric")
          return PokemonType.Electric;
      }
      else if ((int) stringHash != -427058094)
      {
        if ((int) stringHash != -313320937)
        {
          if ((int) stringHash == -133155298 && lowerInvariant == "dragon")
            return PokemonType.Dragon;
        }
        else if (lowerInvariant == "fighting")
          return PokemonType.Fighting;
      }
      else if (lowerInvariant == "normal")
        return PokemonType.Normal;
      throw new Exception("The pokemon type " + typeName + " does not exist");
    }
  }
}

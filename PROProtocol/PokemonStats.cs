using System;
using System.Collections.Generic;

namespace PROProtocol
{
    public class PokemonStats
    {
        public int Health { get; set; }
        public int Attack { get; set; }
        public int Defence { get; set; }
        public int SpAttack { get; set; }
        public int SpDefence { get; set; }
        public int Speed { get; set; }

        public PokemonStats()
        {
        }

        internal PokemonStats(string[] data, int index, int health = -1)
        {
            Attack = Convert.ToInt32(data[index++]);
            Defence = Convert.ToInt32(data[index++]);
            Speed = Convert.ToInt32(data[index++]);
            SpAttack = Convert.ToInt32(data[index++]);
            SpDefence = Convert.ToInt32(data[index++]);
            if (health == -1)
            {
                Health = Convert.ToInt32(data[index]);
            }
            else
            {
                Health = health;
            }
        }

        public int GetStat(StatType stat)
        {
            switch (stat)
            {
                case StatType.Health:
                    return Health;
                case StatType.Attack:
                    return Attack;
                case StatType.Defence:
                    return Defence;
                case StatType.SpAttack:
                    return SpAttack;
                case StatType.SpDefence:
                    return SpDefence;
                case StatType.Speed:
                    return Speed;
            }
            return 0;
        }

        public bool HasOnly(HashSet<StatType> types)
        {
            if ((!types.Contains(StatType.Health) && Health > 0)
                || (!types.Contains(StatType.Attack) && Attack > 0)
                || (!types.Contains(StatType.Defence) && Defence > 0)
                || (!types.Contains(StatType.SpAttack) && SpAttack > 0)
                || (!types.Contains(StatType.SpDefence) && SpDefence > 0)
                || (!types.Contains(StatType.Speed) && Speed > 0))
            {
                return false;
            }
            return true;
        }

        public bool HasOnly(StatType type)
        {
            if ((type != StatType.Health && Health > 0)
                || (type != StatType.Attack && Attack > 0)
                || (type != StatType.Defence && Defence > 0)
                || (type != StatType.SpAttack && SpAttack > 0)
                || (type != StatType.SpDefence && SpDefence > 0)
                || (type != StatType.Speed && Speed > 0))
            {
                return false;
            }
            return true;
        }
    }
}

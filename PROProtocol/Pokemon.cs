using System;
using System.Collections.Generic;
using System.Linq;

namespace PROProtocol
{
    public class Pokemon
    {
        public int Uid { get; private set; }
        public int Id { get; private set; }
        public int Level { get; private set; }
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public PokemonMove[] Moves { get; private set; }
        public int Experience { get; private set; }
        public int BaseExperience { get; private set; }
        public bool IsShiny { get; private set; }
        public string Gender { get; private set; }
        public PokemonStats Stats { get; private set; }
        public PokemonStats IV { get; private set; }
        public PokemonStats EV { get; private set; }

        private string _status;
        public string Status {
            get
            {
                return CurrentHealth == 0 ? "KO" : _status;
            }
            set
            {
                _status = value;
            }
        }

        public string Name
        {
            get { return PokemonNamesManager.Instance.Names[Id]; }
        }

        public string Health
        {
            get { return CurrentHealth + "/" + MaxHealth; }
        }

        public int RemainingExperience
        {
            get
            {
                if (Level == 100)
                {
                    return 0;
                }
                double num = Math.Pow(210.0 / (105.0 - Level), 4.0);
                double num2 = ((int)((num + Math.Pow(Level, 3.0)) * (BaseExperience / 20.0)));
                return (int)(num2 - Experience);
            }
        }

        internal Pokemon(string[] data)
        {
            Uid = Convert.ToInt32(data[0]);
            Id = Convert.ToInt32(data[1]);
            Level = Convert.ToInt32(data[3]);
            MaxHealth = Convert.ToInt32(data[5]);
            CurrentHealth = Convert.ToInt32(data[6]);

            Moves = new PokemonMove[4];
            Moves[0] = new PokemonMove(1, Convert.ToInt32(data[7]), Convert.ToInt32(data[11]), Convert.ToInt32(data[15]));
            Moves[1] = new PokemonMove(2, Convert.ToInt32(data[8]), Convert.ToInt32(data[12]), Convert.ToInt32(data[16]));
            Moves[2] = new PokemonMove(3, Convert.ToInt32(data[9]), Convert.ToInt32(data[13]), Convert.ToInt32(data[17]));
            Moves[3] = new PokemonMove(4, Convert.ToInt32(data[10]), Convert.ToInt32(data[14]), Convert.ToInt32(data[18]));

            Experience = Convert.ToInt32(data[19]);
            BaseExperience = Convert.ToInt32(data[28]);
            IsShiny = (data[20] == "1");
            Status = data[21];
            Gender = data[22];

            Stats = new PokemonStats(data, 23, MaxHealth);
            IV = new PokemonStats(data, 30);
            EV = new PokemonStats(data, 41);
        }

        public void UpdateHealth(int max, int current)
        {
            MaxHealth = max;
            CurrentHealth = current;
        }

        public string[] GetMoveNames()
        {
            return Moves.Where(m => m.Id > 0).Select(m => m.Name).ToArray();
        }
    }
}

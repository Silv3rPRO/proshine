using System;
using System.Linq;

namespace PROProtocol
{
    public class Pokemon
    {
        private string _status;

        internal Pokemon(string[] data)
        {
            Uid = Convert.ToInt32(data[0]);
            Id = Convert.ToInt32(data[1]);
            DatabaseId = Convert.ToInt32(data[2]);
            MaxHealth = Convert.ToInt32(data[5]);
            CurrentHealth = Convert.ToInt32(data[6]);

            Moves = new PokemonMove[4];
            Moves[0] = new PokemonMove(1, Convert.ToInt32(data[7]), Convert.ToInt32(data[11]),
                Convert.ToInt32(data[15]));
            Moves[1] = new PokemonMove(2, Convert.ToInt32(data[8]), Convert.ToInt32(data[12]),
                Convert.ToInt32(data[16]));
            Moves[2] = new PokemonMove(3, Convert.ToInt32(data[9]), Convert.ToInt32(data[13]),
                Convert.ToInt32(data[17]));
            Moves[3] = new PokemonMove(4, Convert.ToInt32(data[10]), Convert.ToInt32(data[14]),
                Convert.ToInt32(data[18]));

            Experience = new PokemonExperience(Convert.ToInt32(data[3]), Convert.ToInt32(data[28]),
                Convert.ToInt32(data[19]));
            IsShiny = data[20] == "1";
            Status = data[21];
            Gender = data[22];

            OriginalTrainer = data[29];
            Region = (Region) Convert.ToInt32(data[47]);
            Form = Convert.ToInt32(data[48]);

            Nature = new PokemonNature(Convert.ToInt32(data[36]));
            Ability = new PokemonAbility(Convert.ToInt32(data[38]));
            Happiness = Convert.ToInt32(data[37]);
            ItemHeld = data[40];

            Stats = new PokemonStats(data, 23, MaxHealth);
            Iv = new PokemonStats(data, 30);
            Ev = new PokemonStats(data, 41);

            Type1 = TypesManager.Instance.Type1[Id];
            Type2 = TypesManager.Instance.Type2[Id];
        }

        public int Uid { get; }
        public int Id { get; }
        public int DatabaseId { get; }

        public int Level => Experience.CurrentLevel;

        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public PokemonMove[] Moves { get; }
        public PokemonExperience Experience { get; }
        public bool IsShiny { get; }
        public string Gender { get; }
        public PokemonNature Nature { get; }
        public PokemonAbility Ability { get; }
        public int Happiness { get; }
        public string ItemHeld { get; }
        public PokemonStats Stats { get; }
        public PokemonStats Iv { get; }
        public PokemonStats Ev { get; }
        public string OriginalTrainer { get; }
        public Region Region { get; }
        public int Form { get; }
        public PokemonType Type1 { get; }
        public PokemonType Type2 { get; }

        public string Types
        {
            get
            {
                if (Type2 == PokemonType.None)
                    return Type1.ToString();
                return Type1 + "/" + Type2;
            }
        }

        public string Status
        {
            get => CurrentHealth == 0 ? "KO" : _status;
            set => _status = value;
        }

        public string Name => PokemonNamesManager.Instance.Names[Id];

        public string Health => CurrentHealth + "/" + MaxHealth;

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
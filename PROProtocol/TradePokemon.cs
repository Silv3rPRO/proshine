using System;

namespace PROProtocol
{
    public class TradePokemon
    {
        internal TradePokemon(string[] data)
        {
            Id = Convert.ToInt32(data[0]);
            Uid = -1;
            DatabaseId = Convert.ToInt32(data[34]);
            CurrentHealth = Convert.ToInt32(data[14]);
            Happiness = 75;
            Experience = new PokemonExperience(Convert.ToInt32(data[1]), Convert.ToInt32(data[19]),
                Convert.ToInt32(data[18]));
            Moves = new PokemonMove[4];

            //TODO: identify maxPoints
            var maxPp = 0;
            Moves[0] = new PokemonMove(1, Convert.ToInt32(data[21]), maxPp, maxPp);
            Moves[1] = new PokemonMove(2, Convert.ToInt32(data[22]), maxPp, maxPp);
            Moves[2] = new PokemonMove(3, Convert.ToInt32(data[23]), maxPp, maxPp);
            Moves[3] = new PokemonMove(4, Convert.ToInt32(data[24]), maxPp, maxPp);

            Nature = new PokemonNature(Convert.ToInt32(data[17]));
            Ability = new PokemonAbility(Convert.ToInt32(data[16]));

            IsShiny = data[2] == "1";
            ItemHeld = data[26];
            OriginalTrainer = data[20];
            Gender = data[15];
            Form = Convert.ToInt32(data[33]);
            Stats = new PokemonStats(data, 9);
            Iv = new PokemonStats(data, 4);
            Ev = new PokemonStats(data, 27);
        }

        public int Id { get; }
        public int Uid { get; }
        public int DatabaseId { get; }

        public int Level => Experience.CurrentLevel;

        public PokemonExperience Experience { get; }
        public bool IsShiny { get; }
        public int CurrentHealth { get; }
        public int Happiness { get; }
        public string OriginalTrainer { get; }
        public string ItemHeld { get; }
        public string Gender { get; }
        public PokemonStats Iv { get; }
        public PokemonStats Ev { get; }
        public PokemonStats Stats { get; }
        public PokemonMove[] Moves { get; }
        public PokemonNature Nature { get; }
        public PokemonAbility Ability { get; }
        public int Form { get; }

        public string Name => PokemonNamesManager.Instance.Names[Id];

        public string Health => CurrentHealth + "/" + CurrentHealth;
    }
}
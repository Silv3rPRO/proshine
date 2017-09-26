using System.Collections.Generic;

namespace PROProtocol
{
    public class PokedexPokemon
    {
        public List<string> Area = new List<string>();

        public string Type1 = string.Empty;

        public string Type2 = string.Empty;

        internal PokedexPokemon(int id, int pokeid)
        {
            Id = id;
            Name = PokemonNamesManager.Instance.Names[pokeid];
            Pokeid2 = pokeid;
        }

        // 1: Seen | 2 : Captured | 3 : Obtained by evolving
        public int Id { get; set; }

        public string Name { get; set; }
        public int Pokeid2 { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public bool IsCaught()
        {
            return Id == 2 || Id == 3;
        }

        public bool IsSeen()
        {
            return Id == 1;
        }

        public bool IsEvolved()
        {
            return Id == 3;
        }
    }
}
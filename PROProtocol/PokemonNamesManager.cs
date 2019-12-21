using System.Collections.Generic;

namespace PROProtocol
{
    public class PokemonNamesManager
    {
        private static PokemonNamesManager _instance;

        public static PokemonNamesManager Instance
        {
            get
            {
                return _instance ?? (_instance = new PokemonNamesManager());
            }
        }

        public Dictionary<int, string> Names { get; }

        public PokemonNamesManager()
        {
            Names = new Dictionary<int, string>();
            foreach (var entry in Pokedex.Instance.Entries)
            {
                if (!Names.ContainsKey(entry.ID))
                    Names.Add(entry.ID, entry.Name);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROProtocol
{
    public class Pokedex
    {
        public class PokedexEntry
        {
            public int ID;
            public string Name;
            public PokemonType[] Types;
            public int HP;
            public int ATK;
            public int DEF;
            public int SPATK;
            public int SPDEF;
            public int SPD;
            public string Species;
            public float Height;
            public float Weight;
            public float RatioM;
            public string[] Abilities;
            public int EVATK;
            public int EVDEF;
            public int EVSPD;
            public int EVSPDEF;
            public int EVSPATK;
            public int EVHP;
            public string Desc;
        }

        public PokedexEntry[] Entries { get; }

        private static Pokedex _instance;

        public static Pokedex Instance
        {
            get
            {
                return _instance ?? (_instance = new Pokedex());
            }
        }

        private Pokedex()
        {
            Entries = ResourcesUtil.GetResource<PokedexEntry[]>("PokemonData.json");
        }
    }
}

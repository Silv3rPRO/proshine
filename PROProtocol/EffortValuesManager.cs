using System.Collections.Generic;

namespace PROProtocol
{
    public class EffortValuesManager
    {
        private static EffortValuesManager _instance;

        public static EffortValuesManager Instance
        {
            get
            {
                return _instance ?? (_instance = new EffortValuesManager());
            }
        }

        public Dictionary<int, PokemonStats> BattleValues { get; }

        private EffortValuesManager()
        {
            BattleValues = new Dictionary<int, PokemonStats>();

            foreach (var entry in Pokedex.Instance.Entries)
            {
                Add(entry.ID, entry.EVHP, entry.EVATK, entry.EVDEF, entry.EVSPATK, entry.EVSPDEF, entry.EVSPD);
            }
        }

        private void Add(int id, int hp, int atk, int def, int spatk, int spdef, int speed)
        {
            if (!BattleValues.ContainsKey(id))
            {
                BattleValues.Add(id, new PokemonStats { Health = hp, Attack = atk, Defence = def, SpAttack = spatk, SpDefence = spdef, Speed = speed });
            }
        }
    }
}

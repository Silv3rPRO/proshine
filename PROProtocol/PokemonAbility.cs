namespace PROProtocol
{
    public class PokemonAbility
    {
        public int Id { get; private set; }
        public string Name
        {
            get
            {
                if (Id < 0 || Id >= Abilities.Length)
                {
                    return null;
                }
                return Abilities[Id];
            }
        }

        public PokemonAbility(int id)
        {
            Id = id;
        }

        public static readonly string[] Abilities = {
            "None",
            "Stench",
            "Drizzle",
            "Speed Boost",
            "Battle Armor",
            "Sturdy",
            "Damp",
            "Limber",
            "Sand Veil",
            "Static",
            "Volt Absorb",
            "Water Absorb",
            "Oblivious",
            "Cloud Nine",
            "Compound Eyes",
            "Insomnia",
            "Color Change",
            "Immunity",
            "Flash Fire",
            "Shield Dust",
            "Own Tempo",
            "Suction Cups",
            "Intimidate",
            "Shadow Tag",
            "Rough Skin",
            "Wonder Guard",
            "Levitate",
            "Effect Spore",
            "Synchronize",
            "Clear Body",
            "Natural Cure",
            "Lightning Rod",
            "Serene Grace",
            "Swift Swim",
            "Chlorophyll",
            "Illuminate",
            "Trace",
            "Huge Power",
            "Poison Point",
            "Inner Focus",
            "Magma Armor",
            "Water Veil",
            "Magnet Pull",
            "Soundproof",
            "Rain Dish",
            "Sand Stream",
            "Pressure",
            "Thick Fat",
            "Early Bird",
            "Flame Body",
            "Run Away",
            "Keen Eye",
            "Hyper Cutter",
            "Pickup",
            "Truant",
            "Hustle",
            "Cute Charm",
            "Plus",
            "Minus",
            "Forecast",
            "Sticky Hold",
            "Shed Skin",
            "Guts",
            "Marvel Scale",
            "Liquid Ooze",
            "Overgrow",
            "Blaze",
            "Torrent",
            "Swarm",
            "Rock Head",
            "Drought",
            "Arena Trap",
            "Vital Spirit",
            "White Smoke",
            "Pure Power",
            "Shell Armor",
            "Air Lock",
            "Tangled Feet",
            "Motor Drive",
            "Rivalry",
            "Steadfast",
            "Snow Cloak",
            "Gluttony",
            "Anger Point",
            "Unburden",
            "Heatproof",
            "Simple",
            "Dry Skin",
            "Download",
            "Iron Fist",
            "Poison Heal",
            "AdaptAbility",
            "Skill Link",
            "Hydration",
            "Solar Power",
            "Quick Feet",
            "Normalize",
            "Sniper",
            "Magic Guard",
            "No Guard",
            "Delta Stream",
            "Unnerve"
        };
    }
}

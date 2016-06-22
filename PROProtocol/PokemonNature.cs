namespace PROProtocol
{
    public class PokemonNature
    {
        public int Id { get; private set; }
        public string Name
        {
            get
            {
                if (Id < 0 || Id >= Natures.Length)
                {
                    return null;
                }
                return Natures[Id];
            }
        }

        public PokemonNature(int id)
        {
            Id = id;
        }

        public static readonly string[] Natures = {
            "Hardy",
            "Lonely",
            "Brave",
            "Adamant",
            "Naughty",
            "Bold",
            "Docile",
            "Relaxed",
            "Impish",
            "Lax",
            "Timid",
            "Hasty",
            "Serious",
            "Jolly",
            "Naive",
            "Modest",
            "Mild",
            "Quiet",
            "Bashful",
            "Rash",
            "Calm",
            "Gentle",
            "Sassy",
            "Careful",
            "Quirky"
        };
    }
}

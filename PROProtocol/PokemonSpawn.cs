namespace PROProtocol
{
    public class PokemonSpawn
    {
        internal PokemonSpawn(int id, bool captured, bool surf, bool fish, bool hitem, bool msonly)
        {
            Name = PokemonNamesManager.Instance.Names[id];
            Surf = surf;
            Fish = fish;
            Hitem = hitem;
            Msonly = msonly;
            Captured = captured;
        }

        public string Name { get; set; }

        public bool Fish { get; set; }

        public bool Surf { get; set; }

        public bool Hitem { get; set; }

        public bool Msonly { get; set; }

        public bool Captured { get; set; }
    }
}
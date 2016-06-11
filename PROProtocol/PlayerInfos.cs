using System;

namespace PROProtocol
{
    public class PlayerInfos
    {
        public DateTime Expiration { get; set; }
        public DateTime Added { get; private set; }
        public DateTime Updated { get; set; }

        public string Name { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public char Direction { get; set; }
        public string Skin { get; set; }
        public bool IsAfk { get; set; }
        public bool IsInBattle { get; set; }
        public int PokemonPetId { get; set; }
        public bool IsPokemonPetShiny { get; set; }
        public bool IsMember { get; set; }
        public bool IsOnground { get; set; }
        public int GuildId { get; set; }
        public int PetForm { get; set; } // ???

        public PlayerInfos(DateTime expiration)
        {
            Expiration = expiration;
            Added = DateTime.UtcNow;
        }

        public bool IsExpired()
        {
            return DateTime.UtcNow > Expiration;
        }
    }
}

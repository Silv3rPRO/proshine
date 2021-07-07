using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace PROProtocol
{
    [JsonConverter(typeof(PokemonTypeConverter))]
    public enum PokemonType
    {
        Normal,
        Fighting,
        Flying,
        Poison,
        Ground,
        Rock,
        Bug,
        Ghost,
        Steel,
        Fire,
        Water,
        Grass,
        Electric,
        Psychic,
        Ice,
        Dragon,
        Dark,
        Fairy,
        Bird, // https://bulbapedia.bulbagarden.net/wiki/List_of_glitch_types#Bird
        None
    }

    class PokemonTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(string);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            return PokemonTypeExtensions.FromName(value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }

    public static class PokemonTypeExtensions
    {
        public static PokemonType FromName(string typeName)
        {
            switch (typeName.ToLowerInvariant())
            {
                case "":
                    return PokemonType.None;
                case "normal":
                    return PokemonType.Normal;
                case "fighting":
                    return PokemonType.Fighting;
                case "flying":
                    return PokemonType.Flying;
                case "poison":
                    return PokemonType.Poison;
                case "ground":
                    return PokemonType.Ground;
                case "rock":
                    return PokemonType.Rock;
                case "bug":
                    return PokemonType.Bug;
                case "ghost":
                    return PokemonType.Ghost;
                case "steel":
                    return PokemonType.Steel;
                case "fire":
                    return PokemonType.Fire;
                case "water":
                    return PokemonType.Water;
                case "grass":
                    return PokemonType.Grass;
                case "electric":
                    return PokemonType.Electric;
                case "psychic":
                    return PokemonType.Psychic;
                case "ice":
                    return PokemonType.Ice;
                case "dragon":
                    return PokemonType.Dragon;
                case "dark":
                    return PokemonType.Dark;
                case "fairy":
                    return PokemonType.Fairy;
                case "bird":
                    return PokemonType.Bird;
                default:
                    throw new System.Exception("The pokemon type " + typeName + " does not exist");
            }
        }
    }
}

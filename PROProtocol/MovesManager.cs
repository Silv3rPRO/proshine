using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PROProtocol
{
    public class MovesManager
    {
        public class MoveData
        {
            public string Name;
            public int Power;
            public int Accuracy;
            public string Type;
            [JsonConverter(typeof(MoveStatusConverter))]
            public bool Status;
            public DamageType DamageType;
            public string Desc;

            class MoveStatusConverter : JsonConverter
            {
                public override bool CanConvert(Type t) => t == typeof(string);

                public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
                {
                    if (reader.TokenType == JsonToken.Null) return null;
                    var value = serializer.Deserialize<string>(reader);
                    // value can be either "y" or "n"
                    return value != "y";
                }

                public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
            }
        }

        [JsonConverter(typeof(DamageTypeConverter))]
        public enum DamageType
        {
            Physical,
            Special,
            Z,
        }

        class DamageTypeConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(string);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                switch (value)
                {
                case "p":
                    return DamageType.Physical;
                case "s":
                    return DamageType.Special;
                case "z":
                    return DamageType.Z;
                default:
                    Console.Error.WriteLine($"Can't unmarshal DamageType '{value}'");
                    // better than crashing...
                    return DamageType.Physical;
                }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
        }

        private static MovesManager _instance;

        public static MovesManager Instance
        {
            get
            {
                return _instance ?? (_instance = new MovesManager());
            }
        }

        public MoveData[] Moves;
        public string[] MoveNames;
        private Dictionary<string, MoveData> _namesToMoves;
        private Dictionary<string, int> _namesToIds;
        private MoveData[] _idsToMoves;

        private MovesManager()
        {
            LoadMoves();

            _namesToMoves = new Dictionary<string, MoveData>();
            for (int i = 0; i < Moves.Length; i++)
            {
                if (Moves[i].Name != null && !_namesToMoves.ContainsKey(Moves[i].Name.ToLowerInvariant()))
                {
                    _namesToMoves.Add(Moves[i].Name.ToLowerInvariant(), Moves[i]);
                }
            }

            _idsToMoves = new MoveData[Moves.Length];
            _namesToIds = new Dictionary<string, int>();
            for (int i = 0; i < Moves.Length; i++)
            {
                string lowerName = MoveNames[i].ToLowerInvariant();
                if (_namesToMoves.ContainsKey(lowerName))
                {
                    _idsToMoves[i] = _namesToMoves[lowerName];
                    if (!_namesToIds.ContainsKey(lowerName))
                    {
                        _namesToIds.Add(lowerName, i);
                    }
                }
            }
        }

        public int GetMoveId(string moveName)
        {
            moveName = moveName.ToLowerInvariant();
            if (_namesToIds.ContainsKey(moveName))
            {
                return _namesToIds[moveName];
            }
            return -1;
        }

        public MoveData GetMoveData(string moveName)
        {
            moveName = moveName.ToLowerInvariant();
            if (_namesToMoves.ContainsKey(moveName))
            {
                return _namesToMoves[moveName];
            }
            return null;
        }

        public MoveData GetMoveData(int moveId)
        {
            if (moveId > 0 && moveId < Moves.Length)
            {
                return _idsToMoves[moveId];
            }
            return null;
        }

        public string GetTrueName(string lowerName)
        {
            int id = GetMoveId(lowerName);
            if (id != -1)
            {
                return MoveNames[id];
            }
            return null;
        }

        private void LoadMoves()
        {
            var moves = ResourcesUtil.GetResource<Dictionary<int, MoveData>>("Moves.json");

            Moves = moves.Values.ToArray();

            LoadMoveNames();
        }

        private void LoadMoveNames()
        {
            MoveNames = Moves.Select(m => m.Name ?? "").ToArray();
        }
    }
}

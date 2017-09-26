using System.Collections.Generic;

namespace PROProtocol
{
    public class Npc
    {
        private static readonly Dictionary<int, string> _typeDescriptions = new Dictionary<int, string>
        {
            {1, "pokemon"},
            {2, "camper"},
            {3, "picnicker"},
            {4, "lass"},
            {6, "youngster m"},
            {7, "youngster f"},
            {8, "old man"},
            {9, "old lady"},
            {10, "interactive environment"},
            {11, "item"},
            {17, "fisherman"},
            {18, "scientist"},
            {19, "biker"},
            {25, "lass"},
            {33, "black belt"},
            {34, "sailor"},
            {42, "cherry tree"},
            {43, "pecha tree"},
            {44, "oran tree"},
            {45, "leppa tree"},
            {49, "chesto tree"},
            {50, "rawst tree"},
            {51, "aspear tree"},
            {52, "persim tree"},
            {55, "pomeg tree"},
            {56, "kelpsy tree"},
            {57, "qualot tree"},
            {58, "hondew tree"},
            {59, "grepa tree"},
            {60, "tomato tree"},
            {61, "sitrus tree"},
            {62, "lum tree"},
            {63, "abandoned pokemon"},
            {69, "hiker"},
            {70, "road digspot"},
            {71, "cave digspot"},
            {74, "chuck"},
            {101, "headbuttable tree"},
            {111, "bill"},
            {119, "pokestop"}
        };

        private readonly string _path;

        public Npc(int id, string name, bool isBattler, int type, int x, int y, Direction viewDirection, int losLength,
            string path)
        {
            Id = id;
            Name = name;
            IsBattler = isBattler;
            CanBattle = isBattler;
            Type = type;
            PositionX = x;
            PositionY = y;
            LosLength = losLength;
            ViewDirection = viewDirection;
            _path = path;
        }

        public int Id { get; }
        public string Name { get; }
        public bool IsBattler { get; }
        public bool CanBattle { get; set; }
        public int Type { get; }
        public int PositionX { get; }
        public int PositionY { get; }
        public Direction ViewDirection { get; }
        public int LosLength { get; }

        public string TypeDescription => (_typeDescriptions.ContainsKey(Type) ? _typeDescriptions[Type] + " " : "") +
                                         "(" +
                                         Type + ")";

        public bool IsMoving => _path.Length > 0;

        public Npc Clone()
        {
            return new Npc(Id, Name, IsBattler, Type, PositionX, PositionY, ViewDirection, LosLength, _path);
        }
    }
}
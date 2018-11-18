using System;
using System.Collections.Generic;

namespace PROProtocol
{
    public class Npc
    {
        private static readonly Dictionary<int, string> TypeDescriptions = new Dictionary<int, string>
        {
            {   1, "pokemon"},
            {   2, "camper" },
            {   3, "picnicker"},
            {   4, "lass"},
            {   6, "youngster m" },
            {   7, "youngster f"},
            {   8, "old man"},
            {   9, "old lady"},
            {  10, "interactive environment"},
            {  11, "item" },
            {  17, "fisherman" },
            {  18, "scientist" },
            {  19, "biker"},
            {  25, "lass"},
            {  33, "black belt" },
            {  34, "sailor" },
            {  42, "cherry tree"},
            {  43, "pecha tree"},
            {  44, "oran tree"},
            {  45, "leppa tree"},
            {  49, "chesto tree"},
            {  50, "rawst tree"},
            {  51, "aspear tree"},
            {  52, "persim tree"},
            {  55, "pomeg tree"},
            {  56, "kelpsy tree"},
            {  57, "qualot tree"},
            {  58, "hondew tree"},
            {  59, "grepa tree"},
            {  60, "tomato tree"},
            {  61, "sitrus tree"},
            {  62, "lum tree"},
            {  63, "abandoned pokemon"},
            {  69, "hiker"},
            {  70, "road digspot" },
            {  71, "cave digspot" },
            {  74, "chuck" },
            { 101, "headbuttable tree" },
            { 111, "bill" },
            { 119, "pokestop"},
        };

        private static readonly char[] Movements = new[] { 'U', 'D', 'L', 'R' };
        
        public int Id { get; }
        public string Name { get; }
        public bool IsBattler { get; }
        public bool CanBattle { get; set; }
        public int Type { get; }
        public int PositionX { get; }
        public int PositionY { get; }
        public Direction Direction { get; }
        public int LosLength { get; }
        public string TypeDescription => (TypeDescriptions.ContainsKey(Type) ? TypeDescriptions[Type] + " ":"") + "(" + Type + ")";
        public string Path { get; }
        
        public bool IsMoving => Path.Length > 0 && Path.IndexOfAny(Movements) >= 0;

        public bool CanBlockPlayer => Type != 10;
        
        public Npc(int id, string name, bool isBattler, int type, int x, int y, Direction direction, int losLength, string path)
        {
            Id = id;
            Name = name;
            IsBattler = isBattler;
            CanBattle = isBattler;
            Type = type;
            PositionX = x;
            PositionY = y;
            Direction = direction;
            LosLength = losLength;
            Path = path;
        }

        public Npc Clone()
        {
            return new Npc(Id, Name, IsBattler, Type, PositionX, PositionY, Direction, LosLength, Path);
        }

        public bool IsInLineOfSight(int x, int y)
        {
            if (x != PositionX && y != PositionY) return false;
            int distance = GameClient.DistanceBetween(PositionX, PositionY, x, y);
            if (distance > LosLength) return false;
            switch (Direction)
            {
                case Direction.Up:
                    return x == PositionX && y < PositionY;
                case Direction.Down:
                    return x == PositionX && y > PositionY;
                case Direction.Left:
                    return x < PositionX && y == PositionY;
                case Direction.Right:
                    return x > PositionX && y == PositionY;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

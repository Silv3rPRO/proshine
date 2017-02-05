using System.Collections.Generic;
namespace PROProtocol
{
    public class Npc
    {
        private static Dictionary<int, string> typeDescriptions = new Dictionary<int, string>()
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
        
        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool IsBattler { get; private set; }
        public bool CanBattle { get; set; }
        public int Type { get; private set; }
        public int PositionX { get; private set; }
        public int PositionY { get; private set; }
        public int LosLength { get; private set; }
        public string TypeDescription { get { return (typeDescriptions.ContainsKey(Type) ? typeDescriptions[Type] + " ":"") + "(" + Type.ToString() + ")"; } }
        
        public bool IsMoving { get { return _path.Length > 0; } }

        private string _path;

        public Npc(int id, string name, bool isBattler, int type, int x, int y, int losLength, string path)
        {
            Id = id;
            Name = name;
            IsBattler = isBattler;
            CanBattle = isBattler;
            Type = type;
            PositionX = x;
            PositionY = y;
            LosLength = losLength;
            _path = path;
        }

        public Npc Clone()
        {
            return new Npc(Id, Name, IsBattler, Type, PositionX, PositionY, LosLength, _path);
        }
    }
}

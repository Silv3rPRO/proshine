namespace PROProtocol
{
    public class Npc
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool IsBattler { get; private set; }
        public bool CanBattle { get; set; }
        public int Type { get; private set; }
        public int PositionX { get; private set; }
        public int PositionY { get; private set; }
        public int LosLength { get; private set; }

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

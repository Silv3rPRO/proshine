namespace PROProtocol
{
    public class Npc
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int PositionX { get; private set; }
        public int PositionY { get; private set; }
        public int LosLength { get; private set; }

        public bool IsMoving { get { return _path.Length > 0; } }

        private string _path;

        public Npc(int id, string name, int x, int y, int losLength, string path)
        {
            Id = id;
            Name = name;
            PositionX = x;
            PositionY = y;
            LosLength = losLength;
            _path = path;
        }

        public Npc Clone()
        {
            return new Npc(Id, Name, PositionX, PositionY, LosLength, _path);
        }
    }
}

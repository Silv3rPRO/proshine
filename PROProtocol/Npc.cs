namespace PROProtocol
{
    public class Npc
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool Battler { get; private set; }
        public bool CanBattle { get; private set; }
        public int Num { get; private set; }
        public int PositionX { get; private set; }
        public int PositionY { get; private set; }
        public int LosLength { get; private set; }

        public bool IsMoving { get { return _path.Length > 0; } }

        private string _path;

        public Npc(int id, string name, bool battler, int num, int x, int y, int losLength, string path)
        {
            Id = id;
            Name = name;
            Battler = battler;
            CanBattle = false;
            Num = num;
            PositionX = x;
            PositionY = y;
            LosLength = losLength;
            _path = path;
        }

        public Npc Clone()
        {
            Npc npc = new Npc(Id, Name, Battler , Num, PositionX, PositionY, LosLength, _path);
            npc.setBattle(CanBattle);
            return npc;
        }

        public void setBattle(bool state)
        {
            CanBattle = state;
        }
    }
}

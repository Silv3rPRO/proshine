using System.Globalization;

namespace PROProtocol
{
    public class PokemonMove
    {
        public int Position { get; private set; }
        public int Id { get; private set; }
        public int MaxPoints { get; private set; }
        public int CurrentPoints { get; set; }
        
        private TextInfo ti = CultureInfo.CurrentCulture.TextInfo;

        public MovesManager.MoveData Data
        {
            get { return MovesManager.Instance.GetMoveData(Id); }
        }

        public string Name
        {
            get { return Data?.Name; }
        }
        
        public string Name_PP
        {
            get
            {
                return Name != null ? ti.ToTitleCase(Name) + ": " + CurrentPoints + " / " + MaxPoints : "";
            }
        }

        public PokemonMove(int position, int id, int maxPoints, int currentPoints)
        {
            Position = position;
            Id = id;
            MaxPoints = maxPoints;
            CurrentPoints = currentPoints;
        }
    }
}

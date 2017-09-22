using System.Globalization;

namespace PROProtocol
{
    public class PokemonMove
    {
        private readonly TextInfo _ti = CultureInfo.CurrentCulture.TextInfo;

        public PokemonMove(int position, int id, int maxPoints, int currentPoints)
        {
            Position = position;
            Id = id;
            MaxPoints = maxPoints;
            CurrentPoints = currentPoints;
        }

        public int Position { get; }
        public int Id { get; }
        public int MaxPoints { get; }
        public int CurrentPoints { get; set; }

        public MovesManager.MoveData Data => MovesManager.Instance.GetMoveData(Id);

        public string Name => Data?.Name != null ? _ti.ToTitleCase(Data?.Name) : Data?.Name;

        public string Pp => Name != null ? CurrentPoints + " / " + MaxPoints : "";
    }
}
namespace PROProtocol
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    public static class DirectionExtensions
    {
        public static string AsChar(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return "u";
                case Direction.Down:
                    return "d";
                case Direction.Left:
                    return "l";
                case Direction.Right:
                    return "r";
            }
            return null;
        }

        public static Direction GetOpposite(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Up;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                default:
                    return Direction.Left;
            }
        }

        public static void ApplyToCoordinates(this Direction direction, ref int x, ref int y)
        {
            switch (direction)
            {
                case Direction.Up:
                    y--;
                    break;
                case Direction.Down:
                    y++;
                    break;
                case Direction.Left:
                    x--;
                    break;
                case Direction.Right:
                    x++;
                    break;
            }
        }

        public static Direction FromChar(char c)
        {
            switch (c)
            {
                case 'u':
                    return Direction.Up;
                case 'd':
                    return Direction.Down;
                case 'l':
                    return Direction.Left;
                case 'r':
                    return Direction.Right;
            }
            throw new System.Exception("The direction '" + c + "' does not exist");
        }

        public static Direction FromVector(int vx, int vy)
        {
            if (vy < 0) return Direction.Up;
            if (vy > 0) return Direction.Down;
            if (vx < 0) return Direction.Left;
            if (vx > 0) return Direction.Right;
            throw new System.Exception("The vector (" + vx + "," + vy + ") cannot be translated into a direction");
        }
    }
}

using System;
using System.Drawing;

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

        /// <summary>
        ///     Generates the next point in moving direction.
        /// </summary>
        /// <param name="direction">The moving direction.</param>
        /// <param name="origin">The starting point.</param>
        /// <returns>
        ///     New point after movement in <paramref name="direction" /> was
        ///     applied.
        /// </returns>
        public static Point ApplyToCoordinates(this Direction direction, Point origin)
        {
            int x = origin.X, y = origin.Y;
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

            return new Point(x, y);
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
            throw new Exception("The direction '" + c + "' does not exist");
        }

        /// <summary>
        ///     Converts an integer into an actual direction. Needed for NPC view
        ///     directions, but could also be used elsewhere.
        /// </summary>
        /// <param name="direction">
        ///     Integer representing an npc's view direction.
        /// </param>
        /// <returns>
        ///     The converted direction.
        /// </returns>
        public static Direction FromInt(int direction)
        {
            switch (direction)
            {
                case 0:
                    return Direction.Up;

                case 1:
                    return Direction.Right;

                case 2:
                    return Direction.Down;

                case 3:
                    return Direction.Left;
            }
            throw new Exception("The direction '" + direction + "' does not exist");
        }
    }
}
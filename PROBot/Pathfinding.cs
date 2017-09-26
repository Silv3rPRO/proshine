using PROProtocol;
using System.Collections.Generic;

namespace PROBot
{
    public class Pathfinding
    {
        private static readonly Direction[] _directions =
            {Direction.Up, Direction.Down, Direction.Left, Direction.Right};

        private readonly GameClient _client;
        private readonly bool _hasSurfAbility;

        public Pathfinding(GameClient client)
        {
            _client = client;
            _hasSurfAbility = client.HasSurfAbility();
        }

        public bool MoveTo(int destinationX, int destinationY, int requiredDistance = 0)
        {
            if (destinationX == _client.PlayerX && destinationY == _client.PlayerY)
                return true;

            var node = FindPath(_client.PlayerX, _client.PlayerY, _client.IsOnGround, _client.IsSurfing, destinationX,
                destinationY, requiredDistance);

            if (node != null)
            {
                var directions = new Stack<Direction>();
                while (node.Parent != null)
                {
                    if (!node.Parent.IsSurfing && node.IsSurfing)
                    {
                        directions.Clear();
                        _client.UseSurfAfterMovement();
                    }
                    else
                    {
                        directions.Push(node.FromDirection);
                    }
                    node = node.Parent;
                }

                while (directions.Count > 0)
                    _client.Move(directions.Pop());
                return true;
            }
            return false;
        }

        public bool MoveToSameCell()
        {
            foreach (var direction in _directions)
            {
                var destinationX = _client.PlayerX;
                var destinationY = _client.PlayerY;

                direction.ApplyToCoordinates(ref destinationX, ref destinationY);

                var result = _client.Map.CanMove(direction, destinationX, destinationY, _client.IsOnGround,
                    _client.IsSurfing, _client.CanUseCut, _client.CanUseSmashRock);
                if (result == Map.MoveResult.Success || result == Map.MoveResult.OnGround ||
                    result == Map.MoveResult.NoLongerOnGround)
                {
                    _client.Move(direction);
                    _client.Move(direction.GetOpposite());
                    return true;
                }
            }
            return false;
        }

        private Node FindPath(int fromX, int fromY, bool isOnGround, bool isSurfing, int toX, int toY,
            int requiredDistance)
        {
            var openList = new Dictionary<uint, Node>();
            var closedList = new HashSet<uint>();
            var start = new Node(fromX, fromY, isOnGround, isSurfing);
            openList.Add(start.Hash, start);

            while (openList.Count > 0)
            {
                var current = GetBestNode(openList.Values);
                var distance = GameClient.DistanceBetween(current.X, current.Y, toX, toY);
                if (distance == 0)
                    return current;
                if (distance <= requiredDistance)
                    if (_client.Map.CanInteract(current.X, current.Y, toX, toY))
                        return current;

                openList.Remove(current.Hash);
                closedList.Add(current.Hash);

                var neighbors = GetNeighbors(current);
                foreach (var node in neighbors)
                {
                    if (closedList.Contains(node.Hash))
                        continue;
                    if (_client.Map.HasLink(node.X, node.Y) && node.X != toX && node.Y != toY)
                        continue;

                    node.Parent = current;
                    node.Distance = current.Distance + 1;
                    node.Score = node.Distance;

                    node.DirectionChangeCount = current.DirectionChangeCount;
                    if (node.FromDirection != current.FromDirection)
                        node.DirectionChangeCount += 1;
                    node.Score += node.DirectionChangeCount / 4;
                    if (node.IsSurfing && current.IsSurfing == false)
                        node.Score += 30;

                    if (!openList.ContainsKey(node.Hash))
                    {
                        openList.Add(node.Hash, node);
                    }
                    else if (openList[node.Hash].Score > node.Score)
                    {
                        openList.Remove(node.Hash);
                        openList.Add(node.Hash, node);
                    }
                }
            }
            return null;
        }

        private List<Node> GetNeighbors(Node node)
        {
            var neighbors = new List<Node>();

            foreach (var direction in _directions)
            {
                var destinationX = node.X;
                var destinationY = node.Y;
                var isOnGround = node.IsOnGround;
                var isSurfing = node.IsSurfing;

                direction.ApplyToCoordinates(ref destinationX, ref destinationY);

                var result = _client.Map.CanMove(direction, destinationX, destinationY, isOnGround,
                    isSurfing, _client.CanUseCut, _client.CanUseSmashRock);
                if (_client.Map.ApplyMovement(direction, result, ref destinationX, ref destinationY, ref isOnGround,
                    ref isSurfing))
                {
                    if (result == Map.MoveResult.Icing)
                        _client.Map.ApplyCompleteIceMovement(direction, ref destinationX, ref destinationY,
                            ref isOnGround);
                    else if (result == Map.MoveResult.Sliding)
                        _client.Map.ApplyCompleteSliderMovement(ref destinationX, ref destinationY, ref isOnGround);
                    neighbors.Add(new Node(destinationX, destinationY, isOnGround, isSurfing, direction));
                }
            }

            if (!node.IsSurfing && _hasSurfAbility && _client.Map.CanSurf(node.X, node.Y, node.IsOnGround))
                neighbors.Add(new Node(node.X, node.Y, node.IsOnGround, true));

            return neighbors;
        }

        private Node GetBestNode(IEnumerable<Node> nodes)
        {
            var bestNodes = new List<Node>();
            var bestScore = int.MaxValue;
            foreach (var node in nodes)
            {
                if (node.Score < bestScore)
                {
                    bestNodes.Clear();
                    bestScore = node.Score;
                }
                if (node.Score == bestScore)
                    bestNodes.Add(node);
            }
            return bestNodes[_client.Rand.Next(bestNodes.Count)];
        }

        private class Node
        {
            public int DirectionChangeCount;
            public int Distance;

            public readonly Direction FromDirection;
            public readonly bool IsOnGround;
            public readonly bool IsSurfing;
            public Node Parent;
            public int Score;
            public readonly int X;
            public readonly int Y;

            public Node(int x, int y, bool isOnGround, bool isSurfing)
            {
                X = x;
                Y = y;
                IsOnGround = isOnGround;
                IsSurfing = isSurfing;
            }

            public Node(int x, int y, bool isOnGround, bool isSurfing, Direction direction)
            {
                X = x;
                Y = y;
                IsOnGround = isOnGround;
                IsSurfing = isSurfing;
                FromDirection = direction;
            }

            public uint Hash => (uint)X * 0x7FFFU + (uint)Y + (IsSurfing ? 0x80000000U : 0U) +
                                (IsOnGround ? 0x40000000U : 0U);
        }
    }
}
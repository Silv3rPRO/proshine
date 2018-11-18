using PROProtocol;
using System;
using System.Collections.Generic;

namespace PROBot
{
    public class Pathfinding
    {
        private static Direction[] _directions = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        private GameClient _client;
        private bool _hasSurfAbility;

        private class Node
        {
            public int X;
            public int Y;
            public bool IsOnGround;
            public bool IsSurfing;
            public int Distance;
            public int Score;
            public Node Parent;

            public Direction FromDirection;
            public int DirectionChangeCount;

            public uint Hash
            {
                get { return (uint)X * 0x7FFFU + (uint)Y + (IsSurfing ? 0x80000000U : 0U) + (IsOnGround ? 0x40000000U : 0U); }
            }

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
        }

        public Pathfinding(GameClient client)
        {
            _client = client;
            _hasSurfAbility = client.HasSurfAbility();
        }

        public bool MoveTo(int destinationX, int destinationY, int requiredDistance = 0)
        {
            if (destinationX == _client.PlayerX && destinationY == _client.PlayerY)
            {
                return true;
            }

            Node node = FindPath(_client.PlayerX, _client.PlayerY, _client.IsOnGround, _client.IsSurfing, destinationX, destinationY, requiredDistance);

            if (node != null)
            {
                Stack<Direction> directions = new Stack<Direction>();
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
                {
                    _client.Move(directions.Pop());
                }
                return true;
            }
            return false;
        }

        public bool MoveToSameCell()
        {
            foreach (Direction direction in _directions)
            {
                int destinationX = _client.PlayerX;
                int destinationY = _client.PlayerY;

                direction.ApplyToCoordinates(ref destinationX, ref destinationY);

                Map.MoveResult result = _client.Map.CanMove(direction, destinationX, destinationY, _client.IsOnGround, _client.IsSurfing, _client.CanUseCut, _client.CanUseSmashRock);
                if (result == Map.MoveResult.Success || result == Map.MoveResult.OnGround || result == Map.MoveResult.NoLongerOnGround)
                {
                    _client.Move(direction);
                    _client.Move(direction.GetOpposite());
                    return true;
                }
            }
            return false;
        }

        private Node FindPath(int fromX, int fromY, bool isOnGround, bool isSurfing, int toX, int toY, int requiredDistance)
        {
            Dictionary<uint, Node> openList = new Dictionary<uint, Node>();
            HashSet<uint> closedList = new HashSet<uint>();
            Node start = new Node(fromX, fromY, isOnGround, isSurfing);
            openList.Add(start.Hash, start);

            while (openList.Count > 0)
            {
                Node current = GetBestNode(openList.Values);
                int distance = GameClient.DistanceBetween(current.X, current.Y, toX, toY);
                if (distance == 0)
                {
                    return current;
                }
                else if (distance <= requiredDistance)
                {
                    if (_client.Map.CanInteract(current.X, current.Y, toX, toY))
                    {
                        return current;
                    }
                }

                openList.Remove(current.Hash);
                closedList.Add(current.Hash);

                List<Node> neighbors = GetNeighbors(current);
                foreach (Node node in neighbors)
                {
                    if (closedList.Contains(node.Hash))
                        continue;
                    if (_client.Map.HasLink(node.X, node.Y) && (node.X != toX || node.Y != toY))
                        continue;

                    node.Parent = current;
                    node.Distance = current.Distance + 1;
                    node.Score = node.Distance;

                    node.DirectionChangeCount = current.DirectionChangeCount;
                    if (node.FromDirection != current.FromDirection)
                    {
                        node.DirectionChangeCount += 1;
                    }
                    node.Score += node.DirectionChangeCount / 4;
                    if (node.IsSurfing == true && current.IsSurfing == false)
                    {
                        node.Score += 30;
                    }

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
            List<Node> neighbors = new List<Node>();
            
            foreach (Direction direction in _directions)
            {
                int destinationX = node.X;
                int destinationY = node.Y;
                bool isOnGround = node.IsOnGround;
                bool isSurfing = node.IsSurfing;

                direction.ApplyToCoordinates(ref destinationX, ref destinationY);

                Map.MoveResult result = _client.Map.CanMove(direction, destinationX, destinationY, isOnGround, isSurfing, _client.CanUseCut, _client.CanUseSmashRock);
                if (_client.Map.ApplyMovement(direction, result, ref destinationX, ref destinationY, ref isOnGround, ref isSurfing))
                {
                    if (result == Map.MoveResult.Icing)
                    {
                        _client.Map.ApplyCompleteIceMovement(direction, ref destinationX, ref destinationY, ref isOnGround);
                    }
                    else if (result == Map.MoveResult.Sliding)
                    {
                        _client.Map.ApplyCompleteSliderMovement(ref destinationX, ref destinationY, ref isOnGround);
                    }
                    neighbors.Add(new Node(destinationX, destinationY, isOnGround, isSurfing, direction));
                }
            }

            if (!node.IsSurfing && _hasSurfAbility && _client.Map.CanSurf(node.X, node.Y, node.IsOnGround))
            {
                neighbors.Add(new Node(node.X, node.Y, node.IsOnGround, true));
            }

            return neighbors;
        }

        private Node GetBestNode(IEnumerable<Node> nodes)
        {
            List<Node> bestNodes = new List<Node>();
            int bestScore = int.MaxValue;
            foreach (Node node in nodes)
            {
                if (node.Score < bestScore)
                {
                    bestNodes.Clear();
                    bestScore = node.Score;
                }
                if (node.Score == bestScore)
                {
                    bestNodes.Add(node);
                }
            }
            return bestNodes[_client.Rand.Next(bestNodes.Count)];
        }
    }
}

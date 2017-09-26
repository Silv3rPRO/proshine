﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PROProtocol
{
    public class Map
    {
        public enum MoveResult
        {
            Success,
            Fail,
            Jump,
            NoLongerSurfing,
            OnGround,
            NoLongerOnGround,
            Sliding,
            Icing
        }

        private readonly Dictionary<int, int> _sliderValues = new Dictionary<int, int>
        {
            {6662, 1},
            {6663, 2},
            {6670, 3},
            {6671, 4},
            {6719, 0},
            {6718, 0},
            {6686, 0}
        };

        public Map(byte[] content)
        {
            using (var stream = new MemoryStream(content))
            {
                using (var reader = new BinaryReader(stream))
                {
                    Colliders = ReadTiles(reader);
                    DimensionX = Colliders.GetUpperBound(0) + 1;
                    DimensionY = Colliders.GetUpperBound(1) + 1;
                    Width = DimensionX - 1;
                    Height = DimensionY - 1;

                    Tiles1 = ReadTiles(reader);
                    Tiles2 = ReadTiles(reader);
                    Tiles3 = ReadTiles(reader);
                    Tiles4 = ReadTiles(reader);

                    MapWeather = ReadString(reader);
                    reader.ReadInt16();
                    reader.ReadInt16();
                    IsOutside = reader.ReadByte() != 0;
                    Region = ReadString(reader);
                    reader.ReadInt16();

                    var count = reader.ReadInt16() - 1;
                    reader.ReadInt16();
                    reader.ReadInt16();
                    reader.ReadInt16();
                    reader.ReadInt16();
                    reader.ReadInt16();
                    reader.ReadInt16();
                    reader.ReadInt16();
                    reader.ReadInt16();

                    Links = new MapLink[DimensionX, DimensionY];
                    LinkDestinations = new Dictionary<string, List<Tuple<int, int>>>();
                    for (var i = 0; i < count; ++i)
                    {
                        var destination = ReadString(reader);
                        int x = reader.ReadInt16();
                        int y = reader.ReadInt16();
                        int toX = reader.ReadInt16();
                        int toY = reader.ReadInt16();

                        Links[x, y] = new MapLink(destination, toX, toY);

                        destination = destination.ToUpperInvariant();
                        if (!LinkDestinations.ContainsKey(destination))
                            LinkDestinations.Add(destination, new List<Tuple<int, int>>());
                        LinkDestinations[destination].Add(new Tuple<int, int>(x, y));
                    }

                    int k = reader.ReadInt16();
                    var npcCount = reader.ReadInt16() - 1;
                    for (var i = 0; i < 45; ++i)
                    {
                        int b = reader.ReadInt16();
                    }

                    Npcs = new List<Npc>();
                    OriginalNpcs = new List<Npc>();
                    for (var i = 0; i < npcCount; ++i)
                    {
                        var npcName = ReadString(reader);
                        int x = reader.ReadInt16();
                        int y = reader.ReadInt16();

                        //view direction and length
                        int directionValue = reader.ReadByte();
                        var viewDirection = DirectionExtensions.FromInt(directionValue);
                        int losLength = reader.ReadByte();
                        int type = reader.ReadInt16();

                        ReadString(reader);
                        var path = ReadString(reader);

                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();

                        var isBattler = reader.ReadInt16() != 0;

                        reader.ReadInt16();
                        reader.ReadSingle();
                        reader.ReadSingle();

                        int npcId = reader.ReadInt16();

                        if (npcName != "TileScript")
                            OriginalNpcs.Add(new Npc(npcId, npcName, isBattler, type, x, y, viewDirection, losLength,
                                path));

                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                        reader.ReadInt16();
                    }
                }
            }
        }

        public int[,] Colliders { get; }
        public MapLink[,] Links { get; }
        public int[,] Tiles1 { get; }
        public int[,] Tiles2 { get; }
        public int[,] Tiles3 { get; }
        public int[,] Tiles4 { get; }
        public int DimensionX { get; }
        public int DimensionY { get; }
        public int Width { get; }
        public int Height { get; }
        public string MapWeather { get; }
        public bool IsOutside { get; }
        public string Region { get; }
        public List<Npc> Npcs { get; }
        public List<Npc> OriginalNpcs { get; }
        public Dictionary<string, List<Tuple<int, int>>> LinkDestinations { get; }

        public static bool Exists(string name)
        {
            return File.Exists("Resources/" + name + ".dat");
        }

        private int[,] ReadTiles(BinaryReader reader)
        {
            if (reader.ReadInt16() != 2)
                return null;
            var height = reader.ReadInt32();
            reader.ReadInt32();
            var width = reader.ReadInt32();
            reader.ReadInt32();
            var tiles = new int[width, height];
            for (var y = 0; y < height; ++y)
                for (var x = 0; x < width; ++x)
                    tiles[x, y] = reader.ReadUInt16();
            return tiles;
        }

        public int GetCollider(int x, int y)
        {
            if (x >= 0 && x < DimensionX && y >= 0 && y < DimensionY)
                return Colliders[x, y];
            return -1;
        }

        public bool HasLink(int x, int y)
        {
            if (x >= 0 && x < DimensionX && y >= 0 && y < DimensionY)
                return Links[x, y] != null;
            return false;
        }

        private string ReadString(BinaryReader reader)
        {
            int count = reader.ReadUInt16();
            return new string(reader.ReadChars(count));
        }

        public bool CanSurf(int positionX, int positionY, bool isOnGround)
        {
            var currentCollider = GetCollider(positionX, positionY);
            var collider = GetCollider(positionX, positionY - 1);
            if ((collider == 5 || collider == 12) && isOnGround && currentCollider != 14)
                return true;

            collider = GetCollider(positionX, positionY + 1);
            if ((collider == 5 || collider == 12) && isOnGround)
                return true;

            collider = GetCollider(positionX - 1, positionY);
            if ((collider == 5 || collider == 12) && isOnGround)
                return true;

            collider = GetCollider(positionX + 1, positionY);
            if ((collider == 5 || collider == 12) && isOnGround)
                return true;

            return false;
        }

        public MoveResult CanMove(Direction direction, int destinationX, int destinationY, bool isOnGround,
            bool isSurfing, bool canUseCut, bool canUseSmashRock)
        {
            if (destinationX < 0 || destinationX >= DimensionX
                || destinationY < 0 || destinationY >= DimensionY)
                return MoveResult.Fail;
            foreach (var npc in Npcs)
                if (npc.PositionX == destinationX && npc.PositionY == destinationY && npc.LosLength < 100 &&
                    !npc.IsMoving)
                    return MoveResult.Fail;

            if (direction == Direction.Up && GetCollider(destinationX, destinationY + 1) == 14)
                return MoveResult.Fail;

            var collider = GetCollider(destinationX, destinationY);

            if (!IsMovementValid(direction, collider, isOnGround, isSurfing, canUseCut, canUseSmashRock))
                return MoveResult.Fail;
            if (collider >= 2 && collider <= 4)
                return MoveResult.Jump;
            if (isOnGround && collider == 7)
                return MoveResult.NoLongerOnGround;
            if (!isOnGround && collider == 8)
                return MoveResult.OnGround;
            if (isSurfing && collider != 5 && collider != 12)
                return MoveResult.NoLongerSurfing;
            if (IsIce(destinationX, destinationY))
                return MoveResult.Icing;
            if (GetSlider(destinationX, destinationY) != -1)
                return MoveResult.Sliding;
            return MoveResult.Success;
        }

        public bool CanInteract(int playerX, int playerY, int npcX, int npcY)
        {
            var distance = GameClient.DistanceBetween(playerX, playerY, npcX, npcY);
            if (distance != 1) return false;
            var playerCollider = GetCollider(playerX, playerY);
            var npcCollider = GetCollider(npcX, npcY);
            if (playerCollider == 14 && npcCollider == 14 && playerY == npcY) return true;
            if (playerCollider == 14 || npcCollider == 14) return false;
            return true;
        }

        public bool IsGrass(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                var num = Tiles2[x, y];
                var num2 = Tiles3[x, y];
                var hasLink = HasLink(x, y);
                return num == 6 || num == 14 || num == 55 || num == 15 || num == 248 || num == 249 || num == 250 ||
                       num2 == 6 || num2 == 14 || num2 == 55 || num2 == 15 || num2 == 248 || num2 == 249 ||
                       num2 == 250;
            }
            return false;
        }

        public bool IsWater(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                var collider = Colliders[x, y];
                return collider == 5 || collider == 12;
            }
            return false;
        }

        public bool IsNormalGround(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                var collider = Colliders[x, y];
                var hasLink = HasLink(x, y);
                return (collider == 0 || collider == 6 || collider == 7 || collider == 8 || collider == 9) && !hasLink;
            }
            return false;
        }

        public bool IsIce(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                if (Tiles1[x, y] == 17577 || Tiles1[x, y] == 17580 ||
                    Tiles2[x, y] == 17577 || Tiles2[x, y] == 17580 ||
                    Tiles3[x, y] == 17577 || Tiles3[x, y] == 17580)
                    return true;
            return false;
        }

        public bool IsPc(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return Tiles2[x, y] == 5437 || Tiles3[x, y] == 5437;
            return false;
        }

        // we lazily assume there can be only one PC and it is always accessible
        public Tuple<int, int> GetPc()
        {
            for (var y = 0; y < Height; ++y)
                for (var x = 0; x < Width; ++x)
                    if (IsPc(x, y))
                        return new Tuple<int, int>(x, y);
            return null;
        }

        public int GetSlider(int x, int y)
        {
            var tile = Tiles1[x, y];
            if (_sliderValues.ContainsKey(tile))
                return _sliderValues[tile];
            tile = Tiles2[x, y];
            if (_sliderValues.ContainsKey(tile))
                return _sliderValues[tile];
            tile = Tiles3[x, y];
            if (_sliderValues.ContainsKey(tile))
                return _sliderValues[tile];
            return -1;
        }

        public static Direction? SliderToDirection(int slider)
        {
            switch (slider)
            {
                case 1:
                    return Direction.Up;

                case 2:
                    return Direction.Down;

                case 3:
                    return Direction.Left;

                case 4:
                    return Direction.Right;

                default:
                    return null;
            }
        }

        public IEnumerable<Tuple<int, int>> GetNearestLinks(string linkName, int x, int y)
        {
            if (LinkDestinations.ContainsKey(linkName))
                return LinkDestinations[linkName]
                    .OrderBy(link => GameClient.DistanceBetween(x, y, link.Item1, link.Item2));
            return null;
        }

        private bool IsMovementValid(Direction direction, int collider, bool isOnGround, bool isSurfing, bool canUseCut,
            bool canUseSmashRock)
        {
            if (collider == -1)
                return false;
            switch (direction)
            {
                case Direction.Up:
                    if (isOnGround)
                    {
                        if (collider == 14 || collider == 0 || collider == 6 || collider == 7 || collider == 8 ||
                            collider == 9)
                            return true;
                        if (isSurfing && (collider == 5 || collider == 12))
                            return true;
                    }
                    else if (collider == 14 || collider == 7 || collider == 8 || collider == 9 || collider == 10 ||
                             collider == 12)
                    {
                        return true;
                    }
                    break;

                case Direction.Down:
                    if (isOnGround)
                    {
                        if (collider == 0 || collider == 6 || collider == 7 || collider == 8 || collider == 9 ||
                            collider == 2)
                            return true;
                        if (isSurfing && (collider == 5 || collider == 12))
                            return true;
                    }
                    else if (collider == 7 || collider == 8 || collider == 9 || collider == 10 || collider == 12)
                    {
                        return true;
                    }
                    break;

                case Direction.Left:
                    if (isOnGround)
                    {
                        if (collider == 14 || collider == 0 || collider == 6 || collider == 7 || collider == 8 ||
                            collider == 9 || collider == 4)
                            return true;
                        if (isSurfing && (collider == 5 || collider == 12))
                            return true;
                    }
                    else if (collider == 14 || collider == 7 || collider == 8 || collider == 9 || collider == 10 ||
                             collider == 12)
                    {
                        return true;
                    }
                    break;

                case Direction.Right:
                    if (isOnGround)
                    {
                        if (collider == 14 || collider == 0 || collider == 6 || collider == 7 || collider == 8 ||
                            collider == 9 || collider == 3)
                            return true;
                        if (isSurfing && (collider == 5 || collider == 12))
                            return true;
                    }
                    else if (collider == 14 || collider == 7 || collider == 8 || collider == 9 || collider == 10 ||
                             collider == 12)
                    {
                        return true;
                    }
                    break;
            }
            if (collider == 11 && canUseCut ||
                collider == 13 && canUseSmashRock)
                return true;
            return false;
        }

        public bool ApplyMovement(Direction direction, MoveResult result, ref int destinationX, ref int destinationY,
            ref bool isOnGround, ref bool isSurfing)
        {
            var success = false;
            switch (result)
            {
                case MoveResult.Success:
                    success = true;
                    break;

                case MoveResult.Jump:
                    success = true;
                    switch (direction)
                    {
                        case Direction.Down:
                            destinationY++;
                            break;

                        case Direction.Left:
                            destinationX--;
                            break;

                        case Direction.Right:
                            destinationX++;
                            break;
                    }
                    break;

                case MoveResult.Sliding:
                    success = true;
                    break;

                case MoveResult.Icing:
                    success = true;
                    break;

                case MoveResult.OnGround:
                    success = true;
                    isOnGround = true;
                    break;

                case MoveResult.NoLongerOnGround:
                    success = true;
                    isOnGround = false;
                    break;

                case MoveResult.NoLongerSurfing:
                    success = true;
                    isSurfing = false;
                    break;
            }
            return success;
        }

        public void ApplyCompleteIceMovement(Direction direction, ref int x, ref int y, ref bool isOnGround)
        {
            MoveResult result;
            do
            {
                var destinationX = x;
                var destinationY = y;
                var destinationGround = isOnGround;
                var isSurfing = false;
                direction.ApplyToCoordinates(ref destinationX, ref destinationY);
                result = CanMove(direction, destinationX, destinationY, destinationGround, false, false, false);
                if (ApplyMovement(direction, result, ref destinationX, ref destinationY, ref destinationGround,
                    ref isSurfing))
                {
                    x = destinationX;
                    y = destinationY;
                    isOnGround = destinationGround;
                }
            } while (result == MoveResult.Icing);
        }

        public void ApplyCompleteSliderMovement(ref int x, ref int y, ref bool isOnGround)
        {
            Direction? slidingDirection = null;
            MoveResult result;
            do
            {
                var destinationX = x;
                var destinationY = y;
                var destinationGround = isOnGround;
                var isSurfing = false;

                var slider = GetSlider(destinationX, destinationY);
                if (slider != -1)
                    slidingDirection = SliderToDirection(slider);

                if (slidingDirection == null)
                    break;

                slidingDirection.Value.ApplyToCoordinates(ref destinationX, ref destinationY);
                result = CanMove(slidingDirection.Value, destinationX, destinationY, destinationGround, false, false,
                    false);
                if (ApplyMovement(slidingDirection.Value, result, ref destinationX, ref destinationY,
                    ref destinationGround, ref isSurfing))
                {
                    x = destinationX;
                    y = destinationY;
                    isOnGround = destinationGround;
                }
            } while (slidingDirection != null && result != MoveResult.Fail);
        }
    }
}
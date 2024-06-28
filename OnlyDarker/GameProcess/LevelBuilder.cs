using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess.SpriteClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OnlyDarker.GameProcess
{
    public class Level
    {
        private static readonly Dictionary<Direction, Direction> _oppositeDirectionPairs = new()
        {
            {Direction.Left, Direction.Right },
            {Direction.Right, Direction.Left },
            {Direction.Up, Direction.Down },
            {Direction.Down, Direction.Up },
        };
        private static readonly Dictionary<Floor, IFloorConfig> _floorConfigPairs = new()
        {
            {Floor.One , new FloorOneConfig()},
        };
        public readonly Floor FloorType;
        private readonly IFloorConfig _floorConfig;
        public List<Room> BuiltFloor { get; private set; } = new();
        public Room[,] LevelGrid { get; private set; }
        public byte[,] _translatedLevelGrid;
        public Level(Floor floor)
        {
            FloorType = floor;
            _floorConfigPairs.TryGetValue(floor, out _floorConfig);
            EmptyRoom[,] grid = GenerateGrid(floor);
            var startingRoom = grid.OfType<EmptyRoom>().First(room => room.Neighbours == 1);
            startingRoom.RoomType = RoomType.Entry;
            SetBossRoom(grid, startingRoom);
            SetSpecialRooms(grid);
            SetEncounterRooms(grid);
            SetRoomDirections(grid);
            LevelGrid = new Room[_floorConfig.GridSize.Y, _floorConfig.GridSize.X];
            BuildRooms(grid);
            LinkPortals();
        }

        private static void SetEncounterRooms(EmptyRoom[,] grid)
        {
            foreach (var room in grid.OfType<EmptyRoom>().Where(room => room.RoomType == RoomType.Empty))
            {
                room.RoomType = RoomType.Encounter;
            }
        }

        private void BuildRooms(EmptyRoom[,] grid)
        {
            foreach (var room in grid.OfType<EmptyRoom>().Where(room => room is not null))
            {
                LevelGrid[room.Y, room.X] = new Room(room, this);
                BuiltFloor.Add(LevelGrid[room.Y, room.X]);
            }
        }

        private static void SetRoomDirections(EmptyRoom[,] grid)
        {
            foreach (var room in grid.OfType<EmptyRoom>().Where(room => room is not null))
            {
                if (grid[room.Y - 1, room.X] is not null)
                    room.HasUpNeighbour = true;
                if (grid[room.Y + 1, room.X] is not null)
                    room.HasDownNeighbour = true;
                if (grid[room.Y, room.X - 1] is not null)
                    room.HasLeftNeighbour = true;
                if (grid[room.Y, room.X + 1] is not null)
                    room.HasRightNeighbour = true;
            }
        }

        private static void SetSpecialRooms(EmptyRoom[,] grid)
        {
            var oneNeighbourRoom1 = grid.OfType<EmptyRoom>().First(room => room.Neighbours == 1 && room.RoomType == RoomType.Empty);
            oneNeighbourRoom1.RoomType = RoomType.Treasure;
            var oneNeighbourRoom2 = grid.OfType<EmptyRoom>().First(room => room.Neighbours == 1 && room.RoomType == RoomType.Empty);
            oneNeighbourRoom2.RoomType = RoomType.Puzzle;
            foreach (var room in grid.OfType<EmptyRoom>().Where(room => room.Neighbours == 1 && room.RoomType == RoomType.Empty))
            {
                int rng = RandomNumberGenerator.GetInt32(0, 2);
                if (rng == 0)
                {
                    room.RoomType = RoomType.Treasure;
                }
                else
                {
                    room.RoomType = RoomType.Puzzle;
                }
            }
        }

        private EmptyRoom[,] GenerateGrid(Floor floor)
        {
        FailedGenerationRetryMark:
            var grid = new EmptyRoom[_floorConfig.GridSize.Y, _floorConfig.GridSize.X];
            grid[_floorConfig.GridSize.Y / 2, _floorConfig.GridSize.X / 2] = new EmptyRoom(floor, _floorConfig.GridSize.X / 2, _floorConfig.GridSize.Y / 2);
            int rooms = _floorConfig.MaxRooms;
            int testIterations = 0;
            while (rooms > 0)
            {
                foreach (var emptyRoom in grid.OfType<EmptyRoom>().Where(room => room is not null && !room.IsUsed))
                {
                    int random;
                    random = RandomNumberGenerator.GetInt32(0, 2);
                    if (random < 1)
                    {
                        if (grid[emptyRoom.Y, emptyRoom.X - 2] is null && grid[emptyRoom.Y - 1, emptyRoom.X - 1] is null && grid[emptyRoom.Y + 1, emptyRoom.X - 1] is null && rooms > 0)
                        {
                            grid[emptyRoom.Y, emptyRoom.X - 1] = new EmptyRoom(floor, emptyRoom.X - 1, emptyRoom.Y);
                            grid[emptyRoom.Y, emptyRoom.X - 1].Neighbours++;
                            rooms--;
                            emptyRoom.Neighbours++;
                        }
                    }

                    random = RandomNumberGenerator.GetInt32(0, 2);
                    if (random < 1)
                    {
                        if (grid[emptyRoom.Y, emptyRoom.X + 2] is null && grid[emptyRoom.Y - 1, emptyRoom.X + 1] is null && grid[emptyRoom.Y + 1, emptyRoom.X + 1] is null && rooms > 0)
                        {
                            grid[emptyRoom.Y, emptyRoom.X + 1] = new EmptyRoom(floor, emptyRoom.X + 1, emptyRoom.Y);
                            grid[emptyRoom.Y, emptyRoom.X + 1].Neighbours++;
                            rooms--;
                            emptyRoom.Neighbours++;
                        }
                    }

                    random = RandomNumberGenerator.GetInt32(0, 2);
                    if (random < 1)
                    {
                        if (grid[emptyRoom.Y - 2, emptyRoom.X] is null && grid[emptyRoom.Y - 1, emptyRoom.X + 1] is null && grid[emptyRoom.Y - 1, emptyRoom.X - 1] is null && rooms > 0)
                        {
                            grid[emptyRoom.Y - 1, emptyRoom.X] = new EmptyRoom(floor, emptyRoom.X, emptyRoom.Y - 1);
                            grid[emptyRoom.Y - 1, emptyRoom.X].Neighbours++;
                            rooms--;
                            emptyRoom.Neighbours++;
                        }
                    }

                    random = RandomNumberGenerator.GetInt32(0, 2);
                    if (random < 1)
                    {
                        if (grid[emptyRoom.Y + 2, emptyRoom.X] is null && grid[emptyRoom.Y + 1, emptyRoom.X + 1] is null && grid[emptyRoom.Y + 1, emptyRoom.X - 1] is null && rooms > 0)
                        {
                            grid[emptyRoom.Y + 1, emptyRoom.X] = new EmptyRoom(floor, emptyRoom.X, emptyRoom.Y + 1);
                            grid[emptyRoom.Y + 1, emptyRoom.X].Neighbours++;
                            rooms--;
                            emptyRoom.Neighbours++;
                        }
                    }
                    emptyRoom.IsUsed = true;
                }
                testIterations++;
                if (testIterations > 10000)
                    goto FailedGenerationRetryMark;
            }
            if (!(grid.OfType<EmptyRoom>().Where(room => room.Neighbours == 1).Count() >= 4))
            {
                goto FailedGenerationRetryMark;
            }

            return grid;
        }

        private static void SetBossRoom(EmptyRoom[,] grid, EmptyRoom startingRoom)
        {
            int maxManhattanDistance = 0;
            EmptyRoom furthestRoom = grid.OfType<EmptyRoom>().First(room => room.Neighbours == 1 && room.RoomType == RoomType.Empty);
            foreach (var room in grid.OfType<EmptyRoom>().Where(room => room.Neighbours == 1 && room.RoomType == RoomType.Empty))
            {
                int lx, ly;
                lx = Math.Abs(room.X - startingRoom.X);
                ly = Math.Abs(room.Y - startingRoom.Y);
                if (lx + ly > maxManhattanDistance)
                {
                    maxManhattanDistance = lx + ly;
                    furthestRoom = room;
                }
            }
            furthestRoom.RoomType = RoomType.Boss;
        }

        private void LinkPortals()
        {
            foreach (var room in BuiltFloor)
            {
                foreach (var portal in room.Portals)
                {
                    if (portal.Direction == Direction.Left)
                    {
                        portal.SetExitRoom(LevelGrid[room.GridCords.Y, room.GridCords.X - 1]);
                        portal.SetExitPosition(LevelGrid[room.GridCords.Y, room.GridCords.X - 1].Portals.First(portal => portal.Direction == Direction.Right).Position);
                    }
                    if (portal.Direction == Direction.Right)
                    {
                        portal.SetExitRoom(LevelGrid[room.GridCords.Y, room.GridCords.X + 1]);
                        portal.SetExitPosition(LevelGrid[room.GridCords.Y, room.GridCords.X + 1].Portals.First(portal => portal.Direction == Direction.Left).Position);
                    }
                    if (portal.Direction == Direction.Up)
                    {
                        portal.SetExitRoom(LevelGrid[room.GridCords.Y - 1, room.GridCords.X]);
                        portal.SetExitPosition(LevelGrid[room.GridCords.Y - 1, room.GridCords.X].Portals.First(portal => portal.Direction == Direction.Down).Position);
                    }
                    if (portal.Direction == Direction.Down)
                    {
                        portal.SetExitRoom(LevelGrid[room.GridCords.Y + 1, room.GridCords.X]);
                        portal.SetExitPosition(LevelGrid[room.GridCords.Y + 1, room.GridCords.X].Portals.First(portal => portal.Direction == Direction.Up).Position);
                    }
                }
            }
        }
    }
    public class EmptyRoom
    {
        public Floor FloorType;
        public RoomType RoomType = RoomType.Empty;
        public int Neighbours = 0;
        public int X;
        public int Y;
        public bool HasLeftNeighbour = false;
        public bool HasRightNeighbour = false;
        public bool HasUpNeighbour = false;
        public bool HasDownNeighbour = false;
        public bool IsUsed = false;
        public EmptyRoom(Floor floorType, int x, int y)
        {
            FloorType = floorType;
            X = x; Y = y;
        }
    }
}


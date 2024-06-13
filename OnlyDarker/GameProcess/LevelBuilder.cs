using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess.SpriteClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public List<Room> BuiltFloor { get; private set; }
        //private RoomBlueprint[,] _localLevelGrid;
        public Room[,] LevelGrid { get; private set; }
        public byte[,] _translatedLevelGrid;
        public Level(Floor floor)
        {
            FloorType = floor;
            _floorConfigPairs.TryGetValue(floor, out _floorConfig);

            var level = new List<Room>();
            List<RoomBlueprint> levelBlueprint = new();

            for (int i = 0; i < _floorConfig.Encounters; i++)
            {
                levelBlueprint.Add(new(floor, RoomType.Encounter));
            }
            levelBlueprint = levelBlueprint.Prepend(new(floor, RoomType.Entry)).ToList();
            levelBlueprint.Add(new(floor, RoomType.Boss));
            levelBlueprint.Insert(levelBlueprint.Count / 2, new(floor, RoomType.Treasure));
            levelBlueprint.Insert(levelBlueprint.Count / 2, new(floor, RoomType.Puzzle));
            GenerateDirections(levelBlueprint);
            //var secretRoom = new RoomBlueprint(floor, RoomType.Secret);
            //secretRoom.lastRoomDirection = (_direction)Roll4();
            foreach (var blueprint in levelBlueprint)
            {
                level.Add(new Room(blueprint, this));
                Debug.WriteLine(blueprint.roomType);
            }
            for (int i = 0; i < level.Count; i++)
            {
                level[i].SetOrderNumber(i);
            }
            BuiltFloor = level;
            LevelGrid = new Room[_floorConfig.GridSize.Y, _floorConfig.GridSize.X];
            foreach (var room in BuiltFloor)
            {
                LevelGrid[room.GridCords.Y, room.GridCords.X] = room;
            }
            //_translatedLevelGrid = GetTranslatedLevelGrid();
            LinkPortals();
        }
        private void GenerateDirections(List<RoomBlueprint> levelBlueprint)
        {
            if (LevelGrid is not null) { return; }
            var localLevelGrid = new RoomBlueprint[_floorConfig.GridSize.Y, _floorConfig.GridSize.X];
            var blockedDirection = (Direction)Roll4();
            var blockedDirectionTwo = BlockSecondDirection(blockedDirection);
            _oppositeDirectionPairs.TryGetValue(blockedDirection, out Direction oppositeDirection);
            Point currentCell = new((_floorConfig.GridSize.X - 1) / 2,(_floorConfig.GridSize.Y - 1) / 2);
            for (int i = 0; i < levelBlueprint.Count; i++)
            {
                localLevelGrid[currentCell.Y, currentCell.X] = levelBlueprint[i];
                levelBlueprint[i].gridCords = currentCell;
                Direction newDirection;
                if (levelBlueprint[i].roomType is not RoomType.Boss)
                {
                    var newCell = GetNextCell(blockedDirection, blockedDirectionTwo, currentCell, oppositeDirection, out newDirection);
                    SetDirections(localLevelGrid, currentCell, newDirection, oppositeDirection);
                    _oppositeDirectionPairs.TryGetValue(newDirection, out oppositeDirection);
                    currentCell = newCell;
                }
                else
                {
                    SetDirections(localLevelGrid, currentCell, oppositeDirection, oppositeDirection);
                }
            }
        }
        //public byte[,] GetTranslatedLevelGrid()
        //{
        //    byte[,] grid = new byte[LevelGrid.GetLength(0), LevelGrid.GetLength(1)];
        //    for (int y = 0; y < LevelGrid.GetLength(0); y++)
        //    {
        //        for (int x = 0; x < LevelGrid.GetLength(1); x++)
        //        {
        //            if (LevelGrid[y, x] is null)
        //            {
        //                grid[y, x] = 99;
        //                Debug.Write("#");
        //            }
        //            else
        //            {
        //                grid[y, x] = (byte)LevelGrid[y, x].InstanceRoomType;
        //                Debug.Write(grid[y, x].ToString());
        //            }
        //        }
        //        Debug.WriteLine("");
        //    }
        //    return grid;
        //}
        private static int Roll4()
        {
            return GlobalUse.RNG.Next(0, 4);
        }
        private Point GetNextCell(Direction blockedDirection, Direction blockedDirectionTwo, Point currentCell, Direction oppositeDirection, out Direction newDirection)
        {
            do
            {
                newDirection = (Direction)Roll4();
            }
            while (newDirection == blockedDirection || newDirection == oppositeDirection || newDirection == blockedDirectionTwo);
            Point newCell;
            switch (newDirection)
            {
                case Direction.Down:
                    newCell = new Point(currentCell.X, currentCell.Y + 1);
                    break;
                case Direction.Up:
                    newCell = new Point(currentCell.X, currentCell.Y - 1);
                    break;
                case Direction.Left:
                    newCell = new Point(currentCell.X - 1, currentCell.Y);
                    break;
                case Direction.Right:
                    newCell = new Point(currentCell.X + 1, currentCell.Y);
                    break;
                default:
                    newCell = Point.Zero;
                    break;
            }
            return newCell;
        }
        private Direction BlockSecondDirection(Direction blockedDirection)
        {
            Direction blockedDirectionTwo;
            _oppositeDirectionPairs.TryGetValue(blockedDirection, out Direction oppositeDirection);
            do
            {
                blockedDirectionTwo = (Direction)Roll4();
            }
            while (blockedDirectionTwo == oppositeDirection || blockedDirectionTwo == blockedDirection);
            return blockedDirectionTwo;
        }

        private void SetDirections(RoomBlueprint[,] localLevelGrid, Point currentCell, Direction newDirection, Direction oppositeDirection)
        {
            localLevelGrid[currentCell.Y, currentCell.X].nextRoomDirection = newDirection;
            localLevelGrid[currentCell.Y, currentCell.X].lastRoomDirection = oppositeDirection;
        }

        private void LinkPortals()
        {
            foreach (var room in BuiltFloor)
            {
                try
                {
                    room.PortalBack?.SetExitPosition(BuiltFloor[room.OrderNumber - 1].PortalNext.Position);
                    Debug.WriteLine($"Portal back to room {room.OrderNumber - 1}");
                    room.PortalBack?.SetExitRoom(BuiltFloor[room.OrderNumber - 1]);
                }
                catch (Exception) { }
                try
                {
                    room.PortalNext?.SetExitPosition(BuiltFloor[room.OrderNumber + 1].PortalBack.Position);
                    Debug.WriteLine($"Portal forward to room {room.OrderNumber + 1}");
                    room.PortalNext?.SetExitRoom(BuiltFloor[room.OrderNumber + 1]);
                }
                catch (Exception) { }
            }
        }
    }

    public class RoomBlueprint
    {
        public Floor floorType;
        public RoomType roomType;
        public Direction lastRoomDirection;
        public Direction nextRoomDirection;
        public Point gridCords;
        public RoomBlueprint(Floor floorType, RoomType roomType)
        {
            this.floorType = floorType;
            this.roomType = roomType;
        }
    }
}


using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess.SpriteClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess
{
    public class Room
    {
        private readonly Point _roomTileSize;
        public readonly SpriteStandartTile[,] _tiles;
        public readonly SpriteStandartObstacle[,] _standartObstacles;
        public List<Rectangle> RoomColliders { get; private set; }
        public int OrderNumber { get; private set; }
        public Point TileSize { get; private set; }
        public Point RoomSize { get; private set; }
        //private float _sizeMultiplier { get; set; }
        public Room(Floor floor, RoomType roomType)
        {
            //switch (floor)
            //{
            //    case Floor.One: _sizeMultiplier = 1F; break;
            //}
            switch (roomType)
            {
                case RoomType.Entry: _roomTileSize = new(42, 28); break;
                case RoomType.Encounter: _roomTileSize = new(30, 20); break;
                case RoomType.Treasure: _roomTileSize = new(30, 20); break;
                case RoomType.Puzzle: _roomTileSize = new(30, 20); break;
                case RoomType.Boss: _roomTileSize = new(30, 20); break;
                case RoomType.Secret: _roomTileSize = new(30, 20); break;
            }

            _tiles = new SpriteStandartTile[_roomTileSize.X, _roomTileSize.Y];

            _standartObstacles = new SpriteStandartObstacle[_roomTileSize.X, _roomTileSize.Y];

            List<Texture2D> tileTextures = ImportTileTextures(floor, roomType);

            List<Texture2D> standartObstacleTextures = ImportStandartObstacleTextures(floor, roomType);

            TileSize = new(tileTextures[0].Width, tileTextures[0].Height);

            RoomSize = new(TileSize.X * _roomTileSize.X, TileSize.Y * _roomTileSize.Y);

            BuildTiles(tileTextures);

            BuildStandartObstacles(standartObstacleTextures);

            RoomColliders = new();

            foreach (var obstacle in _standartObstacles)
            {
                if (obstacle is not null)
                    RoomColliders.Add(obstacle.MovementCollider);
            }
        }

        public void Draw()
        {
            for (int y = 0; y < _roomTileSize.Y; y++)
            {
                for (int x = 0; x < _roomTileSize.X; x++)
                {
                    _tiles[x, y].Draw();
                    _standartObstacles[x, y]?.Draw();
                }
            }
        }

        private void BuildStandartObstacles(List<Texture2D> standartObstacleTextures) // TEMP
        {
            Random rng = new();

            for (int x = 0; x < _standartObstacles.GetLength(0); x++)
            {
                for (int y = 0; y < _standartObstacles.GetLength(1); y++)
                {
                    int l = rng.Next(0, 100);
                    if (l >= 20) { continue; }
                    int i = rng.Next(0, standartObstacleTextures.Count);
                    _standartObstacles[x, y] = new SpriteStandartObstacle(standartObstacleTextures[i], _tiles[x, y]);
                }
            }
        }

        private void BuildTiles(List<Texture2D> tileTextures)
        {
            Random rng = new();

            for (int x = 0; x < _tiles.GetLength(0); x++)
            {
                for (int y = 0; y < _tiles.GetLength(1); y++)
                {
                    int i = rng.Next(0, tileTextures.Count);
                    _tiles[x, y] = new SpriteStandartTile(tileTextures[i], new Vector2(x * TileSize.X, y * TileSize.Y));
                }
            }
        }

        private static List<Texture2D> ImportTileTextures(Floor floor, RoomType roomType)
        {
            string dirContentPath = $"Content/Floor/{floor}/RoomType/{roomType}/Tile";
            int dirTextureCount = Directory.EnumerateFiles(dirContentPath, "*.xnb").Count();

            List<Texture2D> tileTextures = new(dirTextureCount);

            try
            {
                for (int i = 1; i <= dirTextureCount; i++)
                {
                    tileTextures.Add(GlobalUse.Content.Load<Texture2D>($"Floor/{floor}/RoomType/{roomType}/Tile/{roomType}{i}"));
                }
            }
            catch (NullReferenceException) { throw; }

            return tileTextures;
        }
        private static List<Texture2D> ImportStandartObstacleTextures(Floor floor, RoomType roomType)
        {
            string dirContentPath = $"Content/Floor/{floor}/RoomType/{roomType}/StandartObstacle";
            int dirTextureCount = Directory.EnumerateFiles(dirContentPath, "*.xnb").Count();

            List<Texture2D> standartObstacleTextures = new(dirTextureCount);

            try
            {
                for (int i = 1; i <= dirTextureCount; i++)
                {
                    standartObstacleTextures.Add(GlobalUse.Content.Load<Texture2D>($"Floor/{floor}/RoomType/{roomType}/StandartObstacle/{roomType}{i}"));
                }
            }
            catch (NullReferenceException) { throw; }

            return standartObstacleTextures;
        }
        public void SetOrderNumber(int number)
        {
            OrderNumber = number;
        }
    }
}

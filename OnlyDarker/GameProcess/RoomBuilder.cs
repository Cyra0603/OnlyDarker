using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
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
        private readonly Texture2D _roomPresetImage;
        public readonly SpriteStandartTile[,] _tiles;
        private readonly List<SpriteStandartTile> _tilesYSorted;
        public readonly SpriteStandartObstacle[,] _standartObstacles;
        private readonly List<SpriteStandartObstacle> _obstaclesYSorted;
        public readonly BackgroundSprite CurrentBackground;
        public readonly Level ParentLevelReference;
        private static Dictionary<Vector4, string> PresetColorTranslator = new()
        {
            {new Vector4(255,0,0,255) , "Obstacle"},
            {new Vector4(0,255,0,255) , "Tile"}
        };
        public List<Rectangle> RoomColliders { get; private set; }
        public int OrderNumber { get; private set; }
        public Point TileSize { get; private set; }
        public Point RoomSize { get; private set; }
        //private float _sizeMultiplier { get; set; }
        public Room(Floor floor, RoomType roomType, Level parentLevelReference)
        {
            _roomPresetImage = ImportPreset(floor, roomType);
            var presetData = TextureTo2DArray(_roomPresetImage);
            _roomTileSize.Y = presetData.GetLength(1);
            _roomTileSize.X = presetData.GetLength(0);
            CurrentBackground = new(floor);
            _tiles = new SpriteStandartTile[_roomTileSize.X, _roomTileSize.Y];
            _standartObstacles = new SpriteStandartObstacle[_roomTileSize.X, _roomTileSize.Y];
            List<Texture2D> tileTextures = ImportTileTextures(floor, roomType);
            List<Texture2D> standartObstacleTextures = ImportStandartObstacleTextures(floor, roomType);
            TileSize = new(tileTextures[0].Width, tileTextures[0].Height);
            RoomSize = new(TileSize.X * _roomTileSize.X, TileSize.Y * _roomTileSize.Y);
            FillRoom(tileTextures, standartObstacleTextures, presetData);
            RoomColliders = new();
            foreach (var obstacle in _standartObstacles)
            {
                if (obstacle is not null)
                    RoomColliders.Add(obstacle.MovementCollider);
            }
            _tilesYSorted = _tiles.OfType<SpriteStandartTile>().OrderBy(tile => tile.Position.Y).ToList();
            _obstaclesYSorted = _standartObstacles.OfType<SpriteStandartObstacle>().OrderBy(obstacle => obstacle.Position.Y).ToList();
            ParentLevelReference = parentLevelReference;
        }

        public void Draw()
        {
            foreach (var tile in _tilesYSorted)
            {
                tile.Draw();
            }
            foreach (var obstacle in _obstaclesYSorted)
            {
                obstacle.Draw();
            }
        }
        private Texture2D ImportPreset(Floor floor, RoomType roomType)
        {
            var rng = new Random();
            string contentDir = $"Floor/{floor}/RoomType/{roomType}/Presets";
            int contentDirCount = Directory.EnumerateFiles("Content/" + contentDir, "*.xnb").Count();
            return GlobalUse.Content.Load<Texture2D>(contentDir + $"/Preset{floor}{roomType}{rng.Next(1, contentDirCount)}");
        }
        private Color[,] TextureTo2DArray(Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);

            Color[,] colors2D = new Color[texture.Width, texture.Height];
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    colors2D[x, y] = colors1D[x + y * texture.Width];

            return colors2D;
        }
        private void FillRoom(List<Texture2D> tileTextures, List<Texture2D> standartObstacleTextures, Color[,] presetData)
        {
            Random rng = new();
            for (int x = 0; x < _tiles.GetLength(0); x++)
            {
                for (int y = 0; y < _tiles.GetLength(1); y++)
                {
                    if (PresetColorTranslator.TryGetValue(ColorToVector4(presetData[x,y]), out string presetCellAlias))
                    switch (presetCellAlias)
                    {
                        case "Tile":
                                int a = rng.Next(0, tileTextures.Count);
                                _tiles[x, y] = new SpriteStandartTile(tileTextures[a], new Vector2(x * TileSize.X, y * TileSize.Y));
                                break;
                        case "Obstacle":
                                int b = rng.Next(0, tileTextures.Count);
                                _tiles[x, y] = new SpriteStandartTile(tileTextures[b], new Vector2(x * TileSize.X, y * TileSize.Y));
                                int c = rng.Next(0, standartObstacleTextures.Count);
                                _standartObstacles[x, y] = new SpriteStandartObstacle(standartObstacleTextures[c], _tiles[x, y]);
                                break;
                    }
                    else
                    {
                        int i = rng.Next(0, tileTextures.Count);
                        _tiles[x,y] = new SpriteStandartTile(tileTextures[i], new Vector2(x * TileSize.X, y * TileSize.Y));
                    }
                }
            }
        }
        private Vector4 ColorToVector4(Color color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }

        private void BuildStandartObstacles(List<Texture2D> standartObstacleTextures) // TEMP
        {
            Random rng = new();

            for (int x = 0; x < _standartObstacles.GetLength(0); x++)
            {
                for (int y = 0; y < _standartObstacles.GetLength(1); y++)
                {
                    int l = rng.Next(0, 100);
                    if (l >= 5) { continue; }
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

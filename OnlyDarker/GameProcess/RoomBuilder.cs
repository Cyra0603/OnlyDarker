using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess
{
    public class Room
    {
        private readonly Point _roomTileSize;
        private readonly Sprite[,] _tiles;
        public Point TileSize { get; private set; }
        public Point RoomSize { get; private set; }
        private float _sizeMultiplier { get; set; }
        public Room(Floor roomFloor, RoomType roomType)
        {
            //switch (roomFloor)
            //{
            //    case Floor.One: _sizeMultiplier = 1F; break;
            //}
            string dirContentPath = $"Content/Floor/{roomFloor}/RoomType/{roomType}";
            int dirTextureCount = Directory.EnumerateFiles(dirContentPath,"*.png").Count();
            switch (roomType)
            {
                case RoomType.Entry: _roomTileSize = new(42, 36); break;
            }

            _tiles = new Sprite[_roomTileSize.X, _roomTileSize.Y];

            List<Texture2D> textures = new(dirTextureCount);

            for (int i = 1; i <= dirTextureCount; i++)
            {
                string local = dirContentPath + $"/{roomType}{i}";
                textures.Add(GlobalUse.Content.Load<Texture2D>(local/*dirContentPath + $"/{roomType}{i}"*/));
            }

            TileSize = new(textures[0].Width, textures[0].Height);
            RoomSize = new(TileSize.X * _roomTileSize.X, TileSize.Y * _roomTileSize.Y);

            Random rng = new();

            for (int y = 0; y < dirTextureCount; y++)
            {
                for (int x = 0; x < dirTextureCount; x++)
                {
                    int i = rng.Next(0, textures.Count);
                    _tiles[y, x] = new Sprite(textures[i], new Vector2(x * TileSize.X, y * TileSize.Y));
                }
            }
        }
        public void Draw()
        {
            for (int y = 0; y < _roomTileSize.Y; y++)
            {
                for (int x = 0; x < _roomTileSize.X; x++)
                {
                    _tiles[y, x].Draw();
                }
            }
        }

    }
}

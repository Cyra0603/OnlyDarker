using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class SpriteStandartTile
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; private set; }
        public Vector2 Origin { get; private set; }
        public Rectangle Bounds { get; private set; }
        public int GridX { get; init; }
            public int GridY { get; init; }
        public SpriteStandartTile(Texture2D texture, Vector2 position, int X, int Y )
        {
            _texture = texture;
            Position = position;
            Origin = new(_texture.Width / 2, texture.Height / 2);
            Bounds = new((int)Position.X - _texture.Width / 2, (int)Position.Y - _texture.Height / 2, _texture.Width, _texture.Height);
            GridX = X;
            GridY = Y;
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 0.9F);
        }
        public float GetTextureWidth()
        {
            return _texture.Width;
        }
        public float GetTextureHeight()
        {
            return _texture.Height;
        }
    }
}

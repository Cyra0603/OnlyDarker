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
        private readonly SpriteSheet _spriteSheet;
        public Vector2 Position { get; private set; }
        public Vector2 Origin { get; private set; }
        public Rectangle Bounds { get; private set; }
        public int GridX { get; init; }
        public int GridY { get; init; }
        private int _spriteSheetIndex;
        public SpriteStandartTile(SpriteSheet spriteSheet, Vector2 position, int X, int Y)
        {
            _spriteSheet = spriteSheet;
            GlobalUse.SeededStandartRNG.Next(0, spriteSheet.FrameCount);
            Position = position;
            Origin = spriteSheet.GetTextureOrigin();
            Bounds = new((int)Position.X - _spriteSheet.FrameWidth / 2, (int)Position.Y - _spriteSheet.FrameHeight / 2, _spriteSheet.FrameWidth, _spriteSheet.FrameHeight);
            GridX = X;
            GridY = Y;
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_spriteSheet.Texture, Position, _spriteSheet.GetSourceRectangle(_spriteSheetIndex), Color.White, 0F, Origin, 1F, SpriteEffects.None, 0.9F);
        }
        public float GetTextureWidth()
        {
            return _spriteSheet.FrameWidth;
        }
        public float GetTextureHeight()
        {
            return _spriteSheet.FrameHeight;
        }
    }
}

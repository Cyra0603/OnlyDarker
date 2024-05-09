using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class SpriteStandartObstacle
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; protected set; }
        public Vector2 Origin { get; protected set; }
        public SpriteStandartObstacle(Texture2D texture, SpriteStandartTile parentTile)
        {
            _texture = texture;
            Origin = new(texture.Width / 2, texture.Height / 2);
            Position = new(parentTile.Position.X, parentTile.Position.Y - texture.Width / 2);
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 1F);
        }
    }
    public class SpriteTEMP
    {
        private readonly Texture2D _texture;
        public Vector2 Position => Mouse.GetState().Position.ToVector2();
        public Vector2 Origin { get; protected set; }
        public SpriteTEMP(Texture2D texture)
        {
            _texture = texture;
            Origin = new(texture.Width / 2, texture.Height / 2);
        }
        public void Draw(Vector2 position)
        {
            GlobalUse.SpriteBatch.Draw(_texture, -position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 1F);
        }
    }
}

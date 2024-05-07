using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess
{
    public class SpriteStandartTile
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; protected set; }
        public Vector2 Origin { get; protected set; }
        public SpriteStandartTile(Texture2D texture, Vector2 position)
        {
            _texture = texture;
            Position = position;
            Origin = new(_texture.Width / 2, texture.Height / 2);
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 0F);
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


    public class SpriteStandartObstacle
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; protected set; }
        public Vector2 Origin { get; protected set; }
        public SpriteStandartObstacle(Texture2D texture, SpriteStandartTile parentTile)
        {
            _texture = texture;
            Origin = new(texture.Width / 2, texture.Height / 2);
            Position = new(parentTile.Position.X, parentTile.Position.Y - texture.Width / 6);
        }
        public void Draw()
        {
            //var scaleSpread = new Random();
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 0.1F);
        }
    }

}

using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class SpriteStandartObstacle
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; protected set; }
        public Vector2 Origin { get; protected set; }
        public Rectangle MovementCollider { get; private set; }
        public SpriteStandartObstacle(Texture2D texture, SpriteStandartTile parentTile)
        {
            _texture = texture;
            Origin = new(texture.Width / 2, texture.Height / 2);
            Position = new(parentTile.Position.X, parentTile.Position.Y - texture.Width / 2);
            MovementCollider = new(new Point(
                (int)parentTile.Position.X - (int)parentTile.GetTextureWidth() / 2,
                (int)parentTile.Position.Y - (int)parentTile.GetTextureHeight() / 2),
                new((int)parentTile.GetTextureWidth(),
                (int)parentTile.GetTextureHeight())
                );
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 1F);
        }
    }
}

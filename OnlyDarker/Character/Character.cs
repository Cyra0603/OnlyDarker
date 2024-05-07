using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess;

namespace OnlyDarker
{
    public class Character
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; protected set; }
        public Vector2 Origin { get; protected set; }
        private float Speed { get; set; } = 100F;
        public int HealthPoints { get; set; } = 3;
        private Vector2 _minPosition, _maxPosition;
        public Character(Texture2D texture, SpriteStandartTile parentTile)
        {
            _texture = texture;
            Origin = new(texture.Width / 2, texture.Height / 2);
            Position = new(parentTile.Position.X, parentTile.Position.Y - (parentTile.GetTextureWidth() - texture.Width) / 2);
        }

        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 0F);
        }

        public void SetRoomBounds(Point roomSize, Point tileSize)
        {
            _minPosition = new Vector2((-tileSize.X / 2) + Origin.X, (-tileSize.Y / 2) + Origin.Y);
            _maxPosition = new Vector2(roomSize.X - (tileSize.X / 2) - Origin.X, roomSize.Y - (tileSize.Y / 2) - Origin.Y);
        }

        public void Update()
        {
            Position += InputManager.Direction * GlobalUse.Time * Speed;
            Position = Vector2.Clamp(Position, _minPosition, _maxPosition);
        }

        public void AddSpeed(float amount, float maxspeed = 200, float minspeed = 50)
        {
            Speed += amount;
            if (Speed < minspeed) Speed = minspeed;
            if (Speed > maxspeed) Speed = maxspeed;
        }
    }
}

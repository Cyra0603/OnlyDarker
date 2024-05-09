using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess.SpriteClasses;

namespace OnlyDarker
{
    public class Character
    {
        private readonly Texture2D _bodyTexture;
        private readonly Texture2D _handTexture;
        public Vector2 Position { get; protected set; }
        public Vector2 Origin { get; protected set; }
        public float HandRotation { get; set; } = 0;
        private Vector2 _handOrigin;
        public Vector2 _rightHandPosition => new(Position.X /*+ 70F*/, Position.Y/* + 123F*/);
        public Vector2 _leftHandPosition { get; private set; }
        private float Speed { get; set; } = 100F;
        private const float MAX_CHARACTER_SPEED = 200F;
        private const float MIN_CHARACTER_SPEED = 50F;
        public int HealthPoints { get; private set; } = 3;
        private Vector2 _minPosition, _maxPosition;
        public Character(Texture2D bodyTexture, Texture2D handTexture, SpriteStandartTile parentTile)
        {
            _bodyTexture = bodyTexture;
            _handTexture = handTexture;
            _handOrigin = new(handTexture.Width / 2, handTexture.Height / 2);
            Origin = new(bodyTexture.Width / 2, bodyTexture.Height / 2);
            Position = new(parentTile.Position.X, parentTile.Position.Y - (parentTile.GetTextureWidth() - bodyTexture.Width) / 2);
        }

        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_bodyTexture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 0F);
            GlobalUse.SpriteBatch.Draw(_handTexture, _rightHandPosition, null, Color.White, HandRotation, Origin, 1F, SpriteEffects.None, 0F);
        }

        public void SetRoomBounds(Point roomSize, Point tileSize)
        {
            _minPosition = new Vector2((-tileSize.X / 2) + Origin.X, (-tileSize.Y / 2) + Origin.Y);
            _maxPosition = new Vector2(roomSize.X - (tileSize.X / 2) - Origin.X, roomSize.Y - (tileSize.Y / 2) - Origin.Y);
        }

        public void Update()
        {
            Position += ControlsManager.Direction * GlobalUse.Time * Speed;
            Position = Vector2.Clamp(Position, _minPosition, _maxPosition);
            var cursorPointer = ControlsManager.MousePosition - _rightHandPosition;
            HandRotation = (float)Math.Atan2(cursorPointer.Y, cursorPointer.X) + 90F;
        }

        public void AddSpeed(float amount, float maxspeed = MAX_CHARACTER_SPEED, float minspeed = MIN_CHARACTER_SPEED)
        {
            Speed += amount;
            if (Speed < minspeed) Speed = minspeed;
            if (Speed > maxspeed) Speed = maxspeed;
        }
    }
}

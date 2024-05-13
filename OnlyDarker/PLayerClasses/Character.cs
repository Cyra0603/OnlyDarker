using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess;
using OnlyDarker.GameProcess.SpriteClasses;

namespace OnlyDarker
{

    public class Character
    {
        private readonly Texture2D _bodyTexture;
        private readonly Texture2D _handTexture;
        public Vector2 Position { get; set; }
        private Vector2 _lastAvailablePosition;
        public Vector2 Origin { get; protected set; }
        private Vector2 _handOrigin;
        private Vector2 _minPosition, _maxPosition;
        public Vector2 RightHandPosition => new(Position.X, Position.Y);
        public Vector2 LeftHandPosition { get; private set; }
        public Rectangle MovementCollisionAura => new(new Point(MovementCollider.Center.X - _bodyTexture.Width, MovementCollider.Center.Y - _bodyTexture.Height / 4), new(_bodyTexture.Width * 2, _bodyTexture.Height / 2));
        public Rectangle MovementCollider => new(new Point(
            (int)Position.X - _bodyTexture.Width / 2, (int)Position.Y + _bodyTexture.Height / 2 - (int)GlobalUse.PIXEL_OFFSET),
            new(_bodyTexture.Width, (int)GlobalUse.PIXEL_OFFSET * 2)
            );
        private float Speed { get; set; } = 1F;
        public const float MAX_CHARACTER_SPEED = 0.2F;
        public const float MIN_CHARACTER_SPEED = 0.05F;
        public float HandRotation { get; set; } = 0;
        public int HealthPoints { get; private set; } = 3;

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
            GlobalUse.SpriteBatch.Draw(_handTexture, RightHandPosition, null, Color.White, HandRotation, _handOrigin, 1F, SpriteEffects.None, 0F);
        }

        public void SetRoomBounds(Point roomSize, Point tileSize)
        {
            _minPosition = new Vector2((-tileSize.X / 2) + Origin.X, (-tileSize.Y / 2) + Origin.Y - _bodyTexture.Height + GlobalUse.PIXEL_OFFSET);
            _maxPosition = new Vector2(roomSize.X - (tileSize.X / 2) - Origin.X, roomSize.Y - (tileSize.Y / 2) - Origin.Y);
        }

        public void Update()
        {
            ControlsManager.UpdateCharacterControls();
            if (GameBody.SceneManager.CurrentRoom.RoomColliders.Any(collider => collider.Intersects(MovementCollisionAura)))
            {
                var obstacles = GameBody.SceneManager.CurrentRoom.RoomColliders.Where(collider => collider.Intersects(MovementCollisionAura)).ToList();
                var availableDirection = ControlsManager.GetDirection();
                Position += CalculateAvailableDirection(obstacles, ref availableDirection) * Speed;
            }
            else
            {
                Position += ControlsManager.GetDirection() * Speed;
            }
            Position = Vector2.Clamp(Position, _minPosition, _maxPosition);
            var cursorPointer = ControlsManager.MousePosition - RightHandPosition;
            HandRotation = 0;
        }

        private Vector2 CalculateAvailableDirection(List<Rectangle> obstacles, ref Vector2 availableDirection)
        {
            Vector2 futurePos = Position + availableDirection * Speed;
            Rectangle newRect = CalculateMovementCollider(futurePos);
            if (obstacles.Any(collider => collider.Intersects(newRect)))
            {
                for (float i = 1, j = 7; i <= j; i++)
                {

                    if (obstacles.Any(collider => collider.Intersects(newRect)))
                    {
                        if (availableDirection.Y > 0)
                        {
                            availableDirection.Y--;
                        }
                        if (availableDirection.Y < 0)
                        {
                            availableDirection.Y++;
                        }
                        if (availableDirection.X > 0)
                        {
                            availableDirection.X--;
                        }
                        if (availableDirection.X < 0)
                        {
                            availableDirection.X++;
                        }
                    }
                    else
                    {
                        return availableDirection;
                    }
                }
            }
            else return availableDirection;
        }

        public void AddSpeed(float amount, float maxspeed = MAX_CHARACTER_SPEED, float minspeed = MIN_CHARACTER_SPEED)
        {
            Speed += amount;
            if (Speed < minspeed) Speed = minspeed;
            if (Speed > maxspeed) Speed = maxspeed;
        }
        public void SetPosition(Vector2 position)
        {
            Position = position;
        }
        public Rectangle CalculateMovementCollider(Vector2 position)
        {
            return new(new Point(
            (int)position.X - _bodyTexture.Width / 2,
            (int)position.Y + _bodyTexture.Height / 2 - (int)GlobalUse.PIXEL_OFFSET),
            new(_bodyTexture.Width, (int)GlobalUse.PIXEL_OFFSET * 2)
            );
        }
    }
}


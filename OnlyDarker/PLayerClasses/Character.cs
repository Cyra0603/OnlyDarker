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
using OnlyDarker.PlayerClasses;

namespace OnlyDarker
{

    public class Character : IDamageable
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
            (int)Position.X - _bodyTexture.Width / 2, (int)Position.Y + _bodyTexture.Height / 2 - (int)GlobalUse.PIXEL_OFFSET * 8),
            new(_bodyTexture.Width, (int)GlobalUse.PIXEL_OFFSET * 8)
            );
        public Rectangle BodyHitbox => new(Position.ToPoint(), new(_bodyTexture.Width, _bodyTexture.Height));
        public Rectangle AttackZone => new(RightHandPosition.ToPoint(), new((int)CurrentWeapon.AttackRange, (int)CurrentWeapon.AttackRange));
        public IWeapon CurrentWeapon = new WeaponFist();
        public float Speed { get; private set; } = 1F;
        public const float MAX_CHARACTER_SPEED = 2F;
        public const float MIN_CHARACTER_SPEED = 0.5F;
        public const float I_FRAME_TIME = 500F;
        private bool _isInvincible = false;
        private bool _isAttacking = false;
        public float HandRotation { get; set; } = 0;
        private float _healthPoints = 24;
        public float HealthPoints
        {
            get => _healthPoints;
            private set
            {
                var previousValue = _healthPoints;
                _healthPoints = value;
                if (previousValue != _healthPoints)
                {
                    OnChangingHealth?.Invoke(_healthPoints);
                    if (_healthPoints > previousValue)
                    {
                        OnHealing?.Invoke(_healthPoints);
                    }
                    if (_healthPoints < previousValue)
                    {
                        OnTakingDamage?.Invoke(_healthPoints);
                    }
                }
            }
        }

        public Character(Texture2D bodyTexture, Texture2D handTexture, SpriteStandartTile parentTile)
        {
            _bodyTexture = bodyTexture;
            _handTexture = handTexture;
            _handOrigin = new(handTexture.Width / 2, handTexture.Height / 2);
            Origin = new(bodyTexture.Width / 2, bodyTexture.Height / 2);
            Position = new(parentTile.Position.X, parentTile.Position.Y - (parentTile.GetTextureWidth() - bodyTexture.Width) / 2);
        }
        public delegate void ObserveHP(float healthPoints);
        public event ObserveHP OnChangingHealth;
        public event ObserveHP OnTakingDamage;
        public event ObserveHP OnHealing;

        public async void RunIFrames()
        {
            _isInvincible = true;
            await Task.Delay((int)I_FRAME_TIME);
            _isInvincible = false;
        }

        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_bodyTexture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 0.5F);
            GlobalUse.SpriteBatch.Draw(_handTexture, RightHandPosition, null, Color.White, HandRotation, _handOrigin, 1F, SpriteEffects.None, 0.5F);
        }

        public void SetRoomBounds(Point roomSize, Point tileSize)
        {
            _minPosition = new Vector2((-tileSize.X / 2) + Origin.X, (-tileSize.Y / 2) + Origin.Y - _bodyTexture.Height + GlobalUse.PIXEL_OFFSET);
            _maxPosition = new Vector2(roomSize.X - (tileSize.X / 2) - Origin.X, roomSize.Y - (tileSize.Y / 2) - Origin.Y);
        }

        public void Update()
        {
            ControlsManager.UpdatePlayerControls();
            if (GameBody.SceneManager.CurrentRoom.RoomColliders.Any(collider => collider.Intersects(MovementCollisionAura)))
            {
                var obstacles = GameBody.SceneManager.CurrentRoom.RoomColliders.Where(collider => collider.Intersects(MovementCollisionAura)).ToList();
                for (int i = 0, j = (int)ControlsManager.GetDirection().Length(); i < j; i++)
                {
                    var currentDirection = ControlsManager.GetDirection();
                    CalculatePossibleCollisions(obstacles, ref currentDirection);
                    Position += ControlsManager.GetDirection() / j * Speed;
                }
            }
            else
            {
                Position += ControlsManager.GetDirection() * Speed;
            }
            if (GameBody.SceneManager.CurrentRoom.Pickups is not null && GameBody.SceneManager.CurrentRoom.Pickups.Any(collider => collider.MovementCollider.Intersects(MovementCollisionAura)))
            {
                GameBody.SceneManager.CurrentRoom.Pickups.First(collider => collider.MovementCollider.Intersects(MovementCollisionAura)).ShowPickupMessage();
            }
            Position = Vector2.Clamp(Position, _minPosition, _maxPosition);
            var cursorPointer = ControlsManager.MousePosition - RightHandPosition;
            HandRotation = 0;
        }

        private void CalculatePossibleCollisions(List<Rectangle> obstacles, ref Vector2 currentDirection)
        {
            var ly = currentDirection.Y;
            var lx = currentDirection.X;
            if (currentDirection.Y < 0)
            {
                if (obstacles.Any(collider => collider.Intersects(CalculateMovementCollider(Position + new Vector2(lx, ly - 1) * Speed))))
                {
                    ControlsManager.ZeroDirectionY();
                    ControlsManager.AddFriction();
                }
            }
            if (currentDirection.Y > 0)
            {
                if (obstacles.Any(collider => collider.Intersects(CalculateMovementCollider(Position + new Vector2(lx, ly + 1) * Speed))))
                {
                    ControlsManager.ZeroDirectionY();
                    ControlsManager.AddFriction();
                }
            }
            if (currentDirection.X < 0)
            {
                if (obstacles.Any(collider => collider.Intersects(CalculateMovementCollider(Position + new Vector2(lx - 1, ly) * Speed))))
                {
                    ControlsManager.ZeroDirectionX();
                    ControlsManager.AddFriction();
                }
            }
            if (currentDirection.X > 0)
            {
                if (obstacles.Any(collider => collider.Intersects(CalculateMovementCollider(Position + new Vector2(lx + 1, ly) * Speed))))
                {
                    ControlsManager.ZeroDirectionX();
                    ControlsManager.AddFriction();
                }
            }
        }
        public Rectangle CalculateMovementCollider(Vector2 position)
        {
            return new(new Point(
            (int)position.X - _bodyTexture.Width / 2,
            (int)position.Y + _bodyTexture.Height / 2 - (int)GlobalUse.PIXEL_OFFSET * 8),
            new(_bodyTexture.Width, (int)GlobalUse.PIXEL_OFFSET * 8)
            );
        }
        public void AddSpeed(float amount, float maxspeed = MAX_CHARACTER_SPEED, float minspeed = MIN_CHARACTER_SPEED)
        {
            Speed += amount;
            if (Speed < minspeed) Speed = minspeed;
            if (Speed > maxspeed) Speed = maxspeed;
        }
        public void SetPosition(Vector2 position)
        {
            var newPos = position;
            newPos.Y += Position.Y - MovementCollider.Location.Y;
            Position = newPos;
        }
        public void TakeDamage(float damage)
        {
            if (!_isInvincible)
            {
                HealthPoints -= damage;
                RunIFrames();
            }
            else return;
        }
        public void Heal(float healAmount)
        {
            HealthPoints += healAmount;
        }
        public async void Attack()
        {
            if (CurrentWeapon.IsOnCooldown) return;
            _isAttacking = true;
            
        }
    }
}


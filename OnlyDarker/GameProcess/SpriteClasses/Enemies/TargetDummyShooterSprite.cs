using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses.Enemies
{
    public class TargetDummyShooterSprite : IDamageable, IYSortable, IMyUpdateable
    {
        private readonly Texture2D _bodyTexture;
        private readonly Texture2D _projectileTexture;
        private readonly Room _parentRoomReference;
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; }
        public Vector2 LastUpdatedPosition { get; private set; }
        private Vector2 _direction;
        private Vector2 _destination;
        private Vector2[] _path;
        private Vector2 _mainCharPosition => new(
            GameBody.GetGameInstance().MainCharacter.MovementCollider.Location.X + GameBody.GetGameInstance().MainCharacter.MovementCollider.Width / 2,
            GameBody.GetGameInstance().MainCharacter.MovementCollider.Location.Y + GameBody.GetGameInstance().MainCharacter.MovementCollider.Height / 2);
        public Rectangle MovementCollider;
        public Rectangle BodyHitbox => new(new((int)Position.X - _bodyTexture.Width / 2, (int)Position.Y - _bodyTexture.Height / 2), new(_bodyTexture.Width, _bodyTexture.Height));
        private Timer _attackCooldown;
        public readonly float AttackCooldownTime = 1000F;
        public BaseArmor BaseArmor { get; }
        public bool IsExpired { get; set; } = false;
        public bool IsInvincible { get; set; }
        public bool IsPushable { get; } = false;

        public int XPReward { get; }

        public bool IsSummoned { get; }
        public float Speed { get; }
        private float _healthPoints = 10000;
        private long _test;
        public float HealthPoints
        {
            get => _healthPoints;
            set
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
        public float MaxHealthPoints { get; set; }

        public TargetDummyShooterSprite(SpriteStandartTile parentTile, Room parentRoom)
        {
            _parentRoomReference = parentRoom;
            _bodyTexture = GlobalUse.Content.Load<Texture2D>("Entities/TargetDummyShooter/DummyShooterProjectile");//CHANGE THE TEXTURE
            _projectileTexture = GlobalUse.Content.Load<Texture2D>("Entities/TargetDummyShooter/DummyShooterProjectile");
            Origin = new(_bodyTexture.Width / 2, _bodyTexture.Height / 2);
            Position = new(parentTile.Position.X, parentTile.Position.Y - (parentTile.GetTextureWidth() - _bodyTexture.Width) / 2);
            XPReward = 50;
            IsSummoned = false;
            _direction = Vector2.Zero;
            _path = Array.Empty<Vector2>();
            MovementCollider = BodyHitbox;
            _attackCooldown = new(AttackCooldownTime);
            MaxHealthPoints = 10000;
            Speed = 0.7F;
            BaseArmor = new();
        }
        public delegate void ObserveHP(float healthPoints);
        public event ObserveHP OnChangingHealth;
        public event ObserveHP OnTakingDamage;
        public event ObserveHP OnHealing;
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_bodyTexture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 0.5F);
            if (GlobalUse.IsDebugMode)
            {
                GameBody.DrawRectangleOutline(BodyHitbox, Color.Black, 2);
                GameBody.DrawRectangleOutline(MovementCollider, Color.Black, 2);
                GlobalUse.SpriteBatch.DrawString(GlobalUse.MainFont, $"{HealthPoints}", new(Position.X, Position.Y - _bodyTexture.Height), Color.White, 0F, Origin, 0.25F, SpriteEffects.None, 0.5F);
                GlobalUse.SpriteBatch.DrawString(GlobalUse.MainFont, $"{_test}", Position, Color.White, 0F, Origin, 0.25F, SpriteEffects.None, 0.5F);
                Vector2 lastPos = Position;
                foreach (var pos in _path)
                {
                    if (pos == Vector2.Zero)
                        break;
                    GlobalUse.SpriteBatch.DrawLine(lastPos, pos, Color.Red);
                    lastPos = pos;
                }
            }
        }
        public void Shoot()
        {
            var difference = Vector2.Normalize(GameBody.GetGameInstance().MainCharacter.Position - Position);
            var direction = difference / difference.Length();
            var projectile = new EnemyProjectileSprite(_projectileTexture, Position, direction, new(1, 1, DamageType.Blunt, false), 15000F, true);
            GameBody.GetGameInstance().ProjectileSprites.Add(projectile);
            _attackCooldown.TimeLeft += AttackCooldownTime;
        }

        public void Update(float elapsedMilliseconds)
        {
            if (_parentRoomReference != GameBody.GetGameInstance().SceneManager.CurrentRoom)
                return;
            _attackCooldown.Update(elapsedMilliseconds);
            if (_attackCooldown.TimeLeft <= 0)
            {
                Shoot();
            }
            if (Vector2.Distance(Position, _mainCharPosition) > 42 && Vector2.Distance(Position, LastUpdatedPosition) > 4)
            {
                if (_parentRoomReference.LineCastIsCollidingObstacles(Position, _mainCharPosition))
                {
                    var destination = _parentRoomReference.GetPathDestination(Position, _mainCharPosition);
                    _destination = destination;
                    var ldirection = destination - Position;
                    _direction = Vector2.Normalize(ldirection / ldirection.Length());
                    LastUpdatedPosition = Position;
                }
                else
                {
                    var ldirection = _mainCharPosition - Position;
                    _direction = Vector2.Normalize(ldirection / ldirection.Length());
                    _destination = _mainCharPosition;
                }
            }

            Position += _direction * Speed;
        }
    }
}

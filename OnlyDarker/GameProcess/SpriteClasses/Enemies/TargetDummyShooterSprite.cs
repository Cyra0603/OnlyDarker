using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
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
        public Vector2 Origin { get;}
        public Vector2 LastUpdatedPosition { get; private set; }
        private Vector2 _direction;
        public Rectangle MovementCollider;
        public Rectangle BodyHitbox => new(new((int)Position.X - _bodyTexture.Width / 2, (int)Position.Y - _bodyTexture.Height / 2), new(_bodyTexture.Width, _bodyTexture.Height));
        private Timer _attackCooldown;
        public readonly float AttackCooldownTime = 1000F;
        public Armor BaseArmor { get; private set; } = new(ArmorType.Base);
        public List<Armor> ArmorSet { get; } = new();
        public bool IsExpired { get; private set; } = false;
        public bool IsInvincible { get; set; }
        public bool IsPushable { get; } = false;
        public float Speed { get; }
        private float _healthPoints = 10000;
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
            _bodyTexture = GlobalUse.Content.Load<Texture2D>("Entities/TargetDummy/TargetDummy");
            _projectileTexture = GlobalUse.Content.Load<Texture2D>("Entities/TargetDummyShooter/DummyShooterProjectile");
            Origin = new(_bodyTexture.Width / 2, _bodyTexture.Height / 2);
            Position = new(parentTile.Position.X, parentTile.Position.Y - (parentTile.GetTextureWidth() - _bodyTexture.Width) / 2);
            _direction = Vector2.Zero;
            MovementCollider = BodyHitbox;
            _attackCooldown = new(AttackCooldownTime);
            MaxHealthPoints = 10000;
            Speed = 0.7F;
            ArmorSet.Add(BaseArmor);
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
            }
        }
        public void Shoot()
        {
            var difference = Vector2.Normalize(GameBody.GetGameInstance().MainCharacter.Position - Position);
            var direction = difference / difference.Length();
            var projectile = new ProjectileSprite(_projectileTexture, Position, direction, new(1, 1, DamageType.Blunt, false), 15000F);
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
            if (Vector2.Distance(Position, GameBody.GetGameInstance().MainCharacter.Position) > 42 && Vector2.Distance(Position, LastUpdatedPosition) > 21)
            {
                    var destination = _parentRoomReference.GetPathDestination(Position, GameBody.GetGameInstance().MainCharacter.Position);
                var ldirection = destination - Position;
                _direction = Vector2.Normalize(ldirection / ldirection.Length());
                    LastUpdatedPosition = Position;
            }
            if(Vector2.Distance(Position, GameBody.GetGameInstance().MainCharacter.Position) < 42)
            {
                var ldirection = GameBody.GetGameInstance().MainCharacter.Position - Position;
                _direction = Vector2.Normalize(ldirection / ldirection.Length());
            }
            Position += _direction * Speed;
        }
    }
}

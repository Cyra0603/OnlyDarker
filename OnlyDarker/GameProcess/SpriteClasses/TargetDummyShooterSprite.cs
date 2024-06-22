using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class TargetDummyShooterSprite : IDamageable, IYSortable,IMyUpdateable
    {
        private readonly Texture2D _bodyTexture;
        private readonly Texture2D _projectileTexture;
        private readonly Room _parentRoomReference;
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; protected set; }
        public Rectangle MovementCollider;
        public Rectangle BodyHitbox => new(new((int)Position.X - _bodyTexture.Width / 2, (int)Position.Y - _bodyTexture.Height / 2), new(_bodyTexture.Width, _bodyTexture.Height));
        private Timer _attackCooldown = new(500F); 
        public Armor BaseArmor { get; private set; } = new(ArmorType.Base);
        public List<Armor> ArmorSet { get; } = new();
        public bool IsInvincible { get; set; }
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

        public TargetDummyShooterSprite(SpriteStandartTile parentTile, Room parentRoom)
        {
            _parentRoomReference = parentRoom;
            _bodyTexture = GlobalUse.Content.Load<Texture2D>("Entities/TargetDummy/TargetDummy");
            _projectileTexture = GlobalUse.Content.Load<Texture2D>("Entities/TargetDummyShooter/DummyShooterProjectile");
            Origin = new(_bodyTexture.Width / 2, _bodyTexture.Height / 2);
            Position = new(parentTile.Position.X, parentTile.Position.Y - (parentTile.GetTextureWidth() - _bodyTexture.Width) / 2);
            MovementCollider = BodyHitbox;
            ArmorSet.Add(BaseArmor);
            //foreach(var armor in ArmorSet)
            //{
            //    armor.AddFlatArmor(5F);
            //}
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
                GameBody.DrawRectangleOutline(BodyHitbox, Color.Black, 4);
                GameBody.DrawRectangleOutline(MovementCollider, Color.Black, 4);
                GlobalUse.SpriteBatch.DrawString(GlobalUse.MainFont, $"{HealthPoints}", new(Position.X, Position.Y - _bodyTexture.Height * 2), Color.White, 0F, Origin, 1F, SpriteEffects.None, 0.5F);
            }
        }
        public void Shoot()
        {
            var difference = Vector2.Normalize(GameBody.MainCharacter.Position - Position);
            var direction = difference / difference.Length();
            var projectile = new ProjectileSprite(_projectileTexture, Position, direction * 3F, new(1, 1, DamageType.Blunt, false));
            GameBody.ProjectileSprites.Add(projectile);
            _attackCooldown.TimeLeft += 500F;
        }

        public void Update(float elapsedMilliseconds)
        {
            _attackCooldown.Update(elapsedMilliseconds);
            if(_parentRoomReference == GameBody.SceneManager.CurrentRoom && _attackCooldown.TimeLeft <= 0)
            {
                Shoot();
            }
        }
    }
}

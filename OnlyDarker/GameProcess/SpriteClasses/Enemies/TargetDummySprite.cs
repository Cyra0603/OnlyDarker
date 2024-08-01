using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class TargetDummySprite : IDamageable, IYSortable
    {
        private readonly Texture2D _bodyTexture;
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; protected set; }
        public Rectangle MovementCollider;
        public Rectangle BodyHitbox => new(new((int)Position.X - _bodyTexture.Width / 2, (int)Position.Y - _bodyTexture.Height / 2), new(_bodyTexture.Width, _bodyTexture.Height));
        public BaseArmor BaseArmor { get;}
        public List<ArmorSprite> ArmorSet { get; } = new();
        public bool IsInvincible { get; set; }
        public bool IsExpired { get; set; } = false;
        public bool IsPushable { get; } = false;
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

        public int XPReward { get; }

        public bool IsSummoned { get; }

        public TargetDummySprite(SpriteStandartTile parentTile)
        {
            _bodyTexture = GlobalUse.Content.Load<Texture2D>("Entities/TargetDummy/TargetDummy");
            Origin = new(_bodyTexture.Width / 2, _bodyTexture.Height / 2);
            Position = new(parentTile.Position.X, parentTile.Position.Y - (parentTile.GetTextureWidth() - _bodyTexture.Width) / 2);
            MaxHealthPoints = 10000;
            XPReward = 0;
            IsSummoned = true;
            MovementCollider = BodyHitbox;
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
            }
        }

        public void Update()
        {

        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }
    }
}

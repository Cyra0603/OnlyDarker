using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnlyDarker.GameProcess.SpriteClasses.Enemies
{
    public class FloorOneBossSprite : IMyUpdateable, IYSortable, IDamageable
    {
        public Texture2D BodyTexture { get; }
        private Room _parentRoomRef { get; }
        public Vector2 Position { get; set; }
        public enum AttackPattern
        {
            Pattern1,
            Pattern2,
            Pattern3,
            Pattern4,
            Pattern5,
        }
        public AttackPattern _currentAttackPattern;
        public Timer PatternChanger;
        public bool IsExpired { get; private set; }

        public Rectangle BodyHitbox => new((int)Position.X, (int)Position.Y, BodyTexture.Width, BodyTexture.Height);

        public bool IsInvincible { get; private set; } = false;

        public bool IsPushable { get; } = false;
        private float _patternChangeTime;
        private float _baseDamage;
        private float _healthPoints;
        public float HealthPoints
        {
            get => _healthPoints;
            set
            {
                _healthPoints = value;
                if (_healthPoints <= 0)
                {
                    IsExpired = true;
                }
            }
        }

        public float MaxHealthPoints { get; }
        private Armor _baseArmor;
        public List<Armor> ArmorSet { get; }
        public FloorOneBossSprite(Texture2D bodyTexture, Room parentRoomRef, Vector2 position, float maxHealthPoints)
        {
            BodyTexture = bodyTexture;
            _parentRoomRef = parentRoomRef;
            Position = position;
            MaxHealthPoints = maxHealthPoints;
            ArmorSet = new();
            _baseArmor = new(ArmorType.Base, sliceX: 0.9F, pokeX: 0.9F, bluntX: 0.85F);
            ArmorSet.Add(_baseArmor);
            PatternChanger = new();
        }

        public void Draw()
        {
            if (IsExpired)
            {
                GlobalUse.SpriteBatch.Draw(BodyTexture, Position, null, Color.White, 0F, Vector2.Zero, 1F, SpriteEffects.FlipVertically, 1F);
                return;
            }
            else
            GlobalUse.SpriteBatch.Draw(BodyTexture, Position, null, Color.White, 0F, Vector2.Zero, 1F, SpriteEffects.None, 1F);
        }

        public void Update(float elapsedMilliseconds)
        {
            if (!IsExpired && GameBody.GetGameInstance().SceneManager.CurrentRoom != _parentRoomRef)
            {
                Respawn();
                return;
            }
            if (BodyHitbox.Intersects(GameBody.GetGameInstance().MainCharacter.BodyHitbox))
            {
                DamageInstance damage = new(_baseDamage, 1, DamageType.Poke, false);
                GameBody.GetGameInstance().MainCharacter.TakeDamage(in damage);
            }

        }

        public void Respawn()
        {
            HealthPoints = MaxHealthPoints;
        }
    }
}

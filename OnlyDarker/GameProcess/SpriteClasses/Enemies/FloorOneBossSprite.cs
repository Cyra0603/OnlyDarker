using OnlyDarker.CommonUsing;
using OnlyDarker.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace OnlyDarker.GameProcess.SpriteClasses.Enemies
{
    public class FloorOneBossSprite : IMyUpdateable, IYSortable, IDamageable
    {
        public Texture2D BodyTexture { get; }
        private Room _parentRoomRef { get; }
        public Vector2 Position { get; set; }
        public Vector2 AttackOrigin => new(Position.X + BodyTexture.Width / 2, Position.Y + BodyTexture.Height / 2);
        public string BossName { get; }
        public enum AttackPattern
        {
            Pattern1,
            Pattern2,
            Pattern3,
        }
        private int _patternsCount;
        private Dictionary<AttackPattern, Action> _patternActionPairs;
        private Dictionary<AttackPattern, Action> _patternStatSetterPairs;
        public AttackPattern _currentAttackPattern;
        private float _patternChangeTime;
        public Timer PatternChanger;
        private float _shootCooldownTime;
        public Timer ShootCooldown;
        private float _summonCooldownTime;
        public Timer SummonCooldown;
        public bool IsExpired { get; private set; }
        public Rectangle BodyHitbox => new((int)Position.X, (int)Position.Y, BodyTexture.Width, BodyTexture.Height);
        public bool IsInvincible { get; private set; } = false;
        public bool IsPushable { get; } = false;

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
        public FloorOneBossSprite(Texture2D bodyTexture, Room parentRoomRef, Vector2 position, float maxHealthPoints, string bossName)
        {
            BodyTexture = bodyTexture;
            _parentRoomRef = parentRoomRef;
            Position = position;
            HealthPoints = maxHealthPoints;
            MaxHealthPoints = maxHealthPoints;
            ArmorSet = new();
            _baseArmor = new(ArmorType.Base, sliceX: 0.9F, pokeX: 0.9F, bluntX: 0.85F);
            ArmorSet.Add(_baseArmor);
            PatternChanger = new(0);
            SummonCooldown = new(0);
            ShootCooldown = new(0);
            _patternsCount = 3;
            _patternActionPairs = new()
            {
                [AttackPattern.Pattern1] = new(PatternOneAction),
                [AttackPattern.Pattern2] = new(PatternTwoAction),
                [AttackPattern.Pattern3] = new(PatternThreeAction)
            };
            _patternStatSetterPairs = new()
            {
                [AttackPattern.Pattern1] = new(PatternOneStatSetter),
                [AttackPattern.Pattern2] = new(PatternTwoStatSetter),
                [AttackPattern.Pattern3] = new(PatternThreeStatSetter)
            };
            BossName = bossName;
            BossHPBar.GetInstance().BossName = BossName;
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
        public void DrawHPBar()
        {
            BossHPBar.GetInstance().ShouldBeDrawn = true;
            BossHPBar.GetInstance().HealthPoints = this.HealthPoints;
            BossHPBar.GetInstance().MaxHealthPoints = this.MaxHealthPoints;
        }
        public void Update(float elapsedMilliseconds)
        {
            if (!IsExpired && GameBody.GetGameInstance().SceneManager.CurrentRoom != _parentRoomRef)
            {
                Respawn();
                return;
            }
            PatternChanger.Update(elapsedMilliseconds);
            ShootCooldown.Update(elapsedMilliseconds);
            SummonCooldown.Update(elapsedMilliseconds);
            if (PatternChanger.TimeLeft <= 0)
            {
                int newpat = RandomNumberGenerator.GetInt32(0, _patternsCount);
                _currentAttackPattern = (AttackPattern)newpat;
                PatternChanger.TimeLeft = _patternChangeTime;
                _patternStatSetterPairs[_currentAttackPattern].Invoke();
            }
            if (BodyHitbox.Intersects(GameBody.GetGameInstance().MainCharacter.BodyHitbox))
            {
                DamageInstance damage = new(_baseDamage, 1, DamageType.Poke, false);
                GameBody.GetGameInstance().MainCharacter.TakeDamage(in damage);
            }
            _patternActionPairs[_currentAttackPattern].Invoke();
        }
        private void PatternOneAction()
        {
            if (ShootCooldown.TimeLeft <= 0)
            {
                Vector2 force = Vector2.One;
                for (int i = 0; i < 8; i++)
                {
                    var projectile = new ProjectileSprite(TextureMapper.GetInstance().DruidProjectileSprite, AttackOrigin, force, new(5, 1, DamageType.Poke, false), 10000F);
                    GameBody.GetGameInstance().ProjectileSprites.Add(projectile);
                    force = force.Rotate(MathHelper.ToRadians(45));
                }
                ShootCooldown.TimeLeft = _shootCooldownTime;
            }
            if (SummonCooldown.TimeLeft <= 0)
            {
                var summon = new WaspSprite(AttackOrigin, _parentRoomRef);
                _parentRoomRef.EntitiesToSpawn.Push(summon);
                SummonCooldown.TimeLeft = _summonCooldownTime;
            }
        }
        private void PatternTwoAction()
        {
            if (ShootCooldown.TimeLeft <= 0)
            {
                Vector2 force = Vector2.One;
                for (int i = 0; i < 10; i++)
                {
                    var projectile = new ProjectileSprite(TextureMapper.GetInstance().DruidProjectileSprite, AttackOrigin, force, new(5, 1, DamageType.Poke, false), 10000F);
                    GameBody.GetGameInstance().ProjectileSprites.Add(projectile);
                    force = force.Rotate(MathHelper.ToRadians(36));
                }
                ShootCooldown.TimeLeft = _shootCooldownTime;
            }
        }
        private void PatternThreeAction()
        {
            if (SummonCooldown.TimeLeft <= 0)
            {
                var summon = new WaspSprite(AttackOrigin, _parentRoomRef);
                var summon2 = new WaspSprite(AttackOrigin, _parentRoomRef);
                _parentRoomRef.EntitiesToSpawn.Push(summon);
                _parentRoomRef.EntitiesToSpawn.Push(summon2);
                SummonCooldown.TimeLeft = _summonCooldownTime;
            }
        }
        private void PatternOneStatSetter()
        {
            _patternChangeTime = 7500F;
            _shootCooldownTime = 1500F;
            _summonCooldownTime = 3750F;
        }
        private void PatternTwoStatSetter()
        {
            _patternChangeTime = 4800F;
            _shootCooldownTime = 800F;
            _summonCooldownTime = 3750F;
        }
        private void PatternThreeStatSetter()
        {
            _patternChangeTime = 15000F;
            _shootCooldownTime = 1000F;
            _summonCooldownTime = 3000F;
        }
        public void Respawn()
        {
            HealthPoints = MaxHealthPoints;
        }
    }
}

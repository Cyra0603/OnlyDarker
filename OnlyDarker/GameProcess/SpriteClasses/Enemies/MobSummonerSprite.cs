using OnlyDarker.CommonUsing;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses.Enemies
{
    public class MobSummonerSprite : IYSortable, IMyUpdateable, IDamageable
    {
        private readonly Texture2D _texture;
        private readonly Vector2 _initialPosition;
        public Vector2 Position { get; set; }
        public Rectangle BodyHitbox => new((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
        private readonly Room _parentRoomRef;
        public bool IsExpired { get; set; }
        public bool IsInvincible { get; }
        public bool IsPushable { get; } = false;
        public int XPReward { get; }
        public bool IsSummoned { get; }
        public float MaxHealthPoints { get; }
        private float _healthPoints;
        public float HealthPoints
        {
            get => _healthPoints;
            set
            {
                _healthPoints = value;
                if(_healthPoints <= 0.1)
                {
                    _healthPoints = 0;
                }
                if (_healthPoints <= 0)
                {
                    IsExpired = true;
                    (this as IDamageable).SpawnXPOrbs();
                    _parentRoomRef.RoomColliders.Remove(BodyHitbox);
                }
            }
        }
        private Timer _summonTimer;
        private ISummonable _summonableEntity;
        private float _summonCooldownTime;
        private float _baseDamage;
        private int _maxSummons;
        public BaseArmor BaseArmor { get; }

        public MobSummonerSprite(Texture2D texture, ISummonable summonableEntity, Vector2 position, Room parentRoomRef, float contactDamage, BaseArmor baseArmor, float healthPoints, float summonCooldownMs, int maxSummons)
        {
            _texture = texture;
            Position = position;
            XPReward = 75;
            IsSummoned = false;
            _summonableEntity = summonableEntity;
            _initialPosition = position;
            _parentRoomRef = parentRoomRef;
            _baseDamage = contactDamage;
            BaseArmor = baseArmor;
            _summonCooldownTime = summonCooldownMs;
            _summonTimer.TimeLeft = RandomizeSpawnTime(_summonCooldownTime / 3);
            _maxSummons = maxSummons;
            _healthPoints = healthPoints;
            MaxHealthPoints = _healthPoints;
        }

        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White, 0F, Vector2.Zero, 1F, SpriteEffects.None, 1F);
        }

        public void Update(float elapsedMilliseconds)
        {
            _summonTimer.Update(elapsedMilliseconds);
            if (!IsExpired && GameBody.GetGameInstance().SceneManager.CurrentRoom != _parentRoomRef)
            {
                Respawn();
                return;
            }
            if(_maxSummons > 0 && _summonTimer.TimeLeft <= 0)
            {
                _summonableEntity.GetCopy(out var summon);
                _parentRoomRef.EntitiesToSpawn.Push(summon);
                _summonTimer.TimeLeft = RandomizeSpawnTime(_summonCooldownTime);
                _maxSummons--;
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
            Position = _initialPosition;
        }
        private float RandomizeSpawnTime(float maxTime)
        {
            return RandomNumberGenerator.GetInt32((int)maxTime / 3, (int)maxTime);
        }
    }
}


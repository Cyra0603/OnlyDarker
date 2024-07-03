using OnlyDarker.CommonUsing;
using System;
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
        public float MaxHealthPoints { get; }
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
        private Timer _summonTimer;
        private float _summonCooldownTime;
        private float _baseDamage;
        private Armor _baseArmor;
        public List<Armor> ArmorSet { get; protected set; }
        public MobSummonerSprite(Texture2D texture, Vector2 position, Room parentRoomRef, float contactDamage, Armor baseArmor, float healthPoints, float summonCooldownMs)
        {
            _texture = texture;
            Position = position;
            _initialPosition = position;
            _parentRoomRef = parentRoomRef;
            _baseDamage = contactDamage;
            _baseArmor = baseArmor;
            _summonCooldownTime = summonCooldownMs;
            _summonTimer.TimeLeft = RandomizeSpawnTime();
            ArmorSet = new List<Armor>
            {
                _baseArmor
            };
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
            if(_summonTimer.TimeLeft <= 0)
            {
                var summon = new WaspSprite(Position, _parentRoomRef);
                _parentRoomRef.EntitiesToSpawn.Add(summon);
                _summonTimer.TimeLeft = RandomizeSpawnTime();
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
        private float RandomizeSpawnTime()
        {
            return RandomNumberGenerator.GetInt32((int)_summonCooldownTime / 3, (int)_summonCooldownTime);
        }
    }
}


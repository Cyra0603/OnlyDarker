using OnlyDarker.CommonUsing;
using OnlyDarker.UI;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class WaspSprite : INonSortable, IMyUpdateable, IDamageable, ISummonable
    {
        public Texture2D Texture { get; }
        private readonly Vector2 _initialPosition;
        public Vector2 Position { get; set; }
        static Vector2 SwayOffset => new(0, (float)Math.Sin(GameBody.GetGameInstance().GetSwayFunctionValue() * SWAY_FREQUENCY) * SWAY_AMPLITUDE);
        public Rectangle BodyHitbox => new((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
        public Room ParentRoomRef { get; }
        const float SWAY_AMPLITUDE = 0.5F;
        const float SWAY_FREQUENCY = 5F;
        public bool IsExpired { get; set; }
        public bool IsInvincible { get; }
        public bool IsPushable { get; } = true;
        public bool IsSummoned { get; }
        public int XPReward { get; }
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
                    (this as IDamageable).DropXPOrbs();
                }
            }
        }
        private float _baseDamage = 1F;
        private float _baseSpeed = 0.7F;
        public BaseArmor BaseArmor { get; }
        public WaspSprite(Vector2 position, Room parentRoomRef, bool summoned)
        {
            Texture = GameBody.GetGameInstance().TextureMapper.WaspSpriteTexture;
            Position = position;
            XPReward = 25;
            IsPushable = true;
            IsSummoned = summoned;
            _initialPosition = position;
            ParentRoomRef = parentRoomRef;
            BaseArmor = new(pokeX: 0.95F, bluntX: 1.1F);
            _healthPoints = 10;
            MaxHealthPoints = _healthPoints;
        }

        public void Draw()
        {
            if (IsExpired)
            {
                GlobalUse.SpriteBatch.Draw(Texture, Position, null, Color.White, 0F, Vector2.Zero, 1F, SpriteEffects.FlipVertically, 1F);
                return;
            }
            SpriteEffects fliphz = SpriteEffects.None;
            if ((Position.X - GameBody.GetGameInstance().MainCharacter.Position.X) < 0)
            {
                fliphz = SpriteEffects.FlipHorizontally;
            }
            GlobalUse.SpriteBatch.Draw(Texture, Position, null, Color.White, 0F, Vector2.Zero, 1F, fliphz, 1F);
        }

        public void Update(float elapsedMilliseconds)
        {
            if (!IsExpired && GameBody.GetGameInstance().SceneManager.CurrentRoom != ParentRoomRef)
            {
                Respawn();
                return;
            }
            if (ParentRoomRef.Damageables.Any(entity => entity.BodyHitbox.Intersects(BodyHitbox) && entity != this))
            {
                foreach (var entity in ParentRoomRef.Damageables.Where(entity => entity.BodyHitbox.Intersects(BodyHitbox) && entity != this))
                {
                    var posDif = entity.Position - Position;
                    var force = posDif / posDif.Length();
                    entity.Push(in force);
                }
            }
            if (!BodyHitbox.Intersects(GameBody.GetGameInstance().MainCharacter.BodyHitbox))
            {
                var posDif = GameBody.GetGameInstance().MainCharacter.Position - Position;
                Position += posDif / posDif.Length() * _baseSpeed;
            }
            else
            {
                var posDif = GameBody.GetGameInstance().MainCharacter.Position - Position;
                Position += posDif / posDif.LengthSquared();
                DamageInstance damage = new(_baseDamage, 1, DamageType.Poke, false);
                GameBody.GetGameInstance().MainCharacter.TakeDamage(in damage);
            }
            Position += SwayOffset;
        }

        public void Respawn()
        {
            HealthPoints = MaxHealthPoints;
            Position = _initialPosition;
        }
        public void GetCopy(out ISummonable copy)
        {
            copy = new WaspSprite(Position, ParentRoomRef, true);
        }
    }
}

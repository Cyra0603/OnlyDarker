using OnlyDarker.CommonUsing;
using System.Collections.Generic;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class WaspSprite : IYSortable, IMyUpdateable, IDamageable
    {
        private readonly Texture2D _texture = GameBody.GetGameInstance().TextureMapper.WaspSpriteTexture;
        private readonly Vector2 _initialPosition;
        public Vector2 Position { get; set; }
        static Vector2 SwayOffset => new(0, (float)Math.Sin(GameBody.GetGameInstance().GetSwayFunctionValue() * SWAY_FREQUENCY) * SWAY_AMPLITUDE);
        public Rectangle BodyHitbox => new((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
        private readonly Room _parentRoomRef;
        const float SWAY_AMPLITUDE = 1F;
        const float SWAY_FREQUENCY = 5F;
        public bool IsExpired { get; set; }
        public bool IsInvincible { get; }
        public float MaxHealthPoints {get;}
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
        private float _baseDamage = 1F;
        private Armor _baseArmor;

        public List<Armor> ArmorSet { get; protected set; }



        public WaspSprite(Vector2 position, Room parentRoomRef)
        {
            Position = position;
            _initialPosition = position;
            _parentRoomRef = parentRoomRef;
            _baseArmor = new(ArmorType.Base, pokeX: 0.95F, bluntX: 1.1F);
            ArmorSet = new List<Armor>
            {
                _baseArmor
            };
            _healthPoints = 10;
            MaxHealthPoints = _healthPoints;
        }

        public void Draw()
        {
            if(IsExpired)
            {
                GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White, 0F, Vector2.Zero, 1F, SpriteEffects.FlipVertically, 1F);
                return;
            }
            SpriteEffects fliphz = SpriteEffects.None;
            if ((Position.X - GameBody.GetGameInstance().MainCharacter.Position.X) < 0)
            {
                fliphz = SpriteEffects.FlipHorizontally;
            }
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White,0F, Vector2.Zero,1F,fliphz, 1F);
        }

        public void Update(float elapsedMilliseconds)
        {
            if (!IsExpired && GameBody.GetGameInstance().SceneManager.CurrentRoom != _parentRoomRef)
            {
                Respawn();
                return;
            }
            var posDif = GameBody.GetGameInstance().MainCharacter.Position - Position;
            Position += posDif / posDif.Length();
            Position += SwayOffset;
            if (BodyHitbox.Intersects(GameBody.GetGameInstance().MainCharacter.BodyHitbox))
            {
                GameBody.GetGameInstance().MainCharacter.TakeDamage(new(_baseDamage, 1, DamageType.Poke, false));
            }
        }
        public void Respawn()
        {
            HealthPoints = MaxHealthPoints;
            Position = _initialPosition;
        }

    }
}

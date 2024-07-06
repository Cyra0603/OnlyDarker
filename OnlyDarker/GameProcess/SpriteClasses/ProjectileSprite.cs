using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class ProjectileSprite
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; private set; }
        public Vector2 Origin { get; }
        public Vector2 Force { get; private set; }

        public Timer Lifetime;
        public Rectangle HurtBox => new((Position - Origin).ToPoint(), new(_texture.Width, _texture.Height));
        private DamageInstance _damageInstance;
        public ProjectileSprite(Texture2D texture, Vector2 position, Vector2 force, DamageInstance damageInstance, float lifetime)
        {
            _texture = texture;
            Position = position;
            Origin = new(_texture.Width / 2, _texture.Height / 2);
            Force = force;
            _damageInstance = damageInstance;
            Lifetime = new(lifetime);
        }
        public void Update(float elapsedMilliseconds)
        {
            Position += Force;
            Lifetime.Update(elapsedMilliseconds);
            if (IntersectsCharacterHitbox())
            {
                GameBody.GetGameInstance().MainCharacter.TakeDamage(in _damageInstance);
                Lifetime.TimeLeft = 0;
            }
        }
        private bool IntersectsCharacterHitbox() => Lifetime.TimeLeft > 0 && this.HurtBox.Intersects(GameBody.GetGameInstance().MainCharacter.BodyHitbox);
        public void Draw()
        {
            var rotation = Math.Atan2((double)(Force + Position).Y - Position.Y, (double)(Force + Position).X - Position.X);
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White, (float)rotation, Origin, 1F, SpriteEffects.None, 1F);
        }
        public void ChangeForce(Vector2 force)
        {
            Force = force;
        }
    }
}

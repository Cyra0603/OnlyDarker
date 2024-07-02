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
        public Rectangle HurtBox => new(new((int)Position.X - _texture.Width, (int)Position.Y - _texture.Height), new(_texture.Width, _texture.Height));
        private DamageInstance _damageInstance;
        public ProjectileSprite(Texture2D texture, Vector2 position, Vector2 force, DamageInstance damageInstance, float lifetime)
        {
            _texture = texture;
            Position = position;
            Origin = new(_texture.Width, _texture.Height);
            Force = force;
            _damageInstance = damageInstance;
            Lifetime = new(lifetime);
        }
        public void Update(float elapsedMilliseconds)
        {
            Position += Force;
            Lifetime.Update(elapsedMilliseconds);
            if (Lifetime.TimeLeft > 0 && this.HurtBox.Intersects(GameBody.GetGameInstance().MainCharacter.BodyHitbox))
            {
                GameBody.GetGameInstance().MainCharacter.TakeDamage(_damageInstance);
                Lifetime.TimeLeft = 0;
            }
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 1F);
        }
        public void ChangeForce(Vector2 force)
        {
            Force = force;
        }
        ~ProjectileSprite()
        {
            Debug.WriteLineIf(GlobalUse.IsDebugMode, "projectile disposed");
        }
    }
}

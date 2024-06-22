using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class ProjectileSprite
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; private set; }
        public Vector2 Origin { get;}
        public Vector2 Force { get; private set; }
        public Rectangle HurtBox => new(new((int)Position.X - _texture.Width / 2, (int)Position.Y - _texture.Height / 2), new(_texture.Width, _texture.Height));
        private DamageInstance _damageInstance;
        public ProjectileSprite(Texture2D texture, Vector2 position, Vector2 force, DamageInstance damageInstance)
        {
            _texture = texture;
            Position = position;
            Origin = new(_texture.Width, _texture.Height);
            Force = force;
            _damageInstance = damageInstance;
        }
        public void Update()
        {
            Position += Force;
            if(this.HurtBox.Intersects(GameBody.MainCharacter.BodyHitbox)) 
            {
                (GameBody.MainCharacter as IDamageable).TakeDamage(_damageInstance);
            }
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White, 0F, Origin, 1F,SpriteEffects.None, 1F);
        }
    }
}

using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
using OnlyDarker.PlayerClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public abstract class ProjectileSprite
    {
        public readonly Texture2D Texture;
        private Texture2D HitAnimation => TextureMapper.GetInstance().ProjectileHitAnimation;
        public Vector2 Position { get; protected set; }
        public Vector2 Origin { get; }
        public Vector2 Force { get; protected set; }
        public bool IsRotating { get; }
        public float RotationValue;
        public Timer Lifetime;
        public Rectangle HurtBox => new((Position - Origin).ToPoint(), new(Texture.Width, Texture.Height));
        protected DamageInstance _damageInstance;
        public ProjectileSprite(Texture2D texture, Vector2 position, Vector2 force, DamageInstance damageInstance, float lifetime, bool isRotating)
        {
            Texture = texture;
            Position = position;
            Origin = new(Texture.Width / 2, Texture.Height / 2);
            Force = force;
            _damageInstance = damageInstance;
            Lifetime = new(lifetime);
            IsRotating = isRotating;
            RotationValue = 0F;
        }
        public abstract void Update(float elapsedMilliseconds);

        protected bool CanDamageCharacter() => Lifetime.TimeLeft > 0 && this.HurtBox.Intersects(GameBody.GetGameInstance().MainCharacter.BodyHitbox);

        protected void CreateHitAnimation(Vector2 position)
        {
            var animation = new EffectAnimationManager(HitAnimation, HitAnimation.Height, HitAnimation.Height, HitAnimation.Width / HitAnimation.Height, animationFrequency: 8.3F);
            animation.Activate(new(position));
        }

        public void Draw()
        {
            var rotation = Math.Atan2((double)(Force + Position).Y - Position.Y, (double)(Force + Position).X - Position.X);
            rotation += RotationValue;
            GlobalUse.SpriteBatch.Draw(Texture, Position, null, Color.White, (float)rotation, Origin, 1F, SpriteEffects.None, 1F);
        }
        public void ChangeForce(Vector2 force)
        {
            Force = force;
        }
    }
    public class EnemyProjectileSprite : ProjectileSprite
    {
        public EnemyProjectileSprite(Texture2D texture, Vector2 position, Vector2 force, DamageInstance damageInstance, float lifetime, bool isRotating) : base(texture, position, force, damageInstance, lifetime, isRotating)
        {

        }
        public override void Update(float elapsedMilliseconds)
        {
            if (Lifetime.TimeLeft == 0)
                return;
            if (IsRotating)
            {
                RotationValue += Vector2.Distance(Position, Position + Force);
            }
            Position += Force;
            Lifetime.Update(elapsedMilliseconds);
            if (CanDamageCharacter())
            {
                GameBody.GetGameInstance().MainCharacter.TakeDamage(in _damageInstance);
                Lifetime.TimeLeft = 0;
                CreateHitAnimation(Position);
            }
        }
    }
    public class AllyProjectileSprite : ProjectileSprite
    {
        public AllyProjectileSprite(Texture2D texture, Vector2 position, Vector2 force, DamageInstance damageInstance, float lifetime, bool isRotating) : base(texture, position, force, damageInstance, lifetime, isRotating) { }
        public override void Update(float elapsedMilliseconds)
        {
            if (Lifetime.TimeLeft == 0)
                return;
            if (IsRotating)
            {
                RotationValue += Vector2.Distance(Position, Position + Force);
            }
            Position += Force;
            Lifetime.Update(elapsedMilliseconds);
            foreach (var target in GameBody.GetGameInstance().SceneManager.CurrentRoom.Damageables)
            {
                if (!this.HurtBox.Intersects(target.BodyHitbox))
                {
                    continue;
                }
                if (target.HealthPoints <= 0)
                {
                    continue;
                }
                target.TakeDamage(in _damageInstance);
                Lifetime.TimeLeft = 0;
                CreateHitAnimation(Position);
            }
            foreach (var collider in GameBody.GetGameInstance().SceneManager.CurrentRoom.RoomColliders)
            {
                if (collider.Intersects(this.HurtBox))
                {
                    Lifetime.TimeLeft = 0;
                    CreateHitAnimation(Position);
                }
            }
        }
    }
}

using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public interface IDamageable
    {
        Vector2 Position { get; set; }
        Rectangle BodyHitbox { get; }
        bool IsInvincible { get; }
        float HealthPoints { get; set; }
        float MaxHealthPoints { get; }
        List<Armor> ArmorSet { get; }
        void TakeDamage(DamageInstance damage)
        {
            if (!IsInvincible)
            {
                var locald = damage;
                foreach (var armor in ArmorSet)
                {
                    locald *= armor.Resistances.First(res => res.Type == locald.Type);
                }
                var dmgTaken = locald.ExtractValue();
                if (dmgTaken < 0) 
                    dmgTaken = 0;
                HealthPoints -= dmgTaken;
                var animator = new DamageNumberAnimationManager(new(Position.X, Position.Y), Math.Round((double)dmgTaken, 1).ToString(), damage.IsCritical);
            }
            else return;
        }
        void DrawHPBar()
        {
            var bounds = new Rectangle(BodyHitbox.Left, BodyHitbox.Top, BodyHitbox.Size.X, BodyHitbox.Size.X / 10);
            var currentHpBounds = new Rectangle(bounds.Location.X, bounds.Location.Y, (int)(bounds.Width * HealthPoints / MaxHealthPoints), bounds.Height);
            GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, currentHpBounds, Color.Green);
            GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, currentHpBounds, Color.Red * (1 - (HealthPoints / MaxHealthPoints )));
            GameBody.DrawRectangleOutline(bounds, Color.Black);
        }
    }
}

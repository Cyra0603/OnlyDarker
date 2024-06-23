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
        List<Armor> ArmorSet { get; }
        void TakeDamage(DamageInstance damage)
        {
            if (!IsInvincible)
            {
                var test = Stopwatch.StartNew();
                var locald = damage;
                foreach (var armor in ArmorSet)
                {
                    locald *= armor.Resistances.First(res => res.Type == locald.Type);
                }
                var dmgTaken = locald.ExtractValue();
                HealthPoints -= dmgTaken;
                var animator = new DamageNumberAnimationManager(new(Position.X, Position.Y), Math.Round((double)dmgTaken, 1).ToString(), damage.IsCritical);
                Debug.WriteLineIf(GlobalUse.IsDebugMode, $"Counting damage took {test.ElapsedTicks} ticks");
            }
            else return;
        }
    }
}

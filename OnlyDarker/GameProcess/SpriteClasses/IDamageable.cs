using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public interface IDamageable
    {
        Vector2 Position { get; set; }
        public Rectangle BodyHitbox { get;}
        public void TakeDamage(DamageInstance damage);
    }
}

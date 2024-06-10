using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public interface IDamageable
    {
        public Rectangle BodyHitbox { get;}
        public void TakeDamage(float damage);
    }
}

using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public interface IPickup
    {
        public string PickupName { get; set; }
        public static string PickupMessage => $"[E] Pickup ";
        public string PickupSound { get; }
        public bool IsOneUse { get; }
        public void ShowPickupMessage();
        public Rectangle MovementCollider { get; }
    }

    public class PickupSprite
    {
    }
}

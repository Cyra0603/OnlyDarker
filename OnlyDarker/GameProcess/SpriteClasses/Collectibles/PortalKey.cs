using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses.Collectibles
{
    public class PortalKey : IStackable
    {
        public Texture2D Texture { get; set; }

        public int MaxStackSize { get; set; }

        public bool IsExpired { get; set; }

        public string IngameName { get; set; }

        public string InteractionMessage { get; set; }

        public string Description { get; set; }

        public PortalKey()
        {
            Texture = GlobalUse.Content.Load<Texture2D>("PickupSprites/RoomPortalKey");
            MaxStackSize = 10;
            IsExpired = false;
            IngameName = "Portal key";
            InteractionMessage = " pick up";
            Description = "Can open an inactive portal";
        }
    }
}

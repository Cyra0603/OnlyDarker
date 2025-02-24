using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses.Collectibles
{
    public class IronKey : IStackable
    {
        public Texture2D Texture { get; set; }

        public int ID { get; }

        public int MaxStackSize { get; set; }

        public bool IsExpired { get; set; }

        public string IngameName { get; set; }

        public string InteractionMessage { get; set; }

        public string Description { get; set; }

        public IronKey(int iD)
        {
            Texture = GlobalUse.Content.Load<Texture2D>("PickupSprites/IronKey");
            ID = iD;
            MaxStackSize = 10;
            IsExpired = false;
            IngameName = "Iron key";
            InteractionMessage = " pick up";
            Description = "Can open an things that need a key";
        }
    }
}

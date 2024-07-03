using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess
{
    public class TextureMapper
    {
        public Texture2D HeartPickupSpriteTexture { get; private set; }
        public Texture2D WaspSpriteTexture { get; private set; }
        public Texture2D TargetDummySpriteTexture { get; private set; }
        public TextureMapper()
        {
            WaspSpriteTexture = GlobalUse.Content.Load<Texture2D>("Entities/Floor/One/WaspSprite");
            HeartPickupSpriteTexture = GlobalUse.Content.Load<Texture2D>("PickupSprites/HeartPickupSprite");
            TargetDummySpriteTexture = GlobalUse.Content.Load<Texture2D>("Entities/TargetDummy/TargetDummy");
        }
    }
}

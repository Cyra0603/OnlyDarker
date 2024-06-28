using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess
{
    public static class TextureMapper
    {
        public static bool _isLoaded { get; private set; } = false;
        public static Texture2D HeartPickupSpriteTexture { get; private set; }
        public static Texture2D WaspSpriteTexture { get; private set; }
        public static void LoadTextures()
        {
            if (_isLoaded)
                return;
            WaspSpriteTexture = GlobalUse.Content.Load<Texture2D>("Entities/Floor/One/WaspSprite");
            HeartPickupSpriteTexture = GlobalUse.Content.Load<Texture2D>("PickupSprites/HeartPickupSprite");
            _isLoaded = true;
        }

    }
}

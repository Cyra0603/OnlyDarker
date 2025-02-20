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
        private static TextureMapper _instance;
        public Texture2D ShadowTexture { get; }
        public Texture2D CoinTexture { get; }
        public Texture2D UICoinTexture { get; }
        public Texture2D DummyProjectileSprite { get;private set; }
        public Texture2D DruidProjectileSprite { get; private set; }
        public Texture2D ArrowProjectileSprite { get; private set; }
        public Texture2D IronShurikenSprite { get; private set; }
        public Texture2D ProjectileHitAnimation { get; private set; }
        public Texture2D HeartPickupSpriteTexture { get; private set; }
        public Texture2D XPOrbSpriteTexture { get; private set; }
        public Texture2D XPOrbSpriteTrailTexture { get; private set; }
        public Texture2D WaspSpriteTexture { get; private set; }
        public Texture2D TargetDummySpriteTexture { get; private set; }
        public Texture2D PortalTexture { get; private set; }
        public Texture2D BrokenPortalTexture { get; private set; }
        private TextureMapper()
        {
            ShadowTexture = GlobalUse.Content.Load<Texture2D>("Shadow");
            CoinTexture = GlobalUse.Content.Load<Texture2D>("PickupSprites/Coin");
            UICoinTexture = GlobalUse.Content.Load<Texture2D>("UI/CoinCount");
            WaspSpriteTexture = GlobalUse.Content.Load<Texture2D>("Entities/Floor/One/WaspSprite");
            HeartPickupSpriteTexture = GlobalUse.Content.Load<Texture2D>("PickupSprites/HeartPickupSprite");
            XPOrbSpriteTexture = GlobalUse.Content.Load<Texture2D>("PickupSprites/XPOrb");
            XPOrbSpriteTrailTexture = GlobalUse.Content.Load<Texture2D>("PickupSprites/XPOrbTrail");
            TargetDummySpriteTexture = GlobalUse.Content.Load<Texture2D>("Entities/TargetDummy/TargetDummy");
            DummyProjectileSprite = GlobalUse.Content.Load<Texture2D>("Entities/TargetDummyShooter/DummyShooterProjectile");
            DruidProjectileSprite = GlobalUse.Content.Load<Texture2D>("Entities/DruidBoss/DruidSeed");
            ArrowProjectileSprite = GlobalUse.Content.Load<Texture2D>("Weapons/ArrowProjectile");
            IronShurikenSprite = GlobalUse.Content.Load<Texture2D>("Weapons/IronShurikenProjectile");
            ProjectileHitAnimation = GlobalUse.Content.Load<Texture2D>("Effects/ProjectileHitAnimation");
            PortalTexture = GlobalUse.Content.Load<Texture2D>("RoomGates/RoomGate1");
            BrokenPortalTexture = GlobalUse.Content.Load<Texture2D>("RoomGates/RoomGate2");
            _instance = this;
        }
        public static TextureMapper GetInstance()
        {
            if(_instance is null)
            {
                return new TextureMapper();
            }
            return _instance;
        }
    }
}

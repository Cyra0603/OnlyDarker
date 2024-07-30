using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public interface ICollectible
    {
        Texture2D Texture { get; }
        Vector2 Position { get; set; }
        string IngameName { get; }
        string TextureFileName { get; }
        string Description { get; }
        Rectangle MovementCollider { get; }
        virtual Rectangle ShadowRect => new(MovementCollider.Location.X, MovementCollider.Location.Y + MovementCollider.Height, MovementCollider.Width, MovementCollider.Height / 5);
        static Vector2 SwayOffset => new(0, (float)Math.Sin(GameBody.GetGameInstance().GetSwayFunctionValue() * SWAY_FREQUENCY) * SWAY_AMPLITUDE);
        const float PUSHAWAY_MAX_FORCE = 1F;
        const float SWAY_AMPLITUDE = 2F;
        const float SWAY_FREQUENCY = 5F;
        void Collect();
        void CollectibleDraw()
        {
            GlobalUse.SpriteBatch.Draw(TextureMapper.GetInstance().ShadowTexture, ShadowRect, Color.White);
            GlobalUse.SpriteBatch.Draw(Texture, Position + SwayOffset, Color.White);
        }
        void ManageCollisions()
        {
            foreach (var collider in GameBody.GetGameInstance().SceneManager.CurrentRoom.RoomColliders)
            {
                if (MovementCollider.Intersects(collider))
                {
                    var posDif = Vector2.Normalize(collider.Location.ToVector2() - Position);
                    var newForce = (new Vector2(PUSHAWAY_MAX_FORCE) / posDif.Length());
                    Position += newForce;
                }
            }
        }
    }
}

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
        static Vector2 SwayOffset => new(0,(float)Math.Cos(GameBody.GetSwayFunctionValue() * SwayAmplitude));
        const float SwayAmplitude = 5F;
        void Collect();
        void DynamicDraw()
        {
            GlobalUse.SpriteBatch.Draw(Texture, Position + SwayOffset, Color.White);
        }
    }
}

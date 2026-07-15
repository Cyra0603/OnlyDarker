using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.CommonUsing
{
    public class SpriteSheet
    {
        public Texture2D Texture { get; set; }

        public int FrameWidth { get; set; }

        public int FrameHeight { get; set; }

        public int FrameCount { get; set; }

        public SpriteSheet(Texture2D texture, int frameWidth, int frameHeight, int frameCount)
        {
            Texture = texture;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            FrameCount = frameCount;
        }

        public Rectangle GetSourceRectangle(in int index)
        {
            if (index >= FrameCount)
                throw new ArgumentOutOfRangeException(nameof(index), index, null);
            return new Rectangle(index * FrameWidth, 0, FrameWidth, FrameHeight);
        }

        public Vector2 GetTextureOrigin()
        {
            return new Vector2(FrameWidth / 2, FrameHeight / 2);
        }
    }
}

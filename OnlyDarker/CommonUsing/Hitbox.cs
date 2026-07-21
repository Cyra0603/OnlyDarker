using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.CommonUsing
{
    public struct Hitbox
    {
        private Rectangle internalRect;

        public bool IsExpired;

        public Hitbox (Rectangle rectangle, bool isexpired)
        {
            internalRect = rectangle;
            IsExpired = isexpired;
        }
        public Hitbox (Rectangle rectangle)
        {
            internalRect = rectangle;
            IsExpired = false;
        }
        public readonly Rectangle GetBounds()
        {
            return internalRect;    
        }
        public readonly bool Intersects(in Rectangle rectangle)
        {
            return internalRect.Intersects(rectangle);
        }
        public readonly bool Contains(in Point point)
        {
            return internalRect.Contains(point);
        }
        public readonly bool Contains(in Vector2 point)
        {
            return internalRect.Contains(point);
        }
    }
}

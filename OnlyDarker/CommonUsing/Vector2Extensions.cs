using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.CommonUsing
{
    public static class Vector2Extensions
    {
        public static Vector2 Rotate(this Vector2 vector, double radians)
        {
            var ca = Math.Cos(radians);
            var sa = Math.Sin(radians);
            return new Vector2((float)(ca * vector.X - sa * vector.Y), (float)(sa * vector.X + ca * vector.Y));
        }
        public static Vector2 RotateAround(this Vector2 vector, Vector2 origin, double radians)
        {
            Vector2 dif = vector - origin;
            return dif.Rotate(MathHelper.ToRadians((float)radians)) + origin;
        }
        public static float Cross(this Vector2 thisv, Vector2 v)
        {
            return thisv.X * v.Y - thisv.Y * v.X;
        }
    }
}

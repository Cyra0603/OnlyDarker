using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OnlyDarker.CommonUsing
{
    public struct Triangle(Vector2 x, Vector2 y, Vector2 z)
    {
        public Vector2 X = x;
        public Vector2 Y = y;
        public Vector2 Z = z;

        public bool Intersects(Rectangle rectangle)
        {
            if (rectangle.Contains(X) || rectangle.Contains(Y) || rectangle.Contains(Z))
                return true;
            Vector2 rectcenter = new(rectangle.Location.X + rectangle.Width / 2, rectangle.Location.Y + rectangle.Height / 2);
            Vector2 topleft = rectangle.Location.ToVector2();
            Vector2 topright = new(topleft.X + rectangle.Width, topleft.Y);
            Vector2 bottomleft = new(topleft.X, topleft.Y + rectangle.Height);
            Vector2 bottomright = new(bottomleft.X + rectangle.Width, bottomleft.Y);
            if (this.Contains(rectcenter))
            {
                return true;
            }
            Span<Line> rectlines =
            [
                new(topleft, topright),
                new(topright, bottomright),
                new(bottomright, bottomleft),
                new(bottomleft, topleft),
            ];
            Line line1 = new(Z, Y);
            Line line2 = new(Z, X);
            Line line3 = new(X, Y);
            for (int i = 0; i < rectlines.Length; i++)
            {
                if (rectlines[i].Intersects(line1) || rectlines[i].Intersects(line2) || rectlines[i].Intersects(line3))
                {
                    return true;
                }
            }
            return false;
        }
        public bool Contains(Vector2 position)
        {
            var x = position.X;
            var y = position.Y;
            var ax = this.X.X;
            var bx = this.Y.X;
            var cx = this.Z.X;
            var ay = this.X.Y;
            var by = this.Y.Y;
            var cy = this.Z.Y;
            //float side_1 = (position.X - b.X) * (a.Y - b.Y) - (a.X - b.X) * (position.Y - b.Y);
            //float side_2 = (position.X - c.X) * (b.Y - c.Y) - (b.X - c.X) * (position.Y - c.Y);
            //float side_3 = (position.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (position.Y - a.Y);
            //return ((side_1 < 0.0) == (side_2 < 0.0) == (side_3 < 0.0));
            var det = (bx - ax) * (cy - ay) - (by - ay) * (cx - ax);
            return det * ((bx - ax) * (y - ay) - (by - ay) * (x - ax)) >= 0 &&
                    det * ((cx - bx) * (y - by) - (cy - by) * (x - bx)) >= 0 &&
                    det * ((ax - cx) * (y - cy) - (ay - cy) * (x - cx)) >= 0;
        }
    }
    public struct Line(Vector2 x, Vector2 y)
    {
        public Vector2 StartPoint = x;
        public Vector2 EndPoint = y;
        public bool Intersects(Line line)
        {
            Vector2 p = this.StartPoint;
            Vector2 p2 = this.EndPoint;
            Vector2 q = line.StartPoint;
            Vector2 q2 = line.EndPoint;
            float epsilon = 1e-10F;
            var r = p2 - p;
            var s = q2 - q;
            var rxs = r.Cross(s);
            var qpxr = (q - p).Cross(r);
            if (Math.Abs(rxs) < epsilon && Math.Abs(qpxr) < epsilon)
            {
                //if ((0 <= (q - p) * r && (q - p) * r <= r * r) || (0 <= (p - q) * s && (p - q) * s <= s * s))
                //    return true;
                return false;
            }
            if (Math.Abs(rxs) < epsilon && !(Math.Abs(qpxr) < epsilon))
                return false;
            var t = (q - p).Cross(s) / rxs;
            var u = (q - p).Cross(r) / rxs;
            if (!(Math.Abs(rxs) < epsilon) && (0 <= t && t <= 1) && (0 <= u && u <= 1))
            {
                return true;
            }
            return false;
        }
    }
}

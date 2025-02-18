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

        public readonly bool Intersects(in Rectangle rectangle)
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
        public readonly bool Contains(in Vector2 position)
        {
            float x = position.X;
            float y = position.Y;
            float ax = this.X.X;
            float bx = this.Y.X;
            float cx = this.Z.X;
            float ay = this.X.Y;
            float by = this.Y.Y;
            float cy = this.Z.Y;
            float det = (bx - ax) * (cy - ay) - (by - ay) * (cx - ax);
            return det * ((bx - ax) * (y - ay) - (by - ay) * (x - ax)) >= 0 &&
                    det * ((cx - bx) * (y - by) - (cy - by) * (x - bx)) >= 0 &&
                    det * ((ax - cx) * (y - cy) - (ay - cy) * (x - cx)) >= 0;
        }
    }

    public struct Line(Vector2 x, Vector2 y)
    {
        public Vector2 StartPoint = x;
        public Vector2 EndPoint = y;
        public readonly bool Intersects(Line line)
        {
            Vector2 p = this.StartPoint;
            Vector2 p2 = this.EndPoint;
            Vector2 q = line.StartPoint;
            Vector2 q2 = line.EndPoint;
            float epsilon = 1e-10F;
            Vector2 r = p2 - p;
            Vector2 s = q2 - q;
            float rxs = r.Cross(s);
            float qpxr = (q - p).Cross(r);
            if (Math.Abs(rxs) < epsilon && Math.Abs(qpxr) < epsilon)
            {
                return false;
            }
            if (Math.Abs(rxs) < epsilon && !(Math.Abs(qpxr) < epsilon))
                return false;
            float t = (q - p).Cross(s) / rxs;
            float u = (q - p).Cross(r) / rxs;
            if (!(Math.Abs(rxs) < epsilon) && (0 <= t && t <= 1) && (0 <= u && u <= 1))
            {
                return true;
            }
            return false;
        }
    }

    public struct Circle(Vector2 center, float radius)
    {
        public Vector2 Center = center;
        public float Radius = radius;

        public readonly bool Contains(in Vector2 p)
        {
            if(Vector2.Distance(this.Center, p) < Radius) 
                return true;
            return false;
        }

        public readonly bool Contains(in Point p)
        {
            return this.Contains(p.ToVector2());
        }
    }
}

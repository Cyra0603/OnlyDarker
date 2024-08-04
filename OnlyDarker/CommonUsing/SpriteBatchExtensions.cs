using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.CommonUsing
{
    internal static class SpriteBatchExtensions
    {
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1F)
        {
            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            DrawLine(spriteBatch, point1, distance, angle, color, thickness);
        }
        public static void DrawLine(this SpriteBatch spriteBatch, Line line, Color color, float thickness = 1F)
        {
            var point1 = line.StartPoint;
            var point2 = line.EndPoint;  
            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            DrawLine(spriteBatch, point1, distance, angle, color, thickness);
        }
        //public static void DrawLine(this SpriteBatch spriteBatch, Line line, Color color, float thickness = 1F)
        //{
        //    var distance = Vector2.Distance(line.StartPoint, line.EndPoint);
        //    var angle = (float)Math.Atan2(line.EndPoint.Y - line.StartPoint.Y, line.EndPoint.X - line.StartPoint.X);
        //    DrawLine(spriteBatch, line.StartPoint, distance, angle, color, thickness);
        //}
        private static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1F)
        {
            var origin = new Vector2(0F, 0.5F);
            var scale = new Vector2(length, thickness);
            spriteBatch.Draw(GameBody.EmptyTexture, point, null, color, angle, origin, scale, SpriteEffects.None, 0F);
        }
    }
}


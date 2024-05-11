global using System;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using OnlyDarker.CommonUsing.Rendering;

namespace OnlyDarker.CommonUsing
{
    public enum Direction
    {
        Left = 1,
        Right,
        Up,
        Down
    }
    public enum Floor
    {
        One = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Eleven,
        Twelve,
        Thirteen
    }
    public enum RoomType
    {
        Entry,
        Treasure,
        Encounter,
        Secret,
        Puzzle,
        Boss,
    }
    public static class GlobalUse
    {
        public const float DIMENSION_DRAWING_OFFSET = 0.667F;
        public const float PIXEL_OFFSET = 4F;
        public static ContentManager Content { get; set; }
        public static SpriteBatch SpriteBatch { get; set; }
        public static Point WindowSize { get; set; }
        public static float TicksToMilliseconds(long ticks)
        {
            return ticks / 10000;
        }
    }
}

global using System;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace OnlyDarker.CommonUsing
{
    public enum ChestType
    {
        Wooden,
        Iron,
        Golden,
        Luxurious
    }
    public enum ArmorType
    {
        Base,
        Helmet,
        Chest,
        Pants,
        Gloves,
        Boots,
    }
    public enum DamageType
    {
        Slice,
        Poke,
        Blunt
    }
    public enum Direction
    {
        Left,
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
        public static SpriteFont Arial { get; set; }
        public static SpriteFont MainFont { get; set; }
        public static ContentManager Content { get; set; }
        public static SpriteBatch SpriteBatch { get; set; }
        public static Point WindowSize { get; set; }
        public readonly static Random RNG = new();
        public readonly static RandomNumberGenerator rnjesus = RandomNumberGenerator.Create();
        public static bool IsDebugMode { get; private set; } = false;
        public static void ToggleDebugMode()
        {
            IsDebugMode = !IsDebugMode;
        }
        public static bool TryChance (float chance)
        {
            var value = RandomNumberGenerator.GetInt32(0, 101);
            return value < chance;
        }
        public static bool TryBasicRNG(float chance)
        {
            var value = RNG.Next(0,101);
            return value < chance;
        }
    }
}

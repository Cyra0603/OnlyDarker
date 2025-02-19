global using System;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace OnlyDarker.CommonUsing
{
    public enum GameState
    {
        Paused,
        IsRunning,
    }
    public enum ChestType
    {
        Wooden,
        Iron,
        Golden,
        Luxurious,
    }
    public enum ArmorType
    {
        Helmet,
        Chest,
        Pants,
        Gloves,
        Boots,
        Accessory,
    }
    public enum DamageType
    {
        Slice,
        Poke,
        Blunt,
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
        Empty,
        Entry,
        Treasure,
        Encounter,
        Shop,
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
        public static readonly int CurrentSeed;
        public static Point WindowSize { get; set; }
        public readonly static Random SeededStandartRNG;
        public readonly static RandomNumberGenerator rnjesus = RandomNumberGenerator.Create();
        public static bool IsDebugMode { get; private set; } = false;
        static GlobalUse()
        {
            CurrentSeed = RandomNumberGenerator.GetInt32(10000000, 100000000);
            SeededStandartRNG = new(CurrentSeed);
        }
        public static void ToggleDebugMode()
        {
            IsDebugMode = !IsDebugMode;
        }
        public static bool Roll(float chance)
        {
            var value = RandomNumberGenerator.GetInt32(0, 101);
            return value <= chance;
        }
        public static bool BasicRoll(float chance)
        {
            var value = SeededStandartRNG.Next(0, 101);
            return value <= chance;
        }
        public static Color[,] TextureTo2DArray(Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);

            Color[,] colors2D = new Color[texture.Width, texture.Height];
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    colors2D[x, y] = colors1D[x + y * texture.Width];

            return colors2D;
        }
        public static string GetName(this Floor floorType) => floorType switch
        {
            Floor.One => nameof(Floor.One),
            Floor.Two => nameof(Floor.Two),
            Floor.Three => nameof(Floor.Three),
            Floor.Four => nameof(Floor.Four),
            Floor.Five => nameof(Floor.Five),
            Floor.Six => nameof(Floor.Six),
            Floor.Seven => nameof(Floor.Seven),
            Floor.Eight => nameof(Floor.Eight),
            Floor.Nine => nameof(Floor.Nine),
            Floor.Ten => nameof(Floor.Ten),
            Floor.Eleven => nameof(Floor.Eleven),
            Floor.Twelve => nameof(Floor.Twelve),
            Floor.Thirteen => nameof(Floor.Thirteen),
            _ => throw new ArgumentOutOfRangeException(nameof(floorType), floorType, null)
        };
        public static string GetName(this RoomType roomType) => roomType switch
        {
            RoomType.Empty => nameof(RoomType.Empty),
            RoomType.Entry => nameof(RoomType.Entry),
            RoomType.Encounter => nameof(RoomType.Encounter),
            RoomType.Shop => nameof(RoomType.Shop),
            RoomType.Puzzle => nameof(RoomType.Puzzle),
            RoomType.Secret => nameof(RoomType.Secret),
            RoomType.Treasure => nameof(RoomType.Treasure),
            RoomType.Boss => nameof(RoomType.Boss),
            _ => throw new ArgumentOutOfRangeException(nameof(roomType), roomType, null)
        };
        public static string GetName(this DamageType damageType) => damageType switch
        {
            DamageType.Poke => nameof(DamageType.Poke),
            DamageType.Slice => nameof(DamageType.Slice),
            DamageType.Blunt => nameof(DamageType.Blunt),
            _ => throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null)
        };
        public static string GetName(this ArmorType armorType) => armorType switch
        {
            ArmorType.Helmet => nameof(ArmorType.Helmet),
            ArmorType.Chest => nameof(ArmorType.Chest),
            ArmorType.Pants => nameof(ArmorType.Pants),
            ArmorType.Gloves => nameof(ArmorType.Gloves),
            ArmorType.Boots => nameof(ArmorType.Boots),
            ArmorType.Accessory => nameof(ArmorType.Accessory),
            _ => throw new ArgumentOutOfRangeException(nameof(armorType), armorType, null)
        };
        public static string GetName(this ChestType chestType) => chestType switch
        {
            ChestType.Wooden => nameof(ChestType.Wooden),
            ChestType.Iron => nameof(ChestType.Iron),
            ChestType.Golden => nameof(ChestType.Golden),
            ChestType.Luxurious => nameof(ChestType.Luxurious),
            _ => throw new ArgumentOutOfRangeException(nameof(chestType), chestType, null)
        };
        public static char ToChar(this Keys key) => key switch
        {
            Keys.Q => 'q',
            Keys.W => 'w',
            Keys.E => 'e',
            Keys.R => 'r',
            Keys.T => 't',
            Keys.Y => 'y',
            Keys.U => 'u',
            Keys.I => 'i',
            Keys.O => 'o',
            Keys.P => 'p',
            Keys.A => 'a',
            Keys.S => 's',
            Keys.D => 'd',
            Keys.F => 'f',
            Keys.G => 'g',
            Keys.H => 'h',
            Keys.J => 'j',
            Keys.K => 'k',
            Keys.L => 'l',
            Keys.Z => 'z',
            Keys.X => 'x',
            Keys.C => 'c',
            Keys.V => 'v',
            Keys.B => 'b',
            Keys.N => 'n',
            Keys.M => 'm',
            Keys.D1 => '1',
            Keys.D2 => '2',
            Keys.D3 => '3',
            Keys.D4 => '4',
            Keys.D5 => '5',
            Keys.D6 => '6',
            Keys.D7 => '7',
            Keys.D8 => '8',
            Keys.D9 => '9',
            Keys.D0 => '0',
            Keys.OemPeriod => '.',
            Keys.OemMinus => '_',
            Keys.OemComma => ',',
            Keys.Space => ' ',
            _ => default,
        };
    }
}

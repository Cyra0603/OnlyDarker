using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using OnlyDarker.CommonUsing;

namespace OnlyDarker
{
    public class Character
    {
        public int Width { get; set; } // = 90;
        public int Height { get; set; } // = 90;
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Speed { get; set; } = 10;
        public Direction Facing { get; set; } = Direction.Down;
        public Rectangle BodyHitbox { get; private set; }
        public Rectangle PlacementHitbox { get; private set; }
        public Character (int x, int y, Rectangle hitbox, Rectangle placementhitbox)
        {
            X = x;
            Y = y;
            BodyHitbox = hitbox;
            PlacementHitbox = placementhitbox;
        }
    }

    //internal class CharacterBodyHitbox (int width, int height, int x, int y, Rectangle hitbox)
    //{

    //}
    //internal class CharacterPlacementHitbox(int width, int height = 1, int x, int y, Rectangle hitbox)
    //{

    //}
}

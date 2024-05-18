using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess.SpriteClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess
{
    public class Level
    {
        private int Encounters { get; set; }
        public readonly Floor FloorType;
        public List<Room> BuiltFloor { get; private set; }
        public Level(Floor floor)
        {
            switch (floor)
            {
                case Floor.One:Encounters = 6;FloorType = floor; break;
            }
            var level = new List<Room>(Encounters);
            for (int i = 0; i < Encounters; i++)
            {
                level.Add(new Room(floor, RoomType.Encounter, this));
            }
            level = level.Prepend(new Room(floor, RoomType.Entry, this)).ToList();
            level.Add(new Room(floor, RoomType.Boss, this));
            level.Add(new Room(floor, RoomType.Secret, this));
            level.Insert(level.Count / 2, new Room(floor, RoomType.Treasure, this));

            for (int i = 0;i < level.Count; i++)
            {
                level[i].SetOrderNumber(i);
            }
            BuiltFloor = level;
        }

    }  
}

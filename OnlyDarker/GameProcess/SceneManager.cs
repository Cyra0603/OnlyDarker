using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace OnlyDarker.GameProcess
{
    public class SceneManager
    {
        public Room CurrentRoom { get; private set; }
        public Level CurrentLevel { get;private set; }
        public Dictionary<Floor, string> FloorNameAssigner = new()
        {
            {Floor.One, "The Forest"},
            {Floor.Two, "The Mines"}
        };
        
        public SceneManager(Level floor) 
        {
            CurrentLevel = floor;
            CurrentRoom = CurrentLevel.BuiltFloor.First(room => room.InstanceRoomType == RoomType.Entry);
        }

        public void GoToRoom (Room room)
        {
            CurrentRoom = room;
            GameBody.MainCharacter.SetRoomBounds(CurrentRoom.RoomSize, CurrentRoom.TileSize);
            GameBody.UpdateMinimap();
        }
    }
}

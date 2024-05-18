using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
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
            CurrentRoom = CurrentLevel.BuiltFloor[0];
        }
        public Room GetCurrentRoom()
        {
            return CurrentRoom;
        }
        public Level GetCurrentFloor()
        {
            return CurrentLevel;
        }
        public void GoToScene (int sceneIndex)
        {
            CurrentRoom = CurrentLevel.BuiltFloor[sceneIndex];
            GameBody.MainCharacter.SetRoomBounds(CurrentRoom.RoomSize, CurrentRoom.TileSize);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public interface ISummonable
    {
        Vector2 Position { get; set; }
        Room ParentRoomRef { get; }
        void GetCopy(out ISummonable copy);
    }
}

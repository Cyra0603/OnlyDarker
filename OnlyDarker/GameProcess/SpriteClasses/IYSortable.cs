using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public interface IYSortable
    {
        Vector2 Position { get; set; }
        void Draw();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public interface INonSortable
    {
        Texture2D Texture { get; }
        Vector2 Position { get; set; }
        bool IsExpired { get; }
        void Draw();
    }
}

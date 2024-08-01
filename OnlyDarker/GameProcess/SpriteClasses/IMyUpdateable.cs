using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public interface IMyUpdateable
    {
        bool IsExpired { get; }
        void Update(float elapsedMilliseconds);
    }
}

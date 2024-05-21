using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OnlyDarker.GameProcess
{
    public interface IFloorConfig
    {
        public Point GridSize { get; }
        public int Encounters { get; }
    }
    //GridSize Y and X values must be odd!!
    public class FloorOneConfig : IFloorConfig
    {
        Point IFloorConfig.GridSize => new(27, 27);
        int IFloorConfig.Encounters => 8;
        public FloorOneConfig() { }
    }
}

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
        public int MaxRooms { get; }
    }
    //GridSize Y and X values must be odd!!
    public class FloorOneConfig : IFloorConfig
    {
        private readonly int _gridSizeValue = 27;
        Point IFloorConfig.GridSize => new(_gridSizeValue, _gridSizeValue);
        int IFloorConfig.MaxRooms => (_gridSizeValue - 1) / 2;
        public FloorOneConfig() { }
    }
}

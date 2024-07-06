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
        Point GridSize { get; }
        int MaxRooms { get; }
    }
    public class FloorOneConfig : IFloorConfig
    {
        private readonly int _gridSizeValue = 27;
        public Point GridSize => new(_gridSizeValue, _gridSizeValue);
        public int MaxRooms => (_gridSizeValue - 1) / 2;
        public FloorOneConfig() { }
    }
    public class FloorTwoConfig : IFloorConfig
    {
        private readonly int _gridSizeValue = 29;
        public Point GridSize => new(_gridSizeValue, _gridSizeValue);
        public int MaxRooms => (_gridSizeValue - 1) / 2;
        public FloorTwoConfig() { }
    }
}

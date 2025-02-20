using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.Interfaces
{
    public interface IStackable
    {
        public Texture2D Texture { get; set; }

        public int MaxStackSize { get; set; }

        public bool IsExpired { get; set; }

        public string IngameName { get; set; }

        public string InteractionMessage { get; set; }

        public string Description { get; set; }
    }
}

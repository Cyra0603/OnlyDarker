using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.UI
{
    public class CurrentFloorBar
    {
        private Vector2 _barPosition;
        public CurrentFloorBar()
        {
            _barPosition = new(GlobalUse.WindowSize.X / 2, 0);
        }
        public void Draw()
        {
            GameBody.SceneManager.FloorNameAssigner.TryGetValue(GameBody.SceneManager.CurrentLevel.FloorType, out string floorName);
            var stringLength = GlobalUse.MainFont.MeasureString(floorName);
            GlobalUse.SpriteBatch.DrawString(GlobalUse.MainFont, $"{floorName}", (_barPosition - stringLength / 2), Color.White, 0F, Vector2.Zero, 1.1F, SpriteEffects.None, 0F);
        }
    }
}

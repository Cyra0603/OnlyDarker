using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.UI
{
    public class StatsBar
    {
        private Vector2 _statsPosition;
        public StatsBar()
        {
            _statsPosition = new(0, GlobalUse.WindowSize.Y / 5);
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.DrawString(
                GlobalUse.MainFont, (
                $" HP: {GameBody.MainCharacter.HealthPoints}\n\n " +
                $"Speed: {GameBody.MainCharacter.Speed}\n\n" +
                $"Tile X: {(int)(GameBody.MainCharacter.Position.X / 64)}\n\n" +
                $"Tile Y: {(int)(GameBody.MainCharacter.Position.Y / 42)}\n\n" +
                $"SEED: {GlobalUse.CurrentSeed}\n\n"
                ),
                _statsPosition, Color.White, 0F, _statsPosition, 0.6F, SpriteEffects.None, 0F);
        }
    }
}

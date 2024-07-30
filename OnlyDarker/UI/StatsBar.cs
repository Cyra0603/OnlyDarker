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
        private float posY => GlobalUse.WindowSize.Y / 5;
        public StatsBar()
        {
            _statsPosition = new(0, posY);
        }
        public void Draw()
        {
            if(GameBody.GetGameInstance().MainCharacter.Inventory.IsActive)
            {
                return;
            }
            GlobalUse.SpriteBatch.DrawString(
                GlobalUse.MainFont, (
                $" HP: {Math.Floor(GameBody.GetGameInstance().MainCharacter.Stats.HealthPoints)}\n\n " +
                $"Speed: {GameBody.GetGameInstance().MainCharacter.Stats.Speed}\n\n" +
                $"Tile X: {(int)(GameBody.GetGameInstance().MainCharacter.Position.X / 64)}\n\n" +
                $"Tile Y: {(int)(GameBody.GetGameInstance().MainCharacter.Position.Y / 42)}\n\n" +
                $"Position: {GameBody.GetGameInstance().MainCharacter.Position.X} : {GameBody.GetGameInstance().MainCharacter.Position.Y}\n\n" +
                $"SEED: {GlobalUse.CurrentSeed}\n\n"
                ),
                _statsPosition, Color.White, 0F, _statsPosition, 0.6F, SpriteEffects.None, 0F);
        }
    }
}

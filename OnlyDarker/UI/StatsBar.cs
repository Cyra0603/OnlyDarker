using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.UI
{
    public class StatsBar
    {
        public Texture2D UICoinTexture => TextureMapper.GetInstance().UICoinTexture;
        private Vector2 _statsPosition;
        private float PosY => GlobalUse.WindowSize.Y / 2.7F;
        private float CoinCountPosY => GlobalUse.WindowSize.Y / 11;
        public Rectangle CoinRect => new(0, (int)CoinCountPosY, UICoinTexture.Width, UICoinTexture.Height);
        public bool ShouldDrawStats = true;
        public StatsBar()
        {
            _statsPosition = new(15, PosY);
        }
        public void Draw()
        {
            if (GameBody.GetGameInstance().MainCharacter.Inventory.IsActive)
            {
                return;
            }
            var coinCount = $"{GameBody.GetGameInstance().MainCharacter.Stats.CoinCount}";
            var coinCountSize = GlobalUse.MainFont.MeasureString(coinCount);
            GlobalUse.SpriteBatch.Draw(UICoinTexture, CoinRect, Color.White);
            GlobalUse.SpriteBatch.DrawString(GlobalUse.MainFont, coinCount, new(CoinRect.Width + coinCountSize.X, CoinRect.Location.Y + CoinRect.Height / 2 - coinCountSize.Y / 2), Color.White, 0F, coinCountSize / 2, 1F, SpriteEffects.None, 0F);
            if (!ShouldDrawStats)
            {
                return;
            }
            GlobalUse.SpriteBatch.DrawString(
                GlobalUse.MainFont, (
                $"HP: {Math.Floor(GameBody.GetGameInstance().MainCharacter.Stats.HealthPoints)}\n\n " +
                $"Speed: {GameBody.GetGameInstance().MainCharacter.Stats.Speed}\n\n" +
                $"Tile X: {(int)(GameBody.GetGameInstance().MainCharacter.Position.X / 64)}\n\n" +
                $"Tile Y: {(int)(GameBody.GetGameInstance().MainCharacter.Position.Y / 42)}\n\n" +
                $"Position: {GameBody.GetGameInstance().MainCharacter.Position.X} : {GameBody.GetGameInstance().MainCharacter.Position.Y}\n\n" +
                $"SEED: {GlobalUse.CurrentSeed}\n\n" +
                $"Level: {GameBody.GetGameInstance().MainCharacter.Stats.CharacterLevel} [{GameBody.GetGameInstance().MainCharacter.Stats.XP} / {GameBody.GetGameInstance().MainCharacter.Stats.LevelThreshold}]\n\n" +
                $"Total XP: {GameBody.GetGameInstance().MainCharacter.Stats.TotalXP} \n\n" +
                $"CPU: {GameBody.GetGameInstance().DrawnCPUFrameTime}ms \n\n" +
                $"GPU: {GameBody.GetGameInstance().DrawnGPUFrameTime}ms \n\n" +
                $"RAM: {GameBody.GetGameInstance().AllocatedMemoryInMB}MB \n\n" +
                $"NETWORK: {GameBody.GetGameInstance().Ping}ms \n\n" 
                ),
                _statsPosition, Color.White, 0F, _statsPosition, 0.4F, SpriteEffects.None, 0F);
            var cpu = GameBody.GetGameInstance().DrawnCPUFrameTime;
            var gpu = GameBody.GetGameInstance().DrawnGPUFrameTime;
            var sad = 321;
        }
    }
}

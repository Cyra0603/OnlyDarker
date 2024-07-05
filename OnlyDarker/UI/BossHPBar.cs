using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.UI
{
    public class BossHPBar
    {
        private static BossHPBar _instance;
        private Vector2 _barPosition = new(GlobalUse.WindowSize.X / 2, GlobalUse.WindowSize.X / 17);
        public bool ShouldBeDrawn = false;
        public float HealthPoints;
        public float MaxHealthPoints;
        public string BossName { get; set; }
        private BossHPBar() 
        {
            _instance = this;
        }
        public static BossHPBar GetInstance()
        {
            if (_instance is null)
                return new BossHPBar();
            return _instance;
        }
        public void Draw()
        {
            if (ShouldBeDrawn)
            {
                float textScale = 0.7F;
                var stringSize = GlobalUse.MainFont.MeasureString(BossName);
                Vector2 textPos = new(_barPosition.X - stringSize.X / 2 * textScale, _barPosition.Y + stringSize.Y / 2 * textScale);
                GlobalUse.SpriteBatch.DrawString(GlobalUse.MainFont, $"{BossName}", textPos, Color.OrangeRed, 0F, Vector2.Zero, textScale, SpriteEffects.None, 0F);
                {
                    var bounds = new Rectangle((int)_barPosition.X - (int)stringSize.X, (int)(_barPosition.Y - stringSize.Y), (int)stringSize.X * 2, (int)stringSize.Y);
                    var currentHpBounds = new Rectangle(bounds.Location.X, bounds.Location.Y, (int)(bounds.Width * HealthPoints / MaxHealthPoints), bounds.Height);
                    GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, currentHpBounds, Color.Green);
                    GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, currentHpBounds, Color.Red * (1 - (HealthPoints / MaxHealthPoints)));
                    GameBody.DrawRectangleOutline(bounds, Color.Black, 2);
                }
            }
            ShouldBeDrawn = false;
        }
    }
}

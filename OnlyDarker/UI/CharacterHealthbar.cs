using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.UI
{
    public class CharacterHealthbar
    {
        private Texture2D _texture;
        private Rectangle _barBounds;
        public const int TEXTURE_ONE_FOURTH = 8;
        private Character СurrentCharacter => GameBody.GetGameInstance().MainCharacter;
        private float _healthPoints;
        public CharacterHealthbar(Texture2D texture)
        {
            _texture = texture;
            _healthPoints = СurrentCharacter.Stats.HealthPoints;
            AdjustBounds();
            СurrentCharacter.Stats.OnChangingHealth += ObserveHealthPoints;
        }
        public void ObserveHealthPoints(float healthPoints)
        {
            _healthPoints = healthPoints;
            AdjustBounds();
        }
        public void AdjustBounds()
        {
            _barBounds = new Rectangle(Point.Zero, new Point((int)Math.Ceiling(_healthPoints) * (_texture.Width / TEXTURE_ONE_FOURTH), _texture.Height));
        }
        public void StandaloneDraw()
        {
            GlobalUse.SpriteBatch.Begin(samplerState: SamplerState.LinearWrap);
            GlobalUse.SpriteBatch.Draw(_texture, Vector2.Zero, _barBounds, Color.White);
            GlobalUse.SpriteBatch.End();
        }
        public int GetTextureHeight()
        {
            return _texture.Height;
        }
    }
}

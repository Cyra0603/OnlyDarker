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
        private Character _currentCharacter => GameBody.MainCharacter;
        private float _healthPoints;
        public CharacterHealthbar(Texture2D texture)
        {
            _texture = texture;
            _healthPoints = _currentCharacter.HealthPoints;
            AdjustBounds(_healthPoints);
            _currentCharacter.OnChangingHealth += ObserveHealthPoints;
            _currentCharacter.OnChangingHealth += AdjustBounds;
        }
        public void ObserveHealthPoints(float healthPoints)
        {
            _healthPoints = healthPoints;
        }
        public void AdjustBounds(float healthPoints)
        {
            _barBounds = new Rectangle(Point.Zero, new Point((int)Math.Ceiling(_healthPoints) * (_texture.Width / 10), _texture.Height));
        }
        public void StandaloneDraw()
        {
            GlobalUse.SpriteBatch.Begin(samplerState: SamplerState.LinearWrap);
            GlobalUse.SpriteBatch.Draw(_texture, Vector2.Zero, _barBounds, Color.White);
            GlobalUse.SpriteBatch.End();
        }
    }
}

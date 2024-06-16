using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.UI
{
    public class CharacterStaminaBar
    {
        private Texture2D _texture;
        private Rectangle _barBounds;
        private Point _location => new(0, GlobalUse.WindowSize.Y / 17);
        private Character CurrentCharacter => GameBody.MainCharacter;
        private float _stamina;
        private float _maxStamina;
        private int _barHeight = 20;
        public CharacterStaminaBar(GraphicsDevice graphicsDevice)
        {
            _texture = new(graphicsDevice, 1, 1);
            _texture.SetData(new[] { Color.Green });
            _stamina = CurrentCharacter.Stamina;
            _maxStamina = CurrentCharacter.MaxStamina;
            AdjustBounds(_maxStamina);
            CurrentCharacter.OnChangingStamina += ObserveStamina;
            CurrentCharacter.OnChangingMaxStamina += ObserveMaxStamina;
            CurrentCharacter.OnChangingMaxStamina += AdjustBounds;
        }
        public void ObserveStamina(float stamina)
        {
            _stamina = stamina;
        }
        public void ObserveMaxStamina(float maxStamina)
        {
            _maxStamina = maxStamina;
        }
        public void AdjustBounds(float maxStamina)
        {
            _barBounds = new Rectangle(_location, new Point((int)maxStamina * 2, _barHeight));
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_texture, new Rectangle(_barBounds.Location, new Point((int)(_stamina * 2), _barBounds.Height)), Color.White);
            GameBody.DrawRectangleOutline(_barBounds, Color.Black, 2);
        }
    }
}

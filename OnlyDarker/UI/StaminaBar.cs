using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.UI
{
    public class StaminaBar
    {
        private Texture2D _texture;
        private Rectangle _barBounds;
        private Character СurrentCharacter => GameBody.MainCharacter;
        private float _stamina;
        private float _maxStamina;
        public StaminaBar(Texture2D texture)
        {
            _texture = texture;
            _stamina = СurrentCharacter.Stamina;
            _maxStamina = СurrentCharacter.MaxStamina;
            AdjustBounds();
            СurrentCharacter.OnChangingStamina += ObserveStamina;
        }
        public void ObserveStamina(float stamina)
        {
            _stamina = stamina;
        }
        public void ObserveMaxStamina(float maxStamina)
        {

        }
        public void AdjustBounds()
        {
            
        }
        public void StandaloneDraw()
        {
            
        }
    }
}

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
        private Rectangle _bounds;
        private int HealthCount => GameBody.MainCharacter.HealthPoints;
        public CharacterHealthbar(Texture2D texture)
        {
            _texture = texture;
            _bounds = _texture.Bounds;
        }
        public void Update()
        {

        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_texture, _bounds, Color.White);
        }
    }
}

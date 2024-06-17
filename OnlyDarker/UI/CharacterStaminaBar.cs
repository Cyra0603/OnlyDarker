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
        private Texture2D _notifiedTexture;
        private Rectangle _barBounds;
        private Point _location = new(0, GlobalUse.WindowSize.Y / 17);
        private Point _notifiedLocation;
        private Timer NotificationLifeSpan = new(0);
        private const float NOTIFICATION_LIFESPAN = 250F;
        private bool _notifiedNotEnoughStamina
        {
            get
            {
                if (NotificationLifeSpan.TimeLeft <= 0)
                    return false;
                else
                    return true;
            }
        }
        private Character CurrentCharacter => GameBody.MainCharacter;
        private float _stamina;
        private float _maxStamina;
        private int _barHeight = 20;
        private int _notifiedBarHeight;
        public CharacterStaminaBar(GraphicsDevice graphicsDevice)
        {
            _texture = new(graphicsDevice, 1, 1);
            _texture.SetData(new[] { Color.Green });
            _notifiedTexture = new(graphicsDevice, 1, 1);
            _notifiedTexture.SetData(new[] { Color.Red });
            _stamina = CurrentCharacter.Stamina;
            _maxStamina = CurrentCharacter.MaxStamina;
            AdjustBounds(_maxStamina);
            CurrentCharacter.OnChangingStamina += ObserveStamina;
            CurrentCharacter.OnChangingMaxStamina += ObserveMaxStamina;
            CurrentCharacter.OnChangingMaxStamina += AdjustBounds;
            CurrentCharacter.OnNotEnoughStamina += NotifyNotEnoughStamina;
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
            if (_notifiedNotEnoughStamina)
            {
                GlobalUse.SpriteBatch.Draw(_notifiedTexture, new Rectangle(_notifiedLocation, new Point((int)(_stamina * 2), _barBounds.Height)), Color.White);
                GameBody.DrawRectangleOutline(_barBounds, Color.Black, 2);
            }
            else
            {
                GlobalUse.SpriteBatch.Draw(_texture, new Rectangle(_barBounds.Location, new Point((int)(_stamina * 2), _barBounds.Height)), Color.White);
                GameBody.DrawRectangleOutline(_barBounds, Color.Black, 2);
            }
        }
        public void Update(float elapsedMilliseconds)
        {
            NotificationLifeSpan.Update(elapsedMilliseconds);
            if (_notifiedNotEnoughStamina)
            {
                int lx = GlobalUse.RNG.Next(_location.X - 3, _location.X + 4);
                int ly = GlobalUse.RNG.Next(_location.Y - 3, _location.Y + 6);
                _notifiedLocation = new Point(lx, ly);
            }

        }
        public void NotifyNotEnoughStamina()
        {
            if (NotificationLifeSpan.TimeLeft <= 0)
                NotificationLifeSpan.TimeLeft += NOTIFICATION_LIFESPAN;
            else return;
        }
    }
}

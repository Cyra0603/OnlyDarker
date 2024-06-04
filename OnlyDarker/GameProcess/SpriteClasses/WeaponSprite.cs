using OnlyDarker.CommonUsing;
using OnlyDarker.PlayerClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class WeaponSprite : IPickup
    {
        private Texture2D _texture;
        private Vector2 Position;
        public Rectangle MovementCollider { get; }

        public string PickupName { get; set; }
        public string PickupSound { get; } //temp
        private string _pickupMessage;
        public bool IsOneUse => false;

        public WeaponSprite(Vector2 position, WeaponType weaponType)
        {
            _texture = GlobalUse.Content.Load<Texture2D>("");
            Position = position;
            PickupName = weaponType.ToString();
            _pickupMessage = IPickup.PickupMessage + PickupName;
            MovementCollider = new(Position.ToPoint(), _texture.Bounds.Size);
        }
        public void ShowPickupMessage()
        {
            GlobalUse.SpriteBatch.DrawString(GlobalUse.MainFont, _pickupMessage, new Vector2(GlobalUse.WindowSize.Y, GlobalUse.WindowSize.X - GlobalUse.MainFont.MeasureString(_pickupMessage).X), Color.White);
        }
    }
}

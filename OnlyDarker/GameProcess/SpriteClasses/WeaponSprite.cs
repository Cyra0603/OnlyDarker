using OnlyDarker.CommonUsing;
using OnlyDarker.PlayerClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class WeaponSprite : IYSortable,IInteractive,ICollectible
    {
        public Texture2D Texture { get; }
        public Vector2 Position { get; set; }
        public Rectangle MovementCollider => new((int)Position.X - Texture.Width / 2, (int)Position.Y - Texture.Height / 2, Texture.Width, Texture.Height);
        private IWeapon _weaponReference;

        public string Name { get; set; }
        public string PickupSound { get; } //temp

        public string InteractionMessage => "pick up ";


        public WeaponSprite(IWeapon weaponReference, Vector2 position, string weaponName)
        {
            Texture = GlobalUse.Content.Load<Texture2D>("Weapons/" + weaponName);
            _weaponReference = weaponReference;
            Position = position;
            Name = weaponName;
        }
        public void Draw()
        {
            (this as ICollectible).DynamicDraw();  
        }
        public void Interact()
        {
            //SoundManager.PlaySoundEffect(PickupSound);
            Collect();
        }
        public void Collect()
        {
            GameBody.MainCharacter.CurrentWeapon = _weaponReference;
        }
    }
}

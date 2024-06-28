using OnlyDarker.CommonUsing;
using OnlyDarker.PlayerClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class WeaponSprite : IYSortable, IInteractive, ICollectible
    {
        public Texture2D Texture { get; }
        public Vector2 Position { get; set; }
        public Rectangle MovementCollider => new((int)Position.X - Texture.Width / 2, (int)Position.Y - Texture.Height / 2, Texture.Width, Texture.Height);
        public IWeapon WeaponInstance;

        public string Name { get; set; }
        public string PickupSound { get; } //temp
        public bool IsExpired { get; private set; } = false;

        public string InteractionMessage
        {
            get
            {
                if (GameBody.MainCharacter.CurrentWeapon is WeaponFist)
                    return "pick up ";
                else
                    return "swap to ";
            }
        }


        public WeaponSprite(Vector2 position, string weaponName)
        {
            Texture = GlobalUse.Content.Load<Texture2D>("Weapons/" + weaponName);
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
            if (GameBody.MainCharacter.CurrentWeapon is not WeaponFist)
            {
                GameBody.MainCharacter.CurrentWeapon.WeaponPickupSprite.Position = Position;
                GameBody.SceneManager.CurrentRoom.Interactives.Add(GameBody.MainCharacter.CurrentWeapon.WeaponPickupSprite);
                GameBody.SceneManager.CurrentRoom.ObjectsYSorted.Add(GameBody.MainCharacter.CurrentWeapon.WeaponPickupSprite);
            }
            GameBody.MainCharacter.CurrentWeapon = WeaponInstance;
            GameBody.SceneManager.CurrentRoom.Interactives.Remove(this);
            GameBody.SceneManager.CurrentRoom.ObjectsYSorted.Remove(this);

        }
    }
}

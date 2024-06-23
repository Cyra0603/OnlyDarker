using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess.SpriteClasses;

namespace OnlyDarker.PlayerClasses
{
    public enum WeaponType
    {
        Stick,
        Sword,
        Lance
    }
    public interface IWeapon
    {
        public float AttackRange { get; set; }
        public float AttackDamage { get; set; }
        public float AttackSpeed { get; set; }
        public DamageType WeaponDamageType { get; set; }
        public WeaponSprite WeaponPickupSprite { get; set; }
    }
    public class WeaponFist : IWeapon
    {
        public float AttackRange { get; set; }
        public float AttackDamage { get; set; }
        public float AttackSpeed { get; set; }
        public DamageType WeaponDamageType { get; set; }
        public WeaponSprite WeaponPickupSprite { get; set; }
        public WeaponFist()
        {
            AttackRange = 256F;
            AttackDamage = 50F;
            AttackSpeed = 2F;
            WeaponDamageType = DamageType.Blunt;
        }
    }
    public class WeaponSword : IWeapon
    {
        public float AttackRange { get; set; }
        public float AttackDamage { get; set; }
        public float AttackSpeed { get; set; }
        public DamageType WeaponDamageType { get; set; }
        public WeaponSprite WeaponPickupSprite { get; set; }

        public WeaponSword()
        {
            AttackRange = 100F;
            AttackDamage = 5F;
            AttackSpeed = 2F;
            WeaponDamageType = DamageType.Slice;
        }
    }

}

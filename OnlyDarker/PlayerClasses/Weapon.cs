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
        public float AttackRange { get; }
        public float AttackDamage { get; }
        public float AttackSpeed { get; }
        public DamageType WeaponDamageType { get; }
        public WeaponSprite WeaponPickupSprite { get; }
    }
    public class WeaponFist : IWeapon
    {
        public float AttackRange { get; }
        public float AttackDamage { get; }
        public float AttackSpeed { get; }
        public DamageType WeaponDamageType { get; }
        public WeaponSprite WeaponPickupSprite { get; }
        public WeaponFist()
        {
            AttackRange = 64F;
            AttackDamage = 2F;
            AttackSpeed = 2F;
            WeaponDamageType = DamageType.Blunt;
        }
    }
    public class WeaponSword : IWeapon
    {
        public float AttackRange { get; }
        public float AttackDamage { get; }
        public float AttackSpeed { get; }
        public DamageType WeaponDamageType { get; }
        public WeaponSprite WeaponPickupSprite { get; }

        public WeaponSword(WeaponSprite spriteRef)
        {
            AttackRange = 84F;
            AttackDamage = 5F;
            AttackSpeed = 2F;
            WeaponDamageType = DamageType.Slice;
            WeaponPickupSprite = spriteRef;
        }
    }
    public class WeaponStick : IWeapon
    {
        public float AttackRange { get; }
        public float AttackDamage { get; }
        public float AttackSpeed { get; }
        public DamageType WeaponDamageType { get; }
        public WeaponSprite WeaponPickupSprite { get; }

        public WeaponStick(WeaponSprite spriteRef)
        {
            AttackRange = 100F;
            AttackDamage = 2F;
            AttackSpeed = 3F;
            WeaponDamageType = DamageType.Blunt;
            WeaponPickupSprite = spriteRef;
        }
    }
    public class WeaponLance : IWeapon
    {
        public float AttackRange { get; }
        public float AttackDamage { get; }
        public float AttackSpeed { get; }
        public DamageType WeaponDamageType { get; }
        public WeaponSprite WeaponPickupSprite { get; }

        public WeaponLance(WeaponSprite spriteRef)
        {
            AttackRange = 100F;
            AttackDamage = 7F;
            AttackSpeed = 1.5F;
            WeaponDamageType = DamageType.Poke;
            WeaponPickupSprite = spriteRef;
        }
    }

}

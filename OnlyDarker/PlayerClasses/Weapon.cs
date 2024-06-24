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
            AttackRange = 256F;
            AttackDamage = 2F;
            AttackSpeed = 3F;
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

        public WeaponSword(Vector2 position)
        {
            AttackRange = 333F;
            AttackDamage = 5F;
            AttackSpeed = 2F;
            WeaponDamageType = DamageType.Slice;
            WeaponPickupSprite = new(this, position, "Sword");
        }
    }
    public class WeaponStick : IWeapon
    {
        public float AttackRange { get; }
        public float AttackDamage { get; }
        public float AttackSpeed { get; }
        public DamageType WeaponDamageType { get; }
        public WeaponSprite WeaponPickupSprite { get; }

        public WeaponStick(Vector2 position)
        {
            AttackRange = 400F;
            AttackDamage = 2F;
            AttackSpeed = 3F;
            WeaponDamageType = DamageType.Blunt;
            WeaponPickupSprite = new(this, position, "Stick");
        }
    }
    public class WeaponLance : IWeapon
    {
        public float AttackRange { get; }
        public float AttackDamage { get; }
        public float AttackSpeed { get; }
        public DamageType WeaponDamageType { get; }
        public WeaponSprite WeaponPickupSprite { get; }

        public WeaponLance(Vector2 position)
        {
            AttackRange = 400F;
            AttackDamage = 7F;
            AttackSpeed = 1.5F;
            WeaponDamageType = DamageType.Poke;
            WeaponPickupSprite = new(this, position, "Lance");
        }
    }

}

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
        public float AttackRangeBase { get; set; }
        public float AttackRange { get; set; }
        public float AttackDamageBase { get; set; }
        public float AttackDamage { get; set; }
        public float AttackSpeedBase { get; set; }
        public float AttackSpeed { get; set; }
        public DamageType WeaponDamageType { get; set; }
        public WeaponSprite WeaponPickupSprite { get; set; }
    }
    //public abstract class Weapon : IWeapon
    //{
    //    public float AttackRangeBase { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public float AttackRange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public float AttackDamageBase { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public float AttackDamage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public float AttackSpeedBase { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public float AttackSpeed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public DamageType WeaponDamageType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public WeaponSprite WeaponPickupSprite { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public bool IsOnCooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    //    public void Cooldown()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    public class WeaponFist : IWeapon
    {
        public float AttackRangeBase { get; set; }
        public float AttackRange { get; set; }
        public float AttackDamageBase { get; set; }
        public float AttackDamage { get; set; }
        public float AttackSpeedBase { get; set; }
        public float AttackSpeed { get; set; }
        public DamageType WeaponDamageType { get; set; }
        public WeaponSprite WeaponPickupSprite { get; set; }
        public WeaponFist()
        {
            AttackRangeBase = 256F;
            AttackDamageBase = 4F;
            AttackSpeedBase = 200F;
            WeaponDamageType = DamageType.Blunt;
        }
    }
    public class WeaponSword : IWeapon
    {
        public float AttackRange { get; set; }
        public float AttackRangeBase { get; set; }
        public float AttackDamage { get; set; }
        public float AttackDamageBase { get; set; }
        public float AttackSpeed { get; set; }
        public float AttackSpeedBase { get; set; }
        public DamageType WeaponDamageType { get; set; }
        public WeaponSprite WeaponPickupSprite { get; set; }

        public WeaponSword()
        {
            AttackRangeBase = 100F;
            AttackDamageBase = 5F;
            AttackSpeedBase = 2F;
            WeaponDamageType = DamageType.Slice;
        }
    }

}

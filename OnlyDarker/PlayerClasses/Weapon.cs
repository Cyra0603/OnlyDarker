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
        public bool IsOnCooldown { get; set; }
        public void Attack();
        public void Cooldown();
    }
    public class WeaponSword : IWeapon
    {
        public float AttackRange { get; set; }
        public float AttackRangeBase { get; set; }
        public float AttackDamage { get; set; }
        public float AttackDamageBase { get; set; }
        public float AttackSpeed { get; set; }
        public float AttackSpeedBase { get; set; }
        public bool IsOnCooldown { get; set; }
        private Character _characterReference;
        public DamageType WeaponDamageType { get; set; }
        public WeaponSprite WeaponPickupSprite { get; set; }

        public WeaponSword()
        {
            AttackRange = 100;
            AttackDamageBase = 5;
            AttackSpeedBase = 2F;
        }
        public void Attack()
        {
            if (IsOnCooldown) return;
            var damageArea = new Rectangle();
            Cooldown();
        }
        public async void Cooldown()
        {
            IsOnCooldown = true;
            await Task.Delay((int)(1000 / AttackSpeedBase));
            IsOnCooldown = false;
        }
    }
}

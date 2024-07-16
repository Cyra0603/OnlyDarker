using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess;
using OnlyDarker.GameProcess.SpriteClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.PlayerClasses
{
    public class Inventory
    {
        //data
        public readonly List<Armor> ArmorSet;
        public readonly Stats Stats;
        public IWeapon Weapon;
        public readonly ICollectible[] Slots;
        //drawing
        public Rectangle MainBounds => new(0, GlobalUse.WindowSize.Y / 4, GlobalUse.WindowSize.X / 5, GlobalUse.WindowSize.Y / 2);
        public Rectangle SlotsBounds => new(GlobalUse.WindowSize.X / 4, GlobalUse.WindowSize.Y, GlobalUse.WindowSize.Y / 2, GlobalUse.WindowSize.Y / 10);
        private const int SLOTS_DRAWING_OFFSET = 5;
        public Point SlotSize => new(SlotsBounds.Height - SLOTS_DRAWING_OFFSET, SlotsBounds.Height - SLOTS_DRAWING_OFFSET);

        public Inventory(List<Armor> armorSet, Stats stats, IWeapon weapon) 
        { 
            ArmorSet = armorSet;
            Stats = stats;
            Weapon = weapon;
            Slots = new ICollectible[10];
        }
        public bool TryStore(ICollectible collectible, out string message)
        {
            for(int i = 0; i < Slots.Length; i++ )
            {
                if (Slots[i] is null)
                {
                    Slots[i] = collectible;
                    message = $"Picked up {collectible.IngameName}";
                    return true;
                }
            }
            message = $"Not enough space for {collectible.IngameName}";
            return false;
        }
        public bool TryWear(Armor newItem, out string message, out Armor currentItem)
        {
            int index = ArmorSet.FindIndex(item => item.Type == newItem.Type);
            if (index == -1)
            {
                currentItem = null;
                ArmorSet.Add(newItem);
                message = $"Picked up {newItem.IngameName}";
                return true;
            }
            else
            {
                currentItem = ArmorSet[index];
                ArmorSet[index] = newItem;
                message = $"Swapped {currentItem.IngameName} to {newItem.IngameName}";
                return true;
            }
        }
        public bool TryPickupWeapon(IWeapon newWeapon, out string message, out IWeapon currentWeapon)
        {
            if(Weapon is WeaponFist)
                { 
                currentWeapon = Weapon;
                message = $"Picked up {newWeapon.IngameName}";
                Weapon = newWeapon;
                return true;
            }
            else
            {
                currentWeapon = Weapon;
                message = $"Swapped to {newWeapon.IngameName}";
                Weapon = newWeapon;
                return true;
            }
        }
        public void Update()
        {

        }
        public void Draw()
        {

        }
    }
}

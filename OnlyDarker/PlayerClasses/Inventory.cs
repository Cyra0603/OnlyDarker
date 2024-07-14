using OnlyDarker.GameProcess;
using OnlyDarker.GameProcess.SpriteClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.PlayerClasses
{
    internal class Inventory
    {
        //data
        public readonly List<Armor> ArmorSet;
        public readonly Stats Stats;
        public readonly IWeapon Weapon;
        public readonly ICollectible[] Slots;
        //drawing

        public Inventory(List<Armor> armorSet, Stats stats, IWeapon weapon) 
        { 
            ArmorSet = armorSet;
            Stats = stats;
            Weapon = weapon;
            Slots = new ICollectible[10];
        }
        public bool TryCollect(ICollectible collectible)
        {

        }
        public void Update()
        {

        }
        public void Draw()
        {

        }
    }
}

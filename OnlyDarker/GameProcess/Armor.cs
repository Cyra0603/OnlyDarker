using OnlyDarker.CommonUsing;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess
{
    public class Armor
    {
        public Resistance SliceResistance { get; private set; }
        public Resistance PokeResistance { get; private set; }
        public Resistance BluntResistance { get; private set; }
        public List<Resistance> Resistances { get; } = new();
        public string IngameName { get; }
        public ArmorType Type { get; }

        public Armor(ArmorType armorType, string ingameName, float sliceX = 1F, float pokeX = 1F, float bluntX = 1F)
        {
            Type = armorType;
            Resistances.Add(SliceResistance = new(DamageType.Slice, sliceX));
            Resistances.Add(PokeResistance = new(DamageType.Poke, pokeX));
            Resistances.Add(BluntResistance = new(DamageType.Blunt, bluntX));
            if (ingameName == string.Empty || ingameName == null)
            {
                IngameName = string.Empty;
            }
            else IngameName = ingameName;
        }
        public void AddFlatArmor(float value)
        {
            foreach (var res in Resistances)
            {
                res.AddFlatValue(value);
            }
        }
        public void AddCommonModifier(float value)
        {
            foreach (Resistance res in Resistances)
            {
                res.AddModifier(value);
            }
        }
    }
    public class Resistance
    {
        public float Modifier { get; private set; } = 1F;
        public float FlatValue { get; private set; }
        public DamageType Type { get; }
        public Resistance(DamageType type, float modifier = 1F, float flatValue = 0F)
        {
            FlatValue = flatValue;
            Type = type;
            Modifier = modifier;
        }
        public void AddFlatValue(float value)
        {
            FlatValue += value;
        }
        public void AddModifier(float value)
        {
            Modifier += value;
        }
    }
}

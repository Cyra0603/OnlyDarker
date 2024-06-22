using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess
{
    public struct DamageInstance
    {
        private float _baseValue;
        private float _modifier = 1F;
        public bool IsCritical { get; private set; }
        public DamageType Type;
        public DamageInstance(float baseValue, float modifier, DamageType type, bool isCritical)
        {
            _baseValue = baseValue;
            _modifier = modifier;
            Type = type;
            IsCritical = isCritical;
        }
        public static DamageInstance operator +(DamageInstance a, float b)
        {
            return new DamageInstance { _baseValue = a._baseValue + b };
        }
        public static DamageInstance operator -(DamageInstance a, float b)
        {
            return new DamageInstance { _baseValue = a._baseValue - b };
        }
        //public static float operator *(DamageInstance a, Resistance r)
        //{
        //    return a._baseValue * a._modifier / r.Modifier - r.FlatValue;
        //}
        public static DamageInstance operator *(DamageInstance a, Resistance r)
        {
            var newb = a._baseValue - r.FlatValue;
            var newm = a._modifier * r.Modifier;
            Debug.WriteLineIf(a.Type != r.Type, $"DMG type conflict: dmg is {a.Type} res is {r.Type}");
            return new DamageInstance
            {
                _baseValue = newb,
                _modifier = newm,
                Type = a.Type,
                IsCritical = a.IsCritical
            };
        }
        public float ExtractValue()
        {
            return _baseValue * _modifier;
        }
    }
}

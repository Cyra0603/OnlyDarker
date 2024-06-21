using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
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

        public static DamageInstance operator *(DamageInstance a, float b)
        {
            return new DamageInstance { _baseValue = a._baseValue * b };
        }
        public static DamageInstance operator /(DamageInstance a, float b)
        {
            return new DamageInstance { _baseValue = a._baseValue / b };
        }
        public static DamageInstance operator +(DamageInstance a, float b)
        {
            return new DamageInstance { _baseValue = a._baseValue + b };
        }
        public static DamageInstance operator -(DamageInstance a, float b)
        {
            return new DamageInstance { _baseValue = a._baseValue - b };
        }
        public static float operator *(DamageInstance a, Resistance r)
        {
            return a._baseValue * a._modifier * r.Modifier - r.FlatValue;
        }
        public float ExtractValue(Resistance r)
        {
            return this * r;
        }
    }
}

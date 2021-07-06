using System;
using UnityEngine;

namespace Core.BigNumberAsset
{
    [Serializable]
    public class BigValue : IBigValue
    {
        [field: SerializeField, HideInInspector] public float Value { get; set; }
        [field: SerializeField, HideInInspector] public int Tier { get; set; }

        public static BigValue Zero => new BigValue(0, 0);

        public BigValue()
        {
            Value = 0;
            Tier = 0;
        }
        
        public BigValue(IBigValue bigValue)
        {
            Value = bigValue.Value;
            Tier = bigValue.Tier;
        }
        
        public BigValue(float value, int tier)
        {
            Value = Mathf.Abs(value) < 1000 && tier == 0 ? Mathf.Floor(value) : value;
            Tier = value != 0 ? tier : 0;

            if (value != 0)
            {
                var fixedValues = FixValues(this);
                Value = fixedValues.Value;
                Tier = fixedValues.Tier;
            }
        }
        
        public static BigValue Parse(string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value)) return Zero;
            for (var i = value.Length - 1; i >= 0; i--)
            {
                if (value[i] != '0')
                {
                    var numConsecutiveZeroes = value.Length - 1 - i; 
                    var tier = numConsecutiveZeroes / 3;
                    var reminder = numConsecutiveZeroes % 3;
                    return new BigValue(
                        float.Parse(value.Substring(0, i + 1 + reminder)), 
                        tier);
                }
            }

            return Zero;
        }

        public IBigValue Add(IBigValue other)
        {
            var sum = ConvertToBigNumber(this) + ConvertToBigNumber(other);
            Set(ConvertToBigValue(sum));
            return this;
        }

        public IBigValue Subtract(IBigValue other)
        {
            var sum = ConvertToBigNumber(this) - ConvertToBigNumber(other);
            Set(ConvertToBigValue(sum));
            return this;
        }
        
        public IBigValue Multiply(decimal other)
        {
            var sum = ConvertToBigNumber(this) * other;
            Set(ConvertToBigValue(sum));
            return this;
        }

        public IBigValue Set(IBigValue bigValue)
        {
            Value = bigValue.Value;
            Tier = bigValue.Tier;
            return this;
        }

        public string TierToString()
        {
            return TierConverter.TierToString(Tier).ToUpper();
        }

        public override string ToString()
        {
            return $"{Value:0.0}{TierToString()}";
        }

        public string ToColorString()
        {
            return $"{Value:0.0}<color=#FFD925>{TierToString()}";
        }

        public string ToColorString(string hexColor)
        {
            return $"{Value:0.0}<color={hexColor}>{TierToString()}";
        }

        public IBigValue Clone()
        {
            return new BigValue(Value, Tier);
        }

        #region Operator overload

        public static IBigValue operator +(BigValue a, IBigValue b)
        {
            var bigValue = new BigValue(a);
            return bigValue.Add(b);
        }

        public static IBigValue operator +(IBigValue a, BigValue b)
        {
            return b + a;
        }

        public static IBigValue operator -(BigValue a, IBigValue b)
        {
            var bigValue = new BigValue(a);
            return bigValue.Subtract(b);
        }

        public static IBigValue operator -(IBigValue a, BigValue b)
        {
            return b - a;
        }

        public static bool operator ==(BigValue a, BigValue b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(BigValue a, BigValue b)
        {
            return !(a == b);
        }

        public static bool operator >(BigValue a, BigValue b)
        {
            return ConvertToBigNumber(a) > ConvertToBigNumber(b);
        }

        public static bool operator <(BigValue a, BigValue b)
        {
            return ConvertToBigNumber(a) < ConvertToBigNumber(b);
        }

        public static bool operator >=(BigValue a, BigValue b)
        {
            return ConvertToBigNumber(a) >= ConvertToBigNumber(b);
        }

        public static bool operator <=(BigValue a, BigValue b)
        {
            return ConvertToBigNumber(a) <= ConvertToBigNumber(b);
        }

        #endregion

        #region IEquatable

        public bool Equals(BigValue other)
        {
            if (other == null) return false;
            return Math.Abs(Value - other.Value) < float.Epsilon && Tier == other.Tier;
        }

        public override bool Equals(object obj)
        {
            return obj is BigValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Value.GetHashCode() * 397) ^ Tier;
            }
        }

        #endregion
        
        #region Static

        public static BigNumber ConvertToBigNumber(IBigValue bigValue)
        {
            var converted = new BigNumber((decimal) bigValue.Value, bigValue.Tier * 3);
            return converted;
        }
        
        public static IBigValue ConvertToBigValue(BigNumber bigNumber)
        {
            var newRank = bigNumber.rank / 3;
            var newAmount = bigNumber.amount * (decimal)Mathf.Pow(10, bigNumber.rank % 3);
            var converted = new BigValue((float)newAmount, newRank);
            return converted;
        }
        
        private static IBigValue FixValues(BigValue bigValue)
        {
            if (bigValue.Value == 0) throw new ArgumentException();

            var safeCount = 0;
            
            while (bigValue.Value >= 1_000)
            {
                bigValue.Value /= 1_000;
                bigValue.Tier++;
                
                safeCount++;
                if (safeCount > 100) break;
            }

            while (bigValue.Value < 1)
            {
                bigValue.Value *= 1_000;
                bigValue.Tier--;
                
                safeCount++;
                if (safeCount > 100) break;
            }

            return bigValue;
        }

        #endregion

        public int CompareTo(IBigValue other)
        {
            if (other == null) throw new NullReferenceException();
            if (ReferenceEquals(this, other)) return 0;

            var tierComparison = Tier.CompareTo(other.Tier);
            return tierComparison != 0 ? tierComparison : Value.CompareTo(other.Value);
        }
    }
}
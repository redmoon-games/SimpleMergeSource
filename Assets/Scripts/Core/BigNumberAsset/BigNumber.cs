using System;
using UnityEngine;

namespace Core.BigNumberAsset
{
    [Serializable]
    public struct BigNumber : IEquatable<BigNumber>, IComparable<BigNumber>
    {
        public bool isNegative;
        
        public int rank;
        
        [Range(0, 1f)] public decimal amount;

        public BigNumber(decimal amount = 0, int exponent = 0)
        {
            if (amount == 0)
            {
                rank = 0;
                this.amount = 0;
                isNegative = false;
                return;
            }
            
            this.amount = Math.Abs(amount);
            rank = exponent;

            while (this.amount >= 10)
            {
                this.amount /= 10;
                rank++;
            }

            while (this.amount < 1)
            {
                this.amount *= 10;
                rank--;
            }
            
            isNegative = amount < 0;
        }

        public static BigNumber Zero => new BigNumber();
        public static BigNumber One => new BigNumber(1);
        public static BigNumber Million => new BigNumber(1, 6);
        public static BigNumber Trillion => new BigNumber(1, 12);

        public int CompareTo(BigNumber other)
        {
            if (other.isNegative != isNegative) return isNegative ? -1 : 1;
            if (rank != other.rank) return rank < other.rank ? -1 : 1;
            if (amount == other.amount) return 0;

            return amount < other.amount ? -1 : 1;
        }

        public bool Equals(BigNumber other)
        {
            return (amount == other.amount) && rank == other.rank;
        }

        public override bool Equals(object obj)
        {
            return obj is BigNumber other && Equals(other);
        }

        public override int GetHashCode()
        {
            return string.Format("{0}_{1}", amount, rank).GetHashCode();
        }

        #region Logic

        public static bool operator <(BigNumber bigNumberA, BigNumber bigNumberB)
        {
            return bigNumberA.CompareTo(bigNumberB) < 0;
        }

        public static bool operator >(BigNumber bigNumberA, BigNumber bigNumberB)
        {
            return bigNumberA.CompareTo(bigNumberB) > 0;
        }

        public static bool operator <=(BigNumber bigNumberA, BigNumber bigNumberB)
        {
            return bigNumberA.CompareTo(bigNumberB) <= 0;
        }

        public static bool operator >=(BigNumber bigNumberA, BigNumber bigNumberB)
        {
            return bigNumberA.CompareTo(bigNumberB) >= 0;
        }


        public static bool operator ==(BigNumber bigNumberA, BigNumber bigNumberB)
        {
            return bigNumberA.Equals(bigNumberB);
        }

        public static bool operator !=(BigNumber bigNumberA, BigNumber bigNumberB)
        {
            return !bigNumberA.Equals(bigNumberB);
        }

        #endregion

        #region Math

        public static BigNumber operator +(BigNumber bigNumberA, BigNumber bigNumberB)
        {
            if (bigNumberA.rank != bigNumberB.rank)
            {
                var large = bigNumberA.rank > bigNumberB.rank ? bigNumberA : bigNumberB;
                var small = bigNumberA.rank > bigNumberB.rank ? bigNumberB : bigNumberA;
                var exponentDifference = large.rank - small.rank;

                return new BigNumber(large.amount + small.amount / (decimal)Math.Pow(10, exponentDifference), large.rank);
            }

            if (bigNumberA.isNegative == bigNumberB.isNegative)
                return bigNumberA.isNegative
                    ? -new BigNumber(bigNumberA.amount + bigNumberB.amount, bigNumberA.rank)
                    : new BigNumber(bigNumberA.amount + bigNumberB.amount, bigNumberA.rank);
            return bigNumberA.isNegative
                ? -new BigNumber(bigNumberA.amount - bigNumberB.amount, bigNumberA.rank)
                : new BigNumber(bigNumberA.amount - bigNumberB.amount, bigNumberA.rank);
        }

        public static BigNumber operator -(BigNumber bigNumberA, BigNumber bigNumberB)
        {
            if (bigNumberA.rank != bigNumberB.rank)
            {
                var large = bigNumberA.rank > bigNumberB.rank ? bigNumberA : bigNumberB;
                var small = bigNumberA.rank > bigNumberB.rank ? bigNumberB : bigNumberA;
                var exponentDifference = large.rank - small.rank;

                return new BigNumber(large.amount - small.amount / (decimal)Math.Pow(10, exponentDifference), large.rank);
            }

            if (bigNumberA.isNegative == bigNumberB.isNegative)
                return bigNumberA.isNegative
                    ? -new BigNumber(bigNumberA.amount - bigNumberB.amount, bigNumberA.rank)
                    : new BigNumber(bigNumberA.amount - bigNumberB.amount, bigNumberA.rank);
            return bigNumberA.isNegative
                ? -new BigNumber(bigNumberA.amount + bigNumberB.amount, bigNumberA.rank)
                : new BigNumber(bigNumberA.amount + bigNumberB.amount, bigNumberA.rank);
        }

        public static BigNumber operator *(BigNumber bigNumberA, decimal numberB)
        {
            var returnValue = new BigNumber(bigNumberA.amount * numberB, bigNumberA.rank);
            return returnValue;
        }

        public static BigNumber operator /(BigNumber bigNumberA, decimal numberB)
        {
            return new BigNumber(bigNumberA.amount / numberB, bigNumberA.rank);
        }

        public static BigNumber operator -(BigNumber bigNumber)
        {
            return new BigNumber(-bigNumber.amount, bigNumber.rank);
        }

        #endregion
    }
}
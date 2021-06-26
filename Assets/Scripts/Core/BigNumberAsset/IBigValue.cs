using System;

namespace Core.BigNumberAsset
{
    public interface IBigValue: IEquatable<BigValue>, IClone<IBigValue>, IComparable<IBigValue>
    {
        float Value { get; }
        int Tier { get; }

        IBigValue Add(IBigValue bigValue);
        IBigValue Subtract(IBigValue bigValue);
        IBigValue Set(IBigValue bigValue);
        IBigValue Multiply(decimal other);

        string ToString();
    }

    public interface IClone<T>
    {
        T Clone();
    }
}
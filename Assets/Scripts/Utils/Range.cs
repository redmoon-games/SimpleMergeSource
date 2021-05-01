using System;

namespace Utils
{
    [Serializable]
    public class FloatRange : Range<float> {}
    
    [Serializable]
    public class IntRange : Range<int> {}
    
    public class Range<T>
    {
        public T min;
        public T max;
    }
}
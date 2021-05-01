using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class ListExtensions
    {
        public static T GetRandom<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static T GetBy<T>(this List<T> list, Func<T, bool> condition, T defaultValue = default)
        {
            foreach (var icon in list)
            {
                if (condition(icon))
                    return icon;
            }

            Debug.LogWarning($"{typeof(T)} object is not found");
            return defaultValue;
        }
    }
}

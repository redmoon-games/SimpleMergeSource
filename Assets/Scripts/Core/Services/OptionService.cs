using System.Collections.Generic;
using UnityEngine;

namespace Core.Services
{
    public class OptionService : MonoBehaviour
    {
        public BoolOption canMerge = new BoolOption();
    }

    public class BoolOption
    {
        private readonly Stack<bool> _stack = new Stack<bool>();

        public bool Value
        {
            get => _stack.Count == 0;

            set
            {
                if (value)
                {
                    _stack.Pop();
                }
                else
                {
                    _stack.Push(true);
                }
            }
        }
    }
}

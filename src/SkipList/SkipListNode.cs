using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkipList.Generic
{
    public sealed class SkipListNode<T>
    {
        public T Value { get; init; }
        public SkipListNode<T>[] ForwardNodes { get; init; }

        public SkipListNode(T value, int amountOfLayers)
        {
            if(amountOfLayers < 1)
                throw new ArgumentOutOfRangeException($"{nameof(amountOfLayers)} cannot have less than 1 layer, as 1 layer is needed to maintain a linked list.");

            Value = value;
            ForwardNodes = new SkipListNode<T>[amountOfLayers];
        }
    }
}

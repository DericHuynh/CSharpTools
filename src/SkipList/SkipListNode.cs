using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SkipList
{
    /// <summary>
    /// Represents a node in a Skip List data structure.
    /// Each node contains a value and pointers to other nodes at various levels.
    /// </summary>
    /// <typeparam name="T">The type of elements in the Skip List. Must implement IComparable.</typeparam>
    [DebuggerDisplay("{Value}")]
    [Serializable]
    public sealed class SkipListNode<T> where T : IComparable<T>
    {
        /// <summary>
        /// Gets the value stored in this node.
        /// </summary>
        public T Value { get; init; }

        /// <summary>
        /// Gets the list of forward pointers to other nodes at various levels.
        /// The number of elements in this list determines the maximum level this node participates in.
        /// </summary>
        public IList<SkipListNode<T>> ForwardNodes { get; init; }

        /// <summary>
        /// Initializes a new instance of the SkipListNode class with the specified value and number of levels.
        /// </summary>
        /// <param name="value">The value to store in the node.</param>
        /// <param name="levelCount">The number of levels this node will participate in. Must be at least 1.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when levelCount is less than 1.</exception>
        public SkipListNode(T value, int levelCount)
        {
            if (levelCount < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(levelCount), 
                    "Cannot have fewer than 1 layer, as at least 1 layer is needed to maintain a linked list."
                );
            }

            Value = value;
            ForwardNodes = [.. new SkipListNode<T>[levelCount]];
        }
    }
}

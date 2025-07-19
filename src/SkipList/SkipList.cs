using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace SkipList
{
    /// <summary>
    /// A Skip List implementation that provides O(log n) average performance for add, remove, and lookup operations.
    /// This implementation maintains elements in sorted order based on their natural comparison.
    /// </summary>
    /// <typeparam name="T">The type of elements in the Skip List. Must implement IComparable.</typeparam>
    [DebuggerTypeProxy(typeof(SkipList.SkipListDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public sealed class SkipList<T> : IList<T>, IList, IReadOnlyList<T>, ISerializable, IDeserializationCallback
        where T : IComparable<T>
    {
        #region Serialization Fields

        /// <summary>
        /// Temporary storage for serialization information during deserialization.
        /// </summary>
        private SerializationInfo? _siInfo;

        // Constant names for serialization (do not change - used for binary serialization)
        /// <summary>
        /// Version name for Serialization.
        /// </summary>
        private const string VersionName = "Version";

        /// <summary>
        /// Count name for Serialization
        /// </summary>
        private const string CountName = "Count";

        /// <summary>
        /// Data/Array name for Serialization
        /// </summary>
        private const string ValuesName = "Data";

        #endregion

        #region Private Fields

        /// <summary>
        /// The probability of a node being promoted to the next level.
        /// This value affects the structure of the skip list and its performance characteristics.
        /// </summary>
        private const double PROBABILITY = 0.2;

        /// <summary>
        /// Tracks modifications to the list to support consistent iteration.
        /// </summary>
        private int _version;

        /// <summary>
        /// The number of elements in the list.
        /// </summary>
        private int _count;

        /// <summary>
        /// The current maximum level being used in the skip list (may be less than MAX_LEVEL).
        /// </summary>
        private int _currentMaxLevel;

        /// <summary>
        /// Random number generator for determining node levels.
        /// </summary>
        private readonly Random _random;

        /// <summary>
        /// The head node of the skip list, which doesn't store an actual value.
        /// </summary>
        private readonly SkipListNode<T> _head;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the maximum number of levels in the skip list.
        /// This affects the memory usage and performance characteristics of the data structure.
        /// </summary>
        public int MaxLevel { get; private set; } = 4;

        /// <summary>
        /// Gets the number of elements in the skip list.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Gets a value indicating whether the skip list is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets a value indicating whether the skip list has a fixed size.
        /// </summary>
        public bool IsFixedSize => false;

        /// <summary>
        /// Gets a value indicating whether access to the skip list is synchronized (thread-safe).
        /// </summary>
        public bool IsSynchronized => false;

        /// <summary>
        /// Gets an object that can be used to synchronize access to the skip list.
        /// </summary>
        public object SyncRoot => _head;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the SkipList class.
        /// </summary>
        public SkipList()
        {
            _version = 0;
            _count = 0;
            _currentMaxLevel = 1;
            _random = new Random();
#pragma warning disable CS8604 // Possible null reference argument.
            _head = new SkipListNode<T>(default, MaxLevel);
#pragma warning restore CS8604 // Possible null reference argument.
            Initialize();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes the skip list with an empty head node.
        /// </summary>
        private void Initialize()
        {
            // Initialize all forward pointers of the head to point to itself (empty list)
            for (int i = 0; i < MaxLevel; i++)
            {
                _head.ForwardNodes[i] = _head;
            }
        }

        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to get.</param>
        /// <returns>The item at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Index is not a valid index in the skip list.</exception>
        private T GetItem(int index)
        {
            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be within the range of the list.");

            SkipListNode<T> currentNode = _head;

            // Traverse the list to the specified index
            for (int i = 0; i <= index; i++)
            {
                currentNode = currentNode.ForwardNodes[0];
            }

            return currentNode.Value;
        }

        /// <summary>
        /// Determines the level for a new node using a probabilistic algorithm.
        /// The higher the level, the fewer nodes that will participate at that level.
        /// </summary>
        /// <returns>The level for the new node.</returns>
        private int DetermineNodeLevel()
        {
            double randomValue = _random.NextDouble();
            int maxPossibleLevel = Math.Min(_currentMaxLevel + 1, MaxLevel);
            
            // Calculate level based on probability
            int level = (int)(Math.Log(randomValue) / Math.Log(PROBABILITY)) + 1;

            // Ensure the level is within bounds
            return Math.Min(level, maxPossibleLevel);
        }

        /// <summary>
        /// Resizes the skip list's maximum level based on the current number of elements.
        /// </summary>
        private void Resize()
        {
            int newSize = (int)Math.Log10(_count + 1) + 1;

            // Skip downsizing as it would require rebalancing the list
            if (newSize <= MaxLevel)
                return;

            for (int i = 0; i < newSize - MaxLevel; i++)
            {
                _head.ForwardNodes.Add(_head);
            }

            MaxLevel = newSize;
        }

        #endregion

        #region IList<T> Implementation

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="NotSupportedException">Setting values by index is not supported.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Index is not a valid index in the skip list.</exception>
        public T this[int index]
        {
            get => GetItem(index);
            set => throw new NotSupportedException("Skip list does not support setting values by index because it maintains sorted order.");
        }

        /// <summary>
        /// Adds an item to the skip list in its correct sorted position.
        /// </summary>
        /// <param name="item">The item to add to the skip list.</param>
        public void Add(T item)
        {
            if (item is null)
                return;

            // Array to keep track of the nodes that need to be updated
            SkipListNode<T>[] nodesToUpdate = new SkipListNode<T>[_currentMaxLevel + 1];
            SkipListNode<T> currentNode = _head;

            // Find the position to insert the new node at each level (from top to bottom)
            for (int i = _currentMaxLevel - 1; i >= 0; --i)
            {
                // Move forward at the current level as long as the next value is less than our item
                while (currentNode.ForwardNodes[i] != _head && currentNode.ForwardNodes[i].Value.CompareTo(item) < 0)
                {
                    currentNode = currentNode.ForwardNodes[i];
                }
                nodesToUpdate[i] = currentNode;
            }

            // Determine the level for the new node
            int level = DetermineNodeLevel();

            // If we're going up a level, update the update array and the current max level
            if (level > _currentMaxLevel)
            {
                nodesToUpdate[level - 1] = _head;
                _currentMaxLevel = level;
            }

            // Create the new node with the determined level
            SkipListNode<T> newNode = new(item, level);

            // Update pointers for all levels to insert the new node
            for (int i = 0; i < level; ++i)
            {
                // The new node points to what the update node was pointing to
                newNode.ForwardNodes[i] = nodesToUpdate[i].ForwardNodes[i];
                // The update node now points to our new node
                nodesToUpdate[i].ForwardNodes[i] = newNode;
            }

            // Check if we need to resize the list to allow more layers
            Resize();

            _count++;
            _version++;
        }

        /// <summary>
        /// Removes all elements from the skip list.
        /// </summary>
        public void Clear()
        {
            _version++;
            _count = 0;
            _currentMaxLevel = 1;

            // Reset all forward pointers to the head (empty list)
            IList<SkipListNode<T>> forwardNodes = _head.ForwardNodes;

            for (int i = 0; i < MaxLevel; i++)
            {
                forwardNodes[i] = _head;
            }
        }

        /// <summary>
        /// Determines whether the skip list contains a specific item.
        /// </summary>
        /// <param name="item">The item to locate in the skip list.</param>
        /// <returns>true if the item is found; otherwise, false.</returns>
        public bool Contains(T item)
        {
            if (item is null)
                return false;

            SkipListNode<T> currentNode = _head;

            // Search from top level to bottom
            for (int i = _currentMaxLevel - 1; i >= 0; --i)
            {
                // Move forward at the current level as long as the next value is less than our item
                while (currentNode.ForwardNodes[i] != _head && currentNode.ForwardNodes[i].Value.CompareTo(item) < 0)
                {
                    currentNode = currentNode.ForwardNodes[i];
                }
            }

            // If the next node is pointing to head, the item does not exist.
            if (currentNode.ForwardNodes[0] == _head)
                return false;

            // If the next node has our value, the item is in the list
            return currentNode.ForwardNodes[0].Value.CompareTo(item) == 0;
        }

        /// <summary>
        /// Copies the elements of the skip list to an array, starting at a particular array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array, nameof(array));

            ArgumentOutOfRangeException.ThrowIfLessThan(arrayIndex, 0, nameof(arrayIndex));

            if (arrayIndex + _count > array.Length)
                throw new ArgumentException("The number of elements to copy exceeds the available space in the array.");

            int currentIndex = arrayIndex;
            foreach (T item in this)
            {
                array[currentIndex++] = item;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the skip list.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the skip list.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IEnumerator<T> GetEnumerator()
        {
            SkipListNode<T> currentNode = _head;
            int initialVersion = _version;

            // Traverse the bottom level (level 0) of the skip list
            while (currentNode.ForwardNodes[0] is not null && currentNode.ForwardNodes[0] != _head)
            {
                currentNode = currentNode.ForwardNodes[0];

                // Check for concurrent modification
                if (_version != initialVersion)
                    throw new InvalidOperationException("Collection was modified during enumeration.");

                yield return currentNode.Value;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the skip list.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the skip list.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Finds the index of the first occurrence of a specific item in the skip list.
        /// </summary>
        /// <param name="item">The item to locate in the skip list.</param>
        /// <returns>The index of the first occurrence of the item if found; otherwise, -1.</returns>
        public int IndexOf(T item)
        {
            if (item is null || _count < 1)
                return -1;

            SkipListNode<T> currentNode = _head.ForwardNodes[0];
            int currentIndex = 0;

            // Traverse the bottom level (level 0) of the skip list
            for (int i = 0; i < _count; i++)
            {
                if (currentNode.Value.CompareTo(item) == 0)
                    return currentIndex;

                currentNode = currentNode.ForwardNodes[0];
                currentIndex++;
            }

            return -1;
        }

        /// <summary>
        /// Inserts an item at the specified index in the skip list.
        /// This operation is not supported as the skip list maintains items in sorted order.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The item to insert into the skip list.</param>
        /// <exception cref="NotSupportedException">Skip list does not support inserting at a specific index.</exception>
        public void Insert(int index, T item)
        {
            throw new NotSupportedException("Skip list does not support inserting at a specific index because it maintains sorted order.");
        }

        /// <summary>
        /// Removes the first occurrence of a specific item from the skip list.
        /// </summary>
        /// <param name="item">The item to remove from the skip list.</param>
        /// <returns>true if item was successfully removed; otherwise, false.</returns>
        public bool Remove(T item)
        {
            if (item is null)
                return false;

            SkipListNode<T> currentNode = _head;
            SkipListNode<T>[] nodesToUpdate = new SkipListNode<T>[_currentMaxLevel + 1];

            // Find the position of the item at each level (from top to bottom)
            for (int i = _currentMaxLevel - 1; i >= 0; --i)
            {
                // Move forward at the current level as long as the next value is less than our item
                while (currentNode.ForwardNodes[i] != _head && currentNode.ForwardNodes[i].Value.CompareTo(item) < 0)
                {
                    currentNode = currentNode.ForwardNodes[i];
                }
                nodesToUpdate[i] = currentNode;
            }

            // Check if the item exists in the list
            SkipListNode<T> candidateNode = currentNode.ForwardNodes[0];
            if (candidateNode == _head || candidateNode.Value.CompareTo(item) != 0)
                return false;

            // Update pointers to bypass the node being removed
            for (int i = 0; i < candidateNode.ForwardNodes.Count; i++)
            {
                nodesToUpdate[i].ForwardNodes[i] = candidateNode.ForwardNodes[i];
            }

            _count--;
            _version++;

            return true;
        }

        /// <summary>
        /// Removes the item at the specified index from the skip list.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">Index is not a valid index in the skip list.</exception>
        public void RemoveAt(int index)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _count, nameof(index));

            SkipListNode<T> currentNode = _head;

            // Traverse to the node at the specified index
            for (int i = 0; i < index; i++)
            {
                currentNode = currentNode.ForwardNodes[0];
            }

            // Remove the node at that position's value 
            // (we're getting the next node's value and removing it)
            Remove(currentNode.ForwardNodes[0].Value);
        }
        #endregion

        #region IList Implementation

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        object? IList.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException("Skip list does not support setting values by index because it maintains sorted order.");
        }

        /// <summary>
        /// Determines whether the skip list contains a specific item.
        /// </summary>
        /// <param name="value">The item to locate in the skip list.</param>
        /// <returns>true if the item is found; otherwise, false.</returns>
        bool IList.Contains(object? value)
        {
            if (value is T item)
            {
                return Contains(item);
            }
            return false;
        }

        /// <summary>
        /// Copies the elements of the skip list to an array, starting at a particular array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void CopyTo(Array array, int index)
        {
            ArgumentNullException.ThrowIfNull(array, nameof(array));

            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));

            if (array.Rank != 1)
                throw new ArgumentException("Multidimensional arrays are not supported.", nameof(array));

            if (index + _count > array.Length)
                throw new ArgumentException("The number of elements to copy exceeds the available space in the array.");

            int currentIndex = index;
            foreach (T item in this)
            {
                array.SetValue(item, currentIndex++);
            }
        }

        /// <summary>
        /// Finds the index of the first occurrence of a specific item in the skip list.
        /// </summary>
        /// <param name="value">The item to locate in the skip list.</param>
        /// <returns>The index of the first occurrence of the item if found; otherwise, -1.</returns>
        public int IndexOf(object? value)
        {
            if (value is T element)
            {
                return IndexOf(element);
            }
            return -1;
        }

        /// <summary>
        /// Inserts an item at the specified index in the skip list.
        /// This operation is not supported as the skip list maintains items in sorted order.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The item to insert into the skip list.</param>
        /// <exception cref="NotSupportedException">Skip list does not support inserting at a specific index.</exception>
        public void Insert(int index, object? value)
        {
            throw new NotSupportedException("Skip list does not support inserting at a specific index because it maintains sorted order.");
        }

        /// <summary>
        /// Adds an item to the skip list.
        /// </summary>
        /// <param name="value">The item to add to the skip list.</param>
        /// <returns>The index at which the item was added, or -1 if the item could not be added.</returns>
        public int Add(object? value)
        {
            if (value is T item)
            {
                Add(item);
                return IndexOf(item);
            }
            return -1;
        }

        /// <summary>
        /// Removes the first occurrence of a specific item from the skip list.
        /// </summary>
        /// <param name="value">The item to remove from the skip list.</param>
        public void Remove(object? value)
        {
            if (value is T element)
            {
                Remove(element);
            }
        }

        #endregion

        #region ISerializable Implementation

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the skip list.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        /// <exception cref="ArgumentNullException">info is null.</exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArgumentNullException.ThrowIfNull(info, "SerializationInfo cannot be null.");

            // Store version and count
            info.AddValue(VersionName, _version);
            info.AddValue(CountName, _count);

            // Store the values as an array
            if (_count != 0)
            {
                T[] array = new T[_count];
                CopyTo(array, 0);
                info.AddValue(ValuesName, array, typeof(T[]));
            }
        }

        #endregion

        #region IDeserializationCallback Implementation

        /// <summary>
        /// Implements the IDeserializationCallback interface and is called after deserialization.
        /// </summary>
        /// <param name="sender">The object that initiated the callback.</param>
        public void OnDeserialization(object? sender)
        {
            if (_siInfo == null)
            {
                return; // Already deserialized or wasn't serialized
            }

            // Retrieve the version and count
            int realVersion = _siInfo.GetInt32(VersionName);
            int count = _siInfo.GetInt32(CountName);

            if (count != 0)
            {
                // Retrieve and add the values, throw exception if array is null.
                T[]? array = (T[]?)_siInfo.GetValue(ValuesName, typeof(T[])) 
                    ?? throw new SerializationException("Missing values during deserialization");
                
                for (int i = 0; i < array.Length; i++)
                {
                    Add(array[i]);
                }
            }
            else
            {
                // Initialize an empty list
                Initialize();
            }

            _version = realVersion;
            _siInfo = null;
        }

        #endregion

        #region Debugging Support

        /// <summary>
        /// Gets an array representation of the skip list for debugging.
        /// </summary>
        /// <returns>An array containing all nodes in the skip list.</returns>
        internal SkipListNode<T>[] GetDebugViewArray()
        {
            SkipListNode<T>[] skipListNodes = new SkipListNode<T>[_count + 1];
            SkipListNode<T> currentNode = _head;
            skipListNodes[0] = _head;
            int currentIndex = 1;

            while (currentNode.ForwardNodes[0] is not null && currentNode.ForwardNodes[0] != _head)
            {
                currentNode = currentNode.ForwardNodes[0];
                skipListNodes[currentIndex] = currentNode;
                currentIndex++;
            }
          
            return skipListNodes;
        }

        #endregion
    }

    /// <summary>
    /// Debug view class for the SkipList to enhance debugging experience.
    /// </summary>
    /// <typeparam name="T">The type of elements in the Skip List.</typeparam>
    internal sealed class SkipListDebugView<T> where T : IComparable<T>
    {
        private readonly SkipList<T> _skipList;

        /// <summary>
        /// Initializes a new instance of the SkipListDebugView class.
        /// </summary>
        /// <param name="skipList">The skip list to debug.</param>
        public SkipListDebugView(SkipList<T> skipList)
        {
            ArgumentNullException.ThrowIfNull(skipList);
            _skipList = skipList;
        }

        /// <summary>
        /// Gets the items in the skip list for the debugger to display.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public SkipListNode<T>[] Items
        {
            get
            {
                return _skipList.GetDebugViewArray();
            }
        }
    }
}

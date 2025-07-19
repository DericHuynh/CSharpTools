using SkipList.Generic;
using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;

namespace SkipList
{
    [DebuggerTypeProxy(typeof(SkipList.SkipListDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public sealed class SkipList<T> : ISerializable, IDeserializationCallback, IList<T>, IList, IReadOnlyList<T> where T : IComparable<T>
    {
        private const int MAX_LEVEL = 6;
        private const double PROBABILITY = 0.1;
        private int _version {  get; set; }
        private int _count { get; set; }
        private int _currentMaxLevel { get; set; }
        private Random _random { get; init; }
        private SkipListNode<T> _head { get; set; }

        //A temporary variable which we need during deserialization.
        private SerializationInfo? _siInfo; 
        
        // names for serialization
        private const string VersionName = "Version"; // Do not rename (binary serialization)
        private const string CountName = "Count"; // Do not rename (binary serialization)
        private const string ValuesName = "Data"; // Do not rename (binary serialization)

        public SkipList()
        {
            _version = 0;
            _count = 0;
            _currentMaxLevel = 1;
            _random = new Random();
            Initialize();
        }

        public T this[int index] { get => GetItem(index); set => throw new NotSupportedException(); }
        object? IList.this[int index] { get => this[index]; set => throw new NotSupportedException(); }

        public int Count => _count;

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public bool IsSynchronized => false;

        public object SyncRoot => _head;

        private void Initialize()
        {
            _head = new SkipListNode<T>(default, MAX_LEVEL);

            SkipListNode<T>[] forwardNodes = _head.ForwardNodes;

            for (int i = 0; i < MAX_LEVEL; i++)
            {
                _head.ForwardNodes[i] = _head;
            }

        }

        private T GetItem(int index)
        {
            SkipListNode<T> currentNode = _head;

            if(index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException("Indexing has to be within range of the list.");

            for (int i = 0; i <= index; i++)
            {
                currentNode = currentNode.ForwardNodes[0];
            }

            return currentNode.Value;
        }

        public void Add(T item)
        {
            SkipListNode<T> currentNode = _head;
            SkipListNode<T>[] nodesToUpdate = new SkipListNode<T>[_currentMaxLevel + 1];
            int currentIndex = 0;

            if (item is null)
                return;

            //We traverse the list both vertically and horizontally until value of the next node is greater than our item
            for(int i = _currentMaxLevel - 1; i >= 0; --i)
            {
                while (currentNode.ForwardNodes[i] != _head && currentNode.ForwardNodes[i].Value.CompareTo(item) < 0)
                {
                    currentNode = currentNode.ForwardNodes[i];
                }
                nodesToUpdate[i] = currentNode;
            }

            int level = _getNextLevelFunctional();

            if(level > _currentMaxLevel)
            {
                nodesToUpdate[level - 1] = _head;
                _currentMaxLevel = level;
            }

            //We need to insert the item between the current and next node, since the next node is of a higher value and the current is a lower value.
            SkipListNode<T> newNode = new SkipListNode<T>(item, level);
            
            for(int i = 0; i < level; ++i)
            {
                newNode.ForwardNodes[i] = nodesToUpdate[i].ForwardNodes[i];
                nodesToUpdate[i].ForwardNodes[i] = newNode;             
            }

            _count++;
        }

        public int Add(object? value)
        {
            if(value is T item)
            {
                Add(item);
                return IndexOf(item);
            }

            return -1;
        }

        public void Clear()
        {
            _version++;
            _count = 0;
            _currentMaxLevel = 1;

            SkipListNode<T>[] forwardNodes = _head.ForwardNodes;

            for (int i = 0; i < MAX_LEVEL; i++)
            {
                forwardNodes[i] = _head;
            }
        }

        public bool Contains(T item)
        {
            SkipListNode<T> currentNode = _head;

            if (item is null)
                return false;

            //We traverse the list both vertically and horizontally until value of the next node is greater than our item
            for (int i = _currentMaxLevel - 1; i >= 0; --i)
            {
                while (currentNode.ForwardNodes[i] != _head && currentNode.ForwardNodes[i].Value.CompareTo(item) < 0)
                {
                    currentNode = currentNode.ForwardNodes[i];
                }
            }

            //If the next node is our element, it contains it, otherwise it doesnt;
            if (currentNode.ForwardNodes[0].Value.CompareTo(item) == 0)
            {
                return true;
            }

            return false;
        }

        public bool Contains(object? value)
        {
            if(value is T item)
            {
                return Contains(item);
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int newIndex = arrayIndex;

            if (array is null)
                return;

            foreach (T item in this)
            {
                array[newIndex] = item;
                newIndex++;
            }
        }

        public void CopyTo(Array array, int index)
        {
            int newIndex = index;

            foreach (T item in this)
            {
                array.SetValue(item, newIndex);
                newIndex++;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            SkipListNode<T> currentNode = _head;

            while (currentNode.ForwardNodes[0] is not null && currentNode.ForwardNodes[0] != _head)
            {
                currentNode = currentNode.ForwardNodes[0];
                yield return currentNode.Value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if(info == null)
            {
                throw new ArgumentNullException($"info cannot be null.");
            }

            // Customized serialization for LinkedList.
            // We need to do this because it will be too expensive to Serialize each node.
            // This will give us the flexiblility to change internal implementation freely in future.

            info.AddValue(VersionName, _version);
            info.AddValue(CountName, _count); // this is the length of the bucket array.

            if (_count != 0)
            {
                T[] array = new T[_count];
                CopyTo(array, 0);
                info.AddValue(ValuesName, array, typeof(T[]));
            }
        }

        public void OnDeserialization(object? sender)
        {
            if (_siInfo == null)
            {
                return; //Somebody had a dependency on this LinkedList and fixed us up before the ObjectManager got to it.
            }

            int realVersion = _siInfo.GetInt32(VersionName);
            int count = _siInfo.GetInt32(CountName);

            if (count != 0)
            {
                T[]? array = (T[]?)_siInfo.GetValue(ValuesName, typeof(T[]));

                if (array == null)
                {
                    throw new SerializationException("Missing values");
                }

                for (int i = 0; i < array.Length; i++)
                {
                    Add(array[i]);
                }
            }
            else
            {
                Initialize();
            }

            _version = realVersion;
            _siInfo = null;
        }

        public int IndexOf(T item)
        {
            SkipListNode<T> currentNode = _head.ForwardNodes[0];
            int currentIndex = 0;

            if (item is null || _count < 1)
                return -1;

            for (int i = 0; i < _count; i++)
            {
                if(currentNode.Value.CompareTo(item) == 0)
                    return currentIndex;
                currentNode = currentNode.ForwardNodes[0];
                currentIndex++;
            }

            return -1;
        }

        public int IndexOf(object? value)
        {
            if(value is T element)
            {
                return IndexOf(element);
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException(); //Can't insert at index when this is a sorted list
        }

        public void Insert(int index, object? value)
        {
            if(value is T item)
            {
                Insert(index, item);
            }
        }

        public bool Remove(T item)
        {
            SkipListNode<T> currentNode = _head;
            SkipListNode<T>[] nodesToUpdate = new SkipListNode<T>[_currentMaxLevel + 1];
            int currentIndex = 0;

            if (item is null)
                return false;

            //We traverse the list both vertically and horizontally until value of the next node is greater than our item
            for (int i = _currentMaxLevel - 1; i >= 0; --i)
            {
                while (currentNode.ForwardNodes[i] != _head && currentNode.ForwardNodes[i].Value.CompareTo(item) < 0)
                {
                    currentNode = currentNode.ForwardNodes[i];
                }
                nodesToUpdate[i] = currentNode;
            }

            //If the value is not the value to remove, it does not exist in list.
            if (currentNode.ForwardNodes[0].Value.CompareTo(item) != 0)
                return false;

            //Get the next node which is the node we want to remove;
            currentNode = currentNode.ForwardNodes[0];

            //Update the pointers of all layers of the previous nodes to point to whatever the removed node was pointing to.
            for (int i = 0; i < currentNode.ForwardNodes.Length; i++)
            {
                nodesToUpdate[i].ForwardNodes[i] = currentNode.ForwardNodes[i];
            }

            _count--;

            return true;
        }

        public void Remove(object? value)
        {
            if(value is T element)
            {
                Remove(element);
            }
        }

        public void RemoveAt(int index)
        {
            SkipListNode<T> currentNode = _head;
            
            for (int i = 0; i < index; i++)
            {
                currentNode = currentNode.ForwardNodes[0];
            }

            Remove(currentNode.Value);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int _getNextLevelFunctional()
        {
            double u = _random.NextDouble();
            int max_value = Math.Min(_currentMaxLevel + 1, MAX_LEVEL);
            int level = (int)(Math.Log(u) / Math.Log(PROBABILITY)) + 1;

            return Math.Min(level, max_value);
        }

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
    }

    // internal debug view class for skip list
    internal sealed class SkipListDebugView<T> where T : IComparable<T>
    {
        private readonly SkipList<T> _skipList;

        public SkipListDebugView(SkipList<T> sortedList)
        {
            ArgumentNullException.ThrowIfNull(sortedList);

            _skipList = sortedList;
        }

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

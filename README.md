# Skip List Implementation in C#

This repository contains a fully-featured implementation of a Skip List data structure in C#. The Skip List is a probabilistic data structure that allows for efficient searching, insertion, and deletion operations.

## Key Features

- O(log n) average time complexity for search, insert, and delete operations
- Implementation of multiple .NET interfaces:
  - `IList<T>` and `IList` for collection operations
  - `IReadOnlyList<T>` for read-only access
  - `ISerializable` and `IDeserializationCallback` for serialization support
- Automatic resizing based on the number of elements
- Proper debug visualization support
- Comprehensive unit tests

## Skip List Overview

A Skip List is a data structure that uses multiple levels of linked lists to achieve faster search. Each higher level skips over more elements, allowing searches to quickly narrow down to the relevant portion of the list.

The implementation uses a probabilistic approach to determine which nodes participate in higher levels, resulting in a well-balanced structure without explicit rebalancing operations.

### Implementation Details

The main components of this implementation are:

1. **SkipList<T>** - The main class that implements the Skip List data structure
2. **SkipListNode<T>** - A node in the Skip List containing a value and forward pointers to other nodes

The main operations are:

- `Add(T item)` - Adds an item to the Skip List in its proper sorted position
- `Remove(T item)` - Removes the specified item from the Skip List
- `Contains(T item)` - Checks if the Skip List contains the specified item
- `IndexOf(T item)` - Returns the index of the specified item
- `GetEnumerator()` - Returns an enumerator to iterate through the Skip List

## Usage Example

```csharp
// Create a new Skip List
SkipList<int> skipList = new SkipList<int>();

// Add elements
skipList.Add(5);
skipList.Add(10);
skipList.Add(3);
skipList.Add(7);

// Elements are automatically maintained in sorted order
foreach (int value in skipList)
{
    Console.WriteLine(value); // Outputs: 3, 5, 7, 10
}

// Check if an element exists
bool containsSeven = skipList.Contains(7); // true

// Get element by index (since list is sorted, index 0 is the smallest element)
int smallestElement = skipList[0]; // 3

// Remove an element
skipList.Remove(5);
```

## Performance Characteristics

- **Search, Insert, Delete**: O(log n) average time complexity
- **Space**: O(n) for the main list, plus approximately O(log n) additional space for the index structure
- **Memory overhead**: Each node requires storage for its value and an array of forward pointers

## Implementation Notes

1. The `PROBABILITY` constant (set to 0.1) controls how many nodes participate in higher levels
2. The implementation auto-adjusts the maximum level based on the number of elements
3. The Skip List maintains elements in sorted order based on their natural comparison
4. Setting elements by index is not supported as it would break the sorted order

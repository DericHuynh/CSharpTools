using SkipList;
using System.Text.Json;
using Xunit.Abstractions;

namespace SkipList.Test
{
    /// <summary>
    /// Unit tests for the SkipList<int> class.
    /// </summary>
    public class SkipListTests(ITestOutputHelper testOutputHelper)
    {
        /// <summary>
        /// Tests that adding elements to the skip list increases its count accordingly.
        /// </summary>
        [Fact]
        public void List_ShouldHaveFourElements_AfterAddingFourElements()
        {
            // Arrange
            SkipList<int> list = new();

            // Act
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            // Assert
            testOutputHelper.WriteLine($"List currently has {list.Count} elements inside");
            foreach (int element in list)
                testOutputHelper.WriteLine($"{element}");
                
            Assert.Equal(4, list.Count);
        }

        /// <summary>
        /// Tests that the skip list maintains elements in sorted order regardless of insertion order.
        /// </summary>
        [Fact]
        public void List_ShouldStillBeSorted_AfterAddingAnElement()
        {
            // Arrange
            int[] ints = [1, 25, 36, 41, 2, 37, 36, 12];
            SkipList<int> list = new();
            List<int> expectedSortedList = [.. ints];
            expectedSortedList.Sort();

            // Act - Add elements in unsorted order
            foreach (int value in ints)
            {
                list.Add(value);
            }

            // Assert - Check that the skip list contains the elements in sorted order
            testOutputHelper.WriteLine($"List currently has {list.Count} elements inside");
            foreach (int element in list)
                testOutputHelper.WriteLine($"{element}");
                
            Assert.Equal(expectedSortedList, list);
        }

        /// <summary>
        /// Tests that the Contains method correctly identifies elements in the skip list.
        /// </summary>
        [Fact]
        public void List_ShouldContainElement_AfterAddingAnElement()
        {
            // Arrange
            int[] ints = [1, 25, 25, 62, 26, 1, 6, 7, 8];
            SkipList<int> list = new();

            // Act
            foreach (int element in ints)
            {
                list.Add(element);
            }

            // Assert
            foreach (int element in ints)
            {
                bool expectedResult = ints.Contains(element);
                bool actualResult = list.Contains(element);
                Assert.Equal(expectedResult, actualResult);
            }
        }

        /// <summary>
        /// Tests that the Contains method correctly returns false for elements not in the skip list.
        /// </summary>
        [Fact]
        public void List_ShouldNotContainElement_AfterAddingOtherElements()
        {
            // Arrange
            int[] elementsToAdd = [1, 25, 25, 62, 26, 1, 6, 7, 8];
            int[] elementsToCheck = [19, 219, 9, 34, 2];
            SkipList<int> list = new();

            // Act
            foreach (int element in elementsToAdd)
            {
                list.Add(element);
            }

            // Assert
            foreach (int element in elementsToCheck)
            {
                bool expectedResult = elementsToAdd.Contains(element);
                bool actualResult = list.Contains(element);
                Assert.Equal(expectedResult, actualResult);
            }
        }

        /// <summary>
        /// Tests that the Remove method correctly removes elements from the skip list.
        /// </summary>
        [Fact]
        public void List_ShouldNotContainElement_AfterRemovingAnElement()
        {
            // Arrange
            int[] ints = [1, 25, 25, 62, 26, -1, 6, 7, 8];
            SkipList<int> list = new();
            
            foreach (int element in ints)
            {
                list.Add(element);
            }
            
            // Assert initial state
            Assert.Contains(8, list);

            // Act
            list.Remove(8);

            // Assert final state
            Assert.DoesNotContain(8, list);
        }

        /// <summary>
        /// Tests that the IndexOf method correctly returns the index of elements in the skip list.
        /// </summary>
        [Fact]
        public void List_ShouldGetIndexOfElement_AfterAddingElement()
        {
            // Arrange
            int[] ints = [1, 25, 25, 62, 26, 1, 6, 7, 8];
            SkipList<int> list = new();
            List<int> sortedList = [.. ints];
            
            // Act
            foreach (int element in ints)
            {
                list.Add(element);
            }
            
            sortedList.Sort();

            // Assert
            foreach(int element in ints)
            {
                int expectedIndex = sortedList.IndexOf(element); 
                int actualIndex = list.IndexOf(element);
                Assert.Equal(expectedIndex, actualIndex);
            }
        }

        /// <summary>
        /// Tests that the IndexOf method returns -1 for elements not in the skip list.
        /// </summary>
        [Fact]
        public void List_ShouldNotGetIndexOfElement_AfterAddingOtherElements()
        {
            // Arrange
            int[] elementsToAdd = [1, 25, 25, 62, 26, 1, 6, 7, 8];
            int[] elementsToCheck = [19, 219, 9, 34, 2];
            SkipList<int> list = new();

            // Act
            foreach (int element in elementsToAdd)
            {
                list.Add(element);
            }

            // Assert
            foreach(int element in elementsToCheck)
            {
                Assert.Equal(-1, list.IndexOf(element));
            }
        }

        /// <summary>
        /// Tests that the skip list can be properly serialized and deserialized.
        /// </summary>
        [Fact]
        public void List_ShouldBeDeserializable_AfterSerializing()
        {
            // Arrange
            int[] ints = [1, 25, 25, 62, 26, 1, 6, 7, 8];
            SkipList<int> list = new();

            foreach (int element in ints)
            {
                list.Add(element);
            }

            try
            {
                // Act
                var json = JsonSerializer.Serialize(list);
                var newList = JsonSerializer.Deserialize<SkipList<int>>(json);
                
                // Assert
                Assert.NotNull(newList);
                foreach (int element in ints)
                {
                    Assert.Contains(element, newList);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Serialization/deserialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests that the indexer works correctly for the skip list.
        /// </summary>
        [Fact]
        public void List_ShouldBeIndexable_AfterAddingElements()
        {
            // Arrange
            int[] ints = [1, 25, 25, 62, 26, 1, 6, 7, 8];
            SkipList<int> list = new();
            List<int> expected = [.. ints];

            // Act
            foreach (int element in ints)
            {
                list.Add(element);
            }

            expected.Sort();

            // Assert
            for(int i = 0; i < ints.Length; i++)
            {
                Assert.Equal(expected[i], list[i]);
            }
        }

        /// <summary>
        /// Tests that trying to access an empty skip list by index throws the expected exception.
        /// </summary>
        [Fact]
        public void List_ShouldNotBeIndexable_WhenEmpty()
        {
            // Arrange
            SkipList<int> list = new();
            List<int> expected = new();

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => expected[0]);
            Assert.Throws<ArgumentOutOfRangeException>(() => list[0]);
        }

        /// <summary>
        /// Tests that the skip list resizes correctly when many elements are added.
        /// </summary>
        [Fact]
        public void List_ShouldResize_WhenTooManyElements()
        {
            // Arrange
            Random rnd = new(1); // Use a fixed seed for reproducibility
            int[] ints = [.. Enumerable.Range(0, 1_000_000).Select(i => rnd.Next())];
            SkipList<int> list = new();

            // Act
            foreach (int element in ints)
            {
                list.Add(element);
            }

            // Assert
            Assert.True(list.MaxLevel > 6, $"MAX_LEVEL should be greater than 6, but was {list.MaxLevel}");
        }

        /// <summary>
        /// Tests that the skip list is empty when cleared.
        /// </summary>
        [Fact]
        public void List_ShouldBeEmpty_WhenCleared()
        {
            // Arrange
            int[] ints = [1, 25, 25, 62, 26, 1, 6, 7, 8];
            SkipList<int> list = new();

            // Act
            foreach (int element in ints)
            {
                list.Add(element);
            }

            list.Clear();

            // Assert
            Assert.Empty(list);
        }

        /// <summary>
        /// Tests that the skip list can copy to an array when array is a valid size.
        /// </summary>
        [Fact]
        public void List_ShouldCopyToArray_WhenArrayIsValid()
        {
            // Arrange
            int[] ints = [1, 25, 25, 62, 26, 1, 6, 7, 8];
            int[] output = new int[ints.Length];
            SkipList<int> list = new();

            // Act
            foreach (int element in ints)
            {
                list.Add(element);
            }

            list.CopyTo(output, 0);

            // Assert
            Assert.Equivalent(ints, output, strict: true);
        }

        /// <summary>
        /// Tests that the skip list can copy to an array when array is a valid size.
        /// </summary>
        [Fact]
        public void List_ShouldRemoveElementAtIndex_WhenElementExists()
        {
            // Arrange
            int[] ints = [1, 25, 25, 62, 26, 1, 6, 7, 8];
            SkipList<int> list = new();
            List<int> expected = [.. ints];

            // Act
            foreach (int element in ints)
            {
                list.Add(element);
            }

            expected.Sort();
            expected.RemoveAt(0);
            expected.RemoveAt(6);

            list.RemoveAt(0);
            list.RemoveAt(6);

            // Assert
            Assert.Equivalent(list, expected, strict: true);
        }
    }
}

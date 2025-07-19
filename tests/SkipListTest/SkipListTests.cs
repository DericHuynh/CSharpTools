using SkipList;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using Xunit.Abstractions;

namespace SkipListTest
{
    public class SkipListTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SkipListTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void List_ShouldHaveFourElements_AfterAddingFourElements()
        {
            //Arrange
            SkipList<int> list = new SkipList<int>();

            //Act
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            //Assert
            _testOutputHelper.WriteLine($"List currently has {list.Count} elements inside");
            foreach (int element in list)
                _testOutputHelper.WriteLine($"{element}");
            Assert.Equal(4, list.Count);
        }

        [Fact]
        public void List_ShouldStillBeSorted_AfterAddingAnElement()
        {
            //Arrange
            int[] ints = [1, 25, 36, 41, 2, 37, 36, 12];
            SkipList<int> list = new SkipList<int>();
            List<int> truth = new List<int>(ints);
            truth.Sort();

            //Act
            list.Add(1);
            list.Add(25);
            list.Add(36);
            list.Add(41);
            list.Add(2);
            list.Add(37);
            list.Add(36);
            list.Add(12);

            //Assert
            _testOutputHelper.WriteLine($"List currently has {list.Count} elements inside");
            foreach (int element in list)
                _testOutputHelper.WriteLine($"{element}");
            Assert.Equal(truth, list);
        }

        [Fact]
        public void List_ShouldContainElement_AfterAddingAnElement()
        {
            int[] ints = { 1, 25, 25, 62, 26, 1, 6, 7, 8 };
            SkipList<int> list = new SkipList<int>();

            foreach (int element in ints)
            {
                list.Add(element);
            }

            foreach (int element in ints)
            {
                Assert.Equal(list.Contains(element), ints.Contains(element));
            }
        }

        [Fact]
        public void List_ShouldNotContainElement_AfterAddingOtherElements()
        {
            int[] ints = { 1, 25, 25, 62, 26, 1, 6, 7, 8 };
            int[] notContains = { 19, 219, 9, 34, 2};
            SkipList<int> list = new SkipList<int>();

            foreach (int element in ints)
            {
                list.Add(element);
            }

            foreach (int element in notContains)
            {
                Assert.Equal(list.Contains(element), ints.Contains(element));
            }
        }

        [Fact]
        public void List_ShouldNotContainElement_AfterRemovingAnElement()
        {
            int[] ints = { 1, 25, 25, 62, 26, -1, 6, 7, 8 };
            SkipList<int> list = new SkipList<int>();

            foreach (int element in ints)
            {
                list.Add(element);
            }

            Assert.Contains(8, list);

            list.Remove(8);

            Assert.DoesNotContain(8, list);
        }

        [Fact]
        public void List_ShouldGetIndexOfElement_AfterAddingElement()
        {
            int[] ints = { 1, 25, 25, 62, 26, 1, 6, 7, 8 };
            SkipList<int> list = new SkipList<int>();
            List<int> sortedList = new List<int>(ints);

            foreach (int element in ints)
            {
                list.Add(element);
            }

            sortedList.Sort();

            foreach(int element in ints)
            {
                Assert.Equal(sortedList.IndexOf(element), list.IndexOf(element));
            }
        }

        [Fact]
        public void List_ShouldNotGetIndexOfElement_AfterAddingOtherElements()
        {
            int[] ints = { 1, 25, 25, 62, 26, 1, 6, 7, 8 };
            int[] notContains = { 19, 219, 9, 34, 2 };
            SkipList<int> list = new SkipList<int>();

            foreach (int element in ints)
            {
                list.Add(element);
            }

            foreach(int element in notContains)
            {
                Assert.Equal(-1, list.IndexOf(element));
            }
        }

        [Fact]
        public void List_ShouldBeDeserializable_AfterSerializing()
        {
            int[] ints = { 1, 25, 25, 62, 26, 1, 6, 7, 8 };
            SkipList<int> list = new SkipList<int>();

            foreach (int element in ints)
            {
                list.Add(element);
            }

            try
            {
                var json = JsonSerializer.Serialize(list);
                var newList = JsonSerializer.Deserialize<SkipList<int>>(json);
                if (newList is null)
                    Assert.Fail("newList null after deserialization");
                foreach (int element in ints)
                {
                    Assert.Contains(element, newList);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Fact]
        public void List_ShouldBeIndexable_AfterAddingElements()
        {
            int[] ints = { 1, 25, 25, 62, 26, 1, 6, 7, 8 };
            SkipList<int> list = new SkipList<int>();
            List<int> expected = new List<int>(ints);

            foreach (int element in ints)
            {
                list.Add(element);
            }

            expected.Sort();

            for(int i = 0; i < ints.Length; i++)
            {
                Assert.Equal(expected[i], list[i]);
            }
        }

        [Fact]
        public void List_ShouldNotBeIndexable_WhenEmpty()
        {
            int[] ints = { };
            SkipList<int> list = new SkipList<int>();
            List<int> expected = new List<int> { };

            Assert.Throws<ArgumentOutOfRangeException>(() => expected[0]);
            Assert.Throws<ArgumentOutOfRangeException>(() => list[0]);
        }
    }
}

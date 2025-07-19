using SkipList;

namespace SkipListTests
{
    public class SkipListTests
    {
        [Test]
        public void List_ShouldHaveFourElements_AfterAddingFourElements()
        {
            SkipList<int> list = new SkipList<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            Console.WriteLine($"List currently has {list.Count} elements inside");
            foreach (int element in list)
                Console.WriteLine($"{element}");
            Assert.That(4 == list.Count);
        }

        [Test]
        public void List_ShouldStillBeSorted_AfterAddingAnElement()
        {
            int[] ints = [1, 25, 36, 41, 2, 37, 36, 12];
            SkipList<int> list = new SkipList<int>();
            List<int> truth = new List<int>(ints);

            list.Add(1);
            list.Add(25);
            list.Add(36);
            list.Add(41);
            list.Add(2);
            list.Add(37);
            list.Add(36);
            list.Add(12);
            truth.Sort();

            Console.WriteLine($"List currently has {list.Count} elements inside");
            foreach (int element in list)
                Console.WriteLine($"{element}");
            Assert.
        }
    }
}

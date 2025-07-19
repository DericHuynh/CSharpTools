using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Perfolizer.Mathematics.Common;
using SkipList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkipListBenchmarks
{
    [MemoryDiagnoser]
    public class SortedSetVsSkipListBenchmarks
    {
        [Params(100, 10_000, 30_000)]
        public int amountOfElements;

        public int[] elements;
        public SkipList<int> skipList;
        public SortedSet<int> sortedSet;

        [GlobalSetup]
        public void Setup()
        {
            Random random = new Random(1);
            skipList = new SkipList<int>();
            sortedSet = new SortedSet<int>();
            elements = Enumerable.Range(0, amountOfElements).Select(i => random.Next()).ToArray();
        }

        [Benchmark]
        public int SortedSetAdd()
        {
            sortedSet.Clear();

            foreach (int element in elements)
            {
                sortedSet.Add(element);
            }

            return sortedSet.Count;
        }

        [Benchmark]
        public int SkipListAdd()
        {
            skipList.Clear();

            foreach (int element in elements)
            {
                skipList.Add(element);
            }

            return skipList.Count;
        }
    }
}

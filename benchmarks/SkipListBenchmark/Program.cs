using BenchmarkDotNet.Running;
using SkipListBenchmarks;

namespace SkipListBenchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<GetNextLevelBenchmarks>();
        }
    }
}

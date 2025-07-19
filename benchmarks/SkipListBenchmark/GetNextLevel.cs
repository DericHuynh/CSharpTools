using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Perfolizer.Mathematics.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkipListBenchmarks
{
    [MemoryDiagnoser]
    public class GetNextLevelBenchmarks
    {
        private Random _random;
        const int MAX_LEVEL = 32;
        const double PROBABILITY = 0.5;

        [Params(32)]
        public int _currentMaxLevel;

        [GlobalSetup]
        public void Setup()
        { 
            _random = new Random();
        }

        [Benchmark]
        public int GetNextLevel()
        {
            int lvl = 0;

            while (_random.NextDouble() < PROBABILITY && lvl <= _currentMaxLevel && lvl < MAX_LEVEL)
                ++lvl;

            return lvl;
        }

        [Benchmark]
        public int GetNextLevelOtherFunctional()
        {
            double u = _random.NextDouble();
            int max_value = Math.Min(_currentMaxLevel + 1, MAX_LEVEL);
            int level = (int)(Math.Log(u) / Math.Log(PROBABILITY)) + 1;

            return Math.Min(level, max_value);
        }


        // Only works for probability 0.5
        [Benchmark]
        public int GetNextLevelFunctional()
        {
            double u = _random.NextDouble();
            int max_value = Math.Min(_currentMaxLevel + 1, MAX_LEVEL);
            int level = -Math.ILogB(u) + 1;

            return Math.Min(level, max_value);
        }
    }
}

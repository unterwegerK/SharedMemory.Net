using Benchmark;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoadImbalanceTask
{
  /// <summary>
  /// Demonstrates load balancing by splitting work into small tasks and using dynamic task scheduling.
  /// </summary>
  class LoadImbalanceTask
  {
    class PartialSum
    {
      public readonly int index;
      public readonly long blockSize;

      public PartialSum(int index, long blockSize)
      {
        this.index = index;
        this.blockSize = blockSize;
      }
    }

    static int Collatz(long n)
    {
      if (n <= 1)
      {
        return 1;
      }
      if (n % 2 == 0)
      {
        return 1 + Collatz(n / 2);
      }

      return 1 + Collatz(3 * n + 1);
    }

    static void Main(string[] args)
    {
      Chart chart = new Chart("Load Balancing", "Number of Threads", "Parallel Efficiency", false);

      InitializationAndWarmup();

      int maxNumberOfThreads = 4; //Environment.ProcessorCount;

      long n = 1 << 14;
      for (int numberOfTasks = 1 << 2; numberOfTasks <= 1 << 8; numberOfTasks *= 2)
      {
        double? baseLine = null;
        for (int numberOfThreads = 1; numberOfThreads <= maxNumberOfThreads; numberOfThreads+=3)
        {
          
          ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = numberOfThreads };
          var partitioner = Partitioner.Create(0, n, n / numberOfTasks);

          long[] partialSums = new long[numberOfTasks];
          Stopwatch stopwatch = Stopwatch.StartNew();
          Parallel.ForEach(
            partitioner,
            options,
            (range, state) =>
            {
              long partialSum = 0;
              for (long i = range.Item1; i < range.Item2; i++)
              {
                for (long j = 0; j < (long) Math.Sqrt(i); j++)
                {
                  partialSum += Collatz(i + j);
                }
              }

              partialSums[range.Item1 / (n / numberOfTasks)] = partialSum;
            });
          stopwatch.Stop();

          double average = 0.0;
          long max = 0;
          for (int i = 0; i < partialSums.Length; i++)
          {
            average += partialSums[i] / (double) partialSums.Length;
            max = Math.Max(max, partialSums[i]);
          }
          

          if (baseLine == null) baseLine = stopwatch.ElapsedMilliseconds;
          double speedup = (baseLine / stopwatch.ElapsedMilliseconds) ?? 0.0;
          double efficiency = (baseLine / stopwatch.ElapsedMilliseconds / numberOfThreads) ?? 0.0;
          double imbalance = (max - average) / average;
          Console.WriteLine(
            $"{numberOfTasks} tasks, {numberOfThreads} threads: {stopwatch.ElapsedMilliseconds}ms, speedup={speedup:F2} efficiency={efficiency:P} imbalance={imbalance:P}");

          if (numberOfThreads == maxNumberOfThreads)
          {
            chart.Add("Parallel Efficiency", numberOfTasks, efficiency);
            chart.Add("Imbalance", numberOfTasks, imbalance);
          }
        }
      }

      chart.Save(typeof(LoadImbalanceTask));
      chart.Show();
    }

    private static void InitializationAndWarmup()
    {
      Console.WriteLine("Initialization and Warmup...");
      GC.Collect();
      GC.WaitForPendingFinalizers();
    }
  }
}

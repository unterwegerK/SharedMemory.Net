using Benchmark;
using System;
using System.Diagnostics;
using System.Threading;

namespace LoadImbalance
{
  /// <summary>
  /// Demonstrates load-imbalance by using a n*sqrt(n) load per item.
  /// </summary>
  class LoadImbalance
  {

    class PartialSum
    {
      public long partialSum;
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

    static void ThreadFunction(object data)
    {
      PartialSum partialSum = (PartialSum)data;
      for (long i = partialSum.index * partialSum.blockSize; i < (partialSum.index + 1) * partialSum.blockSize; i++)
      {
        for (long j = 0; j < (long)Math.Sqrt(i); j++)
        {
          partialSum.partialSum += Collatz(i + j);
        }
      }
    }

    static void Main(string[] args)
    {
      Chart chart = new Chart("Strong Scaling", "Number of Threads", "Parallel Efficiency", false);

      InitializationAndWarmup();

      int maxNumberOfThreads = Environment.ProcessorCount;

      long n = 1 << 14;
      double? baseLine = null;
      for (int numberOfThreads = 1; numberOfThreads <= maxNumberOfThreads; numberOfThreads++)
      {
        Thread[] threads = new Thread[numberOfThreads];
        PartialSum[] partialSums = new PartialSum[numberOfThreads];
        double sum = 0.0;

        for (int t = 0; t < numberOfThreads; t++)
        {
          threads[t] = new Thread(ThreadFunction);
          partialSums[t] = new PartialSum(t, n / numberOfThreads);
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int t = 0; t < numberOfThreads; t++)
        {
          threads[t].Start(partialSums[t]);
        }

        double average = 0.0;
        long max = 0;
        for (int t = 0; t < numberOfThreads; t++)
        {
          threads[t].Join();
          average += partialSums[t].partialSum / (double) numberOfThreads;
          max = Math.Max(max, partialSums[t].partialSum);
        }

        stopwatch.Stop();

        if (baseLine == null) baseLine = stopwatch.ElapsedMilliseconds;
        double speedup = (baseLine / stopwatch.ElapsedMilliseconds) ?? 0.0;
        double efficiency = (baseLine / stopwatch.ElapsedMilliseconds / numberOfThreads) ?? 0.0;
        double imbalance = (max - average) / average;
        Console.WriteLine(
          $"{n} elements, {numberOfThreads} threads: {stopwatch.ElapsedMilliseconds}ms, speedup={speedup:F2} efficiency={efficiency:P} imbalance={imbalance:P} sum={sum}");
        chart.Add("Parallel Efficiency", numberOfThreads, efficiency);
        chart.Add("Imbalance", numberOfThreads, imbalance);
      }

      chart.Save(typeof(LoadImbalance));
      chart.Show();
    }

    private static void InitializationAndWarmup()
    {
      Console.WriteLine("Initialization and Warmup...");
      double warmupSum = 0.0;
      for (int i = 1; i < 100; i++)
      {
        PartialSum partialSum = new PartialSum(0, i);
        ThreadFunction(partialSum);
        warmupSum += partialSum.partialSum;
      }

      if (warmupSum == 0.0) Console.WriteLine("warmupSum=" + warmupSum);
      GC.Collect();
      GC.WaitForPendingFinalizers();
    }
  }
}

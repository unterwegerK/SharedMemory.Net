using Benchmark;
using System;
using System.Diagnostics;
using System.Threading;

namespace FalseSharing
{
  /// <summary>
  /// Demonstrates False Sharing of a cache line by accessing different memory cells which reside in the same cache line.
  /// </summary>
  unsafe class FalseSharing
  {
    private static double[] partialSums;

    class Range
    {
      public readonly int index;
      public readonly int numberOfThreads;
      public readonly long numberOfSupports;
      public double partialSum;

      public Range(int index, int numberOfThreads, long numberOfSupports)
      {
        this.index = index;
        this.numberOfThreads = numberOfThreads;
        this.numberOfSupports = numberOfSupports;
      }
    }

    static void ThreadFunctionSharedArray(object data)
    {
      Range range = (Range)data;
      double w = 1.0 / range.numberOfSupports;
      long supportsPerThread = range.numberOfSupports / range.numberOfThreads; //To support large number of support points
      for (long i = supportsPerThread * range.index; i < supportsPerThread * (range.index + 1); i++)
      {
        double x = w * (i + 0.5);
        partialSums[range.index] += 4.0 / (1.0 + x * x);
      }
    }

    static void ThreadFunctionLocal(object data)
    {
      Range range = (Range)data;
      range.partialSum = 0.0;
      double w = 1.0 / range.numberOfSupports;
      long supportsPerThread = range.numberOfSupports / range.numberOfThreads; //To support large number of support points
      for (long i = supportsPerThread * range.index; i < supportsPerThread * (range.index + 1); i++)
      {
        double x = w * (i + 0.5);
        range.partialSum += 4.0 / (1.0 + x * x);
      }
    }

    private static double Run(int numberOfThreads, long n, ref double sum, ParameterizedThreadStart threadStart, Func<Range,double> sumFunction)
    {
      Thread[] threads = new Thread[numberOfThreads];
      Range[] ranges = new Range[numberOfThreads];

      for (int t = 0; t < numberOfThreads; t++)
      {
        threads[t] = new Thread(threadStart);
        ranges[t] = new Range(t, numberOfThreads, n);
      }

      Stopwatch stopwatch = Stopwatch.StartNew();
      for (int t = 0; t < numberOfThreads; t++)
      {
        threads[t].Start(ranges[t]);
      }

      for (int t = 0; t < numberOfThreads; t++)
      {
        threads[t].Join();
        sum += sumFunction(ranges[t]) / n;
      }
      stopwatch.Stop();
      return stopwatch.ElapsedMilliseconds;
    }

    static void Main(string[] args)
    {
      Chart chart = new Chart("False Sharing", "Number of Threads", "Parallel Efficiency", false);
      InitializationAndWarmup();

      const long n = 1 << 28;
      const int maxNumberOfThreads = 8;

      partialSums = new double[maxNumberOfThreads];

      Console.WriteLine($"Local Accumulation ({n:E2} Elements)");
      double? localBaseLine = null;
      for (int numberOfThreads = 1; numberOfThreads <= maxNumberOfThreads; numberOfThreads++)
      {
        double sum = 0.0;
        double elapsedMilliseconds = Run(numberOfThreads, n, ref sum, ThreadFunctionLocal, range => range.partialSum);

        if (localBaseLine == null) localBaseLine = elapsedMilliseconds;
        double speedup = localBaseLine / elapsedMilliseconds ?? 0.0;
        double efficiency = speedup / numberOfThreads;
        Console.WriteLine(
          $"{numberOfThreads} threads, elapsed={elapsedMilliseconds}ms, speedup={speedup:F}, efficiency={efficiency:P}");
        if (sum == 0.0) Console.WriteLine($"sum={sum}");
        chart.Add("Local Accumulation", numberOfThreads, efficiency);
      }

      Console.WriteLine($"\nShared-Array Accumulation ({n:E2} Elements)");
      double? sharedArrayBaseLine = null;
      for (int numberOfThreads = 1; numberOfThreads <= maxNumberOfThreads; numberOfThreads++)
      {
        double sum = 0.0;
        double elapsedMilliseconds = Run(
          numberOfThreads,
          n,
          ref sum,
          ThreadFunctionSharedArray,
          range => partialSums[range.index]);

        if (sharedArrayBaseLine == null) sharedArrayBaseLine = elapsedMilliseconds;
        double speedup = sharedArrayBaseLine / elapsedMilliseconds ?? 0.0;
        double efficiency = speedup / numberOfThreads;
        Console.WriteLine(
          $"{numberOfThreads} threads, elapsed={elapsedMilliseconds}ms, speedup={speedup:F}, efficiency={efficiency:P}");
        if (sum == 0.0) Console.WriteLine($"sum={sum}");
        chart.Add("Shared-Array Accumulation", numberOfThreads, efficiency);
      }

      chart.Save(typeof(FalseSharing));
      chart.Show();
    }

    private static void InitializationAndWarmup()
    {
      Console.WriteLine("Initialization and Warmup...");
      double warmupSum = 0.0;
      partialSums = new double[1];
      for (int i = 1; i < 10000; i++)
      {
        Range range = new Range(0, 1, i);
        ThreadFunctionSharedArray(range);
        ThreadFunctionLocal(range);
        warmupSum += range.partialSum;
      }

      if (warmupSum == 0.0) Console.WriteLine("warmupSum=" + warmupSum);
      GC.Collect();
      GC.WaitForPendingFinalizers();
    }
  }
}

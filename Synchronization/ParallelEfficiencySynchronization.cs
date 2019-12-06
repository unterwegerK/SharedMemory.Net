using Benchmark;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelEfficiencySynchronization
{
  /// <summary>
  /// Compares parallel efficiency between different synchronization techniques:
  /// Once without explicit synchronization, once with locking via monitor, and once with atomic operations.
  /// </summary>
  class ParallelEfficiencySynchronization
  {
    private static readonly object Lock = new object();

    enum Synchronization
    {
      None,
      Monitor,
      Interlocked
    }

    class PartialSum
    {
      public readonly int index;
      public readonly int numberOfThreads;
      public readonly long numberOfSupports;
      public long sum;

      public PartialSum(int index, int numberOfThreads, long numberOfSupports)
      {
        this.index = index;
        this.numberOfThreads = numberOfThreads;
        this.numberOfSupports = numberOfSupports;
        sum = 0;
      }
    }

    static void IntegrateWithoutSynchronization(object data)
    {
      PartialSum partialSum = (PartialSum)data;
      double w = 1.0 / partialSum.numberOfSupports;
      long supportsPerThread = partialSum.numberOfSupports / partialSum.numberOfThreads;
      for (long i = supportsPerThread * partialSum.index; i < supportsPerThread * (partialSum.index + 1); i++)
      {
        double x = w * (i + 0.5);
        double increment = 4.0 / (1.0 + x * x) / partialSum.numberOfSupports;
        partialSum.sum += (long) (increment * 1e8);
      }
    }

    static void IntegrateWithLock(object data)
    {
      PartialSum partialSum = (PartialSum)data;
      double w = 1.0 / partialSum.numberOfSupports;
      long supportsPerThread = partialSum.numberOfSupports / partialSum.numberOfThreads;
      for (long i = supportsPerThread * partialSum.index; i < supportsPerThread * (partialSum.index + 1); i++)
      {
        double x = w * (i + 0.5);
        double increment = 4.0 / (1.0 + x * x) / partialSum.numberOfSupports;
        lock (Lock) partialSum.sum += (long)(increment * 1e8);
      }
    }

    static void IntegrateWithInterlocked(object data)
    {
      PartialSum partialSum = (PartialSum)data;
      double w = 1.0 / partialSum.numberOfSupports;
      long supportsPerThread = partialSum.numberOfSupports / partialSum.numberOfThreads;
      for (long i = supportsPerThread * partialSum.index; i < supportsPerThread * (partialSum.index + 1); i++)
      {
        double x = w * (i + 0.5);
        double increment = 4.0 / (1.0 + x * x) / partialSum.numberOfSupports;
        Interlocked.Add(ref partialSum.sum, (long)(increment * 1e8));
      }
    }

    static void Main(string[] args)
    {
      Chart chart = new Chart("Synchronization", "Number of Threads", "Speedup", false);

      InitializationAndWarmup();

      for (long n = 1 << 25; n <= 1 << 25/*28*/; n *= 4)
      {
        double? baseLine = null;
        for (int numberOfThreads = 1; numberOfThreads <= Environment.ProcessorCount; numberOfThreads++)
        {
          foreach (Synchronization locking in numberOfThreads == 1
            ? new[] { Synchronization.None, Synchronization.Monitor, Synchronization.Interlocked }
            : new[] { Synchronization.None, Synchronization.Monitor, Synchronization.Interlocked})
          {
            long sum;
            long elapsedMilliseconds = Run(numberOfThreads, n, locking, out sum);

            if (baseLine == null) baseLine = elapsedMilliseconds;
            double speedup = (baseLine / elapsedMilliseconds) ?? 0.0;
            double efficiency = (baseLine / elapsedMilliseconds / numberOfThreads) ?? 0.0;
            Console.WriteLine(
              $"{n} elements, locking={locking}, {numberOfThreads} threads: {elapsedMilliseconds}ms, speedup={speedup:F2} efficiency={efficiency:P} sum={sum}");
            chart.Add($"{n:E2} elements, " + locking, numberOfThreads, speedup);
          }
        }
      }

      chart.Save(typeof(ParallelEfficiencySynchronization));
      chart.Show();
    }

    private static long Run(int numberOfThreads, long n, Synchronization synchronization, out long sum)
    {
      GC.Collect();
      GC.WaitForPendingFinalizers();
      Thread[] threads = new Thread[numberOfThreads];
      PartialSum[] partialSums = new PartialSum[numberOfThreads];
      sum = 0;

      for (int t = 0; t < numberOfThreads; t++)
      {
        if (synchronization == Synchronization.None)
        {
          threads[t] = new Thread(IntegrateWithoutSynchronization);
        }
        else if (synchronization == Synchronization.Monitor)
        {
          threads[t] = new Thread(IntegrateWithLock);
        }
        else if (synchronization == Synchronization.Interlocked)
        {
          threads[t] = new Thread(IntegrateWithInterlocked);
        }

        partialSums[t] = new PartialSum(t, numberOfThreads, n);
      }

      Stopwatch stopwatch = Stopwatch.StartNew();
      for (int t = 0; t < numberOfThreads; t++)
      {
        threads[t].Start(partialSums[t]);
      }

      for (int t = 0; t < numberOfThreads; t++)
      {
        threads[t].Join();
        sum += partialSums[t].sum;
      }

      stopwatch.Stop();
      return stopwatch.ElapsedMilliseconds;
    }

    private static void InitializationAndWarmup()
    {
      Console.WriteLine("Initialization and Warmup...");
      double warmupSum = 0.0;
      for (int i = 1; i < 1000; i++)
      {
        PartialSum partialSum = new PartialSum(0, 1, i);
        IntegrateWithLock(partialSum);
        IntegrateWithoutSynchronization(partialSum);
        IntegrateWithInterlocked(partialSum);
      }

      if (warmupSum == 1.0) Console.WriteLine("warmupSum=" + warmupSum);
      Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
      GC.Collect();
      GC.WaitForPendingFinalizers();
    }
  }
}

﻿using Benchmark;
using System;
using System.Diagnostics;
using System.Threading;

namespace Speedup
{
  /// <summary>
  /// Tests the parallel speedup for a compute-bound numerical integration.
  /// </summary>
  class Speedup
  {

    class PartialSum
    {
      public double partialSum;
      public readonly int index;
      public readonly int numberOfThreads;
      public readonly long numberOfSupports;

      public PartialSum(int index, int numberOfThreads, long numberOfSupports)
      {
        this.index = index;
        this.numberOfThreads = numberOfThreads;
        this.numberOfSupports = numberOfSupports;
      }
    }

    static void ThreadFunction(object data)
    {
      PartialSum partialSum = (PartialSum) data;
      
      double w = 1.0 / partialSum.numberOfSupports;
      long supportsPerThread = partialSum.numberOfSupports / partialSum.numberOfThreads;
      for (long i = supportsPerThread * partialSum.index; i < supportsPerThread * (partialSum.index + 1); i++)
      {
        double x = w * (i + 0.5);
        partialSum.partialSum += 4.0 / (1.0 + x * x);
      }
    }

    static void Main(string[] args)
    {
      Chart chart = new Chart("Strong Scaling", "Number of Threads", "Parallel Efficiency/Speedup", false);

      InitializationAndWarmup();

      //for (long n = 1<<22; n < 1 << 28; n *= 4) //Use to test different problem sizes.
      long n = 1 << 24;
      {
        double? baseLine = null;
        int maxNumberOfThreads = 4; //Change to test different numbers of tests.
        for (int numberOfThreads = 1; numberOfThreads <= maxNumberOfThreads; numberOfThreads++) 
        {
          Thread[] threads = new Thread[numberOfThreads];
          PartialSum[] partialSums = new PartialSum[numberOfThreads];
          double sum = 0.0;

          double elapsed = 0.0;
          int samples = 4;
          for (int i = 0; i < samples; i++)
          {
            for (int t = 0; t < numberOfThreads; t++)
            {
              threads[t] = new Thread(ThreadFunction);
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
              sum += partialSums[t].partialSum / n;
            }

            stopwatch.Stop();
            elapsed += stopwatch.ElapsedMilliseconds / (double)samples;
          }

          if (baseLine == null) baseLine = elapsed;
          double speedup = (baseLine / elapsed) ?? 0.0;
          double efficiency = (baseLine / elapsed / numberOfThreads) ?? 0.0;
          double mflops = 5 * n / (elapsed / 1000.0) * 1e-6 * numberOfThreads;
          Console.WriteLine($"{n} elements, {numberOfThreads} threads: {elapsed}ms, speedup={speedup:F2} efficiency={efficiency:P} sum={sum} mflops={mflops:E2}");
          chart.Add($"{n:E2} elements, Efficiency", numberOfThreads, efficiency);
          chart.Add($"{n:E2} elements, Speedup", numberOfThreads, speedup);
        }
      }

      chart.Save(typeof(Speedup));
      chart.Show();
    }

    private static void InitializationAndWarmup()
    {
      Console.WriteLine("Initialization and Warmup...");
      double warmupSum = 0.0;
      for (int i = 1; i < 100; i++)
      {
        PartialSum partialSum = new PartialSum(0, 1, i);
        ThreadFunction(partialSum);
        warmupSum += partialSum.partialSum;
      }

      if (warmupSum == 0.0) Console.WriteLine("warmupSum=" + warmupSum);
      GC.Collect();
      GC.WaitForPendingFinalizers();
      Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
    }
  }
}

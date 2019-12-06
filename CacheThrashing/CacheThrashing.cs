using System;
using System.Diagnostics;
using System.Threading;
using Benchmark;

namespace CacheThrashing
{
  /// <summary>
  /// Provokes cache thrashing by accessing memory with a stride in the size of the L3 cache.
  /// </summary>
  unsafe class CacheThrashing
  {
    private static object lockObject = new object();
    private static int totalSum;

    private class Arr
    {
      public int* a;
    }

    private static void Run(
      Arr arr,
      int start,
      int cacheSize,
      int cacheLineLength,
      int associativity,
      long repetitions,
      int coreIndex,
      IDummy dummy)
    {
      int* a = arr.a;
      int sum = 0;
      for (long r = 0; r < repetitions; r++)
      {
        dummy.DummyMethod(a);
        for (int c = 0; c < cacheLineLength; c++)
        {
          for (int i = 0; i < 1 << 20; i++)
          {
            int j = ((i * 1009) % associativity) * cacheSize + c;
            sum += a[start + j];
          }
        }
      }


      lock (lockObject)
        totalSum += sum;
    }

    static void Main(string[] args)
    {
      Chart chart = new Chart("Runtime", "Associativity", "Runtime [s]", false);
      InitializationAndWarmup();

      //int cacheSize = (1 << 23) / 4; //Use to provoke cache thrashing. Value must be the L3 cache-size in integer (i.e. cache size in byte divided by sizeof(int)).
      int cacheSize = 2999999; //Use to avoid cache thrashing. May be larger than the L3 cache-size but should be prime.
      int cacheLineLength = 64 / 4;
      int numberOfThreads = 8;
      long repetitions = 1 << 2;
      int cnt = 0;

      IDummy dummy = DummyFactory.Get();

      for (int associativity = 1; associativity <= 16; associativity++)
      {
        int n = cacheSize * associativity * numberOfThreads;
        int[] arr = new int[n];
        for (int i = 0; i < n; i++) arr[i] = cnt++;

        fixed (int* a = arr)
        {
          Arr array = new Arr() { a = a };
          Thread[] threads = new Thread[numberOfThreads];
          for (int i = 0; i < numberOfThreads; i++)
          {
            int start = associativity * cacheSize * i;
            int coreIndex = i;
            threads[i] = new Thread(
              () => Run(array, start, cacheSize, cacheLineLength, associativity, repetitions, coreIndex, dummy));
          }

          Stopwatch stopwatch = Stopwatch.StartNew();
          foreach (Thread thread in threads) thread.Start();

          for (int i = 0; i < numberOfThreads; i++)
          {
            threads[i].Join();
          }

          stopwatch.Stop();


          string sumIndicator = totalSum == 42 ? "42" : "";
          Console.WriteLine($"associativity={associativity} elapsed={stopwatch.Elapsed} {sumIndicator}");
          chart.Add($"{numberOfThreads} threads", associativity, stopwatch.ElapsedMilliseconds);
        }
      }

      chart.Save(typeof(CacheThrashing));
      chart.Show();

      Console.WriteLine("Finished...");
      Console.ReadKey();
    }

    private static void InitializationAndWarmup()
    {
      Console.WriteLine("Initialization and Warmup...");
      double warmupSum = 0.0;
      IDummy dummy = DummyFactory.Get();
      fixed (int* a = new int[1])
      //int[] a = new int[4 * 2 * 2];
      {
        Arr arr = new Arr() { a = a };
        for (int i = 1; i < 10; i++)
        {
          Run(arr, 0, 4, 2, 2, 10, 0, dummy); 
        }
      }

      if (warmupSum == 42) Console.WriteLine("warmupSum=" + warmupSum);
      Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
      GC.Collect();
      GC.WaitForPendingFinalizers();
    }
  }
}

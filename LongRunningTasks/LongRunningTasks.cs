using Benchmark;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LongRunningTasks
{
  /// <summary>
  /// Shows influence of non-working tasks in the task queue.
  /// </summary>
  class LongRunningTasks
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
      PartialSum partialSum = (PartialSum)data;
      double w = 1.0 / partialSum.numberOfSupports;
      long supportsPerThread =
        partialSum.numberOfSupports / partialSum.numberOfThreads;
      for (long i = supportsPerThread * partialSum.index; i < supportsPerThread * (partialSum.index + 1); i++)
      {
        double x = w * (i + 0.5);
        partialSum.partialSum += 4.0 / (1.0 + x * x);
      }
    }

    static void Main(string[] args)
    {
      InitializationAndWarmup();

      List<Task> tasks = new List<Task>();
      int n = 1 << 30;
      int numberOfThreads = 8;

      for (int i = 0; i < numberOfThreads; i++)
      {
        //tasks.Add(new Task(() => Thread.Sleep(3000))); //Comment in to screw up performance. Overall execution time is increased by 3 seconds.
        //tasks.Add(new Task(() => Thread.Sleep(3000), TaskCreationOptions.LongRunning)); //Use instead of upper line to gain comparable performance to initial state without sleeping tasks.
      }

      for (int i = 0; i < numberOfThreads; i++)
      {
        int id = i;
        tasks.Add(new Task(() => ThreadFunction(new PartialSum(id, numberOfThreads, n))));
      }

      Stopwatch stopwatch = Stopwatch.StartNew();

      for (int i = 0; i < tasks.Count; i++)
      {
        tasks[i].Start();
      }

      Task.WaitAll(tasks.ToArray());

      stopwatch.Stop();
      Console.WriteLine($"Elapsed: {stopwatch.Elapsed}");

      Console.WriteLine("\n\nFinished...");

      Console.ReadKey();
    }

    private static void InitializationAndWarmup()
    {
      Console.WriteLine("Initialization and Warmup...");
      double warmupSum = 0.0;
      for (int i = 1; i < 10000; i++)
      {
        PartialSum partialSum = new PartialSum(0, 1, i);
        ThreadFunction(partialSum);
        warmupSum += partialSum.partialSum;
      }

      if (warmupSum == 0.0) Console.WriteLine("warmupSum=" + warmupSum);
      GC.Collect();
      GC.WaitForPendingFinalizers();
    }
  }
}

//tasks.Add(new Task(() => Thread.Sleep(3000), TaskCreationOptions.LongRunning));

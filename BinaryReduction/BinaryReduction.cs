using Benchmark;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BinaryReduction
{
  /// <summary>
  /// Demonstrates a binary reduction-tree via TPL.
  /// </summary>
  class BinaryReduction
  {

    static double Integrate(long start, long end, long numberOfSupports, int level, int maxLevel)
    {
      if (level < maxLevel)
      {
        double sum1 = 0.0;
        double sum2 = 0.0;

        Parallel.Invoke(
          () => sum1 = Integrate(start, (start + end) / 2, numberOfSupports, level + 1, maxLevel),
          () => sum2 = Integrate((start + end) / 2, end, numberOfSupports, level + 1, maxLevel)
        );

        return sum1 + sum2;
      }
      else
      {
        double w = 1.0 / numberOfSupports;
        double sum = 0;
        for (long i = start; i < end; i++)
        {
          double x = w * (i + 0.5);
          sum += 4.0 / (1.0 + x * x) / numberOfSupports;
        }

        return sum;
      }
    }

    static void Main(string[] args)
    {
      Chart chart = new Chart("Binary Reduction", "Number of Threads", "Parallel Efficiency", false);

      InitializationAndWarmup();

      for (long n = 1 << 26; n <= 1 << 26; n *= 4)
      {
        double? baseLine = null;
        for (int numberOfTasks = 1; numberOfTasks <= 8; numberOfTasks *= 2)
        {

          Stopwatch stopwatch = Stopwatch.StartNew();

          double sum = Integrate(0, n, n, 0, (int)Math.Log(numberOfTasks, 2));

          stopwatch.Stop();
          long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

          if (baseLine == null) baseLine = elapsedMilliseconds;
          double speedup = (baseLine / elapsedMilliseconds) ?? 0.0;
          double efficiency = (baseLine / elapsedMilliseconds / numberOfTasks) ?? 0.0;
          Console.WriteLine(
            $"{n} elements, {numberOfTasks} threads: {elapsedMilliseconds}ms, speedup={speedup:F2} efficiency={efficiency:P} sum={sum}");
          chart.Add($"{n:E2} element", numberOfTasks, speedup);
        }
      }

      chart.Save(typeof(BinaryReduction));
      chart.Show();
    }

    private static void InitializationAndWarmup()
    {
      Console.WriteLine("Initialization and Warmup...");
      double warmupSum = 0.0;
      for (int i = 1; i < 1000; i++)
      {
        Integrate(0, 128, 128, 0, 2);
      }

      if (warmupSum == 1.0) Console.WriteLine("warmupSum=" + warmupSum);
      Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
      GC.Collect();
      GC.WaitForPendingFinalizers();
    }
  }
}

using Benchmark;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Math = System.Math;

namespace SerialVectorTriad
{
  /// <summary>
  /// Benchmarks the memory performance.
  /// 
  /// Schönauer, Willi. Scientific supercomputing: architecture and use of shared and distributed memory parallel computers. Willi Schönauer, 2000.
  /// </summary>
  unsafe class VectorTriad {

    private static int accumulator = 0;

    private static void RunVectorTriad(TriadData data, long vectorLength, long repetitions, IDummy dummy)
    {
      fixed (double* ap = data.a)
      fixed (double* bp = data.b)
      fixed (double* cp = data.c)
      fixed (double* dp = data.d)
      {
        for (long r = 0; r < repetitions; r++)
        {
          if (ap[1] < 1) dummy.DummyMethod(ap, bp, cp, dp);

          for (long i = 0; i < vectorLength; i++)
          {
            long j = i;//(i * 1009) % vectorLength;
            ap[j] = bp[j] + cp[j] * dp[j];
          }
        }
      }
    }

    private static double RunVectorTriad(long vectorLength, long repetitions, int numberOfThreads)
    {

      GC.Collect();
      GC.WaitForPendingFinalizers();

      TriadData[] data = new TriadData[numberOfThreads];
      for (int i = 0; i < numberOfThreads; i++)
      {
        data[i] = new TriadData(vectorLength);
      }

      IDummy dummy = DummyFactory.Get();

      Task[] tasks = new Task[numberOfThreads];
      for (int i = 0; i < numberOfThreads; i++)
      {
        int threadIndex = i;
        tasks[i] = new Task(() => RunVectorTriad(data[threadIndex], vectorLength, repetitions, dummy));
      }

      Stopwatch stopwatch = Stopwatch.StartNew();

      for (int i = 0; i < numberOfThreads; i++) tasks[i].Start();
      Task.WaitAll(tasks.Take(numberOfThreads).ToArray());

      stopwatch.Stop();


      return (double) stopwatch.ElapsedMilliseconds / repetitions;
    }

    static void Main(string[] args)
    {
      Chart chart = new Chart("Vector Triad", "Memory [Bytes]", "Throughput", true);
      Warmup();
      
      int threads = 4;
      {
        long maxLength = (1 << 28) / sizeof(double);
        for (long n = 1 << 1; n <= maxLength; n += Math.Max(1, n))
        {
          long r = maxLength / n;
          double elapsedMilliseconds = RunVectorTriad(n, r, threads);

          double gigabytePerSecond = n * sizeof(double) * 4 * threads / (elapsedMilliseconds / 1000.0) / (1 << 30);
          double mflops = n * 2 * threads / (elapsedMilliseconds / 1000.0) / (1 << 20);

          Console.WriteLine(
            $"{n} ({r}): {elapsedMilliseconds:F2}ms/Iteration, {gigabytePerSecond:F2}GB/s, {mflops:F2}MFLOPS");

          chart.Add($"Throughput [GB/s]", n * sizeof(double) * 4 * threads, gigabytePerSecond);
        }
      }

      chart.Save(typeof(VectorTriad));
      chart.Show();

      Console.WriteLine("Finished..." + (accumulator == 0 ? " " : ""));
      Console.Read();
    }

    private static void Warmup()
    {
      double warmupResult = 0;
      Console.WriteLine("Initialization and Warmup...");
      for (int i = 1 << 4; i < 1 << 12; i *= 2)
      {
        warmupResult += RunVectorTriad(i, 1, 4);
      }

      accumulator += (int) warmupResult;
    }
  }
}

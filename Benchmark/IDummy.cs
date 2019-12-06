using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark
{
  public unsafe interface IDummy
  {

    void DummyMethod(int[] a);

    void DummyMethod(int* a);

    void DummyMethod(double* a, double* b, double* c, double* d);

    void DummyMethod(double[] a, double[] b, double[] c, double[] d);
  }

  public static class DummyFactory
  {
    public static IDummy Get()
    {
      return new Dummy();
    }
  }

  internal unsafe class Dummy : IDummy
  {
    public void DummyMethod(int[] a)
    {

    }

    public void DummyMethod(int* a)
    {
    }

    public void DummyMethod(double* a, double* b, double* c, double* d)
    {
    }

    public void DummyMethod(double[] a, double[] b, double[] c, double[] d)
    {
      return;
    }
  }
}

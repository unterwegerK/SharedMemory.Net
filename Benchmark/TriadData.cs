using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark
{
  public class TriadData
  {
    public double[] a;
    public double[] b;
    public double[] c;
    public double[] d;

    public TriadData(long vectorLength)
    {
      a = new double[vectorLength];
      b = new double[vectorLength];
      c = new double[vectorLength];
      d = new double[vectorLength];

      int cnt = 0;
      for (int i = 0; i < vectorLength; i++)
      {
        a[i] = cnt++;
        b[i] = cnt++;
        c[i] = cnt++;
        d[i] = cnt++;
      }
    }
  }
}

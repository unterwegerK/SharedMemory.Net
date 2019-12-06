using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace Benchmark
{
  public class Chart
  {
    private readonly string name;
    private readonly string xAxisTitle;
    private readonly string yAxisTitle;
    private readonly bool xLogarithmic;
    private readonly bool yLogarithmic = false;
    private readonly IDictionary<string, Series> dataPoints = new Dictionary<string, Series>();

    public Chart(string name, string xAxisTitle, string yAxisTitle, bool xLogarithmic)
    {
      this.name = name;
      this.xAxisTitle = xAxisTitle;
      this.yAxisTitle = yAxisTitle;
      this.xLogarithmic = xLogarithmic;
    }

    public void Add(string seriesName, double x, double y)
    {
      Series series;
      if (!dataPoints.TryGetValue(seriesName, out series))
      {
        series = new Series($"[{dataPoints.Values.Count}] {seriesName}");
        series.XValueType = ChartValueType.Int64;
        series.YValueType = ChartValueType.Double;
        series.ChartType = SeriesChartType.Line;
        series.MarkerStyle = MarkerStyle.Circle;
        series.LegendText = seriesName;
        series.MarkerSize = 10;
        series.BorderWidth = 2;
        dataPoints[seriesName] = series;
      }

      if(y > 0.0 || !yLogarithmic)
      series.Points.AddXY(x, y);
    }


    public void Show()
    {
      ChartForm chartForm = new ChartForm();
      chartForm.Text = name;
      chartForm.chart.Series.Clear();
      chartForm.chart.ChartAreas[0].AxisX.IsLogarithmic = xLogarithmic;
      chartForm.chart.ChartAreas[0].AxisX.Title = xAxisTitle;
      chartForm.chart.ChartAreas[0].AxisX.TitleFont = chartForm.Font;
      chartForm.chart.ChartAreas[0].AxisX.LabelAutoFitMaxFontSize = (int)chartForm.Font.Size;
      chartForm.chart.ChartAreas[0].AxisY.Title = yAxisTitle;
      chartForm.chart.ChartAreas[0].AxisY.IsLogarithmic = yLogarithmic;
      chartForm.chart.ChartAreas[0].AxisY.TitleFont = chartForm.Font;
      chartForm.chart.ChartAreas[0].AxisY.LabelAutoFitMaxFontSize = (int)chartForm.Font.Size;
      chartForm.chart.Legends.FirstOrDefault().LegendItemOrder = LegendItemOrder.SameAsSeriesOrder;

      foreach (Series series in dataPoints.Values.OrderByDescending(s => s.Name))
      {
        chartForm.chart.Series.Add(series);
      }

      chartForm.ShowDialog();
    }

    public void Save(Type type)
    {
      string fileName = type.FullName + "_results.csv";
      using (StreamWriter file = new StreamWriter(fileName))
      {
        foreach (Series series in dataPoints.Values)
        {
          file.Write(series.Name + "\t");
          foreach (var point in series.Points)
          {
            file.Write(point.XValue + "\t");
          }
          file.Write("\nValues:\t");
          foreach (var point in series.Points)
          {
            file.Write(point.YValues.FirstOrDefault() + "\t");
          }
          file.Write("\n");
        }
      }
    }
  }
}

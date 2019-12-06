namespace Benchmark
{
  partial class ChartForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
      System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
      System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
      this.chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
      ((System.ComponentModel.ISupportInitialize)(this.chart)).BeginInit();
      this.SuspendLayout();
      // 
      // chart
      // 
      chartArea2.Name = "ChartArea1";
      this.chart.ChartAreas.Add(chartArea2);
      this.chart.Dock = System.Windows.Forms.DockStyle.Fill;
      legend2.Name = "Legend1";
      this.chart.Legends.Add(legend2);
      this.chart.Location = new System.Drawing.Point(0, 0);
      this.chart.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
      this.chart.Name = "chart";
      series2.ChartArea = "ChartArea1";
      series2.Legend = "Legend1";
      series2.Name = "Series1";
      this.chart.Series.Add(series2);
      this.chart.Size = new System.Drawing.Size(551, 313);
      this.chart.TabIndex = 0;
      // 
      // ChartForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(551, 313);
      this.Controls.Add(this.chart);
      this.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
      this.Name = "ChartForm";
      this.Text = "ChartForm";
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      ((System.ComponentModel.ISupportInitialize)(this.chart)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    public System.Windows.Forms.DataVisualization.Charting.Chart chart;
  }
}
﻿/*
 * Created by SharpDevelop.
 * User: RTM
 * Date: 3.7.2017
 * Time: 18:31
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;

namespace Elektrik
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		readonly List<string> Months = new List<string> {
			"Tammi",
			"Helmi",
			"Maalis",
			"Huhti",
			"Touko",
			"Kesä",
			"Heinä",
			"Elo",
			"Syys",
			"Loka",
			"Marras",
			"Joulu"
		};

		int _currentMonth;
		RecordCollection _data;
		ProgressForm _progressForm = new ProgressForm();
		List<int> _years;
			
		public MainForm()
		{
			InitializeComponent();						
		}
		
		void MainFormLoad(object sender, EventArgs e)
		{
			_data = new RecordCollection();				
			_data.CsvFileName = Directory.GetFiles(Application.StartupPath, "*.csv").FirstOrDefault();
			
			if (string.IsNullOrEmpty(_data.CsvFileName)) {
				MessageBox.Show("CSV-file not found from startup path!");
				return;
			}
			
			Text += " - " + Path.GetFileNameWithoutExtension(_data.CsvFileName);
						
			_progressForm.DoWork += _progressForm_DoWork;
			_progressForm.Argument = _data;
			_progressForm.StartPosition = FormStartPosition.CenterScreen;
			
			var result = _progressForm.ShowDialog();
			switch (result) {
				case DialogResult.Cancel:
					return;
				case DialogResult.Abort:
					MessageBox.Show(_progressForm.Result.Error.Message);
					return;
				case DialogResult.OK:
					break;
			}

			_years = _data.Years;
			InitGui();
								
			UpdateYearAndMonthCharts();
			
			WindowState = FormWindowState.Maximized;
		}
		
		static void _progressForm_DoWork(ProgressForm sender, DoWorkEventArgs e)
		{
			var myArgument = e.Argument as RecordCollection;
		 
			try 
			{
				var lines = File.ReadAllLines(myArgument.CsvFileName);
				
				var i = 0;
				var header = false;
				foreach (var line in lines) 
				{
					if (!header) 
					{
						header = true;
						continue;
					}
					
					var arr = line.Split(';');
					var dt = DateTime.Parse(arr[0] + " " + arr[1]);
					var kwh = Convert.ToDouble(arr[2]);
					var temp = Convert.ToDouble(arr[3]);
					
					myArgument.Items.Add(new Record(dt, kwh, temp));
					i++;
					
					if (sender.CancellationPending) 
					{
						e.Cancel = true;
						return;
					}	
			        
					if (i % 1000 == 0) 
					{
						sender.SetProgress(i, "Step " + i + " / " + lines.Count());
						System.Threading.Thread.Sleep(5);
					}						
				}
			} 
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + " " + ex.InnerException, "ReadCsvFile");
			}			
		}
		
		void InitGui()
		{
			chartYears.Legends.Clear();
			chartYears.ChartAreas[0].AxisY.Title = "kWh";
			chartYears.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
			chartYears.Titles.Add(new Title("Vuosikulutus"));
			chartYears.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
			//chartYears.ChartAreas[0].Area3DStyle.Enable3D = true;
			
			chartMonths.Series.Clear();
			chartMonths.ChartAreas[0].AxisX.LabelStyle.Interval = 1;
			//chart2.ChartAreas[0].AxisX.LabelStyle.Angle = 45;
			chartMonths.ChartAreas[0].AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep45;
			chartMonths.ChartAreas[0].AxisY.Title = "kWh";
			chartMonths.ChartAreas[0].AxisY.Maximum = 600;
			chartMonths.Titles.Add(new Title("Kuukausikulutus"));
			
			chartDays.Series.Clear();
			chartDays.ChartAreas[0].AxisX.LabelStyle.Interval = 1;
			chartDays.ChartAreas[0].AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep45;
			chartDays.ChartAreas[0].AxisY.Title = "kWh";
			chartDays.ChartAreas[0].AxisY.Maximum = 40;
			//chartDays.ChartAreas[0].Area3DStyle.Enable3D = true;
			//chartDays.ChartAreas[0].Area3DStyle.Perspective = 5;
			//chartDays.ChartAreas[0].Area3DStyle.Rotation = 65;
			//chartDays.ChartAreas[0].Area3DStyle.Inclination = 65;
			//chartDays.ChartAreas[0].Area3DStyle.IsRightAngleAxes = true;
			chartDays.Titles.Add(new Title("Päiväkulutus"));
			
			chartHours.Series.Clear();
			chartHours.ChartAreas[0].AxisX.LabelStyle.Interval = 1;
			chartHours.ChartAreas[0].AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep45;
			chartHours.ChartAreas[0].AxisY.Title = "kWh";
			chartHours.ChartAreas[0].AxisY.Maximum = 6;
			//chartHours.ChartAreas[0].Area3DStyle.Enable3D = true;
			chartHours.Titles.Add(new Title("Tuntikulutus"));			

			// Analysis tab
			textBoxYears.Text = _years.First().ToString(); //string.Join(",", _years);
			textBoxMonths.Text = "1"; //string.Join(",", Enumerable.Range(1, 12).ToList());
			textBoxDays.Text = string.Join(",", Enumerable.Range(1, 31).ToList());
			textBoxHours.Text = string.Join(",", Enumerable.Range(0, 24).ToList());
			
			chartAnalysis.Series.Clear();
			chartAnalysis.ChartAreas[0].AxisX.LabelStyle.Interval = 1;
			chartAnalysis.ChartAreas[0].AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep45;
			chartAnalysis.ChartAreas[0].AxisY.Title = "kWh";
			chartAnalysis.ChartAreas[0].AxisX.LabelStyle.Enabled = false;			
			//chartAnalysis.ChartAreas[0].AxisY.Maximum = 40;
			chartAnalysis.ChartAreas[0].Area3DStyle.Enable3D = true;
			//chartAnalysis.ChartAreas[0].Area3DStyle.Perspective = 5;
			chartAnalysis.ChartAreas[0].Area3DStyle.Rotation = 65;
			chartAnalysis.ChartAreas[0].Area3DStyle.Inclination = 65;
			chartAnalysis.ChartAreas[0].Area3DStyle.IsRightAngleAxes = true;
			//chartAnalysis.Titles.Add(new Title("Analyysi"));
			
			UpdateDayChart(DateTime.Now.Month);
		}
		
		void UpdateYearAndMonthCharts()
		{
			chartMonths.Series.Clear();
			chartYears.Series.Clear();
			
			foreach (var year in _years) 
			{
				var yearLabel = year.ToString();
				
				// Total per year	
				var series = new Series(yearLabel);
				chartYears.Series.Add(series);
				series.Points.AddXY(yearLabel, _data.YearTotalKwh(year));			
				series.ToolTip = "Ka. / pvä: " + _data.GetYearDailyAverage(year).ToString("F1") + " kWh";
				series.IsValueShownAsLabel = true;
				series.LabelFormat = "####";
				series["PointWidth"] = "1.5";
								
				// Total per month	
				var monthSeries = new Series(yearLabel);				
				monthSeries.ChartType = SeriesChartType.Column;						
				//series.IsValueShownAsLabel = true;
				chartMonths.Series.Add(monthSeries);
				
				for (var i = 1; i <= 12; i++) 
				{
					monthSeries.Points.AddXY(Months[i - 1], _data.MonthlyTotalKwh(year, i));
				}
			}	
			
			chartYears.AlignDataPointsByAxisLabel();			
		}
		
		void UpdateDayChart(int month)
		{		
			chartDays.Series.Clear();			
			chartDays.Titles[0].Text = "Päiväkulutus / " + GetMonth(month);
			
			foreach (var year in _years) {
				var yearLabel = year.ToString();
				
				var monthData = _data.GetMonthData(year, month);
				var dayCount = monthData.Select(x => x.Timestamp.Day).Distinct().Count();

				// Total per month	
				var series = new Series(yearLabel);
				series.ChartType = SeriesChartType.Column;
				//series.BorderWidth = 12;
				//series.IsValueShownAsLabel = true;
				chartDays.Series.Add(series);
				
				for (var i = 1; i <= dayCount; i++) {
					series.Points.AddXY(i.ToString(), _data.DailyTotalKwh(year, month, i));
				}
			}	

			_currentMonth = month;
			UpdateHourChart(1);
		}
		
		string GetMonth(int month)
		{
			return Months[month - 1] + "kuu";
		}
		
		void UpdateHourChart(int day)
		{		
			chartHours.Series.Clear();			
			chartHours.Titles[0].Text = "Tuntikulutus / " + day + ". " + GetMonth(_currentMonth) + "ta";
			
			foreach (var year in _years) 
			{
				var yearLabel = year.ToString();

				var series = new Series(yearLabel);
				series.ChartType = SeriesChartType.Column;
				//series.BorderWidth = 12;
				//series.IsValueShownAsLabel = true;
				chartHours.Series.Add(series);
				
				var hours = _data.GetDayData(year, _currentMonth, day);
				if (hours.Count == 0) 
				{
					continue;
				}
				
				for (var i = 1; i <= hours.Count; i++) 
				{
					series.Points.AddXY(i.ToString("00"), hours[i - 1].KwhTotal);
				}
			}				
			chartHours.ChartAreas[0].RecalculateAxesScale();
		}
		
		void ChartMonthsMouseDown(object sender, MouseEventArgs e)
		{
			var result = chartMonths.HitTest(e.X, e.Y);		        		  
		    
			if (result.ChartElementType == ChartElementType.DataPoint) 
			{
				var labelStr = result.Series.Points[result.PointIndex].AxisLabel;
				for (var i = 0; i < Months.Count; i++) 
				{
					if (Months[i].Equals(labelStr)) 
					{
						UpdateDayChart(i + 1);
						return;
					}
				}
			}		
		}
		
		void ChartDaysMouseDown(object sender, MouseEventArgs e)
		{
			var result = chartDays.HitTest(e.X, e.Y);		        		  
		    
			if (result.ChartElementType == ChartElementType.DataPoint) 
			{
				var labelStr = result.Series.Points[result.PointIndex].AxisLabel;
				var day = Convert.ToInt32(labelStr);
				UpdateHourChart(day);
			    
				ShowDaysAndHours();
			} 
			else
			{
				ExpandDaysChart();
			}
		}
		
		void ChartHoursMouseDown(object sender, MouseEventArgs e)
		{
			ExpandHoursChart();
		}

		void ChartYearsMouseDown(object sender, MouseEventArgs e)
		{
			var result = chartYears.HitTest(e.X, e.Y);		        		  
		    
			if (result.ChartElementType == ChartElementType.DataPoint) 
			{	
				var labelStr = result.Series.Points[result.PointIndex].AxisLabel;
				var year = Convert.ToInt32(labelStr);	
				_years = new List<int> { year };
				
				/*using(var writer = new StreamWriter(year + ".txt", false))
		      	{
					foreach(var item in _data.GetYearData(year))
					{
						writer.WriteLine(item.Timestamp + ";" + item.KwhTotal);
					}
		      	}*/
			}
			else
			{
				_years = _data.Years;
			}
			
			UpdateYearAndMonthCharts();
			UpdateDayChart(1);			
		}
		
		void ExpandDaysChart()
		{
			if (!chartHours.Visible) 
			{
				ShowDaysAndHours();
			} 
			else 
			{
				chartDays.Visible = true;			    				
				chartHours.Visible = false;			    				
				tableLayoutPanelLowerSummary.SetColumnSpan(chartDays, 2);
			}
		}
		
		void ExpandHoursChart()
		{
			if (!chartDays.Visible) 
			{
				ShowDaysAndHours();
			} 
			else {
				chartDays.Visible = false;			    				
				chartHours.Visible = true;			    				
				tableLayoutPanelLowerSummary.SetColumnSpan(chartHours, 2);
			}
		}

		void ShowDaysAndHours()
		{
			chartDays.Visible = true;	
			chartHours.Visible = true;
			tableLayoutPanelLowerSummary.SetColumnSpan(chartDays, 1);
			tableLayoutPanelLowerSummary.SetColumnSpan(chartHours, 1);
		}
		
		void ButtonAnalyzeClick(object sender, EventArgs e)
		{
			chartAnalysis.Series.Clear();
			
			var years = textBoxYears.Text.Split(',').ToList().Select(x => Convert.ToInt32(x)).ToList();
			var months = textBoxMonths.Text.Split(',').ToList().Select(x => Convert.ToInt32(x)).ToList();
			var days = textBoxDays.Text.Split(',').ToList().Select(x => Convert.ToInt32(x)).ToList();
			var hours = textBoxHours.Text.Split(',').ToList().Select(x => Convert.ToInt32(x)).ToList();

			foreach (var hour in hours)
			{
				var hourLabel = hour.ToString("00");
				var series = new Series(hourLabel);
				series.ChartType = SeriesChartType.Column;	
				series.ToolTip = hourLabel;
				chartAnalysis.Series.Add(series);	
				
				foreach (var year in years)
				{
					foreach(var month in months)
					{
						foreach(var day in days)
						{
							var hourData = _data.HourTotalKwh(year, month, day, hour);
							
							series.Points.AddXY(hour.ToString("00"), hourData);
						}	
					}					
				}				
			}				
		}
		
		void test()
		{
			var years = textBoxYears.Text.Split(',').ToList().Select(x => Convert.ToInt32(x)).ToList();
			var months = textBoxMonths.Text.Split(',').ToList().Select(x => Convert.ToInt32(x)).ToList();
			var days = textBoxDays.Text.Split(',').ToList().Select(x => Convert.ToInt32(x)).ToList();
			var hours = textBoxHours.Text.Split(',').ToList().Select(x => Convert.ToInt32(x)).ToList();
			
			foreach (var year in years) 
			{
				var yearLabel = year.ToString();

				var series = new Series(yearLabel);
				series.ChartType = SeriesChartType.Column;
				//series.BorderWidth = 12;
				//series.IsValueShownAsLabel = true;
				
				foreach(var month in months)
				{
					foreach(var day in days)
					{
						var hoursData = _data.GetDayData(year, month, day);
						if (hoursData.Count == 0) 
						{
							continue;
						}
						
						for (var i = 1; i <= hoursData.Count; i++) 
						{
							series.Points.AddXY(i.ToString("00"), hoursData[i - 1].KwhTotal);
						}						
					}	
				}
				chartAnalysis.Series.Add(series);				
			}				
		}
	}
}

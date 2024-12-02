using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using QLTV_TranBin.Chart.CaculateCharts;
using QLTV_TranBin.ModelCharts;
using QLTV_TranBin.Models;
namespace QLTV_TranBin
{
    /// <summary>
    /// Interaction logic for BaoCaoThongKe.xaml
    /// </summary>
    public partial class BaoCaoThongKe : UserControl
    {
        public SeriesCollection RowSeriesCollection { get; set; }
        public SeriesCollection PieSeriesCollection { get; set; }
        public SeriesCollection LineSeriesCollection { get; set; }
        public SeriesCollection HorizontalRowSeriesCollection { get; set; }

        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        public BaoCaoThongKe()
        {
            InitializeComponent();
            var theLoaiMuon = new TinhTheLoaiMuon(new QLTV2Context()).GetTheLoaiMuon();
            theLoaiMuon = CalculatePercentage(theLoaiMuon);

            PieSeriesCollection = new SeriesCollection();
            foreach (var item in theLoaiMuon)
            {
                PieSeriesCollection.Add(new PieSeries
                {
                    Title = item.TenTheLoai,
                    Values = new ChartValues<ObservableValue> { new ObservableValue(item.PhanTram) },
                    DataLabels = true
                });
            }

            //adding values or series will update and animate the chart automatically
            //SeriesCollection.Add(new PieSeries());
            //SeriesCollection[0].Values.Add(5);
            RowSeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "2015",
                    Values = new ChartValues<double> { 10, 50, 39, 50 }
                }
            };

            //adding series will update and animate the chart automatically
            RowSeriesCollection.Add(new ColumnSeries
            {
                Title = "2016",
                Values = new ChartValues<double> { 11, 56, 42 }
            });

            //also adding values updates and animates the chart automatically
            RowSeriesCollection[1].Values.Add(48d);

            Labels = new[] { "Maria", "Susan", "Charles", "Frida" };
            Formatter = value => value.ToString("N");

            HorizontalRowSeriesCollection = new SeriesCollection
            {
                new RowSeries
                {
                    Title = "2015",
                    Values = new ChartValues<double> { 10, 50, 39, 50 }
                }
            };

            //adding series will update and animate the chart automatically
            

            //also adding values updates and animates the chart automatically
            HorizontalRowSeriesCollection[0].Values.Add(48d);

            Labels = new[] { "Maria", "Susan", "Charles", "Frida" };
            Formatter = value => value.ToString("M");

            LineSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Series 1",
                    Values = new ChartValues<double> { 4, 6, 5, 2 ,4 }
                },
                new LineSeries
                {
                    Title = "Series 2",
                    Values = new ChartValues<double> { 6, 7, 3, 4 ,6 },
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Series 3",
                    Values = new ChartValues<double> { 4,2,7,2,7 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15
                }
            };

            Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
            Formatter = value => value.ToString("C");

            //modifying the series collection will animate and update the chart
            LineSeriesCollection.Add(new LineSeries
            {
                Title = "Series 4",
                Values = new ChartValues<double> { 5, 3, 2, 4 },
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = Brushes.Gray
            });

            //modifying any series values will also animate and update the chart
            LineSeriesCollection[3].Values.Add(5d);


            DataContext = this;

        }


        private void UpdateCartesianChart()
        {
            var random = new Random();
            foreach (var series in RowSeriesCollection)
            {
                for (int i = 0; i < series.Values.Count; i++)
                {
                    series.Values[i] = random.Next(10, 100);
                }
            }
        }
        private void UpdatePieChart()
        {
            var random = new Random();
            foreach (var series in PieSeriesCollection)
            {
                foreach (var value in series.Values.Cast<ObservableValue>())
                {
                    value.Value = random.Next(5, 15);
                }
            }
        }


        //private void AddSeriesOnClick(object sender, RoutedEventArgs e)
        //{
        //    var r = new Random();
        //    var c = SeriesCollection.Count > 0 ? SeriesCollection[0].Values.Count : 5;

        //    var vals = new ChartValues<ObservableValue>();

        //    for (var i = 0; i < c; i++)
        //    {
        //        vals.Add(new ObservableValue(r.Next(0, 10)));
        //    }

        //    SeriesCollection.Add(new PieSeries
        //    {
        //        Values = vals
        //    });
        //}

        //private void RemoveSeriesOnClick(object sender, RoutedEventArgs e)
        //{
        //    if (SeriesCollection.Count > 0)
        //        SeriesCollection.RemoveAt(0);
        //}

        //private void AddValueOnClick(object sender, RoutedEventArgs e)
        //{
        //    var r = new Random();
        //    foreach (var series in SeriesCollection)
        //    {
        //        series.Values.Add(new ObservableValue(r.Next(0, 10)));
        //    }
        //}

        //private void RemoveValueOnClick(object sender, RoutedEventArgs e)
        //{
        //    foreach (var series in SeriesCollection)
        //    {
        //        if (series.Values.Count > 0)
        //            series.Values.RemoveAt(0);
        //    }
        //}

        private void RestartOnClick(object sender, RoutedEventArgs e)
        {
            Chart.Update(true, true);
            
        }
        public List<TheLoaiMuon> CalculatePercentage(List<TheLoaiMuon> theLoaiMuons)
        {
            int total = theLoaiMuons.Sum(x => x.SoLuongMuon);

            foreach (var item in theLoaiMuons)
            {
                item.PhanTram = ((double)item.SoLuongMuon / total) * 100;
            }

            return theLoaiMuons;
        }


    }
}

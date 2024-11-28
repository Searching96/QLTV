using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using LoginApp.Data;
using LoginApp.Models;

namespace LoginApp
{
    public partial class UCThongKe : Window
    {
        private readonly AppDbContext _context;
        private readonly Random _random = new Random();

        public UCThongKe()
        {
            InitializeComponent();
            _context = new AppDbContext();
        }

        private void CriteriaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateChartVisibility();
        }

        //Hiển thị/Ẩn
        private void UpdateChartVisibility()
        {
            var selectedCriteria = (CriteriaComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (selectedCriteria == "Ngày lập thẻ")
            {
                LineChartBorder.Visibility = Visibility.Visible;
                PieChartBorder.Visibility = Visibility.Collapsed;
                ColumnChartBorder.Visibility = Visibility.Collapsed;
            }
            else 
            {
                LineChartBorder.Visibility = Visibility.Collapsed;
                PieChartBorder.Visibility = Visibility.Visible;
                ColumnChartBorder.Visibility = Visibility.Visible;
            }
        }

        //Lấy dữ liệu và cập nhật
        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var startDate = StartDatePicker.SelectedDate ?? DateTime.MinValue;
                var endDate = EndDatePicker.SelectedDate ?? DateTime.MaxValue;
                var criteria = (CriteriaComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (criteria == "Ngày lập thẻ")
                {
                    var data = GetReadersByRegistrationDate(startDate, endDate);
                    UpdateLineChartByDate(data);
                }
                else 
                {
                    var data = GetReadersByType(startDate, endDate);
                    UpdatePieChartByType(data);
                    UpdateColumnChartByType(data);
                }

                UpdateChartVisibility();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //Truy vấn số độc giả theo ngày
        private List<ReaderDateCount> GetReadersByRegistrationDate(DateTime startDate, DateTime endDate)
        {
            return _context.Readers
                .Where(r => r.NgayLapThe >= startDate && r.NgayLapThe <= endDate)
                .GroupBy(r => r.NgayLapThe.Date)
                .Select(g => new ReaderDateCount
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();
        }
        //Truy vấn loại độc giả
        private List<ReaderTypeCount> GetReadersByType(DateTime startDate, DateTime endDate)
        {
            var readers = _context.Readers
       .Where(r => r.NgayLapThe >= startDate && r.NgayLapThe <= endDate)
       .GroupBy(r => r.LoaiDocGia)
       .Select(g => new
       {
           Type = g.Key,
           Count = g.Count(),
           TotalDebt = g.Sum(r => r.TongNo),
           ReadersInGroup = g.ToList() // Lấy danh sách độc giả trong mỗi nhóm
       })
       .ToList();

            return readers.Select(g => new ReaderTypeCount
            {
                Type = g.Type,
                Count = g.Count,
                TotalDebt = g.TotalDebt,
                AverageAge = g.ReadersInGroup.Average(r => (DateTime.Now - r.NgaySinh).TotalDays / 365.25) 
            })
            .ToList();
        }
        private void UpdateLineChartByDate(List<ReaderDateCount> data)
        {
            LineChartCanvas.Children.Clear();

            if (!data.Any()) return;

            double maxCount = data.Max(x => x.Count);
            double minCount = data.Min(x => x.Count);
            double range = maxCount - minCount;
            double chartWidth = LineChartCanvas.ActualWidth;
            double chartHeight = LineChartCanvas.ActualHeight - 30; 

            // Tính thời gian
            DateTime startDate = data.Min(x => x.Date);
            DateTime endDate = data.Max(x => x.Date);
            TimeSpan totalTime = endDate - startDate; 

            // Tỉ lệ co giãn 
            double xScale = chartWidth / totalTime.TotalDays;

            List<Point> points = new List<Point>();

            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];

                // Tính khoảng cách
                TimeSpan timeFromStart = item.Date - startDate;
                double x = timeFromStart.TotalDays * xScale;

                double y = chartHeight - ((item.Count - minCount) / range) * chartHeight;

                points.Add(new Point(x, y));

                // Nhãn
                TextBlock countLabel = new TextBlock
                {
                    Text = item.Count.ToString(),
                    FontSize = 10
                };
                Canvas.SetLeft(countLabel, x - countLabel.ActualWidth / 2);
                Canvas.SetTop(countLabel, y - 20);
                LineChartCanvas.Children.Add(countLabel);

                TextBlock dateLabel = new TextBlock
                {
                    Text = item.Date.ToString("dd/MM"),
                    FontSize = 10
                };
                Canvas.SetLeft(dateLabel, x - dateLabel.ActualWidth / 2);
                Canvas.SetTop(dateLabel, chartHeight + 5);
                LineChartCanvas.Children.Add(dateLabel);
            }

            // Vẽ đường
            Polyline polyline = new Polyline
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                Points = new PointCollection(points)
            };
            LineChartCanvas.Children.Add(polyline);

            // Vẽ tọa độ
            DrawAxis(LineChartCanvas, chartWidth, chartHeight);
        }

        private void DrawAxis(Canvas canvas, double width, double height)
        {
            // y
            Line yAxis = new Line
            {
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = height,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            canvas.Children.Add(yAxis);

            // x
            Line xAxis = new Line
            {
                X1 = 0,
                Y1 = height,
                X2 = width,
                Y2 = height,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            canvas.Children.Add(xAxis);
        }

        private void UpdatePieChartByType(List<ReaderTypeCount> data)
        {
            PieChartCanvas.Children.Clear();
            double total = data.Sum(x => x.Count);
            double startAngle = 0;
            double radius = Math.Min(PieChartCanvas.ActualWidth, PieChartCanvas.ActualHeight) / 2 - 10;
            Point center = new Point(PieChartCanvas.ActualWidth / 2, PieChartCanvas.ActualHeight / 2);

            foreach (var item in data)
            {
                double angle = (item.Count / total) * 360;
                Color color = Color.FromRgb((byte)_random.Next(256), (byte)_random.Next(256), (byte)_random.Next(256));

                double percentage = (item.Count / total) * 100;
                DrawPieSlice(startAngle, angle, radius, center, color, $"{item.Type}: {percentage:F1}%", item.Count);
                startAngle += angle;
            }
        }

        private void DrawPieSlice(double startAngle, double angle, double radius, Point center, Color color, string label, int count)
        {
            Point startPoint = GetPointOnCircle(startAngle, radius, center);
            Point endPoint = GetPointOnCircle(startAngle + angle, radius, center);

            PathFigure pieFigure = new PathFigure
            {
                StartPoint = center,
                Segments = new PathSegmentCollection
                {
                    new LineSegment(startPoint, true),
                    new ArcSegment
                    {
                        Point = endPoint,
                        Size = new Size(radius, radius),
                        IsLargeArc = angle > 180,
                        SweepDirection = SweepDirection.Clockwise
                    },
                    new LineSegment(center, true)
                }
            };

            Path path = new Path
            {
                Fill = new SolidColorBrush(color),
                Data = new PathGeometry(new[] { pieFigure })
            };

            PieChartCanvas.Children.Add(path);

            // Nhãn
            double labelAngle = startAngle + (angle / 2);
            Point labelPoint = GetPointOnCircle(labelAngle, radius * 0.7, center);
            TextBlock labelText = new TextBlock
            {
                Text = $"{label}: {count}",
                FontSize = 10,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(labelText, labelPoint.X - labelText.ActualWidth / 2);
            Canvas.SetTop(labelText, labelPoint.Y - labelText.ActualHeight / 2);
            PieChartCanvas.Children.Add(labelText);
        }

        private Point GetPointOnCircle(double angle, double radius, Point center)
        {
            double radians = (angle - 90) * Math.PI / 180;
            return new Point(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
        }


        private void UpdateColumnChartByType(List<ReaderTypeCount> data)
        {
            ColumnChartCanvas.Children.Clear();
            ColumnChartLabels.ItemsSource = null;

            if (!data.Any()) return;

            double maxCount = data.Max(x => x.Count);
            double columnWidth = ColumnChartCanvas.ActualWidth / data.Count;
            double heightScaleFactor = (ColumnChartCanvas.ActualHeight - 50) / maxCount; // Dành thêm không gian cho nhãn

            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                double height = item.Count * heightScaleFactor;

                Rectangle column = new Rectangle
                {
                    Width = columnWidth * 0.6,
                    Height = height,
                    Fill = new SolidColorBrush(Colors.SkyBlue),
                    Stroke = Brushes.DarkBlue,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(column, i * columnWidth + columnWidth * 0.2);
                Canvas.SetBottom(column, 50); 

                TextBlock countLabel = new TextBlock
                {
                    Text = item.Count.ToString(),
                    TextAlignment = TextAlignment.Center,
                    Width = columnWidth
                };
                Canvas.SetLeft(countLabel, i * columnWidth);
                Canvas.SetBottom(countLabel, height + 35); // Điều chỉnh vị trí nhãn số lượng

                // Thêm nhãn cho loại độc giả
                TextBlock typeLabel = new TextBlock
                {
                    Text = item.Type,
                    TextAlignment = TextAlignment.Center,
                    Width = columnWidth
                };
                Canvas.SetLeft(typeLabel, i * columnWidth);
                Canvas.SetBottom(typeLabel, 15); // Đặt nhãn loại độc giả dưới cùng

                ColumnChartCanvas.Children.Add(column);
                ColumnChartCanvas.Children.Add(countLabel);
                ColumnChartCanvas.Children.Add(typeLabel); // Thêm nhãn loại độc giả vào Canvas
            }
        }

        private void DrawLine(List<Point> points, Color color)
        {
            Polyline polyline = new Polyline
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 2,
                Points = new PointCollection(points)
            };

            ColumnChartCanvas.Children.Add(polyline);
        }
    }

    public class ReaderDateCount
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class ReaderTypeCount
    {
        public string Type { get; set; }
        public int Count { get; set; }
        public decimal TotalDebt { get; set; }
        public double AverageAge { get; set; }
    }
}
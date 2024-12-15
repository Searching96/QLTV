using LiveCharts;
using LiveCharts.Wpf;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace QLTV.Admin
{

    public class BCTraTreModel : INotifyPropertyChanged
    {
        private BCTRATRE bcTraTre;
        public BCTRATRE BCTraTre
        {
            get => bcTraTre;
            set
            {
                bcTraTre = value;
                OnPropertyChanged();
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DSBCTraTreModel : INotifyPropertyChanged
    {
        private ObservableCollection<BCTraTreModel> dsBCTraTre;
        public ObservableCollection<BCTraTreModel> DSBCTraTre
        {
            get => dsBCTraTre;
            set
            {
                dsBCTraTre = value;
                OnPropertyChanged();
            }
        }

        public int TongSoLuotTraTre => DSBCTraTre.Sum(bc => bc.BCTraTre.CTBCTRATRE.Count);

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime Month { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BCMuonSachModel : INotifyPropertyChanged
    {
        private BCMUONSACH _bcMuonSach;
        public BCMUONSACH BCMuonSach
        {
            get => _bcMuonSach;
            set
            {
                _bcMuonSach = value;
                OnPropertyChanged();
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DSBCMuonSachModel : INotifyPropertyChanged
    {
        private ObservableCollection<BCMuonSachModel> dsBCMuonSach;
        public ObservableCollection<BCMuonSachModel> DSBCMuonSach
        {
            get => dsBCMuonSach;
            set
            {
                dsBCMuonSach = value;
                OnPropertyChanged();
            }
        }

        public int TongSoLuotMuon => (DSBCMuonSach.Sum(bc => bc.BCMuonSach.TongSoLuotMuon) / 2);

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime Month { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BCTienPhatModel : INotifyPropertyChanged
    {
        private DateTime _ngay;
        public DateTime Ngay
        {
            get => _ngay;
            set
            {
                _ngay = value;
                OnPropertyChanged();
            }
        }

        private decimal _soTien;
        public decimal SoTien
        {
            get => _soTien;
            set
            {
                _soTien = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DSBCTienPhatModel : INotifyPropertyChanged
    {
        private ObservableCollection<BCTienPhatModel> _DSBCTienPhat;
        public ObservableCollection<BCTienPhatModel> DSBCTienPhat
        {
            get => _DSBCTienPhat;
            set
            {
                _DSBCTienPhat = value;
                OnPropertyChanged();
            }
        }

        private decimal _tongTienPhat;
        public decimal TongTienPhat
        {
            get => _tongTienPhat;
            set
            {
                _tongTienPhat = value;
                OnPropertyChanged();
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime Month { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ChartViewModel : INotifyPropertyChanged
    {
        private SeriesCollection _CurrentChartValues;
        public SeriesCollection CurrentChartValues
        {
            get => _CurrentChartValues;
            set
            {
                if (_CurrentChartValues != value)
                {
                    _CurrentChartValues = value;
                    OnPropertyChanged();
                }
            }
        }


        public List<string> _CurrentLabels;
        public List<string> CurrentLabels
        {
            get => _CurrentLabels;
            set
            {
                if (_CurrentLabels != value)
                {
                    _CurrentLabels = value;
                    OnPropertyChanged();
                }
            }
        }


        public DateTime StartTime;
        public DateTime EndTime;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TopItemsViewModel
    {
        public string Key { get; set; }
        public int Value { get; set; }
        public SolidColorBrush Color { get; set; }
    }

    public class TienPhatTraTreReport
    {
        public DateTime StartMonth { get; set; }
        public DateTime EndMonth { get; set; }
        public decimal TienPhatTraTreMotNgay { get; set; }
    }

    public class TiLeDenBuReport
    {
        public DateTime StartMonth { get; set; }
        public DateTime EndMonth { get; set; }
        public decimal TiLeDenBu { get; set; }
    }

    /// <summary>
    /// Interaction logic for UcBCMuonTra.xaml
    /// </summary>
    public partial class UcBCMuonTra : UserControl
    {
        private ObservableCollection<DSBCMuonSachModel> _borrowReports;
        private ObservableCollection<DSBCTraTreModel> _lateReturnReports;
        private ObservableCollection<DSBCTienPhatModel> _finesReports;
        private ObservableCollection<TienPhatTraTreReport> _lateReturnFeesReports;
        private ObservableCollection<TiLeDenBuReport> _compensationReports;
        private ChartViewModel BCMSLineChartModel;
        private ChartViewModel BCMSPieChartModel;
        private ChartViewModel BCTTLineChartModel;
        private ChartViewModel BCTTPieChartModel;
        private ChartViewModel BCTPLineChartModel;

        public UcBCMuonTra()
        {
            InitializeComponent();
            Loaded += UcBCMuonTra_Loaded;
        }

        private async void UcBCMuonTra_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            await PopulateBCMSChartAndDataGrid();
            PopulateBCMSMonthComboBox(DateTime.Now.Year.ToString());
            await PopulateBCTTChartAndDataGrid();
            PopulateBCTTMonthComboBox(DateTime.Now.Year.ToString());
            await PopulateBCTPChartAndDataGrid();
            PopulateBCTPMonthComboBox(DateTime.Now.Year.ToString());
        }

        private void btnViewDetail_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            var row = DataGridRow.GetRowContainingElement(button);
            if (row?.DataContext is DSBCMuonSachModel dsbcms)
            {
                dsbcms.IsExpanded = !dsbcms.IsExpanded;
            }
            if (row?.DataContext is BCMuonSachModel bcms)
            {
                bcms.IsExpanded = !bcms.IsExpanded;
            }
            if (row?.DataContext is DSBCTraTreModel dsbctt)
            {
                dsbctt.IsExpanded = !dsbctt.IsExpanded;
            }
            if (row?.DataContext is BCTraTreModel bctt)
            {
                bctt.IsExpanded = !bctt.IsExpanded;
            }
            if (row?.DataContext is DSBCTienPhatModel dsbctp)
            {
                dsbctp.IsExpanded = !dsbctp.IsExpanded;
            }
        }

        private async Task LoadBorrowReportsData(DateTime begin, DateTime end)
        {
            try
            {
                using (var _context = new QLTVContext())
                {
                    // Load borrowing reports data from the database
                    var borrowReports = await _context.BCMUONSACH
                        .Include(p => p.CTBCMUONSACH)
                            .ThenInclude(ct => ct.IDTheLoaiNavigation)
                        .Where(bc => bc.Thang >= begin && bc.Thang <= end)
                        .ToListAsync();

                    // Process borrow reports into ObservableCollection
                    _borrowReports = new ObservableCollection<DSBCMuonSachModel>(
                        borrowReports
                            .GroupBy(bc => new { bc.Thang.Year, bc.Thang.Month })
                            .Select(g =>
                            {
                                // Create detailed borrowing report models
                                var dsBCMuonSach = new ObservableCollection<BCMuonSachModel>(
                                    g.Select(b => new BCMuonSachModel
                                    {
                                        BCMuonSach = b,
                                        IsExpanded = false,
                                    }).ToList()
                                );

                                // Calculate totals for the group
                                if (!dsBCMuonSach.Any(bc => bc.BCMuonSach.MaBCMuonSach == "Tổng"))
                                {
                                    var totalBorrowCount = dsBCMuonSach.Sum(bc => bc.BCMuonSach.TongSoLuotMuon);
                                    var totalBCMuonSach = new BCMUONSACH
                                    {
                                        MaBCMuonSach = "Tổng",
                                        Thang = new DateTime(g.Key.Year, g.Key.Month, 1),
                                        TongSoLuotMuon = totalBorrowCount,
                                        CTBCMUONSACH = g
                                            .SelectMany(bc => bc.CTBCMUONSACH)
                                            .GroupBy(ct => ct.IDTheLoai)
                                            .Select(grp => new CTBCMUONSACH
                                            {
                                                IDBCMuonSach = 0,
                                                IDTheLoai = grp.Key,
                                                SoLuotMuon = grp.Sum(ct => ct.SoLuotMuon),
                                                TiLe = totalBorrowCount == 0 ? 0 : grp.Sum(ct => ct.SoLuotMuon) / (double)totalBorrowCount,
                                                IDTheLoaiNavigation = grp.First().IDTheLoaiNavigation
                                            })
                                            .ToList()
                                    };

                                    dsBCMuonSach.Insert(0, new BCMuonSachModel
                                    {
                                        BCMuonSach = totalBCMuonSach,
                                        IsExpanded = false
                                    });
                                }

                                return new DSBCMuonSachModel
                                {
                                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                                    DSBCMuonSach = dsBCMuonSach,
                                    IsExpanded = false
                                };
                            })
                            .ToList()
                    );

                    // Update the data source for the UI
                    dgBorrowingReports.ItemsSource = _borrowReports;
                }
            }
            catch (Exception ex)
            {
                // Show error message in case of failure
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadLateReturnsReportData(DateTime begin, DateTime end)
        {
            try
            {
                using (var _context = new QLTVContext())
                {
                    // Load late return reports data from the database
                    var lateReturnReports = await _context.BCTRATRE
                        .Include(p => p.CTBCTRATRE)
                            .ThenInclude(ct => ct.IDPhieuTraNavigation)
                        .Where(bc => bc.Ngay >= begin && bc.Ngay <= end)
                        .ToListAsync();

                    // Process late return reports into ObservableCollection
                    _lateReturnReports = new ObservableCollection<DSBCTraTreModel>(
                        lateReturnReports
                            .GroupBy(bc => new { bc.Ngay.Year, bc.Ngay.Month })
                            .Select(g =>
                            {
                                var dsBCTraTre = new ObservableCollection<BCTraTreModel>(
                                    g.Select(b => new BCTraTreModel
                                    {
                                        BCTraTre = b,
                                        IsExpanded = false,
                                    }).ToList()
                                );

                                return new DSBCTraTreModel
                                {
                                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                                    DSBCTraTre = dsBCTraTre,
                                    IsExpanded = false
                                };
                            })
                            .ToList()
                    );

                    // Update the data source for the UI
                    dgLateReturnReports.ItemsSource = _lateReturnReports;
                }
            }
            catch (Exception ex)
            {
                // Show error message in case of failure
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /*      Borrowing  Reports      */

        private async Task PopulateBCMSChartAndDataGrid(DateTime? begin = null, DateTime? end = null)
        {
            DateTime startDate = begin ?? DateTime.Now.AddDays(-30);
            DateTime endDate = end ?? DateTime.Now.AddDays(1);

            await LoadBorrowReportsData(startDate, endDate);
            await LoadLateReturnsReportData(startDate, endDate);
            PopulateBCMSLineChartViewModel(startDate, endDate);
            PopulateBCMSPieChartViewModel(startDate, endDate);
        }

        private void PopulateBCMSYearComboBox()
        {
            using (var context = new QLTVContext())
            {
                var NamBCMS = context.BCMUONSACH
                    .Select(r => r.Thang.Year.ToString())
                    .Distinct()
                    .OrderBy(y => y)
                    .ToList();

                // Nạp các tuỳ chọn cho combobox Năm của Báo cáo mượn sách
                NamBCMS.Insert(0, "Tất cả"); // Thêm tuỳ chọn "Tất cả"
                cbYearMS.ItemsSource = NamBCMS;
                cbYearMS.SelectedIndex = 0;
            }
        }

        private void PopulateBCMSMonthComboBox(string? selectedYear)
        {
            using (var context = new QLTVContext())
            {
                var ThangBCMS = context.BCMUONSACH
                    .Select(r => r.Thang)
                    .OrderBy(m => m.Year)
                    .ThenBy(m => m.Month)
                    .Select(m => m.ToString("MM/yyyy"))
                    .Distinct()
                    .ToList();

                cbStartMonthMS.ItemsSource = ThangBCMS.Distinct();
                cbEndMonthMS.ItemsSource = ThangBCMS.Distinct();
                cbStartMonthMS.SelectedIndex = 0;
                cbEndMonthMS.SelectedIndex = cbEndMonthMS.Items.Count - 1;
            }
        }

        private void PopulateBCMSQuarterComboBox(string? selectedYear)
        {
            using (var context = new QLTVContext())
            {
                var QuyBCMS = context.BCMUONSACH
                    .Where(r => selectedYear == "Tất cả" || r.Thang.Year == (selectedYear != null ? int.Parse(selectedYear) : 0))
                    .AsEnumerable()
                    .Select(r =>
                    {
                        var month = r.Thang.Month;
                        return month switch
                        {
                            >= 1 and <= 3 => "Quý 1",
                            >= 4 and <= 6 => "Quý 2",
                            >= 7 and <= 9 => "Quý 3",
                            _ => "Quý 4"
                        };
                    })
                    .Distinct()
                    .OrderBy(q => q)
                    .ToList();
                QuyBCMS.Insert(0, "Tất cả");
                cbQuarterMS.ItemsSource = QuyBCMS;
                cbQuarterMS.SelectedIndex = 0;
            }
        }

        private void lbBCMSChartMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            HideAllBCMSChartControls();

            var selectedMode = ((sender as ListBox).SelectedItem as ListBoxItem)?.Content.ToString();
            switch (selectedMode)
            {
                case "Ngày":
                    spDayMS.Visibility = Visibility.Visible;
                    icMSTimePickerError.Visibility = Visibility.Collapsed;
                    break;
                case "Tháng":
                    spMonthMS.Visibility = Visibility.Visible;
                    PopulateBCMSMonthComboBox(null);
                    icMSTimePickerError.Visibility = Visibility.Collapsed;
                    break;
                case "Quý":

                    cbQuarterMS.Visibility = Visibility.Visible;
                    cbYearMS.Visibility = Visibility.Visible;
                    if (cbYearMS.ItemsSource == null)
                        PopulateBCMSYearComboBox();
                    PopulateBCMSQuarterComboBox((cbYearMS as ComboBox)?.SelectedItem as string);
                    icMSTimePickerError.Visibility = Visibility.Collapsed;
                    break;
                case "Năm":
                    cbYearMS.Visibility = Visibility.Visible;
                    if (cbYearMS.ItemsSource == null)
                        PopulateBCMSYearComboBox();
                    icMSTimePickerError.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }

        private void HideAllBCMSChartControls()
        {
            spDayMS.Visibility = Visibility.Collapsed;
            spMonthMS.Visibility = Visibility.Collapsed;
            cbQuarterMS.Visibility = Visibility.Collapsed;
            cbYearMS.Visibility = Visibility.Collapsed;
            icMSTimePickerError.Visibility = Visibility.Collapsed;
            dpStartDayMS.SelectedDate = null;
            dpEndDayMS.SelectedDate = null;
            cbStartMonthMS.SelectedItem = null;
            cbEndMonthMS.SelectedItem = null;
            cbQuarterMS.SelectedItem = null;
        }

        private void PopulateBCMSLineChartViewModel(DateTime begin, DateTime end)
        {
            BCMSLineChartModel = new ChartViewModel
            {
                StartTime = begin,
                EndTime = end,
                CurrentChartValues = new SeriesCollection(),
                CurrentLabels = new List<string>()
            };

            var values = new ChartValues<int>();
            var labels = new List<string>();

            // Create a dictionary to store borrow counts for each date
            var borrowCounts = _borrowReports
                .SelectMany(report => report.DSBCMuonSach)
                .Where(bcMuonSach => bcMuonSach.BCMuonSach.Thang >= begin && bcMuonSach.BCMuonSach.Thang <= end && bcMuonSach.BCMuonSach.MaBCMuonSach != "Tổng")
                .GroupBy(bcMuonSach => bcMuonSach.BCMuonSach.Thang.Date)
                .ToDictionary(g => g.Key, g => g.Sum(bcMuonSach => bcMuonSach.BCMuonSach.TongSoLuotMuon));

            // Iterate through each day in the date range
            for (var date = begin.Date; date <= end.Date; date = date.AddDays(1))
            {
                if (borrowCounts.TryGetValue(date, out var borrowCount))
                {
                    values.Add(borrowCount);
                }
                else
                {
                    values.Add(0);
                }
                labels.Add(date.ToString("dd/MM/yyyy"));
            }

            BCMSLineChartModel.CurrentChartValues.Add(new LineSeries
            {
                Title = "Số lượt mượn",
                Values = values,
                LineSmoothness = 0.2
            });

            BCMSLineChartModel.CurrentLabels = labels;
            BCMSLineChart.DataContext = BCMSLineChartModel;
        }

        private void PopulateBCMSPieChartViewModel(DateTime begin, DateTime end)
        {
            BCMSPieChartModel = new ChartViewModel
            {
                StartTime = begin,
                EndTime = end,
                CurrentChartValues = new SeriesCollection(),
                CurrentLabels = new List<string>()
            };

            var genreBorrowCounts = new Dictionary<string, int>();

            foreach (var report in _borrowReports)
            {
                foreach (var bcMuonSach in report.DSBCMuonSach)
                {
                    if (bcMuonSach.BCMuonSach.Thang >= begin &&
                        bcMuonSach.BCMuonSach.Thang <= end &&
                        bcMuonSach.BCMuonSach.MaBCMuonSach != "Tổng")
                    {
                        foreach (var ctbcMuonSach in bcMuonSach.BCMuonSach.CTBCMUONSACH)
                        {
                            var genre = ctbcMuonSach.IDTheLoaiNavigation.TenTheLoai;
                            if (genreBorrowCounts.ContainsKey(genre))
                            {
                                genreBorrowCounts[genre] += ctbcMuonSach.SoLuotMuon;
                            }
                            else
                            {
                                genreBorrowCounts[genre] = ctbcMuonSach.SoLuotMuon;
                            }
                        }
                    }
                }
            }

            var top10Genres = genreBorrowCounts
                .OrderByDescending(g => g.Value)
                .Take(10)
                .Select(g => new TopItemsViewModel
                {
                    Key = g.Key,
                    Value = g.Value,
                    Color = new SolidColorBrush(GetRandomColor())
                })
                .ToList();

            foreach (var genre in top10Genres)
            {
                var pieSeries = new PieSeries
                {
                    Values = new ChartValues<int> { genre.Value },
                    Title = genre.Key,
                    DataLabels = false,
                    Fill = genre.Color,
                    LabelPoint = chartPoint => $"{chartPoint.Y} lượt mượn",
                    ToolTip = new DefaultTooltip
                    {
                        SelectionMode = TooltipSelectionMode.OnlySender
                    }
                };

                BCMSPieChartModel.CurrentChartValues.Add(pieSeries);
            }

            BCMSPieChart.DataContext = BCMSPieChartModel;
            GenresDisplay.ItemsSource = top10Genres;
        }

        private System.Windows.Media.Color GetRandomColor()
        {
            Random rand = new Random();
            return System.Windows.Media.Color.FromRgb((byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256));
        }

        private void cbYearMS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedYear = (sender as ComboBox)?.SelectedItem as string;
            if (selectedYear != null)
            {
                DateTime begin, end;
                if (selectedYear == "Tất cả")
                {
                    begin = _borrowReports.Count != 0 ? _borrowReports.Min(bc => bc.Month) : DateTime.Now.AddDays(-30);
                    end = DateTime.Now.AddDays(1);
                }
                else
                {
                    begin = new DateTime(int.Parse(selectedYear), 1, 1);
                    end = new DateTime(int.Parse(selectedYear), 12, 31);
                }
                PopulateBCMSChartAndDataGrid(begin, end);
            }
        }

        private void cbQuarterMS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedQuarter = (sender as ComboBox)?.SelectedItem as string;
            var selectedYear = cbYearMS.SelectedItem as string;
            if (selectedYear == "Tất cả") return;
            if (selectedQuarter != null && int.TryParse(selectedYear, out int year))
            {
                DateTime begin, end;
                switch (selectedQuarter)
                {
                    case "Quý 1":
                        begin = new DateTime(year, 1, 1);
                        end = new DateTime(year, 3, 31);
                        break;
                    case "Quý 2":
                        begin = new DateTime(year, 4, 1);
                        end = new DateTime(year, 6, 30);
                        break;
                    case "Quý 3":
                        begin = new DateTime(year, 7, 1);
                        end = new DateTime(year, 9, 30);
                        break;
                    case "Quý 4":
                        begin = new DateTime(year, 10, 1);
                        end = new DateTime(year, 12, 31);
                        break;
                    default:
                        return;
                }
                PopulateBCMSChartAndDataGrid(begin, end);
            }
        }

        private void cbStartMonthMS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedStartMonth = cbStartMonthMS.SelectedItem as string;
            var selectedEndMonth = cbEndMonthMS.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedStartMonth) && string.IsNullOrEmpty(selectedEndMonth))
            {
                icMSTimePickerError.ToolTip = $"Vui lòng chọn tháng bắt đầu và tháng kết thúc.";
                icMSTimePickerError.Visibility = Visibility.Visible;
                cbStartMonthMS.BorderBrush = Brushes.Red;
                cbEndMonthMS.BorderBrush = Brushes.Red;
                return;
            }
            else if (string.IsNullOrEmpty(selectedStartMonth))
            {
                icMSTimePickerError.ToolTip = $"Vui lòng chọn tháng bắt đầu.";
                icMSTimePickerError.Visibility = Visibility.Visible;
                cbStartMonthMS.BorderBrush = Brushes.Red;
                cbEndMonthMS.BorderBrush = Brushes.LightGray;
                return;
            }
            else if (string.IsNullOrEmpty(selectedEndMonth))
            {
                icMSTimePickerError.ToolTip = $"Vui lòng chọn tháng kết thúc.";
                icMSTimePickerError.Visibility = Visibility.Visible;
                cbEndMonthMS.BorderBrush = Brushes.Red;
                cbStartMonthMS.BorderBrush = Brushes.LightGray;
                return;
            }

            if (DateTime.TryParseExact(selectedStartMonth, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startMonth) &&
                DateTime.TryParseExact(selectedEndMonth, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endMonth))
            {
                if (startMonth > endMonth)
                {
                    icMSTimePickerError.ToolTip = $"Tháng bắt đầu không thể sau tháng kết thúc.";
                    icMSTimePickerError.Visibility = Visibility.Visible;
                    cbStartMonthMS.BorderBrush = Brushes.Red;
                    cbEndMonthMS.BorderBrush = Brushes.Red;
                    return;
                }
                else
                {
                    cbStartMonthMS.BorderBrush = Brushes.LightGray;
                    cbEndMonthMS.BorderBrush = Brushes.LightGray;
                    icMSTimePickerError.Visibility = Visibility.Collapsed;
                    var begin = new DateTime(startMonth.Year, startMonth.Month, 1);
                    var end = new DateTime(endMonth.Year, endMonth.Month, DateTime.DaysInMonth(endMonth.Year, endMonth.Month));
                    PopulateBCMSChartAndDataGrid(begin, end);
                }
            }
            else
            {
                icMSTimePickerError.ToolTip = $"Định dạng tháng không hợp lệ. Vui lòng chọn lại.";
                icMSTimePickerError.Visibility = Visibility.Visible;
                cbStartMonthMS.BorderBrush = Brushes.Red;
                cbEndMonthMS.BorderBrush = Brushes.Red;
            }
        }

        private void cbEndMonthMS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            cbStartMonthMS_SelectionChanged(sender, e);
        }

        private void dpEndDayMS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedStartDay = dpStartDayMS.SelectedDate;
            var selectedEndDay = dpEndDayMS.SelectedDate;

            if (selectedStartDay == null && selectedEndDay == null)
            {
                icMSTimePickerError.ToolTip = $"Vui lòng chọn ngày bắt đầu và ngày kết thúc.";
                icMSTimePickerError.Visibility = Visibility.Visible;
                dpStartDayMS.BorderBrush = Brushes.Red;
                dpEndDayMS.BorderBrush = Brushes.Red;
                return;
            }
            else if (selectedStartDay == null)
            {
                icMSTimePickerError.ToolTip = $"Vui lòng chọn ngày bắt đầu.";
                icMSTimePickerError.Visibility = Visibility.Visible;
                dpStartDayMS.BorderBrush = Brushes.Red;
                dpEndDayMS.BorderBrush = Brushes.LightGray;
                return;
            }
            else if (selectedEndDay == null)
            {
                icMSTimePickerError.ToolTip = $"Vui lòng chọn ngày kết thúc.";
                icMSTimePickerError.Visibility = Visibility.Visible;
                dpEndDayMS.BorderBrush = Brushes.Red;
                dpStartDayMS.BorderBrush = Brushes.LightGray;
                return;
            }
            else if (selectedStartDay.Value > selectedEndDay.Value)
            {
                icMSTimePickerError.ToolTip = $"Ngày bắt đầu không thể sau ngày kết thúc.";
                icMSTimePickerError.Visibility = Visibility.Visible;
                dpStartDayMS.BorderBrush = Brushes.Red;
                dpEndDayMS.BorderBrush = Brushes.Red;
                return;
            }
            else
            {
                dpStartDayMS.BorderBrush = Brushes.LightGray;
                dpEndDayMS.BorderBrush = Brushes.LightGray;
                icMSTimePickerError.Visibility = Visibility.Collapsed;
                PopulateBCMSChartAndDataGrid(selectedStartDay.Value, selectedEndDay.Value);
            }
        }

        /*    Late Return Reports    */

        private async Task PopulateBCTTChartAndDataGrid(DateTime? begin = null, DateTime? end = null)
        {
            DateTime startDate = begin ?? DateTime.Now.AddDays(-30);
            DateTime endDate = end ?? DateTime.Now.AddDays(1);

            await LoadBorrowReportsData(startDate, endDate);
            await LoadLateReturnsReportData(startDate, endDate);
            PopulateBCTTLineChartViewModel(startDate, endDate);
            PopulateBCTTPieChartViewModel(startDate, endDate);
        }

        private void PopulateBCTTYearComboBox()
        {
            using (var context = new QLTVContext())
            {
                var NamBCTT = context.BCTRATRE
                    .Select(r => r.Ngay.Year.ToString())
                    .Distinct()
                    .OrderBy(y => y)
                    .ToList();

                // Nạp các tuỳ chọn năm cho combobox của Báo cáo trả trễ
                NamBCTT.Insert(0, "Tất cả"); // Thêm tuỳ chọn "Tất cả" option
                cbYearTT.ItemsSource = NamBCTT;
                cbYearTT.SelectedIndex = 0;
            }
        }

        private void PopulateBCTTMonthComboBox(string? selectedYear)
        {
            using (var context = new QLTVContext())
            {
                var ThangBCTT = context.BCTRATRE
                    .Select(r => r.Ngay)
                    .OrderBy(m => m.Year)
                    .ThenBy(m => m.Month)
                    .Select(m => m.ToString("MM/yyyy"))
                    .Distinct()
                    .ToList();

                cbStartMonthTT.ItemsSource = ThangBCTT.Distinct();
                cbEndMonthTT.ItemsSource = ThangBCTT.Distinct();
                cbStartMonthTT.SelectedIndex = 0;
                cbEndMonthTT.SelectedIndex = cbEndMonthTT.Items.Count - 1;
            }
        }

        private void PopulateBCTTQuarterComboBox(string? selectedYear)
        {
            using (var context = new QLTVContext())
            {
                var QuyBCTT = context.BCTRATRE
                    .Where(r => selectedYear == "Tất cả" || r.Ngay.Year == int.Parse(selectedYear))
                    .AsEnumerable()
                    .Select(r =>
                    {
                        var month = r.Ngay.Month;
                        return month switch
                        {
                            >= 1 and <= 3 => "Quý 1",
                            >= 4 and <= 6 => "Quý 2",
                            >= 7 and <= 9 => "Quý 3",
                            _ => "Quý 4"
                        };
                    })
                    .Distinct()
                    .OrderBy(q => q)
                    .ToList();
                QuyBCTT.Insert(0, "Tất cả");
                cbQuarterTT.ItemsSource = QuyBCTT;
                cbQuarterTT.SelectedIndex = 0;
            }
        }

        private void lbBCTTChartMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            HideAllBCTTChartControls();

            var selectedMode = ((sender as ListBox).SelectedItem as ListBoxItem)?.Content.ToString();
            switch (selectedMode)
            {
                case "Ngày":
                    spDayTT.Visibility = Visibility.Visible;
                    break;
                case "Tháng":
                    spMonthTT.Visibility = Visibility.Visible;
                    PopulateBCTTMonthComboBox(null);
                    break;
                case "Quý":
                    cbQuarterTT.Visibility = Visibility.Visible;
                    cbYearTT.Visibility = Visibility.Visible;
                    if (cbYearTT.ItemsSource == null)
                        PopulateBCTTYearComboBox();
                    PopulateBCTTQuarterComboBox((cbYearTT as ComboBox)?.SelectedItem as string);
                    icTTTimePickerError.Visibility = Visibility.Collapsed;
                    break;
                case "Năm":
                    cbYearTT.Visibility = Visibility.Visible;
                    if (cbYearTT.ItemsSource == null)
                        PopulateBCTTYearComboBox();
                    icTTTimePickerError.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }

        private void HideAllBCTTChartControls()
        {
            spDayTT.Visibility = Visibility.Collapsed;
            spMonthTT.Visibility = Visibility.Collapsed;
            cbQuarterTT.Visibility = Visibility.Collapsed;
            cbYearTT.Visibility = Visibility.Collapsed;
            icTTTimePickerError.Visibility = Visibility.Collapsed;
            dpStartDayTT.SelectedDate = null;
            dpEndDayTT.SelectedDate = null;
            cbStartMonthTT.SelectedItem = null;
            cbEndMonthTT.SelectedItem = null;
            cbQuarterTT.SelectedItem = null;
        }

        private void PopulateBCTTLineChartViewModel(DateTime begin, DateTime end)
        {
            BCTTLineChartModel = new ChartViewModel
            {
                StartTime = begin,
                EndTime = end,
                CurrentChartValues = new SeriesCollection(),
                CurrentLabels = new List<string>()
            };

            var values = new ChartValues<int>();
            var labels = new List<string>();

            var dailyCounts = _lateReturnReports
                .SelectMany(report => report.DSBCTraTre)
                .Where(bcTraTre => bcTraTre.BCTraTre.Ngay >= begin && bcTraTre.BCTraTre.Ngay <= end)
                .GroupBy(bcTraTre => bcTraTre.BCTraTre.Ngay.Date)
                .Select(g => new { Date = g.Key, Count = g.Sum(bcTraTre => bcTraTre.BCTraTre.CTBCTRATRE.Count) })
                .OrderBy(x => x.Date);

            foreach (var dailyCount in dailyCounts)
            {
                values.Add(dailyCount.Count);
                labels.Add(dailyCount.Date.ToString("dd/MM/yyyy"));
            }

            BCTTLineChartModel.CurrentChartValues.Add(new LineSeries
            {
                Title = "Số lượt trả trễ",
                Values = values,
                LineSmoothness = 0.2
            });

            BCTTLineChartModel.CurrentLabels = labels;
            BCTTLineChart.DataContext = BCTTLineChartModel;
        }

        private void PopulateBCTTPieChartViewModel(DateTime begin, DateTime end)
        {
            BCTTPieChartModel = new ChartViewModel
            {
                StartTime = begin,
                EndTime = end,
                CurrentChartValues = new SeriesCollection(),
                CurrentLabels = new List<string>()
            };

            var overdueTimes = new Dictionary<string, int>();

            foreach (var report in _lateReturnReports)
            {
                foreach (var bcTraTre in report.DSBCTraTre)
                {
                    if (bcTraTre.BCTraTre.Ngay >= begin &&
                        bcTraTre.BCTraTre.Ngay <= end)
                    {
                        foreach (var ctbcTraTre in bcTraTre.BCTraTre.CTBCTRATRE)
                        {
                            var phieuTra = ctbcTraTre.IDPhieuTraNavigation.MaPhieuTra;
                            if (overdueTimes.ContainsKey(phieuTra))
                            {
                                overdueTimes[phieuTra] += ctbcTraTre.SoNgayTraTre;
                            }
                            else
                            {
                                overdueTimes[phieuTra] = ctbcTraTre.SoNgayTraTre;
                            }
                        }
                    }
                }
            }

            var top10OverdueTimes = overdueTimes
                .OrderByDescending(g => g.Value)
                .Take(10)
                .Select(g => new TopItemsViewModel
                {
                    Key = g.Key,
                    Value = g.Value,
                    Color = new SolidColorBrush(GetRandomColor())
                })
                .ToList();

            foreach (var overdue in top10OverdueTimes)
            {
                var pieSeries = new PieSeries
                {
                    Values = new ChartValues<int> { overdue.Value },
                    Title = overdue.Key,
                    DataLabels = false,
                    Fill = overdue.Color,
                    LabelPoint = chartPoint => $"{chartPoint.SeriesView.Title}: {chartPoint.Y} ngày trễ",
                    ToolTip = new DefaultTooltip
                    {
                        SelectionMode = TooltipSelectionMode.OnlySender
                    }
                };

                BCTTPieChartModel.CurrentChartValues.Add(pieSeries);
            }

            BCTTPieChart.DataContext = BCTTPieChartModel;
            LateDisplay.ItemsSource = top10OverdueTimes;
        }

        private void cbYearTT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedYear = (sender as ComboBox)?.SelectedItem as string;
            if (selectedYear != null)
            {
                DateTime begin, end;
                if (selectedYear == "Tất cả")
                {
                    cbQuarterTT.SelectedIndex = 0;
                    begin = _lateReturnReports.Count != 0 ? _lateReturnReports.Min(r => r.Month) : DateTime.Now.AddDays(-30);
                    end = DateTime.Now.AddDays(1);
                }
                else
                {
                    begin = new DateTime(int.Parse(selectedYear), 1, 1);
                    end = new DateTime(int.Parse(selectedYear), 12, 31);
                }
                PopulateBCTTChartAndDataGrid(begin, end);
            }
        }

        private void cbQuarterTT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedQuarter = (sender as ComboBox)?.SelectedItem as string;
            var selectedYear = cbYearTT.SelectedItem as string;
            if (selectedQuarter != null && int.TryParse(selectedYear, out int year))
            {
                DateTime begin, end;
                switch (selectedQuarter)
                {
                    case "Quý 1":
                        begin = new DateTime(year, 1, 1);
                        end = new DateTime(year, 3, 31);
                        break;
                    case "Quý 2":
                        begin = new DateTime(year, 4, 1);
                        end = new DateTime(year, 6, 30);
                        break;
                    case "Quý 3":
                        begin = new DateTime(year, 7, 1);
                        end = new DateTime(year, 9, 30);
                        break;
                    case "Quý 4":
                        begin = new DateTime(year, 10, 1);
                        end = new DateTime(year, 12, 31);
                        break;
                    default:
                        return;
                }
                PopulateBCTTChartAndDataGrid(begin, end);
            }
        }

        private void cbStartMonthTT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedStartMonth = cbStartMonthTT.SelectedItem as string;
            var selectedEndMonth = cbEndMonthTT.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedStartMonth) && string.IsNullOrEmpty(selectedEndMonth))
            {
                icTTTimePickerError.ToolTip = $"Vui lòng chọn tháng bắt đầu và tháng kết thúc.";
                icTTTimePickerError.Visibility = Visibility.Visible;
                cbStartMonthTT.BorderBrush = Brushes.Red;
                cbEndMonthTT.BorderBrush = Brushes.Red;
                return;
            }
            else if (string.IsNullOrEmpty(selectedStartMonth))
            {
                icTTTimePickerError.ToolTip = $"Vui lòng chọn tháng bắt đầu.";
                icTTTimePickerError.Visibility = Visibility.Visible;
                cbStartMonthTT.BorderBrush = Brushes.Red;
                cbEndMonthTT.BorderBrush = Brushes.LightGray;
                return;
            }
            else if (string.IsNullOrEmpty(selectedEndMonth))
            {
                icTTTimePickerError.ToolTip = $"Vui lòng chọn tháng kết thúc.";
                icTTTimePickerError.Visibility = Visibility.Visible;
                cbEndMonthTT.BorderBrush = Brushes.Red;
                cbStartMonthTT.BorderBrush = Brushes.LightGray;
                return;
            }

            if (DateTime.TryParseExact(selectedStartMonth, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startMonth) &&
                DateTime.TryParseExact(selectedEndMonth, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endMonth))
            {
                if (startMonth > endMonth)
                {
                    icTTTimePickerError.ToolTip = $"Tháng bắt đầu không thể sau tháng kết thúc.";
                    icTTTimePickerError.Visibility = Visibility.Visible;
                    cbStartMonthTT.BorderBrush = Brushes.Red;
                    cbEndMonthTT.BorderBrush = Brushes.Red;
                    return;
                }
                else
                {
                    cbStartMonthTT.BorderBrush = Brushes.LightGray;
                    cbEndMonthTT.BorderBrush = Brushes.LightGray;
                    icTTTimePickerError.Visibility = Visibility.Collapsed;
                    var begin = new DateTime(startMonth.Year, startMonth.Month, 1);
                    var end = new DateTime(endMonth.Year, endMonth.Month, DateTime.DaysInMonth(endMonth.Year, endMonth.Month));
                    PopulateBCTTChartAndDataGrid(begin, end);
                }
            }
            else
            {
                icTTTimePickerError.ToolTip = $"Định dạng tháng không hợp lệ. Vui lòng chọn lại.";
                icTTTimePickerError.Visibility = Visibility.Visible;
                cbStartMonthTT.BorderBrush = Brushes.Red;
                cbEndMonthTT.BorderBrush = Brushes.Red;
            }
        }

        private void cbEndMonthTT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            cbStartMonthTT_SelectionChanged(sender, e);
        }

        private void dpEndDayTT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedStartDay = dpStartDayTT.SelectedDate;
            var selectedEndDay = dpEndDayTT.SelectedDate;
            if (selectedStartDay == null && selectedEndDay == null)
            {
                icTTTimePickerError.ToolTip = $"Vui lòng chọn ngày bắt đầu và ngày kết thúc.";
                icTTTimePickerError.Visibility = Visibility.Visible;
                dpStartDayTT.BorderBrush = Brushes.Red;
                dpEndDayMS.BorderBrush = Brushes.Red;
                return;
            }
            else if (selectedStartDay != null)
            {
                icTTTimePickerError.ToolTip = $"Vui lòng chọn ngày kết thúc.";
                icTTTimePickerError.Visibility = Visibility.Visible;
                dpEndDayTT.BorderBrush = Brushes.Red;
                dpStartDayTT.BorderBrush = Brushes.LightGray;
                return;
            }
            else if (selectedEndDay != null)
            {
                icTTTimePickerError.ToolTip = $"Vui lòng chọn ngày bắt đầu.";
                icTTTimePickerError.Visibility = Visibility.Visible;
                dpStartDayTT.BorderBrush = Brushes.Red;
                dpEndDayTT.BorderBrush = Brushes.LightGray;
                return;
            }
            else if (selectedStartDay.Value > selectedEndDay.Value)
            {
                icTTTimePickerError.ToolTip = $"Ngày bắt đầu không thể sau ngày kết thúc.";
                icTTTimePickerError.Visibility = Visibility.Visible;
                icTTTimePickerError.Visibility = Visibility.Collapsed;
                dpStartDayTT.BorderBrush = Brushes.Red;
                dpEndDayMS.BorderBrush = Brushes.Red;
                return;
            }
            else
            {
                dpStartDayTT.BorderBrush = Brushes.LightGray;
                dpEndDayTT.BorderBrush = Brushes.LightGray;
                PopulateBCTTChartAndDataGrid(selectedStartDay.Value, selectedEndDay.Value);
            }
        }

        /*      Fines Reports       */

        private async Task PopulateBCTPChartAndDataGrid(DateTime? begin = null, DateTime? end = null)
        {
            DateTime startDate = begin ?? DateTime.Now.AddDays(-30);
            DateTime endDate = end ?? DateTime.Now.AddDays(1);

            await LoadBCTPData(startDate, endDate);
            PopulateBCTPLineChartViewModel(startDate, endDate);
        }

        private async Task LoadBCTPData(DateTime begin, DateTime end)
        {
            try
            {
                using (var _context = new QLTVContext())
                {
                    // Load BCTP data from the database
                    var BCTP = await _context.PHIEUTRA
                        .Include(p => p.CTPHIEUTRA)
                        .Where(f => f.NgayTra >= begin && f.NgayTra <= end)
                        .ToListAsync();

                    // Process BCTP data into ObservableCollection
                    _finesReports = new ObservableCollection<DSBCTienPhatModel>(
                        BCTP
                            .GroupBy(f => new { f.NgayTra.Year, f.NgayTra.Month })
                            .Select(g =>
                            {
                                var dailyFines = g
                                    .GroupBy(f => f.NgayTra.Date)
                                    .Select(dg => new BCTienPhatModel
                                    {
                                        Ngay = dg.Key,
                                        SoTien = dg.Sum(f => f.CTPHIEUTRA.Sum(ct => ct.TienPhat))
                                    })
                                    .ToList();

                                var BCTienPhat = new ObservableCollection<BCTienPhatModel>(dailyFines);

                                return new DSBCTienPhatModel
                                {
                                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                                    DSBCTienPhat = BCTienPhat,
                                    TongTienPhat = BCTienPhat.Sum(fd => fd.SoTien),
                                    IsExpanded = false
                                };
                            })
                            .ToList()
                    );

                    var thamsoList = _context.THAMSO.OrderBy(ts => ts.ThoiGian).ToList();

                    // Merge consecutive months with the same TienPhatTraTreMotNgay values
                    var tienPhatMerged = new List<TienPhatTraTreReport>();
                    DateTime start = thamsoList.First().ThoiGian;
                    decimal currentTienPhat = thamsoList.First().TienPhatTraTreMotNgay;

                    foreach (var current in thamsoList.Skip(1))
                    {
                        if (current.TienPhatTraTreMotNgay != currentTienPhat)
                        {
                            tienPhatMerged.Add(new TienPhatTraTreReport
                            {
                                StartMonth = start,
                                EndMonth = current.ThoiGian.AddDays(-1),
                                TienPhatTraTreMotNgay = currentTienPhat
                            });

                            start = current.ThoiGian;
                            currentTienPhat = current.TienPhatTraTreMotNgay;
                        }
                    }

                    // Add the last period for TienPhatTraTreMotNgay
                    tienPhatMerged.Add(new TienPhatTraTreReport
                    {
                        StartMonth = start,
                        EndMonth = DateTime.Now,
                        TienPhatTraTreMotNgay = currentTienPhat
                    });

                    // Merge consecutive months with the same TiLeDenBu values
                    var tiLeDenBuMerged = new List<TiLeDenBuReport>();
                    start = thamsoList.First().ThoiGian;
                    decimal currentTiLeDenBu = thamsoList.First().TiLeDenBu;

                    foreach (var current in thamsoList.Skip(1))
                    {
                        if (current.TiLeDenBu != currentTiLeDenBu)
                        {
                            tiLeDenBuMerged.Add(new TiLeDenBuReport
                            {
                                StartMonth = start,
                                EndMonth = current.ThoiGian.AddDays(-1),
                                TiLeDenBu = currentTiLeDenBu
                            });

                            start = current.ThoiGian;
                            currentTiLeDenBu = current.TiLeDenBu;
                        }
                    }

                    // Add the last period for TiLeDenBu
                    tiLeDenBuMerged.Add(new TiLeDenBuReport
                    {
                        StartMonth = start,
                        EndMonth = DateTime.Now,
                        TiLeDenBu = currentTiLeDenBu
                    });

                    // Map merged periods to ObservableCollection
                    _lateReturnFeesReports = new ObservableCollection<TienPhatTraTreReport>(tienPhatMerged);
                    _compensationReports = new ObservableCollection<TiLeDenBuReport>(tiLeDenBuMerged);

                    // Update the data source for the UI
                    dgTienPhatTraTreReports.ItemsSource = _lateReturnFeesReports;
                    dgTiLeDenBuReports.ItemsSource = _compensationReports;
                    dgBCTPReports.ItemsSource = _finesReports;
                }
            }
            catch (Exception ex)
            {
                // Show error message in case of failure
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateBCTPYearComboBox()
        {
            using (var context = new QLTVContext())
            {
                var NamBCTP = context.PHIEUTRA
                    .Select(r => r.NgayTra.Year.ToString())
                    .Distinct()
                    .OrderBy(y => y)
                    .ToList();

                // Nạp các tuỳ chọn năm cho combobox của Thống kê tiền phạt
                NamBCTP.Insert(0, "Tất cả"); // Thêm tuỳ chọn "Tất cả" option
                cbYearBCTP.ItemsSource = NamBCTP;
                cbYearBCTP.SelectedIndex = 0;
            }
        }

        private void PopulateBCTPMonthComboBox(string? selectedYear)
        {
            using (var context = new QLTVContext())
            {
                var ThangBCTP = context.PHIEUTRA
                    .Select(r => r.NgayTra)
                    .Distinct()
                    .OrderBy(m => m.Year)
                    .ThenBy(m => m.Month)
                    .AsEnumerable() // Switch to in-memory processing
                    .Select(m => m.ToString("MM/yyyy")) // Convert to string format
                    .ToList();

                cbStartMonthBCTP.ItemsSource = ThangBCTP.Distinct();
                cbEndMonthBCTP.ItemsSource = ThangBCTP.Distinct();
                cbStartMonthBCTP.SelectedIndex = 0;
                cbEndMonthBCTP.SelectedIndex = cbEndMonthBCTP.Items.Count - 1;
            }
        }

        private void PopulateBCTPQuarterComboBox(string? selectedYear)
        {
            using (var context = new QLTVContext())
            {
                var QuyBCTP = context.PHIEUTRA
                    .Where(r => selectedYear == "Tất cả" || r.NgayTra.Year == int.Parse(selectedYear))
                    .AsEnumerable()
                    .Select(r =>
                    {
                        var month = r.NgayTra.Month;
                        return month switch
                        {
                            >= 1 and <= 3 => "Quý 1",
                            >= 4 and <= 6 => "Quý 2",
                            >= 7 and <= 9 => "Quý 3",
                            _ => "Quý 4"
                        };
                    })
                    .Distinct()
                    .OrderBy(q => q)
                    .ToList();
                QuyBCTP.Insert(0, "Tất cả");
                cbQuarterBCTP.ItemsSource = QuyBCTP;
                cbQuarterBCTP.SelectedIndex = 0;
            }
        }

        private void lbBCTPChartMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            HideAllBCTPChartControls();

            var selectedMode = ((sender as ListBox).SelectedItem as ListBoxItem)?.Content.ToString();
            switch (selectedMode)
            {
                case "Ngày":
                    spDayBCTP.Visibility = Visibility.Visible;
                    break;
                case "Tháng":
                    spMonthBCTP.Visibility = Visibility.Visible;
                    PopulateBCTPMonthComboBox(null);
                    break;
                case "Quý":
                    cbQuarterBCTP.Visibility = Visibility.Visible;
                    cbYearBCTP.Visibility = Visibility.Visible;
                    if (cbYearBCTP.ItemsSource == null)
                        PopulateBCTPYearComboBox();
                    PopulateBCTPQuarterComboBox((cbYearBCTP as ComboBox)?.SelectedItem as string);
                    icBCTPTimePickerError.Visibility = Visibility.Collapsed;
                    break;
                case "Năm":
                    cbYearBCTP.Visibility = Visibility.Visible;
                    if (cbYearBCTP.ItemsSource == null)
                        PopulateBCTPYearComboBox();
                    icBCTPTimePickerError.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }

        private void HideAllBCTPChartControls()
        {
            spDayBCTP.Visibility = Visibility.Collapsed;
            spMonthBCTP.Visibility = Visibility.Collapsed;
            cbQuarterBCTP.Visibility = Visibility.Collapsed;
            cbYearBCTP.Visibility = Visibility.Collapsed;
            icBCTPTimePickerError.Visibility = Visibility.Collapsed;
            dpStartDayBCTP.SelectedDate = null;
            dpEndDayBCTP.SelectedDate = null;
            cbStartMonthBCTP.SelectedItem = null;
            cbEndMonthBCTP.SelectedItem = null;
            cbQuarterBCTP.SelectedItem = null;
        }

        private void PopulateBCTPLineChartViewModel(DateTime begin, DateTime end)
        {
            BCTPLineChartModel = new ChartViewModel
            {
                StartTime = begin,
                EndTime = end,
                CurrentChartValues = new SeriesCollection(),
                CurrentLabels = new List<string>()
            };

            var values = new ChartValues<decimal>();
            var labels = new List<string>();

            // Create a dictionary to store fine amounts for each date
            var fineAmounts = _finesReports
                .SelectMany(report => report.DSBCTienPhat)
                .Where(fine => fine.Ngay >= begin && fine.Ngay <= end)
                .GroupBy(fine => fine.Ngay.Date)
                .ToDictionary(g => g.Key, g => g.Sum(fine => fine.SoTien));

            // Iterate through each day in the date range
            for (var date = begin.Date; date <= end.Date; date = date.AddDays(1))
            {
                if (fineAmounts.TryGetValue(date, out var fineAmount))
                {
                    values.Add(fineAmount);
                }
                else
                {
                    values.Add(0);
                }
                labels.Add(date.ToString("dd/MM/yyyy"));
            }

            BCTPLineChartModel.CurrentChartValues.Add(new LineSeries
            {
                Title = "Số tiền phạt",
                Values = values,
                LineSmoothness = 0.2
            });

            BCTPLineChartModel.CurrentLabels = labels;
            BCTPLineChart.DataContext = BCTPLineChartModel;
        }

        private void cbYearBCTP_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedYear = (sender as ComboBox)?.SelectedItem as string;
            if (selectedYear != null)
            {
                DateTime begin, end;
                if (selectedYear == "Tất cả")
                {
                    begin = _borrowReports.Count != 0 ? _borrowReports.Min(bc => bc.Month) : DateTime.Now.AddDays(-30);
                    end = DateTime.Now.AddDays(1);
                }
                else
                {
                    begin = new DateTime(int.Parse(selectedYear), 1, 1);
                    end = new DateTime(int.Parse(selectedYear), 12, 31);
                }
                PopulateBCTPChartAndDataGrid(begin, end);
            }
        }

        private void cbQuarterBCTP_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedQuarter = (sender as ComboBox)?.SelectedItem as string;
            var selectedYear = cbYearBCTP.SelectedItem as string;
            if (selectedYear == "Tất cả") return;
            if (selectedQuarter != null && int.TryParse(selectedYear, out int year))
            {
                DateTime begin, end;
                switch (selectedQuarter)
                {
                    case "Quý 1":
                        begin = new DateTime(year, 1, 1);
                        end = new DateTime(year, 3, 31);
                        break;
                    case "Quý 2":
                        begin = new DateTime(year, 4, 1);
                        end = new DateTime(year, 6, 30);
                        break;
                    case "Quý 3":
                        begin = new DateTime(year, 7, 1);
                        end = new DateTime(year, 9, 30);
                        break;
                    case "Quý 4":
                        begin = new DateTime(year, 10, 1);
                        end = new DateTime(year, 12, 31);
                        break;
                    default:
                        return;
                }
                PopulateBCTPChartAndDataGrid(begin, end);
            }
        }

        private void cbStartMonthBCTP_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedStartMonth = cbStartMonthBCTP.SelectedItem as string;
            var selectedEndMonth = cbEndMonthBCTP.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedStartMonth) && string.IsNullOrEmpty(selectedEndMonth))
            {
                icBCTPTimePickerError.ToolTip = $"Vui lòng chọn tháng bắt đầu và tháng kết thúc.";
                icBCTPTimePickerError.Visibility = Visibility.Visible;
                cbStartMonthBCTP.BorderBrush = Brushes.Red;
                cbEndMonthBCTP.BorderBrush = Brushes.Red;
                return;
            }
            else if (string.IsNullOrEmpty(selectedStartMonth))
            {
                icBCTPTimePickerError.ToolTip = $"Vui lòng chọn tháng bắt đầu.";
                icBCTPTimePickerError.Visibility = Visibility.Visible;
                cbStartMonthBCTP.BorderBrush = Brushes.Red;
                cbEndMonthBCTP.BorderBrush = Brushes.LightGray;
                return;
            }
            else if (string.IsNullOrEmpty(selectedEndMonth))
            {
                icBCTPTimePickerError.ToolTip = $"Vui lòng chọn tháng kết thúc.";
                icBCTPTimePickerError.Visibility = Visibility.Visible;
                cbEndMonthBCTP.BorderBrush = Brushes.Red;
                cbStartMonthBCTP.BorderBrush = Brushes.LightGray;
                return;
            }

            if (DateTime.TryParseExact(selectedStartMonth, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startMonth) &&
                DateTime.TryParseExact(selectedEndMonth, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endMonth))
            {
                if (startMonth > endMonth)
                {
                    icBCTPTimePickerError.ToolTip = $"Tháng bắt đầu không thể sau tháng kết thúc.";
                    icBCTPTimePickerError.Visibility = Visibility.Visible;
                    cbStartMonthBCTP.BorderBrush = Brushes.Red;
                    cbEndMonthBCTP.BorderBrush = Brushes.Red;
                    return;
                }
                else
                {
                    cbStartMonthBCTP.BorderBrush = Brushes.LightGray;
                    cbEndMonthBCTP.BorderBrush = Brushes.LightGray;
                    icBCTPTimePickerError.Visibility = Visibility.Collapsed;
                    var begin = new DateTime(startMonth.Year, startMonth.Month, 1);
                    var end = new DateTime(endMonth.Year, endMonth.Month, DateTime.DaysInMonth(endMonth.Year, endMonth.Month));
                    PopulateBCTPChartAndDataGrid(begin, end);
                }
            }
            else
            {
                icBCTPTimePickerError.ToolTip = $"Định dạng tháng không hợp lệ. Vui lòng chọn lại.";
                icBCTPTimePickerError.Visibility = Visibility.Visible;
                cbStartMonthBCTP.BorderBrush = Brushes.Red;
                cbEndMonthBCTP.BorderBrush = Brushes.Red;
            }
        }

        private void cbEndMonthBCTP_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            cbStartMonthBCTP_SelectionChanged(sender, e);
        }

        private void dpEndDayBCTP_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedStartDay = dpStartDayBCTP.SelectedDate;
            var selectedEndDay = dpEndDayBCTP.SelectedDate;

            if (selectedStartDay == null && selectedEndDay == null)
            {
                icBCTPTimePickerError.ToolTip = $"Vui lòng chọn ngày bắt đầu và ngày kết thúc.";
                icBCTPTimePickerError.Visibility = Visibility.Visible;
                dpStartDayBCTP.BorderBrush = Brushes.Red;
                dpEndDayBCTP.BorderBrush = Brushes.Red;
                return;
            }
            else if (selectedStartDay == null)
            {
                icBCTPTimePickerError.ToolTip = $"Vui lòng chọn ngày bắt đầu.";
                icBCTPTimePickerError.Visibility = Visibility.Visible;
                dpStartDayBCTP.BorderBrush = Brushes.Red;
                dpEndDayBCTP.BorderBrush = Brushes.LightGray;
                return;
            }
            else if (selectedEndDay == null)
            {
                icBCTPTimePickerError.ToolTip = $"Vui lòng chọn ngày kết thúc.";
                icBCTPTimePickerError.Visibility = Visibility.Visible;
                dpEndDayBCTP.BorderBrush = Brushes.Red;
                dpStartDayBCTP.BorderBrush = Brushes.LightGray;
                return;
            }
            else if (selectedStartDay.Value > selectedEndDay.Value)
            {
                icBCTPTimePickerError.ToolTip = $"Ngày bắt đầu không thể sau ngày kết thúc.";
                icBCTPTimePickerError.Visibility = Visibility.Visible;
                dpStartDayBCTP.BorderBrush = Brushes.Red;
                dpEndDayBCTP.BorderBrush = Brushes.Red;
                return;
            }
            else
            {
                dpStartDayBCTP.BorderBrush = Brushes.LightGray;
                dpEndDayBCTP.BorderBrush = Brushes.LightGray;
                icBCTPTimePickerError.Visibility = Visibility.Collapsed;
                PopulateBCTPChartAndDataGrid(selectedStartDay.Value, selectedEndDay.Value);
            }
        }
    }
}

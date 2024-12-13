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

    /// <summary>
    /// Interaction logic for UcBCMuonTra.xaml
    /// </summary>
    public partial class UcBCMuonTra : UserControl
    {
        private ObservableCollection<DSBCMuonSachModel> _borrowReports;
        private ObservableCollection<DSBCMuonSachModel> _filteredBorrowReports;
        private ObservableCollection<DSBCTraTreModel> _lateReturnReports;
        private ObservableCollection<DSBCTraTreModel> _filteredLateReturnReports;
        private ChartViewModel BCMSLineChartModel;
        private ChartViewModel BCMSPieChartModel;
        private ChartViewModel BCTTLineChartModel;
        private ChartViewModel BCTTPieChartModel;
        private ChartViewModel TienPhatBarChartModel;

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
            PopulateBCMSYearComboBox();
            await PopulateBCTTChartAndDataGrid();
            PopulateBCTTYearComboBox();
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
        }

        private async Task LoadBorrowReportsData(DateTime begin, DateTime end)
        {
            try
            {
                using (var _context = new QLTVContext())
                {
                    var borrowReports = await _context.BCMUONSACH
                        .Include(p => p.CTBCMUONSACH)
                            .ThenInclude(ct => ct.IDTheLoaiNavigation)
                        .ToListAsync();

                    _borrowReports = new ObservableCollection<DSBCMuonSachModel>(
                        borrowReports
                            .GroupBy(bc => new { bc.Thang.Year, bc.Thang.Month })
                            .Select(g =>
                            {
                                var dsBCMuonSach = new ObservableCollection<BCMuonSachModel>(
                                    g.Select(b => new BCMuonSachModel
                                    {
                                        BCMuonSach = b,
                                        IsExpanded = false,
                                    }).ToList()
                                );

                                var totalBCMuonSach = new BCMUONSACH
                                {
                                    MaBCMuonSach = "Tổng",
                                    Thang = new DateTime(g.Key.Year, g.Key.Month, 1),
                                    TongSoLuotMuon = dsBCMuonSach.Sum(bc => bc.BCMuonSach.TongSoLuotMuon),
                                    CTBCMUONSACH = new List<CTBCMUONSACH>()
                                };

                                var totalCTBCMUONSACH = g
                                    .SelectMany(bc => bc.CTBCMUONSACH)
                                    .GroupBy(ct => ct.IDTheLoai)
                                    .Select(grp => new CTBCMUONSACH
                                    {
                                        IDBCMuonSach = 0,
                                        IDTheLoai = grp.Key,
                                        SoLuotMuon = grp.Sum(ct => ct.SoLuotMuon),
                                        TiLe = grp.Sum(ct => ct.SoLuotMuon) / (double)totalBCMuonSach.TongSoLuotMuon,
                                        IDTheLoaiNavigation = grp.First().IDTheLoaiNavigation,
                                        IDBCMuonSachNavigation = totalBCMuonSach
                                    })
                                    .ToList();

                                totalBCMuonSach.CTBCMUONSACH = totalCTBCMUONSACH;

                                var totalBCMuonSachModel = new BCMuonSachModel
                                {
                                    BCMuonSach = totalBCMuonSach,
                                    IsExpanded = false,
                                };

                                dsBCMuonSach.Insert(0, totalBCMuonSachModel);

                                return new DSBCMuonSachModel
                                {
                                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                                    DSBCMuonSach = dsBCMuonSach,
                                    IsExpanded = false
                                };
                            })
                            .ToList()
                    );

                    _filteredBorrowReports = new ObservableCollection<DSBCMuonSachModel>(
                        _borrowReports
                            .Where(r => r.Month >= begin && r.Month <= end)
                            .Select(r =>
                            {
                                var filteredDSBCMuonSach = new ObservableCollection<BCMuonSachModel>(
                                    r.DSBCMuonSach.Where(bc => bc.BCMuonSach.Thang >= begin && bc.BCMuonSach.Thang <= end).ToList()
                                );

                                var totalBCMuonSach = new BCMUONSACH
                                {
                                    MaBCMuonSach = "Tổng",
                                    Thang = r.Month,
                                    TongSoLuotMuon = filteredDSBCMuonSach.Sum(bc => bc.BCMuonSach.TongSoLuotMuon),
                                    CTBCMUONSACH = new List<CTBCMUONSACH>()
                                };

                                var totalCTBCMUONSACH = filteredDSBCMuonSach
                                    .SelectMany(bc => bc.BCMuonSach.CTBCMUONSACH)
                                    .GroupBy(ct => ct.IDTheLoai)
                                    .Select(grp => new CTBCMUONSACH
                                    {
                                        IDBCMuonSach = 0,
                                        IDTheLoai = grp.Key,
                                        SoLuotMuon = grp.Sum(ct => ct.SoLuotMuon),
                                        TiLe = grp.Sum(ct => ct.SoLuotMuon) / (double)totalBCMuonSach.TongSoLuotMuon,
                                        IDTheLoaiNavigation = grp.First().IDTheLoaiNavigation,
                                        IDBCMuonSachNavigation = totalBCMuonSach
                                    })
                                    .ToList();

                                totalBCMuonSach.CTBCMUONSACH = totalCTBCMUONSACH;

                                var totalBCMuonSachModel = new BCMuonSachModel
                                {
                                    BCMuonSach = totalBCMuonSach,
                                    IsExpanded = false,
                                };

                                filteredDSBCMuonSach.Insert(0, totalBCMuonSachModel);

                                return new DSBCMuonSachModel
                                {
                                    Month = r.Month,
                                    DSBCMuonSach = filteredDSBCMuonSach,
                                    IsExpanded = false
                                };
                            })
                            .ToList()
                    );

                    dgBorrowingReports.ItemsSource = _filteredBorrowReports;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadLateReturnsReportData(DateTime begin, DateTime end)
        {
            try
            {
                using (var _context = new QLTVContext())
                {
                    var lateReturnReports = await _context.BCTRATRE
                        .Include(p => p.CTBCTRATRE)
                            .ThenInclude(ct => ct.IDPhieuTraNavigation)
                        .ToListAsync();

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

                    _filteredLateReturnReports = new ObservableCollection<DSBCTraTreModel>(
                        _lateReturnReports
                            .Where(r => r.Month >= begin && r.Month <= end)
                            .Select(r =>
                            {
                                var filteredDSBCTraTre = new ObservableCollection<BCTraTreModel>(
                                    r.DSBCTraTre.Where(bc => bc.BCTraTre.Ngay >= begin && bc.BCTraTre.Ngay <= end).ToList()
                                );

                                return new DSBCTraTreModel
                                {
                                    Month = r.Month,
                                    DSBCTraTre = filteredDSBCTraTre,
                                    IsExpanded = false
                                };
                            })
                            .ToList()
                    );

                    dgLateReturnReports.ItemsSource = _filteredLateReturnReports;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /*      Borrowing  Reports      */

        private async Task PopulateBCMSChartAndDataGrid(DateTime? begin = null, DateTime? end = null)
        {
            DateTime startDate = begin ?? new DateTime(1970, 1, 1);
            DateTime endDate = end ?? DateTime.Now;

            await LoadBorrowReportsData(startDate, endDate);
            await LoadLateReturnsReportData(startDate, endDate);
            PopulateBCMSLineChartViewModel(startDate, endDate);
            PopulateBCMSPieChartViewModel(startDate, endDate);
        }

        private void PopulateBCMSYearComboBox()
        {
            var NamBCMS = _borrowReports
                .Select(r => r.Month.Year.ToString())
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            // Nạp các tuỳ chọn cho combobox Năm của Báo cáo mượn sách
            NamBCMS.Insert(0, "Tất cả"); // Thêm tuỳ chọn "Tất cả"
            cbYearMS.ItemsSource = NamBCMS;
            cbYearMS.SelectedIndex = 0;
        }

        private void PopulateBCMSMonthComboBox(string? selectedYear)
        {
            var ThangBCMS = _borrowReports
                .Select(r => r.Month)
                .Distinct()
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .Select(m => m.ToString("MM/yyyy"))
                .ToList();

            cbStartMonthMS.ItemsSource = ThangBCMS;
            cbEndMonthMS.ItemsSource = ThangBCMS;
            cbStartMonthMS.SelectedIndex = 0;
            cbEndMonthMS.SelectedIndex = cbEndMonthMS.Items.Count - 1;
        }

        private void PopulateBCMSQuarterComboBox(string? selectedYear)
        {
            var QuyBCMS = _borrowReports
                .Where(r => selectedYear == "Tất cả" || r.Month.Year == int.Parse(selectedYear))
                .Select(r =>
                {
                    var month = r.Month.Month;
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

        private void lbBCMSChartMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            HideAllBCMSChartControls();

            var selectedMode = ((sender as ListBox).SelectedItem as ListBoxItem)?.Content.ToString();
            switch (selectedMode)
            {
                case "Ngày":
                    spDayMS.Visibility = Visibility.Visible;
                    break;
                case "Tháng":
                    spMonthMS.Visibility = Visibility.Visible;
                    PopulateBCMSMonthComboBox(null);
                    break;
                case "Quý":
                    cbQuarterMS.Visibility = Visibility.Visible;
                    cbYearMS.Visibility = Visibility.Visible;
                    if (cbYearMS.ItemsSource == null)
                        PopulateBCMSYearComboBox();
                    PopulateBCMSQuarterComboBox((cbYearMS as ComboBox)?.SelectedItem as string);
                    break;
                case "Năm":
                    cbYearMS.Visibility = Visibility.Visible;
                    if (cbYearMS.ItemsSource == null)
                        PopulateBCMSYearComboBox();
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

            foreach (var report in _filteredBorrowReports)
            {
                foreach (var bcMuonSach in report.DSBCMuonSach)
                {
                    if (bcMuonSach.BCMuonSach.Thang >= begin &&
                        bcMuonSach.BCMuonSach.Thang <= end &&
                        bcMuonSach.BCMuonSach.MaBCMuonSach != "Tổng")
                    {
                        var daysInMonth = DateTime.DaysInMonth(bcMuonSach.BCMuonSach.Thang.Year, bcMuonSach.BCMuonSach.Thang.Month);
                        for (int day = 1; day <= daysInMonth; day++)
                        {
                            var currentDate = new DateTime(bcMuonSach.BCMuonSach.Thang.Year, bcMuonSach.BCMuonSach.Thang.Month, day);
                            if (currentDate >= begin && currentDate <= end)
                            {
                                values.Add(bcMuonSach.BCMuonSach.TongSoLuotMuon / daysInMonth);
                                labels.Add(currentDate.ToString("dd/MM/yyyy"));
                            }
                        }
                    }
                }
            }

            BCMSLineChartModel.CurrentChartValues.Add(new LineSeries
            {
                Title = "Số lượt mượn",
                Values = values
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

            foreach (var report in _filteredBorrowReports)
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
                    LabelPoint = chartPoint => $"{chartPoint.SeriesView.Title}: {chartPoint.Y} lượt mượn",
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
                    begin = _borrowReports.Min(r => r.Month);
                    end = _borrowReports.Max(r => r.Month);
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
            DateTime startDate = begin ?? new DateTime(1970, 1, 1);
            DateTime endDate = end ?? DateTime.Now;

            await LoadBorrowReportsData(startDate, endDate);
            await LoadLateReturnsReportData(startDate, endDate);
            PopulateBCTTLineChartViewModel(startDate, endDate);
            PopulateBCTTPieChartViewModel(startDate, endDate);
            PopulateTienPhatBarChartViewModel(startDate, endDate);
        }

        private void PopulateBCTTYearComboBox()
        {
            var NamBCTT = _lateReturnReports
                .Select(r => r.Month.Year.ToString())
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            // Nạp các tuỳ chọn năm cho combobox của Báo cáo trả trễ
            NamBCTT.Insert(0, "Tất cả"); // Thêm tuỳ chọn "Tất cả" option
            cbYearTT.ItemsSource = NamBCTT;
            cbYearTT.SelectedIndex = 0;
        }

        private void PopulateBCTTMonthComboBox(string? selectedYear)
        {
            var ThangBCTT = _lateReturnReports
                .Select(r => r.Month)
                .Distinct()
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .Select(m => m.ToString("MM/yyyy"))
                .ToList();

            cbStartMonthTT.ItemsSource = ThangBCTT;
            cbEndMonthTT.ItemsSource = ThangBCTT;
            cbStartMonthTT.SelectedIndex = 0;
            cbEndMonthTT.SelectedIndex = cbEndMonthTT.Items.Count - 1;
        }

        private void PopulateBCTTQuarterComboBox(string? selectedYear)
        {
            var QuyBCTT = _lateReturnReports
                .Where(r => selectedYear == "Tất cả" || r.Month.Year == int.Parse(selectedYear))
                .Select(r =>
                {
                    var month = r.Month.Month;
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
                    break;
                case "Năm":
                    cbYearTT.Visibility = Visibility.Visible;
                    if (cbYearTT.ItemsSource == null)
                        PopulateBCTTYearComboBox();
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

            var dailyCounts = _filteredLateReturnReports
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
                Values = values
            });

            BCTTLineChartModel.CurrentLabels = labels;
            BCTTLineChart.DataContext = BCTTLineChartModel;
        }

        private void PopulateTienPhatBarChartViewModel(DateTime begin, DateTime end)
        {
            TienPhatBarChartModel = new ChartViewModel
            {
                StartTime = begin,
                EndTime = end,
                CurrentChartValues = new SeriesCollection(),
                CurrentLabels = new List<string>()
            };

            var values = new ChartValues<decimal>();
            var labels = new List<string>();

            using (var context = new QLTVContext())
            {
                var dailyFines = context.PHIEUTRA
                    .Where(phieuTra => phieuTra.NgayTra >= begin && phieuTra.NgayTra <= end)
                    .SelectMany(phieuTra => phieuTra.CTPHIEUTRA, (phieuTra, ct) => new { phieuTra.NgayTra, ct.TienPhat })
                    .GroupBy(x => x.NgayTra.Date)
                    .Select(g => new { Date = g.Key, TotalFine = g.Sum(x => x.TienPhat) })
                    .OrderBy(x => x.Date);
                foreach (var dailyFine in dailyFines)
                {
                    values.Add(dailyFine.TotalFine);
                    labels.Add(dailyFine.Date.ToString("dd/MM/yyyy"));
                }
            }

            TienPhatBarChartModel.CurrentChartValues.Add(new LineSeries
            {
                Title = "Số tiền phạt",
                Values = values
            });

            TienPhatBarChartModel.CurrentLabels = labels;
            TienPhatBarChart.DataContext = TienPhatBarChartModel;
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

            foreach (var report in _filteredLateReturnReports)
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
                    begin = _borrowReports.Min(r => r.Month);
                    end = _borrowReports.Max(r => r.Month);
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
    }
}

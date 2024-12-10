using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts.Wpf;
using LiveCharts;
using Microsoft.EntityFrameworkCore;
using QLTV.Models;
using System.Drawing;

namespace QLTV.UserControls
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

    /// <summary>
    /// Interaction logic for UcBCMuonTra.xaml
    /// </summary>
    public partial class UcBCMuonTra : UserControl
    {
        private ObservableCollection<DSBCMuonSachModel> _borrowReports;
        private ObservableCollection<DSBCMuonSachModel> _filteredBorrowReports;
        private ObservableCollection<BCTraTreModel> _lateReturnReports;
        private ObservableCollection<BCTraTreModel> _filteredLateReturnReports;
        private ChartViewModel BCMSChartModel;

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
            await LoadReportsData();
            PopulateYearComboBox();

        }

        private async Task LoadReportsData()
        {
            try
            {
                using (var _context = new QLTVContext())
                {
                    // Load borrowing reports with related data, excluding soft-deleted records
                    var borrowReports = await _context.BCMUONSACH
                        .Include(p => p.CTBCMUONSACH)
                            .ThenInclude(ct => ct.IDTheLoaiNavigation)
                        .ToListAsync();

                    _borrowReports = new ObservableCollection<DSBCMuonSachModel>(
                        borrowReports
                            //Nhóm theo tháng + năm
                            .GroupBy(bc => new { bc.Thang.Year, bc.Thang.Month })
                            .Select(g =>
                            {
                                //Tạo model tương ứng với các BCMUONSACH
                                var dsBCMuonSach = new ObservableCollection<BCMuonSachModel>(
                                    g.Select(b => new BCMuonSachModel
                                    {
                                        BCMuonSach = b,
                                        IsExpanded = false,
                                    }).ToList()
                                );

                                //Tính báo cáo tổng của tháng
                                var totalBCMuonSach = new BCMUONSACH
                                {
                                    MaBCMuonSach = "Tổng",
                                    Thang = new DateTime(g.Key.Year, g.Key.Month, 1),
                                    TongSoLuotMuon = dsBCMuonSach.Sum(bc => bc.BCMuonSach.TongSoLuotMuon),
                                    CTBCMUONSACH = new List<CTBCMUONSACH>()
                                };

                                //Tổng hợp các CTBCMUONSACH dựa vào IDTheLoai
                                var totalCTBCMUONSACH = g
                                    .SelectMany(bc => bc.CTBCMUONSACH)
                                    .GroupBy(ct => ct.IDTheLoai)
                                    .Select(grp => new CTBCMUONSACH
                                    {
                                        IDBCMuonSach = 0, //Giá trị không sử dụng
                                        IDTheLoai = grp.Key,
                                        SoLuotMuon = grp.Sum(ct => ct.SoLuotMuon),
                                        TiLe = grp.Sum(ct => ct.SoLuotMuon) / (double)totalBCMuonSach.TongSoLuotMuon,
                                        IDTheLoaiNavigation = grp.First().IDTheLoaiNavigation,
                                        IDBCMuonSachNavigation = totalBCMuonSach //Tham chiếu đến BCMUONSACH
                                    })
                                    .ToList();

                                //Thêm các CTBCMUONSACH đã tổng hợp vào BCMUONSACH của tháng
                                totalBCMuonSach.CTBCMUONSACH = totalCTBCMUONSACH;

                                //Tạo model BCMUONSACH của tháng
                                var totalBCMuonSachModel = new BCMuonSachModel
                                {
                                    BCMuonSach = totalBCMuonSach,
                                    IsExpanded = false,
                                };

                                //Thêm BCMUONSACH tổng vào đầu danh sách
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
                    _filteredBorrowReports = new ObservableCollection<DSBCMuonSachModel>(_borrowReports);
                    dgBorrowingReports.ItemsSource = _filteredBorrowReports;

                    // Load late return reports with related data, excluding soft-deleted records
                    var lateReturnReports = await _context.BCTRATRE
                        .Include(p => p.CTBCTRATRE)
                            .ThenInclude(ct => ct.IDPhieuTraNavigation)
                        .ToListAsync();
                    _lateReturnReports = new ObservableCollection<BCTraTreModel>
                        (
                            lateReturnReports.Select(bc => new BCTraTreModel
                            {
                                BCTraTre = bc,
                                IsExpanded = false
                            })
                        );
                    _filteredLateReturnReports = new ObservableCollection<BCTraTreModel>(_lateReturnReports);
                    dgLateReturnReports.ItemsSource = _filteredLateReturnReports;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateYearComboBox()
        {
            var NamBCMS = _borrowReports
                .Select(r => r.Month.Year.ToString())
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            // Populate chart year combobox
            cbYear.ItemsSource = NamBCMS;
            cbYear.SelectedIndex = 0;

            // Create a new list for NamBCMSWithAll and add "Tất cả" option
            var NamBCMSWithAll = new List<string>(NamBCMS);
            NamBCMSWithAll.Insert(0, "Tất cả");
            cboNamBCMS.ItemsSource = NamBCMSWithAll;
            cboNamBCMS.SelectedIndex = 0; // Select "Tất cả" by default

            var NamBCTT = _lateReturnReports
                .Select(r => r.BCTraTre.Ngay.Year.ToString())
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            NamBCTT.Insert(0, "Tất cả"); // Add "Tất cả" option
            cboNamBCTT.ItemsSource = NamBCTT;
            cboNamBCTT.SelectedIndex = 0;


        }

        private void PopulateMonthComboBox(string? selectedYear)
        {
            var ThangBCMS = _borrowReports
                .Select(r => r.Month.ToString("MM/yyyy"))
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            // Populate chart month comboboxes
            cbStartMonth.ItemsSource = ThangBCMS; // Skip "Tất cả" option
            cbEndMonth.ItemsSource = ThangBCMS; // Skip "Tất cả" option
            cbStartMonth.SelectedIndex = 0; // Select the first month by default
            cbEndMonth.SelectedIndex = cbEndMonth.Items.Count - 1; // Select the last month by default

            var ThangBCTT = _lateReturnReports
                .Where(r => selectedYear == "Tất cả" || r.BCTraTre.Ngay.Year == int.Parse(selectedYear))
                .Select(r => r.BCTraTre.Ngay.Month.ToString())
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            ThangBCTT.Insert(0, "Tất cả"); // Add "Tất cả" option
            cboThangBCTT.ItemsSource = ThangBCTT;
            cboThangBCTT.SelectedIndex = 0; // Select "Tất cả" by default
        }

        private void PopulateQuarterComboBox(string? selectedYear)
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
            QuyBCMS.Insert(0, "Tất cả"); // Add "Tất cả" option
            cbQuarter.ItemsSource = QuyBCMS;
            cbQuarter.SelectedIndex = 0; // Select "Tất cả" by default
        }

        private void cboNamBCMS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedYear = cboNamBCMS.SelectedItem as string;

            _filteredBorrowReports = new ObservableCollection<DSBCMuonSachModel>(
                _borrowReports.Where(r => selectedYear == "Tất cả" || r.Month.Year == int.Parse(selectedYear))
            );
            dgBorrowingReports.ItemsSource = _filteredBorrowReports;
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
            if (row?.DataContext is BCTraTreModel bctt)
            {
                bctt.IsExpanded = !bctt.IsExpanded;
            }
        }

        private void cboNamBCTT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PopulateMonthComboBox(cboNamBCTT.SelectedItem as string);
            BaoCaoTraTreSearch();
        }

        private void cboThangBCTT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BaoCaoTraTreSearch();
        }

        private void BaoCaoTraTreSearch()
        {
            var selectedYear = cboNamBCTT.SelectedItem as string;
            var selectedMonth = cboThangBCTT.SelectedItem as string;

            _filteredLateReturnReports = new ObservableCollection<BCTraTreModel>(
                _lateReturnReports.Where(r =>
                    (selectedYear == "Tất cả" || r.BCTraTre.Ngay.Year == int.Parse(selectedYear)) &&
                    (selectedMonth == "Tất cả" || r.BCTraTre.Ngay.Month == int.Parse(selectedMonth))
                )
            );

            dgLateReturnReports.ItemsSource = _filteredLateReturnReports;
        }

        private void lbBCMSChartMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            HideAllChartControls();

            var selectedMode = ((sender as ListBox).SelectedItem as ListBoxItem)?.Content.ToString();
            switch (selectedMode)
            {
                case "Ngày":
                    spDay.Visibility = Visibility.Visible;
                    break;
                case "Tháng":
                    spMonth.Visibility = Visibility.Visible;
                    PopulateMonthComboBox(null);
                    break;
                case "Quý":
                    cbQuarter.Visibility = Visibility.Visible;
                    cbYear.Visibility = Visibility.Visible;
                    if (cbYear.ItemsSource == null)
                        PopulateYearComboBox();
                    PopulateQuarterComboBox((cbYear as ComboBox)?.SelectedItem as string);
                    break;
                case "Năm":
                    cbYear.Visibility = Visibility.Visible;
                    if (cbYear.ItemsSource == null)
                        PopulateYearComboBox();
                    break;
                default:
                    break;
            }
        }

        private void HideAllChartControls()
        {
            spDay.Visibility = Visibility.Collapsed;
            spMonth.Visibility = Visibility.Collapsed;
            cbQuarter.Visibility = Visibility.Collapsed;
            cbYear.Visibility = Visibility.Collapsed;
            dpStartDay.SelectedDate = null;
            dpEndDay.SelectedDate = null;
            cbStartMonth.SelectedItem = null;
            cbEndMonth.SelectedItem = null;
            cbQuarter.SelectedItem = null;
        }

        private void PopulateChartViewModel(DateTime begin, DateTime end)
        {
            BCMSChartModel = new ChartViewModel
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
                        values.Add(bcMuonSach.BCMuonSach.TongSoLuotMuon);
                        labels.Add(bcMuonSach.BCMuonSach.Thang.ToString("MM/yyyy"));
                    }
                }
            }

            BCMSChartModel.CurrentChartValues.Add(new LineSeries
            {
                Title = "Số lượt mượn",
                Values = values
            });

            BCMSChartModel.CurrentLabels = labels;
            BCMSChart.DataContext = BCMSChartModel;
        }

        private void cboYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedYear = (sender as ComboBox)?.SelectedItem as string;
            if (selectedYear != null)
            {
                var begin = new DateTime(int.Parse(selectedYear), 1, 1);
                var end = new DateTime(int.Parse(selectedYear), 12, 31);
                PopulateChartViewModel(begin, end);
            }
        }

        private void cboQuarter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedQuarter = (sender as ComboBox)?.SelectedItem as string;
            var selectedYear = cbYear.SelectedItem as string;
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
                PopulateChartViewModel(begin, end);
            }
        }

        private void cboStartMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedStartMonth = cbStartMonth.SelectedItem as string;
            var selectedEndMonth = cbEndMonth.SelectedItem as string;
            var selectedYear = cbYear.SelectedItem as string;

            if (int.TryParse(selectedStartMonth, out int startMonth) &&
                int.TryParse(selectedEndMonth, out int endMonth) &&
                int.TryParse(selectedYear, out int year))
            {
                var begin = new DateTime(year, startMonth, 1);
                var end = new DateTime(year, endMonth, DateTime.DaysInMonth(year, endMonth));

                PopulateChartViewModel(begin, end);
            }
        }

        private void cboEndMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            cboStartMonth_SelectionChanged(sender, e);
        }

        private void dpEndDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            var selectedStartDay = dpStartDay.SelectedDate;
            var selectedEndDay = dpEndDay.SelectedDate;
            if (selectedStartDay != null && selectedEndDay != null)
            {
                PopulateChartViewModel(selectedStartDay.Value, selectedEndDay.Value);
            }
        }
    }
}

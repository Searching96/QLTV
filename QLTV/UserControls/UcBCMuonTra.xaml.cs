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
using Microsoft.EntityFrameworkCore;
using QLTV.Models;

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

    /// <summary>
    /// Interaction logic for UcBCMuonTra.xaml
    /// </summary>
    public partial class UcBCMuonTra : UserControl
    {
        private readonly QLTVContext _context;
        private ObservableCollection<DSBCMuonSachModel> _borrowReports;
        private ObservableCollection<DSBCMuonSachModel> _filteredBorrowReports;
        private ObservableCollection<BCTraTreModel> _lateReturnReports;
        private ObservableCollection<BCTraTreModel> _filteredLateReturnReports;


        public UcBCMuonTra()
        {
            InitializeComponent();
            _context = new QLTVContext();
            LoadData();
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
            NamBCMS.Insert(0, "Tất cả"); // Thêm tuỳ chọn "Tất cả"
            cboNamBCMS.ItemsSource = NamBCMS;
            cboNamBCMS.SelectedIndex = 0; // Chọn giá trị mặc định "Tất cả"

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
    }
}

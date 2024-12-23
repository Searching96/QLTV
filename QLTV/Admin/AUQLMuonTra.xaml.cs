using Microsoft.EntityFrameworkCore;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace QLTV.Admin
{

    public class BorrowViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<CTPHIEUMUON> _ctPhieuMuon;
        public ObservableCollection<CTPHIEUMUON> ctPhieuMuon
        {
            get => _ctPhieuMuon;
            set
            {
                _ctPhieuMuon = value;
                OnPropertyChanged(nameof(ctPhieuMuon));
            }
        }

        public PHIEUMUON phieuMuon { get; set; }
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ReturnViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<CTPHIEUTRA> _ctPhieuTra;
        public ObservableCollection<CTPHIEUTRA> ctPhieuTra
        {
            get => _ctPhieuTra;
            set
            {
                _ctPhieuTra = value;
                OnPropertyChanged(nameof(ctPhieuTra));
            }
        }

        public string DocGia => _ctPhieuTra.First().IDPhieuMuonNavigation.IDDocGiaNavigation.IDTaiKhoanNavigation.TenTaiKhoan ?? "";

        public PHIEUTRA phieuTra { get; set; }
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public partial class UcQLMuonTra : UserControl
    {
        private List<PHIEUMUON> borrowings;
        private ObservableCollection<BorrowViewModel> _borrowings;
        private ObservableCollection<ReturnViewModel> _returns;

        public UcQLMuonTra()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                using (var _context = new QLTVContext())
                {
                    // Lấy dữ liệu phiếu mượn và các thông tin liên quan, ngoại trừ các phiếu đã xóa mềm
                    borrowings = await _context.PHIEUMUON
                        .Include(p => p.IDDocGiaNavigation)
                            .ThenInclude(d => d.IDTaiKhoanNavigation)
                        .Include(p => p.CTPHIEUMUON)
                            .ThenInclude(ct => ct.IDSachNavigation)
                                .ThenInclude(s => s.IDTuaSachNavigation)
                        .Include(p => p.CTPHIEUMUON)
                            .ThenInclude(ct => ct.IDTinhTrangMuonNavigation)
                        .Where(p => !p.IsDeleted)
                        .ToListAsync();

                    var _borrowings = new ObservableCollection<BorrowViewModel>(
                        borrowings.Select(b => new BorrowViewModel
                        {
                            phieuMuon = b,
                            ctPhieuMuon = new ObservableCollection<CTPHIEUMUON>(b.CTPHIEUMUON),
                            IsExpanded = false
                        })
                    );

                    dgBorrowings.ItemsSource = _borrowings;

                    // Lấy dữ liệu phiếu trả và các thông tin liên quan, ngoại trừ các phiếu đã xóa mềm
                    var returns = await _context.PHIEUTRA
                        .Include(p => p.CTPHIEUTRA)
                            .ThenInclude(ct => ct.IDSachNavigation)
                                .ThenInclude(s => s.IDTuaSachNavigation)
                        .Include(p => p.CTPHIEUTRA)
                            .ThenInclude(ct => ct.IDTinhTrangTraNavigation)
                        .Where(p => !p.IsDeleted)
                        .ToListAsync();
                    _returns = new ObservableCollection<ReturnViewModel>(
                        returns.Select(r => new ReturnViewModel
                        {
                            phieuTra = r,
                            ctPhieuTra = new ObservableCollection<CTPHIEUTRA>(r.CTPHIEUTRA),
                            IsExpanded = false
                        })
                    );
                    dgReturns.ItemsSource = _returns;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddBorrow_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window
            {
                Title = "Thêm phiếu mượn",
                Content = new UcThemPhieuMuon(),
                Width = 850,
                Height = 650,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResizeWithGrip
            };

            if (window.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void btnViewDetail_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            var row = DataGridRow.GetRowContainingElement(button);
            if (row?.DataContext is BorrowViewModel borrowing)
            {
                borrowing.IsExpanded = !borrowing.IsExpanded;
            }
            if (row?.DataContext is ReturnViewModel returning)
            {
                returning.IsExpanded = !returning.IsExpanded;
            }
        }

        private async void btnDeleteBorrow_Click(object sender, RoutedEventArgs e)
        {
            var borrowViewModel = ((FrameworkElement)sender).DataContext as BorrowViewModel;
            if (borrowViewModel == null) return;

            if (MessageBox.Show("Bạn có chắc muốn xóa phiếu mượn này?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var _context = new QLTVContext())
                    {
                        // Xoá mềm phiếu mượn
                        borrowViewModel.phieuMuon.IsDeleted = true;

                        // Khôi phục tình trạng mượn của các quyển sách có trong phiếu mượn
                        foreach (var ctborrowViewModel in borrowViewModel.ctPhieuMuon)
                        {
                            var sach = ctborrowViewModel.IDSachNavigation;
                            if (sach != null)
                            {
                                sach.IsAvailable = true;
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                    LoadData();
                    BorrowSearch((cboLoc.SelectedItem as ComboBoxItem).Content.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa phiếu mượn: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAddReturn_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window
            {
                Title = "Thêm phiếu trả",
                Content = new UcThemPhieuTra(),
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            if (window.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private async void btnDeleteReturn_Click(object sender, RoutedEventArgs e)
        {
            var returnViewModel = ((FrameworkElement)sender).DataContext as ReturnViewModel;
            if (returnViewModel == null) return;

            if (MessageBox.Show("Bạn có chắc muốn xóa phiếu trả này?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var _context = new QLTVContext())
                    {
                        // Xoá mềm phiếu trả
                        returnViewModel.phieuTra.IsDeleted = true;

                        // Xử lí các CTPHIEUTRA có liên quan
                        foreach (var ctPhieuTra in returnViewModel.phieuTra.CTPHIEUTRA)
                        {
                            // Khôi phục trạng thái trả của sách
                            var sach = ctPhieuTra.IDSachNavigation;
                            if (sach != null)
                            {
                                sach.IsAvailable = false;
                            }
                        }

                        await _context.SaveChangesAsync();
                        _returns.Remove(returnViewModel);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa phiếu trả: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void txtSearchBorrow_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBoxItem cbi = (ComboBoxItem)cboLoc.SelectedItem;
            string selectedText = cbi.Content.ToString();
            BorrowSearch(selectedText);
        }

        private void txtSearchReturn_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_returns == null) return;

            var searchText = txtSearchReturn.Text.Trim().ToLower();

            IEnumerable<ReturnViewModel> filteredReturns = _returns;

            if (!string.IsNullOrEmpty(searchText))
            {
                filteredReturns = filteredReturns.Where(s =>
                    s.phieuTra.CTPHIEUTRA.Any(ct => ct.IDPhieuMuonNavigation.IDDocGiaNavigation.IDTaiKhoanNavigation.TenTaiKhoan.Contains(searchText)) ||
                    s.phieuTra.MaPhieuTra.ToLower().Contains(searchText) ||
                    s.phieuTra.CTPHIEUTRA.Any(ct => ct.IDPhieuMuonNavigation.MaPhieuMuon.ToLower().Contains(searchText)) ||
                    s.phieuTra.NgayTra.ToShortDateString().Contains(searchText));
            }

            dgReturns.ItemsSource = filteredReturns;
        }

        private void BorrowSearch(string Loai = "")
        {
            switch (Loai)
            {
                case "Chưa trả hết":
                    {
                        _borrowings = new ObservableCollection<BorrowViewModel>(
                            borrowings.Select(b => new BorrowViewModel
                            {
                                phieuMuon = b,
                                ctPhieuMuon = new ObservableCollection<CTPHIEUMUON>(b.CTPHIEUMUON),
                                IsExpanded = false
                            })
                            .Where(b => b.phieuMuon.CTPHIEUMUON.Count != b.phieuMuon.CTPHIEUTRA.Count)
                        );
                        break;
                    }
                case "Đã trả hết":
                    {
                        _borrowings = new ObservableCollection<BorrowViewModel>(
                            borrowings.Select(b => new BorrowViewModel
                            {
                                phieuMuon = b,
                                ctPhieuMuon = new ObservableCollection<CTPHIEUMUON>(b.CTPHIEUMUON),
                                IsExpanded = false
                            })
                            .Where(b => b.phieuMuon.CTPHIEUMUON.Count == b.phieuMuon.CTPHIEUTRA.Count)
                        );
                        break;
                    }
                case "Chưa duyệt":
                    {
                        _borrowings = new ObservableCollection<BorrowViewModel>(
                            borrowings.Select(b => new BorrowViewModel
                            {
                                phieuMuon = b,
                                ctPhieuMuon = new ObservableCollection<CTPHIEUMUON>(b.CTPHIEUMUON),
                                IsExpanded = false
                            })
                            .Where(b => b.phieuMuon.IsPending)
                        );
                        break;
                    }
                default:
                    {
                        _borrowings = new ObservableCollection<BorrowViewModel>(
                            borrowings.Select(b => new BorrowViewModel
                            {
                                phieuMuon = b,
                                ctPhieuMuon = new ObservableCollection<CTPHIEUMUON>(b.CTPHIEUMUON),
                                IsExpanded = false
                            })
                        );
                        break;
                    }
            }

            if (_borrowings == null) return;

            var searchText = txtSearchBorrow.Text.Trim().ToLower();

            dgBorrowings.ItemsSource = null;

            IEnumerable<BorrowViewModel> filteredBorrows = _borrowings;

            if (!string.IsNullOrEmpty(searchText))
            {
                filteredBorrows = filteredBorrows.Where(s =>
                    s.phieuMuon.IDDocGiaNavigation.IDTaiKhoanNavigation.TenTaiKhoan.Contains(searchText) ||
                    s.phieuMuon.MaPhieuMuon.ToLower().Contains(searchText) ||
                    s.phieuMuon.NgayMuon.ToShortDateString().Contains(searchText));
            }

            dgBorrowings.ItemsSource = filteredBorrows;
        }

        private void cboLoc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem cbi = (ComboBoxItem)cboLoc.SelectedItem;
            if (cbi == null || cbi.Content == null) return;
            string selectedText = cbi.Content.ToString();
            BorrowSearch(selectedText);
        }

        private void btnAcceptBorrow_Click(object sender, RoutedEventArgs e)
        {
            var borrowViewModel = ((FrameworkElement)sender).DataContext as BorrowViewModel;
            if (borrowViewModel != null)
            {
                using (var _context = new QLTVContext())
                {
                    borrowViewModel.phieuMuon.IsPending = false;
                    _context.Update(borrowViewModel.phieuMuon);
                    _context.SaveChanges();
                    LoadData();
                }
            }
        }
    }
}

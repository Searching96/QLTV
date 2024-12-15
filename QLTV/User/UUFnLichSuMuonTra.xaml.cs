using Microsoft.EntityFrameworkCore;
using QLTV.Admin;
using QLTV.Models;
using QLTV.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace QLTV.User
{
    public partial class UUFnLichSuMuonTra : UserControl
    {
        private List<PHIEUMUON> borrowings;
        private ObservableCollection<BorrowViewModel> _borrowings;
        private ObservableCollection<ReturnViewModel> _returns;
        private DOCGIA currentDocGia;

        public UUFnLichSuMuonTra()
        {
            InitializeComponent();
            using (var context = new QLTVContext())
            {
                currentDocGia = context.DOCGIA
                    .Include(dg => dg.IDTaiKhoanNavigation)
                    .Where(dg => dg.IDTaiKhoanNavigation.ID == Settings.Default.CurrentUserID)
                    .FirstOrDefault();
            }
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
                        .Where(p => !p.IsDeleted && p.IDDocGia == currentDocGia.ID)
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
                        .Where(p => !p.IsDeleted && p.CTPHIEUTRA.All(ct => ct.IDPhieuMuonNavigation.IDDocGia == currentDocGia.ID))
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
                        if (!borrowViewModel.phieuMuon.IsPending)
                        {
                            MessageBox.Show("Không thể xóa phiếu mượn đã được duyệt!", "Lỗi",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        // Xoá mềm phiếu mượn
                        _context.PHIEUMUON.First(pm => pm.ID == borrowViewModel.phieuMuon.ID).IsDeleted = true;

                        // Khôi phục tình trạng mượn của các quyển sách có trong phiếu mượn
                        foreach (var ctborrowViewModel in borrowViewModel.ctPhieuMuon)
                        {
                            var sach = _context.SACH.First(s => s.ID == ctborrowViewModel.IDSachNavigation.ID);
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
    }

    public class BooleanToContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string content)
            {
                var contents = content.Split(':');
                return boolValue ? contents[1] : contents[0];
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
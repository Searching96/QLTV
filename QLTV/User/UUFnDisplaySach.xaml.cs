using Microsoft.EntityFrameworkCore;
using Microsoft.Xaml.Behaviors.Core;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

namespace QLTV.User
{
    /// <summary>
    /// Interaction logic for UUFnDisplaySach.xaml
    /// </summary>
    public partial class UUFnDisplaySach : UserControl
    {
        private List<TuaSachViewModel> _fullDataSource = new();
        private ObservableCollection<TuaSachViewModel> _dsSach = new();
        private int _itemsPerPage = 20;
        private int _currentPage = 1;
        public ObservableCollection<THELOAI> lstTheLoai { get; set; } = new ObservableCollection<THELOAI>();
        public ObservableCollection<THELOAI> lstSelectedTheLoai { get; set; } = new ObservableCollection<THELOAI>();
        public THELOAI? selectedTheLoai { get; set; }

        public ObservableCollection<TuaSachViewModel> dsSach => _dsSach;

        public class TuaSachViewModel
        {
            public string TenTuaSach { get; set; } = string.Empty;
            public string BiaSach { get; set; } = string.Empty;
            public string DSTacGia { get; set; } = string.Empty;
            public string DSTheLoai { get; set; } = string.Empty;
            public int LuotMuon { get; set; }
            public decimal DanhGia { get; set; }
        }

        public UUFnDisplaySach()
        {
            InitializeComponent();
            DataContext = this;
            LoadTuaSach();
            LoadTheLoai();
        }

        private void LoadTheLoai()
        {
            using (var context = new QLTVContext())
            {
                var data = context.THELOAI
                    .ToList();

                lstTheLoai = new ObservableCollection<THELOAI>(data);
            }
        }

        private void btnAddTheLoai_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTheLoai != null && !lstSelectedTheLoai.Contains(selectedTheLoai))
            {
                lstSelectedTheLoai.Add(selectedTheLoai);
            }
        }
        private void RemoveTheLoai_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is THELOAI theLoai)
            {
                lstSelectedTheLoai.Remove(theLoai);
            }
        }

        private void LoadTuaSach()
        {
            using (var context = new QLTVContext())
            {
                _fullDataSource = context.TUASACH
                    .Where(ts => !ts.IsDeleted)
                    .Include(ts => ts.TUASACH_TACGIA)
                        .ThenInclude(tg => tg.IDTacGiaNavigation)
                    .Include(ts => ts.TUASACH_THELOAI)
                        .ThenInclude(tl => tl.IDTheLoaiNavigation)
                    .Include(ts => ts.SACH)
                        .ThenInclude(s => s.CTPHIEUMUON)
                            .ThenInclude(ctpm => ctpm.IDPhieuMuonNavigation)
                    .Include(ts => ts.SACH)
                        .ThenInclude(s => s.DANHGIA)
                            .ThenInclude(dg => dg.IDPhieuMuonNavigation)
                    .ToList()
                    .Select(ts => new TuaSachViewModel
                    {
                        TenTuaSach = ts.TenTuaSach,
                        BiaSach = ts.BiaSach,
                        DSTacGia = ts.TUASACH_TACGIA != null && ts.TUASACH_TACGIA.Any()
                            ? string.Join(", ", ts.TUASACH_TACGIA
                                .Select(ts_tg => ts_tg.IDTacGiaNavigation?.TenTacGia ?? ""))
                            : string.Empty,
                        DSTheLoai = ts.TUASACH_THELOAI != null && ts.TUASACH_THELOAI.Any()
                            ? string.Join(", ", ts.TUASACH_THELOAI
                                .Select(ts_tl => ts_tl.IDTheLoaiNavigation?.TenTheLoai ?? ""))
                            : string.Empty,
                        LuotMuon = ts.SACH != null && ts.SACH.Any()
                            ? ts.SACH
                                .Sum(s => s.CTPHIEUMUON != null
                                    ? s.CTPHIEUMUON
                                        .Count(ctpm => ctpm.IDPhieuMuonNavigation != null &&
                                                       !ctpm.IDPhieuMuonNavigation.IsDeleted &&
                                                       !ctpm.IDPhieuMuonNavigation.IsPending)
                                    : 0)
                            : 0,
                        DanhGia = ts.SACH != null && ts.SACH.Any()
                            ? Math.Round(
                                ts.SACH
                                    .SelectMany(s => s.DANHGIA ?? Enumerable.Empty<DANHGIA>())
                                    .Where(dg => dg.IDPhieuMuonNavigation != null &&
                                                 !dg.IDPhieuMuonNavigation.IsDeleted &&
                                                 !dg.IDPhieuMuonNavigation.IsPending)
                                    .Select(dg => dg.DanhGia)
                                    .DefaultIfEmpty(0)
                                    .Average(),
                                1)
                            : 0
                    })
                    .ToList();

            }

            ApplyPaging();
        }

        private void ApplyPaging()
        {
            var paginatedData = _fullDataSource
                .Skip((_currentPage - 1) * _itemsPerPage)
                .Take(_itemsPerPage)
                .ToList();

            _dsSach.Clear();
            foreach (var item in paginatedData)
                _dsSach.Add(item);

            UpdatePageInfo();
        }

        private void UpdatePageInfo()
        {
            int totalItems = _fullDataSource.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / _itemsPerPage);

            tbxPageNumber.Text = $"{_currentPage} / {totalPages}";
            btnPrevious.IsEnabled = _currentPage > 1;
            btnNext.IsEnabled = _currentPage < totalPages;
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                ApplyPaging();
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_fullDataSource.Count / _itemsPerPage);
            if (_currentPage < totalPages)
            {
                _currentPage++;
                ApplyPaging();
            }
        }

        private void btnOpenFilter_Click(object sender, RoutedEventArgs e)
        {
            puFilter.IsOpen = !puFilter.IsOpen;
        }

        private void btnLoc_Click(object sender, RoutedEventArgs e)
        {
            if (!lstSelectedTheLoai.Any())
            {
                MessageBox.Show("Vui lòng chọn ít nhất một thể loại để lọc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new QLTVContext())
            {
                // Lấy danh sách tên thể loại từ lstSelectedTheLoai
                var lstTenTLDaChon = lstSelectedTheLoai.Select(tl => tl.TenTheLoai).ToList();

                // Truy vấn dữ liệu từ cơ sở dữ liệu và chuyển thành TuaSachViewModel
                var filteredBooks = context.TUASACH
                    .Where(ts => !ts.IsDeleted)  // Lọc sách chưa bị xóa
                    .Select(ts => new TuaSachViewModel
                    {
                        TenTuaSach = ts.TenTuaSach,
                        BiaSach = ts.BiaSach,
                        DSTacGia = string.Join(", ", ts.TUASACH_TACGIA.Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                        DSTheLoai = string.Join(", ", ts.TUASACH_THELOAI.Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai))
                    })
                    .AsEnumerable()  // Chuyển sang LINQ-to-Objects
                    .Where(tsvm =>
                        // Kiểm tra xem DSTheLoai của TuaSach có chứa tất cả các thể loại đã chọn không
                        lstTenTLDaChon.All(stl =>
                            tsvm.DSTheLoai.Contains(stl)))
                    .ToList();

                // Cập nhật dữ liệu hiển thị
                _fullDataSource = filteredBooks;
                _currentPage = 1;
                ApplyPaging();
            }
        }


        private string NormalizeString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return new string(
                text.Normalize(NormalizationForm.FormD)
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    .ToArray()
            ).Normalize(NormalizationForm.FormC).ToLower();
        }

        private void PerformSearch()
        {
            string searchTerm = NormalizeString(tbxThongTinTim.Text.Trim().ToLower());
            string selectedProperty = ((ComboBoxItem)cbbThuocTinhTim.SelectedItem)?.Content.ToString();

            // Kiểm tra nếu không có gì được chọn
            if (string.IsNullOrEmpty(selectedProperty))
            {
                MessageBox.Show("Vui lòng chọn thuộc tính tìm kiếm", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new QLTVContext())
            {
                // Truy vấn cơ sở dữ liệu để lấy tất cả các tựa sách
                var data = context.TUASACH
                    .Where(ts => !ts.IsDeleted)
                    .Include(ts => ts.TUASACH_TACGIA)
                        .ThenInclude(tg => tg.IDTacGiaNavigation)
                    .Include(ts => ts.TUASACH_THELOAI)
                        .ThenInclude(tl => tl.IDTheLoaiNavigation)
                    .Include(ts => ts.SACH)
                        .ThenInclude(s => s.CTPHIEUMUON)
                            .ThenInclude(ctpm => ctpm.IDPhieuMuonNavigation)
                    .Include(ts => ts.SACH)
                        .ThenInclude(s => s.DANHGIA)
                            .ThenInclude(dg => dg.IDPhieuMuonNavigation)
                    .ToList()
                    .Select(ts => new TuaSachViewModel
                    {
                        TenTuaSach = ts.TenTuaSach,
                        BiaSach = ts.BiaSach,
                        DSTacGia = ts.TUASACH_TACGIA != null && ts.TUASACH_TACGIA.Any()
                            ? string.Join(", ", ts.TUASACH_TACGIA
                                .Select(ts_tg => ts_tg.IDTacGiaNavigation?.TenTacGia ?? ""))
                            : string.Empty,
                        DSTheLoai = ts.TUASACH_THELOAI != null && ts.TUASACH_THELOAI.Any()
                            ? string.Join(", ", ts.TUASACH_THELOAI
                                .Select(ts_tl => ts_tl.IDTheLoaiNavigation?.TenTheLoai ?? ""))
                            : string.Empty,
                        LuotMuon = ts.SACH != null && ts.SACH.Any()
                            ? ts.SACH
                                .Sum(s => s.CTPHIEUMUON != null
                                    ? s.CTPHIEUMUON
                                        .Count(ctpm => ctpm.IDPhieuMuonNavigation != null &&
                                                       !ctpm.IDPhieuMuonNavigation.IsDeleted &&
                                                       !ctpm.IDPhieuMuonNavigation.IsPending)
                                    : 0)
                            : 0,
                        DanhGia = ts.SACH != null && ts.SACH.Any()
                            ? Math.Round(
                                ts.SACH
                                    .SelectMany(s => s.DANHGIA ?? Enumerable.Empty<DANHGIA>())
                                    .Where(dg => dg.IDPhieuMuonNavigation != null &&
                                                 !dg.IDPhieuMuonNavigation.IsDeleted &&
                                                 !dg.IDPhieuMuonNavigation.IsPending)
                                    .Select(dg => dg.DanhGia)
                                    .DefaultIfEmpty(0)
                                    .Average(),
                                1)
                            : 0
                    })
                    .ToList();

                // Initialize _fullDataSource as an ObservableCollection
                _fullDataSource = new List<TuaSachViewModel>(data);

                // Áp dụng phân trang
                ApplyPaging();
            }
        }

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void btnSort_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the visibility of the popup
            puSortOptions.IsOpen = !puSortOptions.IsOpen;
        }

        private void SortByBorrowCountDesc_Click(object sender, RoutedEventArgs e)
        {
            _fullDataSource = _fullDataSource.OrderByDescending(book => book.LuotMuon).ToList();
            _currentPage = 1;
            ApplyPaging();
            puSortOptions.IsOpen = false;
        }

        private void SortByBorrowCountAsc_Click(object sender, RoutedEventArgs e)
        {
            _fullDataSource = _fullDataSource.OrderBy(book => book.LuotMuon).ToList();
            _currentPage = 1;
            ApplyPaging();
            puSortOptions.IsOpen = false;
        }

        private void SortByRating_Click(object sender, RoutedEventArgs e)
        {
            // Implement sorting by rating
            _fullDataSource = _fullDataSource.OrderByDescending(book => book.DanhGia).ToList();
            _currentPage = 1;
            ApplyPaging();
            puSortOptions.IsOpen = false; // Close popup after sorting
        }

    }
}

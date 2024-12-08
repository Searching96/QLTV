﻿using QLTV.Models;
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
        private int _itemsPerPage = 10;
        private int _currentPage = 1;

        public ObservableCollection<TuaSachViewModel> dsSach => _dsSach;

        public class TuaSachViewModel
        {
            public string TenTuaSach { get; set; } = string.Empty;
            public string BiaSach { get; set; } = string.Empty;
            public string DSTacGia { get; set; } = string.Empty;
            public string DSTheLoai { get; set; } = string.Empty;
        }

        public UUFnDisplaySach()
        {
            InitializeComponent();
            DataContext = this;
            LoadTuaSach();
        }

        private void LoadTuaSach()
        {
            using (var context = new QLTVContext())
            {
                _fullDataSource = context.TUASACH
                    .Where(ts => !ts.IsDeleted)
                    .Select(ts => new TuaSachViewModel
                    {
                        TenTuaSach = ts.TenTuaSach,
                        BiaSach = ts.BiaSach,
                        DSTacGia = string.Join(", ", ts.TUASACH_TACGIA.Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                        DSTheLoai = string.Join(", ", ts.TUASACH_THELOAI.Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai))
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

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {

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
                    .Select(ts => new TuaSachViewModel
                    {
                        TenTuaSach = ts.TenTuaSach,
                        BiaSach = ts.BiaSach,
                        DSTacGia = string.Join(", ", ts.TUASACH_TACGIA.Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                        DSTheLoai = string.Join(", ", ts.TUASACH_THELOAI.Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai))
                    })
                    .AsEnumerable() // Chuyển về IEnumerable để lọc trên máy khách
                    .Where(ts =>
                        selectedProperty == "Tựa Sách" ?
                            NormalizeString(ts.TenTuaSach).Contains(searchTerm) :
                        selectedProperty == "Tác Giả" ?
                            NormalizeString(ts.DSTacGia).Contains(searchTerm) :
                        selectedProperty == "Thể Loại" ?
                            NormalizeString(ts.DSTheLoai).Contains(searchTerm) :
                        true
                    )
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
    }
}
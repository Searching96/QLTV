using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Globalization;
using MaterialDesignThemes.Wpf;
using System.Windows.Data;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using Azure.Core;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Media.Imaging;
using Microsoft.Identity.Client;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static QLTV.AUQuanLyTacGia;
using Microsoft.IdentityModel.Tokens;

namespace QLTV
{
    /// <summary>
    /// Interaction logic for ADUCQuanLySach.xaml
    /// </summary>
    public partial class AUQuanLyTuaSach : UserControl
    {
        public static readonly Thickness DisplayElementMargin = new Thickness(0, 0, 0, 10);
        public static readonly Thickness ErrorIconMargin = new Thickness(0, 0, 5, 10);

        public List<string> lstSelectedMaTuaSach = new List<string>();
        private ObservableCollection<TuaSachViewModel> _dsTuaSach;
        private ObservableCollection<TuaSachViewModel> _fullDataSource; // Nguồn dữ liệu đầy đủ
        private int _currentPage = 1;
        private int _itemsPerPage = 10;
        private int _totalItems = 0;
        private bool _isSearchMode = false; // Cờ để phân biệt chế độ

        public class TuaSachViewModel : INotifyPropertyChanged
        {
            public string MaTuaSach { get; set; }
            public string TenTuaSach { get; set; }
            public int SoLuong { get; set; }
            public int HanMuonToiDa { get; set; }
            public string DSTacGia { get; set; }
            public string DSTheLoai { get; set; }
            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected != value)
                    {
                        _isSelected = value;
                        OnPropertyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public AUQuanLyTuaSach()
        {
            InitializeComponent();
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            _dsTuaSach = new ObservableCollection<TuaSachViewModel>();
            LoadTuaSach(true);
        }

        private void LoadTuaSach(bool isInitialLoad = false)
        {
            using (var context = new QLTVContext())
            {
                // Truy vấn cơ sở dữ liệu gốc
                var data = context.TUASACH
                    .Where(ts => !ts.IsDeleted)
                    .Select(ts => new TuaSachViewModel
                    {
                        MaTuaSach = ts.MaTuaSach,
                        TenTuaSach = ts.TenTuaSach,
                        SoLuong = ts.SoLuong,
                        HanMuonToiDa = ts.HanMuonToiDa,
                        DSTacGia = string.Join(", ", ts.TUASACH_TACGIA
                            .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                        DSTheLoai = string.Join(", ", ts.TUASACH_THELOAI
                            .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai))
                    })
                    .ToList();

                // Initialize _fullDataSource as an ObservableCollection
                _fullDataSource = new ObservableCollection<TuaSachViewModel>(data);

                // Reset trạng thái
                _isSearchMode = false;
                _currentPage = 1;

                // Hủy nút select all
                cbxSelectAll.IsChecked = false;

                // Áp dụng phân trang
                ApplyPaging();
            }
        }

        private void PerformSearch()
        {
            string searchTerm = NormalizeString(tbxThongTinTimKiem.Text.Trim().ToLower());
            string selectedProperty = ((ComboBoxItem)cbbThuocTinhTimKiem.SelectedItem)?.Content.ToString();

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
                        MaTuaSach = ts.MaTuaSach,
                        TenTuaSach = ts.TenTuaSach,
                        SoLuong = ts.SoLuong,
                        HanMuonToiDa = ts.HanMuonToiDa,
                        DSTacGia = string.Join(", ", ts.TUASACH_TACGIA.Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                        DSTheLoai = string.Join(", ", ts.TUASACH_THELOAI.Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai))
                    })
                    .AsEnumerable() // Chuyển về IEnumerable để lọc trên máy khách
                    .Where(ts =>
                        selectedProperty == "Tên Tựa Sách" ?
                            NormalizeString(ts.TenTuaSach).Contains(searchTerm) :
                        selectedProperty == "Tác Giả" ?
                            NormalizeString(ts.DSTacGia).Contains(searchTerm) :
                        selectedProperty == "Thể Loại" ?
                            NormalizeString(ts.DSTheLoai).Contains(searchTerm) :
                        true
                    )
                    .ToList();

                // Initialize _fullDataSource as an ObservableCollection
                _fullDataSource = new ObservableCollection<TuaSachViewModel>(data);

                // Đánh dấu đang ở chế độ tìm kiếm
                _isSearchMode = true;
                _currentPage = 1;

                // Hủy nút select all
                cbxSelectAll.IsChecked = false;

                // Áp dụng phân trang
                ApplyPaging();
            }
        }

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void ApplyPaging()
        {
            if (_fullDataSource == null || _fullDataSource.Count == 0)
            {
                // Không có dữ liệu
                dgTuaSach.ItemsSource = new ObservableCollection<TuaSachViewModel>();
                UpdatePageInfo(0);
                return;
            }

            // Tính toán phân trang
            int totalItems = _fullDataSource.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / _itemsPerPage);

            // Lấy dữ liệu cho trang hiện tại
            var pageData = _fullDataSource
                .Skip((_currentPage - 1) * _itemsPerPage)
                .Take(_itemsPerPage)
                .ToList();

            // Cập nhật ObservableCollection
            _dsTuaSach.Clear();
            foreach (var item in pageData)
            {
                _dsTuaSach.Add(item);
            }

            // Cập nhật DataGrid
            dgTuaSach.ItemsSource = _dsTuaSach;

            // Cập nhật thông tin trang
            UpdatePageInfo(totalItems);
        }

        private void UpdatePageInfo(int totalItems)
        {
            int totalPages = (int)Math.Ceiling((double)totalItems / _itemsPerPage);
            var tbxPageNumber = (TextBlock)FindName("tbxPageNumber");

            if (tbxPageNumber != null)
            {
                tbxPageNumber.Text = totalItems > 0
                    ? $"Trang {_currentPage}/{totalPages} (Tổng: {totalItems} kết quả)"
                    : "Không có kết quả";
            }

            // Kiểm soát trạng thái nút
            var btnPrevious = (Button)FindName("btnPrevious");
            var btnNext = (Button)FindName("btnNext");

            if (btnPrevious != null) btnPrevious.IsEnabled = _currentPage > 1;
            if (btnNext != null) btnNext.IsEnabled = _currentPage < totalPages;
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

        private DataGridRow GetRow(CheckBox cbx)
        {
            var dgRow = VisualTreeHelper.GetParent(cbx) as FrameworkElement;
            while (dgRow != null && !(dgRow is DataGridRow))
                dgRow = VisualTreeHelper.GetParent(dgRow) as FrameworkElement;
            return dgRow as DataGridRow;
        }

        private CheckBox GetCheckBox(FrameworkElement element)
        {
            if (element == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
                if (child is CheckBox checkBox)
                {
                    return checkBox; // Tìm thấy CheckBox
                }
                else
                {
                    var result = GetCheckBox(child); // Đệ quy tìm sâu hơn
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        private void cbxSelectRow_Checked(object sender, RoutedEventArgs e)
        {
            var cbx = sender as CheckBox;
            var row = GetRow(cbx);
            if (row != null)
            {
                var tuaSach = row.Item as TuaSachViewModel;
                tuaSach.IsSelected = true;
            }
        }

        private void cbxSelectRow_Unchecked(object sender, RoutedEventArgs e)
        {
            var cbx = sender as CheckBox;
            var row = GetRow(cbx);
            if (row != null)
            {
                var tuaSach = row.Item as TuaSachViewModel;
                tuaSach.IsSelected = false;
            }
        }

        private void cbxSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            var lstTuaSach = _fullDataSource;
            MessageBox.Show(lstTuaSach.Count.ToString());
            if (lstTuaSach != null)
            {
                foreach (var tuaSach in lstTuaSach)
                {
                    tuaSach.IsSelected = true;
                }
            }
        }

        private void cbxSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            var lstTuaSach = _fullDataSource;
            if (lstTuaSach != null)
            {
                foreach (var tuaSach in lstTuaSach)
                {
                    tuaSach.IsSelected = false;
                }
            }
        }

        private void btnThemTuaSach_Click(object sender, RoutedEventArgs e)
        {
            AWThemTuaSach awThemTuaSach = new AWThemTuaSach();
            if (awThemTuaSach.ShowDialog() == true)
                LoadTuaSach();
        }

        public bool HasError()
        {
            // Tìm tất cả các PackIcon trong UserControl
            foreach (var icon in FindVisualChildren<PackIcon>(this))
            {
                if (icon.Style == FindResource("ErrorIcon") && icon.Visibility == Visibility.Visible)
                {
                    return true;
                }
            }
            return false;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T t)
                    {
                        yield return t;
                    }
                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void btnSuaTuaSach_Click(object sender, RoutedEventArgs e)
        {
            if (dgTuaSach.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn tựa sách cần sửa!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (HasError())
            {
                MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi sửa!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new QLTVContext())
            {
                // Lấy MaTuaSach từ item được chọn
                dynamic selectedItem = dgTuaSach.SelectedItem;
                string maTuaSach = selectedItem.MaTuaSach;

                // Tìm tựa sách cần sửa
                var tuaSachToUpdate = context.TUASACH
                    .Include(ts => ts.TUASACH_TACGIA)
                    .Include(ts => ts.TUASACH_THELOAI)
                    .FirstOrDefault(ts => ts.MaTuaSach == maTuaSach);

                if (tuaSachToUpdate != null)
                {
                    // Cập nhật thông tin cơ bản
                    tuaSachToUpdate.TenTuaSach = tbxTenTuaSach.Text;
                    tuaSachToUpdate.SoLuong = int.Parse(tbxSoLuong.Text);
                    tuaSachToUpdate.HanMuonToiDa = int.Parse(tbxHanMuonToiDa.Text.Replace(" tuần", ""));

                    // Cập nhật quan hệ với Tác giả
                    // Xóa các quan hệ cũ
                    context.TUASACH_TACGIA.RemoveRange(tuaSachToUpdate.TUASACH_TACGIA);

                    // Thêm quan hệ mới từ danh sách tác giả đã chọn
                    var selectedAuthors = ParseDSTacGia(tbxDSTacGia.Text);
                    foreach (var author in selectedAuthors)
                    {
                        tuaSachToUpdate.TUASACH_TACGIA.Add(new TUASACH_TACGIA
                        {
                            IDTuaSach = tuaSachToUpdate.ID,
                            IDTacGia = author.ID
                        });
                    }

                    // Cập nhật quan hệ với Thể loại
                    // Xóa các quan hệ cũ
                    context.TUASACH_THELOAI.RemoveRange(tuaSachToUpdate.TUASACH_THELOAI);

                    // Thêm quan hệ mới từ danh sách thể loại
                    var selectedCategories = ParseDSTheLoai(tbxDSTheLoai.Text);
                    foreach (var category in selectedCategories)
                    {
                        tuaSachToUpdate.TUASACH_THELOAI.Add(new TUASACH_THELOAI
                        {
                            IDTuaSach = tuaSachToUpdate.ID,
                            IDTheLoai = category.ID
                        });
                    }

                    // Lưu tất cả thay đổi
                    context.SaveChanges();

                    MessageBox.Show("Cập nhật tựa sách thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh lại DataGrid
                    LoadTuaSach();
                }
            }
        }

        private void dgTuaSach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = dgTuaSach.SelectedItem;

            if (selectedItem != null)
            {
                dynamic selectedBook = selectedItem;
                tbxMaTuaSach.Text = selectedBook.MaTuaSach;
                tbxTenTuaSach.Text = selectedBook.TenTuaSach;
                tbxDSTacGia.Text = selectedBook.DSTacGia;
                tbxDSTheLoai.Text = selectedBook.DSTheLoai;
                tbxSoLuong.Text = selectedBook.SoLuong.ToString();
                tbxHanMuonToiDa.Text = selectedBook.HanMuonToiDa.ToString();

                string maTuaSach = selectedBook.MaTuaSach;

                using (var context = new QLTVContext())
                {
                    var tuaSach = context.TUASACH
                        .Where(ts => ts.MaTuaSach == maTuaSach)
                        .FirstOrDefault();

                    try
                    {
                        Uri uri = new Uri(tuaSach.BiaSach, UriKind.Absolute);

                        if (Uri.IsWellFormedUriString(uri.ToString(), UriKind.Absolute))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = uri;
                            bitmap.EndInit();

                            imgBiaSach.Source = bitmap;
                            bdBiaSach.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            imgBiaSach.Source = null; // Nếu Uri không hợp lệ, không gán hình ảnh
                        }
                    }
                    catch (UriFormatException)
                    {
                        imgBiaSach.Source = null; // Nếu có lỗi trong việc tạo Uri, không gán hình ảnh
                    }
                }
            }
            else
            {
                tbxMaTuaSach.Text = "";
                tbxTenTuaSach.Text = "";
                tbxDSTacGia.Text = "";
                tbxDSTheLoai.Text = "";
                tbxSoLuong.Text = "";
                tbxHanMuonToiDa.Text = "";
                imgBiaSach.Source = null;
            }
        }

        private void btnSuaTacGia_Click(object sender, RoutedEventArgs e)
        {
            // Chuyển DSTacGia trong TextBox thành List<TacGia>
            var currentAuthors = ParseDSTacGia(tbxDSTacGia.Text);

            // Lấy danh sách tất cả các tác giả từ cơ sở dữ liệu
            List<TACGIA> allAuthors;
            using (var context = new QLTVContext())
            {
                allAuthors = context.TACGIA
                    .Where(tg => !tg.IsDeleted)
                    .ToList();
            }

            // Mở cửa sổ WDChonTacGia
            var wdChonTacGia = new AWChonTacGia(allAuthors, currentAuthors);

            if (wdChonTacGia.ShowDialog() == true)
            {
                // Cập nhật DSTacGia từ danh sách tác giả mới
                var newSelectedAuthors = wdChonTacGia.SelectedAuthors;
                tbxDSTacGia.Text = string.Join(", ", newSelectedAuthors.Select(a => a.TenTacGia));
            }
        }

        private List<TACGIA> ParseDSTacGia(string DSTacGia)
        {
            if (string.IsNullOrWhiteSpace(DSTacGia)) return new List<TACGIA>();

            // Tách DSTacGia thành các tên tác giả dựa vào dấu phẩy
            var lstTenTacGia = DSTacGia.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(tg => tg.Trim())
                .ToList();

            // Lấy danh sách từ cơ sở dữ liệu khớp với tên
            using (var context = new QLTVContext())
            {
                return context.TACGIA.Where(tg => lstTenTacGia
                    .Contains(tg.TenTacGia)).ToList();
            }
        }

        private void btnSuaTheLoai_Click(object sender, RoutedEventArgs e)
        {
            // Chuyển DSTacGia trong TextBox thành List<TacGia>
            var currentCategories = ParseDSTheLoai(tbxDSTheLoai.Text);

            // Lấy danh sách tất cả các tác giả từ cơ sở dữ liệu
            List<THELOAI> allCategories;
            using (var context = new QLTVContext())
            {
                allCategories = context.THELOAI
                    .Where(tl => !tl.IsDeleted)
                    .ToList();
            }

            // Mở cửa sổ WDChonTacGia
            var awChonTheLoai = new AWChonTheLoai(allCategories, currentCategories);

            if (awChonTheLoai.ShowDialog() == true)
            {
                // Cập nhật DSTacGia từ danh sách tác giả mới
                var newSelectedCategories = awChonTheLoai.SelectedCategories;
                tbxDSTheLoai.Text = string.Join(", ", newSelectedCategories.Select(c => c.TenTheLoai));
            }
        }

        private List<THELOAI> ParseDSTheLoai(string DSTheLoai)
        {
            if (string.IsNullOrWhiteSpace(DSTheLoai)) return new List<THELOAI>();

            var lstTenTheLoai = DSTheLoai.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(tl => tl.Trim())
                .ToList();

            using (var context = new QLTVContext())
            {
                return context.THELOAI.Where(tl => lstTenTheLoai
                              .Contains(tl.TenTheLoai)).ToList();
            }
        }

        private void btnXoaTuaSach_Click(object sender, RoutedEventArgs e)
        {
            if (dgTuaSach.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn tựa sách cần xóa!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selectedItem = dgTuaSach.SelectedItem;
            string maTuaSach = selectedItem.MaTuaSach;

            MessageBoxResult mbrXacNhan = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa tựa sách có mã: {maTuaSach}?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (mbrXacNhan == MessageBoxResult.Yes)
            {
                using (var context = new QLTVContext())
                {
                    var tuaSachToDelete = context.TUASACH
                        .Include(ts => ts.TUASACH_TACGIA)
                        .Include(ts => ts.TUASACH_THELOAI)
                        .FirstOrDefault(tg => tg.MaTuaSach == maTuaSach);

                    // Truong hop bat dong bo?
                    if (tuaSachToDelete != null)
                    {
                        var lstSachToDelete = context.SACH
                            .Where(s => s.IDTuaSach == tuaSachToDelete.ID)
                            .ToList();

                        foreach (var sach in lstSachToDelete)
                            sach.IsDeleted = true;

                        context.TUASACH_TACGIA.RemoveRange(tuaSachToDelete.TUASACH_TACGIA);
                        context.TUASACH_THELOAI.RemoveRange(tuaSachToDelete.TUASACH_THELOAI);
                        tuaSachToDelete.IsDeleted = true;
                        context.SaveChanges();
                        MessageBox.Show($"Tựa sách có mã {maTuaSach} đã được xóa.", "Thông báo",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTuaSach(); 
                    }
                }
            }
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            icTenTuaSachError.Visibility = Visibility.Collapsed;
            icDSTacGiaError.Visibility = Visibility.Collapsed;
            icDSTheLoaiError.Visibility = Visibility.Collapsed;
            icHanMuonToiDaError.Visibility = Visibility.Collapsed;
            LoadTuaSach();
        }

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportDataGridToExcel();
        }

        private void ExportDataGridToExcel()
        {
            // Cấu hình đường dẫn lưu file Excel
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Lưu file Excel",
                FileName = "DanhSachTuaSach.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var filePath = saveFileDialog.FileName;

                // Tạo file Excel mới
                using (ExcelPackage package = new ExcelPackage())
                {
                    // Tạo một sheet mới
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Danh Sách Tựa Sách");

                    // Đặt tiêu đề cho các cột trong Excel
                    worksheet.Cells[1, 1].Value = "Mã Tựa Sách";
                    worksheet.Cells[1, 2].Value = "Tên Tựa Sách";
                    worksheet.Cells[1, 3].Value = "Tác Giả";
                    worksheet.Cells[1, 4].Value = "Thể Loại";
                    worksheet.Cells[1, 5].Value = "Số Lượng";
                    worksheet.Cells[1, 6].Value = "Hạn Mượn Tối Đa (Tuần)";

                    // Áp dụng style cho tiêu đề
                    using (var range = worksheet.Cells[1, 1, 1, 6])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Duyệt qua dữ liệu trong DataGrid và ghi vào Excel
                    var items = _fullDataSource as System.Collections.IEnumerable;
                    int rowIndex = 2;

                    foreach (var item in items)
                    {
                        dynamic data = item;
                        worksheet.Cells[rowIndex, 1].Value = data.MaTuaSach;
                        worksheet.Cells[rowIndex, 2].Value = data.TenTuaSach;
                        worksheet.Cells[rowIndex, 3].Value = data.DSTacGia;
                        worksheet.Cells[rowIndex, 4].Value = data.DSTheLoai;
                        worksheet.Cells[rowIndex, 5].Value = data.SoLuong;
                        worksheet.Cells[rowIndex, 6].Value = data.HanMuonToiDa;
                        rowIndex++;
                    }

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Cells.AutoFitColumns();

                    // Lưu file Excel
                    FileInfo excelFile = new FileInfo(filePath);
                    package.SaveAs(excelFile);
                }

                MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private int ImportExcelToDb(string filePath)
        {
            int thanhCong = 0;
            HashSet<int> lstDongBiLoi = new HashSet<int>();
            var context = new QLTVContext();
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) return -1;

                var existingAuthors = context.TACGIA.ToDictionary(tg => tg.TenTacGia);
                var existingCategories = context.THELOAI.ToDictionary(tl => tl.TenTheLoai);

                // Tìm các dòng bị lỗi và cho vào lst
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    string tenTuaSach = worksheet.Cells[row, 1].Text;
                    string hanMuonToiDaText = worksheet.Cells[row, 4].Text;
                    if (string.IsNullOrWhiteSpace(tenTuaSach) || !int.TryParse(hanMuonToiDaText, out int hanMuonToiDa))
                        lstDongBiLoi.Add(row);
                }

                MessageBoxResult mbrXacNhan = MessageBox.Show(
                    $"Có {lstDongBiLoi.Count} dòng bị lỗi. Bạn có muốn tiếp tục nhập dữ liệu bỏ qua các dòng đó không?",
                    "Xác nhận nhập",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (mbrXacNhan != MessageBoxResult.Yes)
                    return -1;

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++) 
                {
                    if (lstDongBiLoi.Contains(row))
                        continue;

                    string tenTuaSach = worksheet.Cells[row, 1].Text;
                    int hanMuonToiDa = int.Parse(worksheet.Cells[row, 4].Text);

                    var newTuaSach = new TUASACH
                    {
                        TenTuaSach = tenTuaSach,
                        HanMuonToiDa = hanMuonToiDa
                    };
                    context.TUASACH.Add(newTuaSach);
                    context.SaveChanges();

                    var lstTenTacGia = worksheet.Cells[row, 2].Text.Split(", ").Select(n => n.Trim()).ToList();
                    var lstTenTheLoai = worksheet.Cells[row, 3].Text.Split(", ").Select(n => n.Trim()).ToList();

                    // Add authors
                    foreach (var tenTacGia in lstTenTacGia)
                    {
                        if (!existingAuthors.TryGetValue(tenTacGia, out var tacGia))
                        {
                            tacGia = new TACGIA { TenTacGia = tenTacGia, NamSinh = -1, QuocTich = "Chưa Có" };
                            context.TACGIA.Add(tacGia);
                            context.SaveChanges();
                            existingAuthors[tenTacGia] = tacGia;
                        }
                        context.TUASACH_TACGIA.Add(new TUASACH_TACGIA { IDTuaSach = newTuaSach.ID, IDTacGia = tacGia.ID });
                    }

                    // Add categories
                    foreach (var tenTheLoai in lstTenTheLoai)
                    {
                        if (!existingCategories.TryGetValue(tenTheLoai, out var theLoai))
                        {
                            theLoai = new THELOAI { TenTheLoai = tenTheLoai };
                            context.THELOAI.Add(theLoai);
                            context.SaveChanges();
                            existingCategories[tenTheLoai] = theLoai;
                        }
                        context.TUASACH_THELOAI.Add(new TUASACH_THELOAI { IDTuaSach = newTuaSach.ID, IDTheLoai = theLoai.ID });
                    }
                    thanhCong++;
                }

                // Save all changes at the end for efficiency
                context.SaveChanges();
            }

            return thanhCong;
        }

        private void btnImportExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                int thanhCong = ImportExcelToDb(openFileDialog.FileName);
                if (thanhCong != -1)
                {
                    MessageBox.Show($"Nhập thành công {thanhCong} dòng từ Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTuaSach();
                }
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

        private void tbxTenTuaSach_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgTuaSach.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxTenTuaSach.Text))
            {
                icTenTuaSachError.ToolTip = "Tên Tựa Sách không được để trống!";
                icTenTuaSachError.Visibility = Visibility.Visible;
                return;
            }

            icTenTuaSachError.Visibility = Visibility.Collapsed;
        }

        private void tbxHanMuonToiDa_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgTuaSach.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxHanMuonToiDa.Text))
            {
                icHanMuonToiDaError.ToolTip = "Hạn Mượn Tối Đa không được để trống";
                icHanMuonToiDaError.Visibility = Visibility.Visible;
                return;
            }

            if (!int.TryParse(tbxHanMuonToiDa.Text, out int hmtd) || hmtd <= 0 || hmtd > 16)
            {
                icHanMuonToiDaError.ToolTip = "Hạn Mượn Tối Đa phải là số nguyên dương không quá 16";
                icHanMuonToiDaError.Visibility = Visibility.Visible;
                return;
            }

            icHanMuonToiDaError.Visibility = Visibility.Collapsed;
        }

        private void tbxDSTheLoai_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgTuaSach.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxDSTheLoai.Text))
            {
                icDSTheLoaiError.ToolTip = "Thể Loại không được để trống";
                icDSTheLoaiError.Visibility = Visibility.Visible;
                return;
            }

            icDSTheLoaiError.Visibility = Visibility.Collapsed;
        }

        private void tbxDSTacGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgTuaSach.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxDSTacGia.Text))
            {
                icDSTacGiaError.ToolTip = "Tác Giả không được để trống";
                icDSTacGiaError.Visibility = Visibility.Visible;
                return;
            }

            icDSTacGiaError.Visibility = Visibility.Collapsed;
        }

        private async void btnSuaBiaSach_Click(object sender, RoutedEventArgs e)
        {
            dynamic tuaSach = dgTuaSach.SelectedItem;
            if (tuaSach == null)
            {
                MessageBox.Show("Hãy chọn tựa sách muốn sửa bìa.", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string maTuaSach = string.Empty;
            if (tuaSach != null) 
                maTuaSach = tuaSach.MaTuaSach;

            using (var context = new QLTVContext())
            {
                var TuaSach = context.TUASACH
                    .Where(ts => ts.MaTuaSach == maTuaSach)
                    .FirstOrDefault();

                AWSuaBiaSach awSuaBiaSach = new AWSuaBiaSach(TuaSach);
                if (awSuaBiaSach.ShowDialog() == true)
                {
                    LoadTuaSach();

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(awSuaBiaSach.TuaSach.BiaSach, UriKind.Absolute);
                    bitmap.EndInit();
                    imgBiaSach.Source = bitmap;
                }
            }
        }

        private void btnXoaChon_Click(object sender, RoutedEventArgs e)
        {
            lstSelectedMaTuaSach = _fullDataSource
                .Where(ts => ts.IsSelected)
                .Select(ts => ts.MaTuaSach)
                .ToList();

            if (lstSelectedMaTuaSach.IsNullOrEmpty())
            {
                MessageBox.Show("Chưa chọn tựa sách để xóa.", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult mbrXacNhan = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa {lstSelectedMaTuaSach.Count} tựa sách đã chọn?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (mbrXacNhan == MessageBoxResult.Yes)
            {
                using (var context = new QLTVContext())
                {
                    using (var transaction = context.Database.BeginTransaction()) // Đảm bảo đồng bộ các thao tác
                    {
                        try
                        {
                            foreach (var maTuaSach in lstSelectedMaTuaSach)
                            {
                                var tuaSachToDelete = context.TUASACH
                                    .Include(ts => ts.TUASACH_TACGIA)
                                    .Include(ts => ts.TUASACH_THELOAI)
                                    .FirstOrDefault(tg => tg.MaTuaSach == maTuaSach);

                                if (tuaSachToDelete != null)
                                {
                                    // Cập nhật các sách liên quan
                                    var lstSachToDelete = context.SACH
                                        .Where(s => s.IDTuaSach == tuaSachToDelete.ID)
                                        .ToList();

                                    foreach (var sach in lstSachToDelete)
                                        sach.IsDeleted = true;

                                    // Xóa mối quan hệ giữa TuaSach và TacGia/ TheLoai
                                    context.TUASACH_TACGIA.RemoveRange(tuaSachToDelete.TUASACH_TACGIA);
                                    context.TUASACH_THELOAI.RemoveRange(tuaSachToDelete.TUASACH_THELOAI);

                                    // Đánh dấu TuaSach là đã xóa
                                    tuaSachToDelete.IsDeleted = true;
                                }
                            }

                            // Lưu tất cả thay đổi trong một giao dịch
                            context.SaveChanges();
                            transaction.Commit(); // Commit giao dịch

                            MessageBox.Show($"Đã xóa thành công các tựa sách được chọn.", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                            LoadTuaSach();
                            tbxThongTinTimKiem.Text = "";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback(); // Rollback giao dịch nếu có lỗi
                            MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }
}

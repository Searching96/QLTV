using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Xps;
using static QLTV.AUQuanLyTuaSach;

namespace QLTV
{
    /// <summary>
    /// Interaction logic for AUQuanLyTacGia.xaml
    /// </summary>
    public partial class AUQuanLyTacGia : UserControl
    {
        public List<string> lstSelectedMaTacGia = new List<string>();
        private ObservableCollection<TacGiaViewModel> _dsTacGia;
        private ObservableCollection<TacGiaViewModel> _fullDataSource; // Nguồn dữ liệu đầy đủ
        private int _currentPage = 1;
        private int _itemsPerPage = 12;
        private int _totalItems = 0;
        private bool _isSearchMode = false; // Cờ để phân biệt chế độ

        public class TacGiaViewModel : INotifyPropertyChanged
        {
            public string MaTacGia { get; set; }
            public string TenTacGia { get; set; }
            public int NamSinh { get; set; }
            public string QuocTich { get; set; }
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

        public AUQuanLyTacGia()
        {
            InitializeComponent();
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            _dsTacGia = new ObservableCollection<TacGiaViewModel>();
            LoadTacGia();
        }

        private void LoadTacGia()
        {
            using (var context = new QLTVContext())
            {
                var data = context.TACGIA
                    .Where(tg => !tg.IsDeleted)
                    .Select(tg => new TacGiaViewModel
                    {
                        MaTacGia = tg.MaTacGia,
                        TenTacGia = tg.TenTacGia,
                        NamSinh = tg.NamSinh,
                        QuocTich = tg.QuocTich,
                    })
                    .ToList();

                _fullDataSource = new ObservableCollection<TacGiaViewModel>(data);

                _isSearchMode = false;
                _currentPage = 1;

                cbxSelectAll.IsChecked = false;

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
                var data = context.TACGIA
                    .Where(tg => !tg.IsDeleted)
                    .Select(tg => new TacGiaViewModel
                    {
                        MaTacGia = tg.MaTacGia,
                        TenTacGia = tg.TenTacGia,
                        NamSinh = tg.NamSinh,
                        QuocTich = tg.QuocTich
                    })
                    .AsEnumerable() // Chuyển về IEnumerable để lọc trên máy khách
                    .Where(tg =>
                        selectedProperty == "Tên Tác Giả" ?
                            NormalizeString(tg.TenTacGia).Contains(searchTerm) :
                        selectedProperty == "Năm Sinh" ?
                            (int.TryParse(searchTerm, out int searchYear) && tg.NamSinh == searchYear) :
                        selectedProperty == "Quốc Tịch" ?
                            NormalizeString(tg.QuocTich).Contains(searchTerm) :
                        true
                    )
                    .ToList();

                // Initialize _fullDataSource as an ObservableCollection
                _fullDataSource = new ObservableCollection<TacGiaViewModel>(data);

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
                _dsTacGia.Clear();
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
            _dsTacGia.Clear();
            foreach (var item in pageData)
            {
                _dsTacGia.Add(item);
            }

            // Cập nhật DataGrid
            dgTacGia.ItemsSource = _dsTacGia;

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
                var tacGia = row.Item as TacGiaViewModel;
                tacGia.IsSelected = true;
            }
        }

        private void cbxSelectRow_Unchecked(object sender, RoutedEventArgs e)
        {
            var cbx = sender as CheckBox;
            var row = GetRow(cbx);
            if (row != null)
            {
                var tacGia = row.Item as TacGiaViewModel;
                tacGia.IsSelected = false;
            }
        }

        private void cbxSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            var lstTacGia = _fullDataSource;
            MessageBox.Show(lstTacGia.Count.ToString());
            if (lstTacGia != null)
            {
                foreach (var tacGia in lstTacGia)
                {
                    tacGia.IsSelected = true;
                }
            }
        }

        private void cbxSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            var lstTacGia = _fullDataSource;
            if (lstTacGia != null)
            {
                foreach (var tacGia in lstTacGia)
                {
                    tacGia.IsSelected = false;
                }
            }
        }

        private void btnThemTacGia_Click(object sender, RoutedEventArgs e)
        {
            AWThemTacGia awThemTacGia = new AWThemTacGia();
            if (awThemTacGia.ShowDialog() == true)
                LoadTacGia();
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

        private void btnSuaTacGia_Click(object sender, RoutedEventArgs e)
        {
            if (dgTacGia.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn tác giả cần sửa!", "Thông báo",
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
                dynamic selectedItem = dgTacGia.SelectedItem;
                string maTacGia = selectedItem.MaTacGia;

                // Tìm tựa sách cần sửa
                var tacGiaToUpdate = context.TACGIA
                                            .FirstOrDefault(tg => tg.MaTacGia == maTacGia);

                if (tacGiaToUpdate != null)
                {
                    // Cập nhật thông tin cơ bản
                    tacGiaToUpdate.TenTacGia = tbxTenTacGia.Text;
                    tacGiaToUpdate.NamSinh = int.Parse(tbxNamSinh.Text);
                    tacGiaToUpdate.QuocTich = tbxQuocTich.Text;

                    // Lưu tất cả thay đổi
                    context.SaveChanges();

                    MessageBox.Show("Cập nhật tác giả thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh lại DataGrid
                    LoadTacGia();
                }
            }
        }

        private void dgTacGia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = dgTacGia.SelectedItem;

            if (selectedItem != null)
            {
                dynamic selectedAuthor = selectedItem;
                tbxMaTacGia.Text = selectedAuthor.MaTacGia;
                tbxTenTacGia.Text = selectedAuthor.TenTacGia;
                tbxNamSinh.Text = selectedAuthor.NamSinh.ToString();
                tbxQuocTich.Text = selectedAuthor.QuocTich;
            }
            else
            {
                tbxMaTacGia.Text = "";
                tbxTenTacGia.Text = "";
                tbxNamSinh.Text = "";
                tbxQuocTich.Text = "";
            }
        }

        private void btnXoaTacGia_Click(object sender, RoutedEventArgs e)
        {
            if (dgTacGia.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn tác giả cần xóa!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selectedItem = dgTacGia.SelectedItem;
            string maTacGia = selectedItem.MaTacGia;

            MessageBoxResult mbrXacNhan = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa tác giả có mã: {maTacGia}?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (mbrXacNhan == MessageBoxResult.Yes)
            {
                using (var context = new QLTVContext())
                {
                    var tacGiaToDelete = context.TACGIA
                        .Include(tg => tg.TUASACH_TACGIA)
                        .FirstOrDefault(tg => tg.MaTacGia == maTacGia);

                    // Truong hop bat dong bo?
                    if (tacGiaToDelete != null)
                    {
                        context.TUASACH_TACGIA.RemoveRange(tacGiaToDelete.TUASACH_TACGIA);
                        tacGiaToDelete.IsDeleted = true;
                        context.SaveChanges();
                        MessageBox.Show($"Tác giả có mã {maTacGia} đã được xóa.", "Thông báo", 
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTacGia();
                    }
                }
            }
        }
        
        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            LoadTacGia();
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

        private void ExportDataGridToExcel()
        {
            // Cấu hình đường dẫn lưu file Excel
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Lưu file Excel",
                FileName = "DanhSachTacGia.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var filePath = saveFileDialog.FileName;

                // Tạo file Excel mới
                using (ExcelPackage package = new ExcelPackage())
                {
                    // Tạo một sheet mới
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Danh Sách Tác Giả");

                    // Đặt tiêu đề cho các cột trong Excel
                    worksheet.Cells[1, 1].Value = "Mã Tác Giả";
                    worksheet.Cells[1, 2].Value = "Tên Tác Giả";
                    worksheet.Cells[1, 3].Value = "Năm Sinh";
                    worksheet.Cells[1, 4].Value = "Quốc Tịch";

                    // Áp dụng style cho tiêu đề
                    using (var range = worksheet.Cells[1, 1, 1, 6])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Duyệt qua dữ liệu trong DataGrid và ghi vào Excel
                    var items = dgTacGia.ItemsSource as System.Collections.IEnumerable;
                    int rowIndex = 2;

                    foreach (var item in items)
                    {
                        dynamic data = item;
                        worksheet.Cells[rowIndex, 1].Value = data.MaTacGia;
                        worksheet.Cells[rowIndex, 2].Value = data.TenTacGia;
                        worksheet.Cells[rowIndex, 3].Value = data.NamSinh;
                        worksheet.Cells[rowIndex, 4].Value = data.QuocTich;
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

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportDataGridToExcel();
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

                // Tìm các dòng bị lỗi và cho vào lst
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    string tenTacGia = worksheet.Cells[row, 1].Text;
                    string namSinh = worksheet.Cells[row, 2].Text;
                    if (string.IsNullOrWhiteSpace(tenTacGia) || !int.TryParse(namSinh, out int ns))
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

                    string tenTacGia = worksheet.Cells[row, 1].Text;
                    int namSinh = int.Parse(worksheet.Cells[row, 2].Text);
                    string quocTich = worksheet.Cells[row, 3].Text;

                    var checkTacGia = context.TACGIA
                        .Where(tg => tg.TenTacGia == tenTacGia && tg.NamSinh == namSinh)
                        .FirstOrDefault();

                    if (checkTacGia != null)
                        continue;

                    var newTacGia = new TACGIA
                    {
                        TenTacGia = tenTacGia,
                        NamSinh = namSinh,
                        QuocTich = quocTich
                    };
                    context.TACGIA.Add(newTacGia);
                    context.SaveChanges();

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
                    LoadTacGia();
                }
            }
        }

        private void tbxTenTacGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgTacGia.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxTenTacGia.Text))
            {
                icTenTacGiaError.ToolTip = "Tên Tác Giả không được để trống";
                icTenTacGiaError.Visibility = Visibility.Visible;
                return;
            }

            foreach (char c in tbxTenTacGia.Text)
            {
                if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
                {
                    icTenTacGiaError.ToolTip = "Tên Tác Giả không được có số hay kí tự đặc biệt";
                    icTenTacGiaError.Visibility = Visibility.Visible;
                    return;
                }
            }

            icTenTacGiaError.Visibility = Visibility.Collapsed;
        }

        private void tbxNamSinh_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgTacGia.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxNamSinh.Text))
            {
                icNamSinhError.ToolTip = "Năm Sinh không được để trống";
                icNamSinhError.Visibility = Visibility.Visible;
                return;
            }

            if (!int.TryParse(tbxNamSinh.Text, out int ns) || ns <= 0)
            {
                icNamSinhError.ToolTip = "Năm Sinh phải là số nguyên dương";
                icNamSinhError.Visibility = Visibility.Visible;
                return;
            }

            icNamSinhError.Visibility = Visibility.Collapsed;
        }

        private void tbxQuocTich_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgTacGia.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxQuocTich.Text))
            {
                icQuocTichError.ToolTip = "Quốc Tịch không được để trống";
                icQuocTichError.Visibility = Visibility.Visible;
                return;
            }

            foreach (char c in tbxQuocTich.Text)
            {
                if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
                {
                    icQuocTichError.ToolTip = "Quốc Tịch không được có số hay kí tự đặc biệt";
                    icQuocTichError.Visibility = Visibility.Visible;
                    return;
                }
            }

            icQuocTichError.Visibility = Visibility.Collapsed;
        }

        private void btnSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            var lstTacGia = dgTacGia.ItemsSource as List<TacGiaViewModel>;
            if (lstTacGia != null)
            {
                foreach (var tacGia in lstTacGia)
                {
                    tacGia.IsSelected = true;
                }
                // Refresh DataGrid để cập nhật giao diện => đã dùng OnPropChange
            }
        }
        private void btnSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            var lstTacGia = dgTacGia.ItemsSource as List<TacGiaViewModel>;
            if (lstTacGia != null)
            {
                foreach (var tacGia in lstTacGia)
                {
                    tacGia.IsSelected = false;
                }
                // Refresh DataGrid để cập nhật giao diện => đã dùng OnPropChange
            }
        }

        private void btnXoaChon_Click(object sender, RoutedEventArgs e)
        {
            // Lấy danh sách các tác giả được chọn
            lstSelectedMaTacGia = _fullDataSource
                .Where(tg => tg.IsSelected)
                .Select(tg => tg.MaTacGia)
                .ToList();

            if (lstSelectedMaTacGia.IsNullOrEmpty())
            {
                MessageBox.Show("Chưa chọn tác giả để xóa.", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult mbrXacNhan = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa {lstSelectedMaTacGia.Count} tác giả đã chọn?",
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
                            foreach (var maTacGia in lstSelectedMaTacGia)
                            {
                                var tacGiaToDelete = context.TACGIA
                                    .Include(ts => ts.TUASACH_TACGIA)
                                    .FirstOrDefault(tg => tg.MaTacGia == maTacGia);

                                if (tacGiaToDelete != null)
                                {
                                    context.TUASACH_TACGIA.RemoveRange(tacGiaToDelete.TUASACH_TACGIA);
                                    tacGiaToDelete.IsDeleted = true;
                                }
                            }

                            // Lưu tất cả thay đổi trong một giao dịch
                            context.SaveChanges();
                            transaction.Commit(); // Commit giao dịch

                            MessageBox.Show($"Đã xóa thành công các tác giả được chọn.", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                            LoadTacGia();
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

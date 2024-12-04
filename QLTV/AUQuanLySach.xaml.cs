using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
using Microsoft.Win32;
using OfficeOpenXml.Drawing.Style.Coloring;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using System.Globalization;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;

namespace QLTV
{
    /// <summary>
    /// Interaction logic for AUQuanLySach.xaml
    /// </summary>
    public partial class AUQuanLySach : UserControl
    {
        private bool isUpdatingText = false;
        private List<SachViewModel> _fullDataSource;
        private ObservableCollection<SachViewModel> _dsSach;
        private int _currentPage = 1;
        private int _itemsPerPage = 10;
        private bool _isSearchMode = false;

        public class SachViewModel
        {
            public string MaSach { get; set; }
            public string TuaSach { get; set; }
            public string DSTacGia { get; set; }
            public string DSTheLoai { get; set; }
            public string NhaXuatBan { get; set; }
            public int NamXuatBan { get; set; }
            public string NgayNhap { get; set; }
            public decimal TriGia { get; set; }
            public string TinhTrang { get; set; }
        }

        public AUQuanLySach()
        {
            InitializeComponent();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _dsSach = new ObservableCollection<SachViewModel>();
            LoadSach();
        }

        private void LoadSach(bool isInitialLoad = false)
        {
            using (var context = new QLTVContext())
            {
                _fullDataSource = context.SACH
                    .Where(s => !s.IsDeleted && !s.IDTuaSachNavigation.IsDeleted)
                    .Select(s => new SachViewModel
                    {
                        MaSach = s.MaSach,
                        TuaSach = s.IDTuaSachNavigation.TenTuaSach,
                        DSTacGia = string.Join(", ", s.IDTuaSachNavigation.TUASACH_TACGIA
                            .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                        DSTheLoai = string.Join(", ", s.IDTuaSachNavigation.TUASACH_THELOAI
                            .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai)),
                        NhaXuatBan = s.NhaXuatBan,
                        NamXuatBan = s.NamXuatBan,
                        NgayNhap = s.NgayNhap.ToString("dd/MM/yyyy"),
                        TriGia = s.TriGia,
                        TinhTrang = s.IDTinhTrangNavigation.TenTinhTrang
                    })
                    .ToList();

                _isSearchMode = false;
                _currentPage = 1;
                ApplyPaging();
            }
        }

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = NormalizeString(tbxThongTinTimKiem.Text.Trim().ToLower());
            string selectedProperty = ((ComboBoxItem)cbbThuocTinhTimKiem.SelectedItem)?.Content.ToString();

            if (string.IsNullOrEmpty(selectedProperty))
            {
                MessageBox.Show("Vui lòng chọn thuộc tính tìm kiếm", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new QLTVContext())
            {
                _fullDataSource = context.SACH
                    .Where(s => !s.IsDeleted && !s.IDTuaSachNavigation.IsDeleted)
                    .Select(s => new SachViewModel
                    {
                        MaSach = s.MaSach,
                        TuaSach = s.IDTuaSachNavigation.TenTuaSach,
                        DSTacGia = string.Join(", ", s.IDTuaSachNavigation.TUASACH_TACGIA
                            .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                        DSTheLoai = string.Join(", ", s.IDTuaSachNavigation.TUASACH_THELOAI
                            .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai)),
                        NhaXuatBan = s.NhaXuatBan,
                        NamXuatBan = s.NamXuatBan,
                        NgayNhap = s.NgayNhap.ToString("dd/MM/yyyy"),
                        TriGia = s.TriGia,
                        TinhTrang = s.IDTinhTrangNavigation.TenTinhTrang
                    })
                    .AsEnumerable()
                    .Where(s =>
                        selectedProperty == "Tựa Sách" ? NormalizeString(s.TuaSach).Contains(searchTerm) :
                        selectedProperty == "Tác Giả" ? NormalizeString(s.DSTacGia).Contains(searchTerm) :
                        selectedProperty == "Thể Loại" ? NormalizeString(s.DSTheLoai).Contains(searchTerm) :
                        selectedProperty == "Nhà Xuất Bản" ? NormalizeString(s.NhaXuatBan).Contains(searchTerm) :
                        selectedProperty == "Tình Trạng" ? NormalizeString(s.TinhTrang).Contains(searchTerm) :
                        true
                    )
                    .ToList();

                _isSearchMode = true;
                _currentPage = 1;
                ApplyPaging();
            }
        }

        private void ApplyPaging()
        {
            if (_fullDataSource == null || _fullDataSource.Count == 0)
            {
                dgSach.ItemsSource = new ObservableCollection<dynamic>();
                UpdatePageInfo(0);
                return;
            }

            int totalItems = _fullDataSource.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / _itemsPerPage);

            var pageData = _fullDataSource
                .Skip((_currentPage - 1) * _itemsPerPage)
                .Take(_itemsPerPage)
                .ToList();

            _dsSach.Clear();
            foreach (var item in pageData)
            {
                _dsSach.Add(item);
            }

            dgSach.ItemsSource = _dsSach;
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

        private void dgSach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = dgSach.SelectedItem;

            if (selectedItem != null)
            {
                dynamic selectedBook = selectedItem;
                tbxMaSach.Text = selectedBook.MaSach;
                tbxTuaSach.Text = selectedBook.TuaSach;
                tbxDSTacGia.Text = selectedBook.DSTacGia;
                tbxDSTheLoai.Text = selectedBook.DSTheLoai;
                tbxNhaXuatBan.Text = selectedBook.NhaXuatBan;
                tbxNamXuatBan.Text = selectedBook.NamXuatBan.ToString();

                DateTime parsedDate;
                bool isDateParsed = DateTime.TryParseExact(
                    selectedBook.NgayNhap,
                    "dd/MM/yyyy",  // Định dạng dmy mong muốn
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out parsedDate
                );

                if (isDateParsed)
                {
                    dpNgayNhap.SelectedDate = parsedDate;  // Đồng bộ ngày đã chọn
                    dpNgayNhap.DisplayDate = parsedDate;   // Đặt DisplayDate chính xác
                    dpNgayNhap.Text = parsedDate.ToString("dd/MM/yyyy");  // Đảm bảo Text theo định dạng mong muốn
                }

                tbxTriGia.Text = selectedBook.TriGia.ToString();
                cbbTinhTrang.Text = selectedBook.TinhTrang;

                using (var context = new QLTVContext())
                {
                    // Truyền giá trị TinhTrang vào dưới dạng string
                    string tenTinhTrang = selectedBook.TinhTrang;

                    // Kiểm tra nếu TinhTrang không phải là null hoặc rỗng
                    if (!string.IsNullOrEmpty(tenTinhTrang))
                    {
                        var idTinhTrang = context.TINHTRANG
                            .Where(tt => tt.TenTinhTrang == tenTinhTrang)
                            .Select(tt => tt.ID)
                            .FirstOrDefault();

                        if (idTinhTrang > 0)
                        {
                            // Lưu lại giá trị đã chọn trước khi thay đổi ItemsSource
                            var selectedTinhTrang = cbbTinhTrang.SelectedItem;

                            // Gán ItemsSource cho ComboBox với các tình trạng có ID >= idTinhTrang
                            var lstTinhTrang = context.TINHTRANG
                                .Where(tt => tt.ID >= idTinhTrang)
                                .ToList();

                            cbbTinhTrang.ItemsSource = lstTinhTrang;
                            cbbTinhTrang.DisplayMemberPath = "TenTinhTrang"; // Thiết lập thuộc tính cần hiển thị

                            // Cập nhật lại SelectedItem sau khi thay đổi ItemsSource
                            if (selectedTinhTrang != null)
                            {
                                // Tìm kiếm item đã chọn trong danh sách mới và gán lại
                                var selectedItemInList = lstTinhTrang
                                    .FirstOrDefault(tt => tt.TenTinhTrang == ((TINHTRANG)selectedTinhTrang).TenTinhTrang);

                                cbbTinhTrang.SelectedItem = selectedItemInList;
                            }
                            else
                            {
                                // Nếu không có item đã chọn, chọn mặc định (ví dụ, giá trị đầu tiên)
                                cbbTinhTrang.SelectedIndex = 0;
                            }
                        }
                    }

                    // Xử lý truyền book cover image
                    string tenTuaSach = selectedBook.TuaSach;
                    var tuaSach = context.TUASACH
                        .Where(ts => ts.TenTuaSach == tenTuaSach)
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
                tbxMaSach.Text = "";
                tbxTuaSach.Text = "";
                tbxDSTacGia.Text = "";
                tbxDSTheLoai.Text = "";
                tbxNhaXuatBan.Text = "";
                tbxNamXuatBan.Text = "";
                dpNgayNhap.Text = "";
                tbxTriGia.Text = "";
                cbbTinhTrang.Text = "";
            }
        }

        private void btnThemSach_Click(object sender, RoutedEventArgs e)
        {
            AWThemSach awThemSach = new AWThemSach();
            if (awThemSach.ShowDialog() == true)
                LoadSach();
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

        private void btnSuaSach_Click(object sender, RoutedEventArgs e)
        {
            if (dgSach.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn sách cần sửa!", "Thông báo",
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
                dynamic selectedItem = dgSach.SelectedItem;
                string maSach = selectedItem.MaSach;

                // Tìm tựa sách cần sửa
                var sachToUpdate = context.SACH
                    .FirstOrDefault(s => s.MaSach == maSach);

                if (sachToUpdate != null)
                {
                    // Cập nhật thông tin cơ bản
                    sachToUpdate.NhaXuatBan = tbxNhaXuatBan.Text;
                    sachToUpdate.NamXuatBan = int.Parse(tbxNamXuatBan.Text);
                    sachToUpdate.NgayNhap = DateTime.ParseExact(
                        dpNgayNhap.Text,
                        "dd/MM/yyyy",  // Định dạng ngày dmy
                        System.Globalization.CultureInfo.InvariantCulture
                    ); 
                    sachToUpdate.TriGia = decimal.Parse(tbxTriGia.Text);

                    // Lưu tất cả thay đổi
                    context.SaveChanges();

                    MessageBox.Show("Cập nhật tựa sách thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh lại DataGrid
                    LoadSach();
                }
            }
        }

        private void btnXoaSach_Click(object sender, RoutedEventArgs e)
        {
            if (dgSach.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn sách cần xóa!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selectedItem = dgSach.SelectedItem;
            string maSach = selectedItem.MaSach;

            MessageBoxResult mbrXacNhan = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa sách có mã: {maSach}?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (mbrXacNhan == MessageBoxResult.Yes)
            {
                using (var context = new QLTVContext())
                {
                    var sachToDelete = context.SACH
                        .FirstOrDefault(s => s.MaSach == maSach);

                    // Truong hop bat dong bo?
                    if (sachToDelete != null)
                    {
                        sachToDelete.IsDeleted = true;
                        var tuaSach = context.TUASACH
                            .FirstOrDefault(ts => ts.ID == sachToDelete.IDTuaSach);
                        if (tuaSach != null)
                            tuaSach.SoLuong--;
                        context.SaveChanges();
                        MessageBox.Show($"Sách có mã {maSach} đã được xóa.", "Thông báo",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadSach();
                    }
                }
            }
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            LoadSach();
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
                FileName = "DanhSachSach.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var filePath = saveFileDialog.FileName;

                // Tạo file Excel mới
                using (ExcelPackage package = new ExcelPackage())
                {
                    // Tạo một sheet mới
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Danh Sách Sách");

                    // Đặt tiêu đề cho các cột trong Excel
                    worksheet.Cells[1, 1].Value = "Mã Sách";
                    worksheet.Cells[1, 2].Value = "Tựa Sách";
                    worksheet.Cells[1, 3].Value = "Tác Giả";
                    worksheet.Cells[1, 4].Value = "Thể Loại";
                    worksheet.Cells[1, 5].Value = "Nhà Xuất Bản";
                    worksheet.Cells[1, 6].Value = "Năm Xuất Bản";
                    worksheet.Cells[1, 7].Value = "Ngày Nhập";
                    worksheet.Cells[1, 8].Value = "Trị Giá";
                    worksheet.Cells[1, 9].Value = "Tình Trạng";

                    // Áp dụng style cho tiêu đề
                    using (var range = worksheet.Cells[1, 1, 1, 9])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Duyệt qua dữ liệu trong DataGrid và ghi vào Excel
                    var items = dgSach.ItemsSource as System.Collections.IEnumerable;
                    int rowIndex = 2;

                    foreach (var item in items)
                    {
                        dynamic data = item;
                        worksheet.Cells[rowIndex, 1].Value = data.MaSach;
                        worksheet.Cells[rowIndex, 2].Value = data.TuaSach;
                        worksheet.Cells[rowIndex, 3].Value = data.DSTacGia;
                        worksheet.Cells[rowIndex, 4].Value = data.DSTheLoai;
                        worksheet.Cells[rowIndex, 5].Value = data.NhaXuatBan;
                        worksheet.Cells[rowIndex, 6].Value = data.NamXuatBan;
                        worksheet.Cells[rowIndex, 7].Value = data.NgayNhap;
                        worksheet.Cells[rowIndex, 8].Value = data.TriGia;
                        worksheet.Cells[rowIndex, 9].Value = data.TinhTrang;
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

        private void ImportExcelToDb(string filePath)
        {
            var context = new QLTVContext();
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) return;

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    // Extract data from Excel row
                    string tenTuaSach = worksheet.Cells[row, 1].Text;
                    string dsTacGia = worksheet.Cells[row, 2].Text;
                    string dsTheLoai = worksheet.Cells[row, 3].Text;
                    string nhaXuatBan = worksheet.Cells[row, 4].Text;
                    int namXuatBan = int.Parse(worksheet.Cells[row, 5].Text);
                    DateTime ngayNhap = DateTime.ParseExact(worksheet.Cells[row, 6].Text, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    decimal triGia = decimal.Parse(worksheet.Cells[row, 7].Text);
                    string tenTinhTrang = worksheet.Cells[row, 8].Text;

                    // Find or create TinhTrang
                    var tinhTrang = context.TINHTRANG
                        .FirstOrDefault(tt => !tt.IsDeleted && tt.TenTinhTrang == tenTinhTrang);
                    if (tinhTrang == null) continue;

                    // Prepare dictionaries for existing entities
                    var existingAuthors = context.TACGIA.ToDictionary(tg => tg.TenTacGia);
                    var existingCategories = context.THELOAI.ToDictionary(tl => tl.TenTheLoai);

                    // Find existing TuaSach
                    var existingTuaSach = context.TUASACH
                        .FirstOrDefault(ts => !ts.IsDeleted && ts.TenTuaSach == tenTuaSach);

                    // Determine if we need to create a new TuaSach
                    bool createNewTuaSach = existingTuaSach == null;
                    if (existingTuaSach != null)
                    {
                        // Check if authors or categories are different
                        string existingDSTacGia = string.Join(", ", existingTuaSach.TUASACH_TACGIA
                            .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia));
                        string existingDSTheLoai = string.Join(", ", existingTuaSach.TUASACH_THELOAI
                            .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai));

                        createNewTuaSach = (dsTacGia != existingDSTacGia || dsTheLoai != existingDSTheLoai);
                    }

                    // Create new TuaSach if needed
                    TUASACH tuaSach = existingTuaSach;
                    if (createNewTuaSach)
                    {
                        tuaSach = new TUASACH
                        {
                            TenTuaSach = tenTuaSach,
                        };
                        context.TUASACH.Add(tuaSach);
                        context.SaveChanges();

                        // Process authors
                        var lstTenTacGia = dsTacGia.Split(", ").Select(n => n.Trim()).ToList();
                        foreach (var tenTacGia in lstTenTacGia)
                        {
                            if (!existingAuthors.TryGetValue(tenTacGia, out var tacGia))
                            {
                                tacGia = new TACGIA { TenTacGia = tenTacGia, NamSinh = -1, QuocTich = "Chưa Có" };
                                context.TACGIA.Add(tacGia);
                                context.SaveChanges();
                                existingAuthors[tenTacGia] = tacGia;
                            }
                            context.TUASACH_TACGIA.Add(new TUASACH_TACGIA { IDTuaSach = tuaSach.ID, IDTacGia = tacGia.ID });
                        }

                        // Process categories
                        var lstTenTheLoai = dsTheLoai.Split(", ").Select(n => n.Trim()).ToList();
                        foreach (var tenTheLoai in lstTenTheLoai)
                        {
                            if (!existingCategories.TryGetValue(tenTheLoai, out var theLoai))
                            {
                                theLoai = new THELOAI { TenTheLoai = tenTheLoai };
                                context.THELOAI.Add(theLoai);
                                context.SaveChanges();
                                existingCategories[tenTheLoai] = theLoai;
                            }
                            context.TUASACH_THELOAI.Add(new TUASACH_THELOAI { IDTuaSach = tuaSach.ID, IDTheLoai = theLoai.ID });
                        }
                        context.SaveChanges();
                    }

                    // Create new Sach
                    var newSach = new SACH
                    {
                        IDTuaSach = tuaSach.ID,
                        NhaXuatBan = nhaXuatBan,
                        NamXuatBan = namXuatBan,
                        NgayNhap = ngayNhap,
                        TriGia = triGia,
                        IDTinhTrang = tinhTrang.ID
                    };
                    context.SACH.Add(newSach);
                    context.SaveChanges();
                }
            }
        }

        private void btnImportExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ImportExcelToDb(openFileDialog.FileName);
                MessageBox.Show("Nhập Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSach();
            }
        }

        private void dpNgayNhap_Loaded(object sender, RoutedEventArgs e)
        {
            // Tìm TextBox bên trong DatePicker
            var textBox = (dpNgayNhap.Template.FindName("PART_TextBox", dpNgayNhap) as TextBox);
            if (textBox != null)
            {
                textBox.TextChanged += TextBox_TextChanged;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgSach.SelectedItem == null)
                return;

            var textBox = sender as TextBox;

            // Danh sách định dạng hỗ trợ nhiều cách nhập ngày
            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy" };
            if (!DateTime.TryParseExact(textBox.Text, formats, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out DateTime ngayNhap))
            {
                icNgayNhapError.ToolTip = "Ngày Nhập không hợp lệ (định dạng đúng: dd/MM/yyyy)";
                icNgayNhapError.Visibility = Visibility.Visible;
                return;
            }

            // Kiểm tra giới hạn ngày từ 1/1/2000 đến hiện tại
            DateTime minDate = new DateTime(2000, 1, 1);
            if (ngayNhap < minDate)
            {
                icNgayNhapError.ToolTip = "Ngày Nhập không được trước ngày 1/1/2000";
                icNgayNhapError.Visibility = Visibility.Visible;
                return;
            }

            if (ngayNhap > DateTime.Now)
            {
                icNgayNhapError.ToolTip = "Ngày Nhập không được sau ngày hiện tại";
                icNgayNhapError.Visibility = Visibility.Visible;
                return;
            }

            // Nếu hợp lệ, ẩn thông báo lỗi
            icNgayNhapError.Visibility = Visibility.Collapsed;
        }

        private void tbxNhaXuatBan_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgSach.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxNhaXuatBan.Text))
            {
                icNhaXuatBanError.ToolTip = "Nhà Xuất Bản không được để trống";
                icNhaXuatBanError.Visibility = Visibility.Visible;
                return;
            }

            foreach (char c in tbxNhaXuatBan.Text)
            {
                if (!char.IsLetter(c) && !char.IsDigit(c) && !char.IsWhiteSpace(c))
                {
                    icNhaXuatBanError.ToolTip = "Nhà Xuất Bản không được có kí tự đặc biệt";
                    icNhaXuatBanError.Visibility = Visibility.Visible;
                    return;
                }
            }

            icNhaXuatBanError.Visibility = Visibility.Collapsed;
        }

        private void tbxNamXuatBan_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgSach.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxNamXuatBan.Text))
            {
                icNamXuatBanError.ToolTip = "Năm Xuất Bản không được để trống";
                icNamXuatBanError.Visibility = Visibility.Visible;
                return;
            }

            if (!int.TryParse(tbxNamXuatBan.Text, out int nxb))
            {
                icNamXuatBanError.ToolTip = "Năm Xuất Bản phải là số nguyên dương";
                icNamXuatBanError.Visibility = Visibility.Visible;
                return;
            }

            if (nxb <= 1900 || nxb > DateTime.Now.Year)
            {
                icNamXuatBanError.ToolTip = "Năm Xuất Bản phải sau 1900 và không quá năm hiện tại";
                icNamXuatBanError.Visibility = Visibility.Visible;
                return;
            }

            icNamXuatBanError.Visibility = Visibility.Collapsed;
        }

        private void tbxTriGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgSach.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxTriGia.Text))
            {
                icTriGiaError.ToolTip = "Trị Giá không được để trống";
                icTriGiaError.Visibility = Visibility.Visible;
                return;
            }

            if (!int.TryParse(tbxTriGia.Text, out int tg) || tg < 5000 || tg > 10000000)
            {
                icTriGiaError.ToolTip = "Trị Giá phải là lớn hơn 5 ngàn và nhỏ hơn 10 triệu";
                icTriGiaError.Visibility = Visibility.Visible;
                return;
            }

            icTriGiaError.Visibility = Visibility.Collapsed;
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

        //private void dpNgayNhap_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (isUpdatingText) return;

        //    if (dpNgayNhap.SelectedDate.HasValue)
        //    {
        //        isUpdatingText = true;  // Đánh dấu là đang cập nhật text
        //        dpNgayNhap.Text = dpNgayNhap.SelectedDate.Value.ToString("dd/MM/yyyy");
        //        isUpdatingText = false; // Đặt lại flag sau khi cập nhật
        //    }
        //}

        //private void dpNgayNhap_CalendarOpened(object sender, RoutedEventArgs e)
        //{
        //    var datePicker = sender as DatePicker;

        //    if (datePicker != null && DateTime.TryParseExact(datePicker.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
        //    {
        //        datePicker.DisplayDate = parsedDate; // Đặt DisplayDate thành ngày đã phân tích
        //    }
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QLTV.Models;
using System.IO;
using System.Windows.Media;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Globalization;
using System.Text;
using MaterialDesignThemes.Wpf;

namespace QLTV.Admin
{
    public partial class AUQuanLyDocGia : UserControl
    {
        public List<string> TenTaiKhoanList { get; set; }
        public List<string> TenLoaiDocGiaList { get; set; }

        private QLTVContext _context = new QLTVContext();
        public ObservableCollection<DOCGIA> Readers { get; set; }
        public ObservableCollection<PHIEUTHUTIENPHAT> PenaltyReceipts { get; set; }
        public ObservableCollection<LOAIDOCGIA> ReaderTypes { get; set; }

        private AWThemDocGia themDocGiaWindow;
        private AWThemLoaiDocGia themLoaiDocGiaWindow;
        private AWThemPhieuThuTienPhat themPhieuThuWindow;

        public AUQuanLyDocGia()
        {
            InitializeComponent();
            _context = new QLTVContext();
            Readers = new ObservableCollection<DOCGIA>();
            PenaltyReceipts = new ObservableCollection<PHIEUTHUTIENPHAT>();
            ReaderTypes = new ObservableCollection<LOAIDOCGIA>();

            ReadersDataGrid.ItemsSource = Readers;
            LoadReadersData();

            ReaderTypesDataGrid.ItemsSource = ReaderTypes;
            LoadReaderTypesData();

            PenaltyReceiptsDataGrid.ItemsSource = PenaltyReceipts;
            LoadPenaltyReceiptsData();

            TenTaiKhoanList = _context.TAIKHOAN.Select(tk => tk.TenTaiKhoan).ToList();
            TenTaiKhoanComboBox.ItemsSource = TenTaiKhoanList;
            TenTaiKhoanPhatComboBox.ItemsSource = TenTaiKhoanList;

            TenLoaiDocGiaList = _context.LOAIDOCGIA.Where(ldg => !ldg.IsDeleted).Select(ldg => ldg.TenLoaiDocGia).ToList();
            TenLoaiDocGiaComboBox.ItemsSource = TenLoaiDocGiaList;
        }

        private void OpenExportMenu_Click(object sender, RoutedEventArgs e)
        {
            // Tham chiếu đến nút
            Button button = sender as Button;

            // Mở ContextMenu
            if (button.ContextMenu != null)
            {
                button.ContextMenu.IsOpen = true;
            }
        }

        private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
        {
            bool isCollapsed = SidebarOverlay.Visibility == Visibility.Collapsed;

            SidebarOverlay.Visibility = isCollapsed ? Visibility.Visible : Visibility.Collapsed;

            var animation = new DoubleAnimation
            {
                From = isCollapsed ? -250 : 0,
                To = isCollapsed ? 0 : -250,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = isCollapsed ? EasingMode.EaseOut : EasingMode.EaseIn }
            };

            if (!isCollapsed)
            {
                animation.Completed += (s, _) => SidebarOverlay.Visibility = Visibility.Collapsed;
            }

            Sidebar.RenderTransform = new TranslateTransform(-250, 0);
            Sidebar.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void CloseSidebar_Click(object sender, MouseButtonEventArgs e)
        {
            // Ẩn sidebar 
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = -250,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            animation.Completed += (s, _) => SidebarOverlay.Visibility = Visibility.Collapsed;
            Sidebar.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void BookManagement_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("BookManagement");
        }

        private void LoanManagement_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("LoanManagement");
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("BCTK");
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                // Kiểm tra xem tab đầu tiên có được chọn không
                if (((TabControl)sender).SelectedIndex == 0)
                {
                    // Cập nhật lại danh sách TenLoaiDocGiaList
                    TenLoaiDocGiaList = _context.LOAIDOCGIA.Where(ldg => !ldg.IsDeleted)
                                                         .Select(ldg => ldg.TenLoaiDocGia).ToList();
                    TenLoaiDocGiaComboBox.ItemsSource = TenLoaiDocGiaList;
                }
            }
        }

        // Readers 
        private void LoadReadersData()
        {
            Readers.Clear();
            var readers = _context.DOCGIA.Include(d => d.IDTaiKhoanNavigation).ToList();
            foreach (var reader in readers)
            {
                Readers.Add(reader);
            }
        }

        private void AddReader_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem cửa sổ thêm độc giả đã tồn tại chưa
            if (themDocGiaWindow == null || !themDocGiaWindow.IsVisible)
            {
                // Lấy tên tài khoản và tên loại độc giả từ ComboBox
                string tenTaiKhoan = TenTaiKhoanComboBox.Text;
                string tenLoaiDocGia = TenLoaiDocGiaComboBox.Text;

                themDocGiaWindow = new AWThemDocGia(_context, tenTaiKhoan, tenLoaiDocGia); // Truyền context
                bool? result = themDocGiaWindow.ShowDialog();

                if (result == true)
                {
                    LoadReadersData();
                }
            }
            else
            {
                // Nếu cửa sổ đang mở, chỉ cần focus vào cửa sổ đó
                themDocGiaWindow.Focus();
            }
        }

        private void UpdateReader_Click(object sender, RoutedEventArgs e)
        {
            if (ReadersDataGrid.SelectedItem is DOCGIA selectedReader)
            {
                try
                {
                    // Validate dữ liệu đầu vào
                    if (TenTaiKhoanComboBox.SelectedItem == null)
                    {
                        icTenTaiKhoanError.ToolTip = "Tên Tài Khoản không được để trống";
                        icTenTaiKhoanError.Visibility = Visibility.Visible;
                    }

                    if (dpNgayLapThe.SelectedDate > dpNgayHetHan.SelectedDate)
                    {
                        icNgayLapTheError.ToolTip = "Ngày Lập Thẻ không được lớn hơn Ngày Hết Hạn";
                        icNgayLapTheError.Visibility = Visibility.Visible;
                        icNgayHetHanError.ToolTip = "Ngày Hết Hạn không được lớn nhỏ Ngày Lập Thẻ";
                        icNgayHetHanError.Visibility = Visibility.Visible;
                    }

                    string tentaiKhoan = TenTaiKhoanComboBox.SelectedItem.ToString();
                    if (_context.DOCGIA.Any(dg => dg.IDTaiKhoanNavigation.TenTaiKhoan == tentaiKhoan && dg.ID != selectedReader.ID))
                    {
                        icTenTaiKhoanError.ToolTip = "Tên tài khoản đã được sử dụng!";
                        icTenTaiKhoanError.Visibility = Visibility.Visible;
                    }
                    if (TenLoaiDocGiaComboBox.SelectedItem == null)
                    {
                        icTenLoaiDocGiaError.ToolTip = "Phải chọn Loại Độc Giả";
                        icTenLoaiDocGiaError.Visibility = Visibility.Visible;
                    }
                    if (string.IsNullOrWhiteSpace(TongNoTextBox.Text))
                    {
                        icTongNoError.ToolTip = "Tổng Nợ không được để trống";
                        icTongNoError.Visibility = Visibility.Visible;
                    }

                    // Kiểm tra còn lỗi không
                    if (HasError())
                    {
                        MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi cập nhật!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Lấy tên tài khoản và tên loại độc giả từ ComboBox
                    string tenTaiKhoan = TenTaiKhoanComboBox.SelectedItem as string;
                    string tenLoaiDocGia = TenLoaiDocGiaComboBox.SelectedItem as string;

                    // Tìm ID tương ứng trong database
                    var taiKhoan = _context.TAIKHOAN.FirstOrDefault(tk => tk.TenTaiKhoan == tenTaiKhoan);
                    var loaiDocGia = _context.LOAIDOCGIA.FirstOrDefault(ldg => ldg.TenLoaiDocGia == tenLoaiDocGia);

                    if (taiKhoan == null)
                    {
                        icTenTaiKhoanError.ToolTip = "Tên tài khoản không tồn tại trong cơ sở dữ liệu!";
                        icTenTaiKhoanError.Visibility = Visibility.Visible;
                        MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi cập nhật!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    selectedReader.IDTaiKhoan = taiKhoan.ID;
                    selectedReader.IDLoaiDocGia = loaiDocGia.ID;
                    selectedReader.TongNo = decimal.TryParse(TongNoTextBox.Text, out decimal tongNo) ? tongNo : selectedReader.TongNo;
                    selectedReader.GioiThieu = GioiThieu.Text;
                    _context.SaveChanges();

                    taiKhoan.NgayMo = dpNgayLapThe.SelectedDate ?? taiKhoan.NgayMo;
                    taiKhoan.NgayDong = dpNgayHetHan.SelectedDate ?? taiKhoan.NgayDong;
                    _context.SaveChanges();

                    // Cập nhật UI
                    var index = Readers.IndexOf(selectedReader);
                    Readers[index] = selectedReader;
                    ReadersDataGrid.Items.Refresh();
                    MessageBox.Show("Cập nhật thông tin độc giả thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (DbUpdateException ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật độc giả trong cơ sở dữ liệu: {ex.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi không xác định: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một độc giả để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        //Validate cho Update
        private void cbbTenTaiKhoan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Kiểm tra đã chọn tên tài khoản chưa
            if (TenTaiKhoanComboBox.SelectedItem != null)
            {
                icTenTaiKhoanError.Visibility = Visibility.Collapsed;
            }
        }

        private void cbbTenLoaiDocGia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Kiểm tra đã chọn loại độc giả chưa
            if (TenLoaiDocGiaComboBox.SelectedItem != null)
            {
                icTenLoaiDocGiaError.Visibility = Visibility.Collapsed;
            }
        }

        private void GioiThieu_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(GioiThieu.Text) && GioiThieu.Text.Length > 200)
            {
                icGioiThieuError.ToolTip = "Thông Tin Giới Thiệu không được vượt quá 200 kí tự";
                icGioiThieuError.Visibility = Visibility.Visible;
                return;
            }

            icGioiThieuError.Visibility = Visibility.Collapsed;
        }

        private void dpNgayLapThe_Loaded(object sender, RoutedEventArgs e)
        {
            // Tìm TextBox bên trong DatePicker
            var textBox = (dpNgayLapThe.Template.FindName("PART_TextBox", dpNgayLapThe) as TextBox);
            if (textBox != null)
            {
                textBox.TextChanged += NgayLapThe_TextChanged;
            }
        }

        private void NgayLapThe_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            // Danh sách định dạng hỗ trợ nhiều cách nhập ngày
            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy" };
            if (!DateTime.TryParseExact(textBox.Text, formats, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out DateTime ngayLapThe))
            {
                icNgayLapTheError.ToolTip = "Ngày Lập Thẻ không hợp lệ (định dạng đúng: dd/MM/yyyy)";
                icNgayLapTheError.Visibility = Visibility.Visible;
                return;
            }

            // Kiểm tra ngày lập thẻ không được sau ngày hiện tại
            if (ngayLapThe > DateTime.Now)
            {
                icNgayLapTheError.ToolTip = "Ngày Lập Thẻ không được sau ngày hiện tại";
                icNgayLapTheError.Visibility = Visibility.Visible;
                return;
            }

            // Nếu hợp lệ, ẩn thông báo lỗi và cập nhật ngày hết hạn
            icNgayLapTheError.Visibility = Visibility.Collapsed;
            dpNgayHetHan.SelectedDate = ngayLapThe.AddYears(1);
        }

        private void dpNgayHetHan_Loaded(object sender, RoutedEventArgs e)
        {
            // Tìm TextBox bên trong DatePicker
            var textBox = (dpNgayHetHan.Template.FindName("PART_TextBox", dpNgayHetHan) as TextBox);
            if (textBox != null)
            {
                textBox.TextChanged += NgayHetHan_TextChanged;
            }
        }

        private void NgayHetHan_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            // Danh sách định dạng hỗ trợ nhiều cách nhập ngày
            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy" };
            if (!DateTime.TryParseExact(textBox.Text, formats, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out DateTime ngayHetHan))
            {
                icNgayLapTheError.ToolTip = "Ngày Hết Hạn không hợp lệ (định dạng đúng: dd/MM/yyyy)";
                icNgayLapTheError.Visibility = Visibility.Visible;
                return;
            }

            // Nếu hợp lệ, ẩn thông báo lỗi
            icNgayLapTheError.Visibility = Visibility.Collapsed;
        }

        private void DeleteReader_Click(object sender, RoutedEventArgs e)
        {
            if (ReadersDataGrid.SelectedItem is DOCGIA selectedReader)
            {
                var result = MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa độc giả này?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Xóa độc giả khỏi CSDL
                        var readerToDelete = _context.DOCGIA.Find(selectedReader.ID);
                        if (readerToDelete != null)
                        {
                            _context.DOCGIA.Remove(readerToDelete);
                            _context.SaveChanges();

                            Readers.Remove(selectedReader);
                            ClearInputs();

                            MessageBox.Show("Xóa độc giả thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy độc giả để xóa.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (DbUpdateException ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa độc giả: {ex.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi không xác định: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn một độc giả để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void TTDGSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = NormalizeString(TTDGSearchTextBox.Text.Trim());
            string searchCriteria = (TTDGSearchCriteriaComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            var query = _context.DOCGIA
                .Include(r => r.IDTaiKhoanNavigation)
                .Include(r => r.IDLoaiDocGiaNavigation)
                .AsEnumerable() // Chuyển về IEnumerable để lọc trên máy khách
                .ToList();

            // Kiểm tra xem có tiêu chí tìm kiếm được chọn hay không
            if (string.IsNullOrEmpty(searchCriteria))
            {
                // Nếu không có tiêu chí, tìm kiếm trên tất cả các trường
                query = query.Where(r =>
                    NormalizeString(r.MaDocGia).Contains(searchTerm) ||
                    NormalizeString(r.IDTaiKhoanNavigation.TenTaiKhoan).Contains(searchTerm) ||
                    NormalizeString(r.IDLoaiDocGiaNavigation.TenLoaiDocGia).Contains(searchTerm)
                ).ToList();
            }
            else
            {
                // Nếu có tiêu chí, lọc theo tiêu chí được chọn
                switch (searchCriteria)
                {
                    case "Mã Độc Giả":
                        query = query.Where(r => NormalizeString(r.MaDocGia).Contains(searchTerm)).ToList();
                        break;
                    case "Tên Tài Khoản":
                        query = query.Where(r => NormalizeString(r.IDTaiKhoanNavigation.TenTaiKhoan).Contains(searchTerm)).ToList();
                        break;
                    case "Tên Loại Độc Giả":
                        query = query.Where(r => NormalizeString(r.IDLoaiDocGiaNavigation.TenLoaiDocGia).Contains(searchTerm)).ToList();
                        break;
                }
            }

            // Cập nhật ItemsSource cho DataGrid
            Readers.Clear();
            foreach (var reader in query)
            {
                Readers.Add(reader);
            }
        }

        private void ClearInputs()
        {
            TenTaiKhoanComboBox.SelectedItem = null; // Cập nhật cho ComboBox
            TenLoaiDocGiaComboBox.SelectedItem = null; // Cập nhật cho ComboBox
            dpNgayLapThe.SelectedDate = null;
            dpNgayHetHan.SelectedDate = null;
            TongNoTextBox.Text = string.Empty;
            GioiThieu.Text = string.Empty;
        }

        private void ReadersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReadersDataGrid.SelectedItem is DOCGIA selectedReader)
            {
                // Đặt SelectedItem cho ComboBox
                TenTaiKhoanComboBox.SelectedItem = selectedReader.IDTaiKhoanNavigation.TenTaiKhoan;
                TenLoaiDocGiaComboBox.SelectedItem = selectedReader.IDLoaiDocGiaNavigation.TenLoaiDocGia;

                TongNoTextBox.Text = selectedReader.TongNo.ToString();
                GioiThieu.Text = selectedReader.GioiThieu;

                var taiKhoan = _context.TAIKHOAN
                    .Where(tk => tk.ID == selectedReader.IDTaiKhoan)
                    .FirstOrDefault();

                dpNgayLapThe.SelectedDate = taiKhoan.NgayMo;
                dpNgayHetHan.SelectedDate = taiKhoan.NgayDong;
            }
        }

        // Reader Types 
        private void LoadReaderTypesData()
        {
            var readerTypes = _context.LOAIDOCGIA.Where(r => !r.IsDeleted).ToList(); // Chỉ lấy những loại độc giả chưa bị xóa
            ReaderTypesDataGrid.ItemsSource = readerTypes;
            TenLoaiDocGiaList = _context.LOAIDOCGIA.Where(ldg => !ldg.IsDeleted)
                                         .Select(ldg => ldg.TenLoaiDocGia).ToList();
            TenLoaiDocGiaComboBox.ItemsSource = TenLoaiDocGiaList;
        }

        private void AddReaderType_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem cửa sổ thêm loại độc giả đã tồn tại chưa
            if (themLoaiDocGiaWindow == null || !themLoaiDocGiaWindow.IsVisible)
            {
                themLoaiDocGiaWindow = new AWThemLoaiDocGia(_context); // Truyền context
                themLoaiDocGiaWindow.ShowDialog();

                // Load lại dữ liệu sau khi đóng cửa sổ thêm
                if (themLoaiDocGiaWindow.DialogResult == true)
                {
                    LoadReaderTypesData();
                }
            }
            else
            {
                // Nếu cửa sổ đang mở, chỉ cần focus vào cửa sổ đó
                themLoaiDocGiaWindow.Focus();
            }
        }

        private void UpdateReaderType_Click(object sender, RoutedEventArgs e)
        {
            if (ReaderTypesDataGrid.SelectedItem is LOAIDOCGIA selectedReaderType)
            {
                try
                {
                    // Validate dữ liệu đầu vào
                    if (string.IsNullOrWhiteSpace(TenLoaiDocGiaTextBox.Text))
                    {
                        icLoaiDocGiaError.ToolTip = "Tên Loại Độc Giả không được để trống";
                        icLoaiDocGiaError.Visibility = Visibility.Visible;
                    }
                    if (string.IsNullOrWhiteSpace(SoSachMuonToiDaTextBox.Text))
                    {
                        icSoSachMuonToiDaError.ToolTip = "Số Sách Mượn Tối Đa không được để trống";
                        icSoSachMuonToiDaError.Visibility = Visibility.Visible;
                    }

                    // Kiểm tra trùng lặp tên loại độc giả
                    string tenLoaiDocGia = TenLoaiDocGiaTextBox.Text;
                    if (_context.LOAIDOCGIA.Any(ldg => ldg.TenLoaiDocGia == tenLoaiDocGia && ldg.ID != selectedReaderType.ID && !ldg.IsDeleted))
                    {
                        icLoaiDocGiaError.ToolTip = "Tên loại độc giả đã tồn tại!";
                        icLoaiDocGiaError.Visibility = Visibility.Visible;
                    }

                    // Kiểm tra còn lỗi không
                    if (HasError())
                    {
                        MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi cập nhật!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    // Validate 
                    if (string.IsNullOrWhiteSpace(TenLoaiDocGiaTextBox.Text) ||
                        !int.TryParse(SoSachMuonToiDaTextBox.Text, out int soSachMuonToiDa))
                    {
                        MessageBox.Show("Vui lòng điền đầy đủ và chính xác thông tin.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var readerTypeToUpdate = _context.LOAIDOCGIA.Find(selectedReaderType.ID);
                    if (readerTypeToUpdate == null)
                    {
                        MessageBox.Show("Không tìm thấy loại độc giả để cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    readerTypeToUpdate.TenLoaiDocGia = TenLoaiDocGiaTextBox.Text;
                    readerTypeToUpdate.SoSachMuonToiDa = soSachMuonToiDa;

                    _context.SaveChanges();

                    LoadReaderTypesData();
                    ClearReaderTypeInputs();
                    LoadReadersData();
                    ReadersDataGrid.Items.Refresh();
                    icLoaiDocGiaError.Visibility = Visibility.Collapsed;
                    icSoSachMuonToiDaError.Visibility = Visibility.Collapsed;
                    MessageBox.Show("Cập nhật loại độc giả thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật loại độc giả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một loại độc giả để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteReaderType_Click(object sender, RoutedEventArgs e)
        {
            if (ReaderTypesDataGrid.SelectedItem is LOAIDOCGIA selectedReaderType)
            {
                // Kiểm tra xem loại độc giả có đang được sử dụng bởi độc giả nào không
                var existingReaders = _context.DOCGIA.Any(d => d.IDLoaiDocGia == selectedReaderType.ID);
                if (existingReaders)
                {
                    // Lấy ID của loại độc giả "Mặc định"
                    var defaultReaderType = _context.LOAIDOCGIA.FirstOrDefault(ldg => ldg.TenLoaiDocGia == "Mặc định");
                    if (defaultReaderType == null)
                    {
                        MessageBox.Show("Không tìm thấy loại độc giả 'Mặc định'.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Cập nhật IDLoaiDocGia của các độc giả đang sử dụng loại độc giả cần xóa
                    var readersToUpdate = _context.DOCGIA.Where(d => d.IDLoaiDocGia == selectedReaderType.ID).ToList();
                    foreach (var reader in readersToUpdate)
                    {
                        reader.IDLoaiDocGia = defaultReaderType.ID;
                    }
                    _context.SaveChanges();
                }

                // Hiển thị hộp thoại xác nhận xóa
                var result = MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa loại độc giả này?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Soft delete loại độc giả
                        var readerTypeToDelete = _context.LOAIDOCGIA.Find(selectedReaderType.ID);
                        if (readerTypeToDelete != null)
                        {
                            readerTypeToDelete.IsDeleted = true;
                            _context.SaveChanges();

                            LoadReaderTypesData();
                            ClearReaderTypeInputs();

                            MessageBox.Show("Xóa loại độc giả thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa loại độc giả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một loại độc giả để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ReaderTypesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReaderTypesDataGrid.SelectedItem is LOAIDOCGIA selectedReaderType)
            {
                TenLoaiDocGiaTextBox.Text = selectedReaderType.TenLoaiDocGia;
                SoSachMuonToiDaTextBox.Text = selectedReaderType.SoSachMuonToiDa.ToString();
            }
        }
        private void ClearReaderTypeInputs()
        {
            TenLoaiDocGiaTextBox.Text = string.Empty;
            SoSachMuonToiDaTextBox.Text = string.Empty;
        }

        private void LDGSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = NormalizeString(LDGSearchTextBox.Text.Trim());
            string searchCriteria = (LDGSearchCriteriaComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            var query = _context.LOAIDOCGIA
                .Where(r => !r.IsDeleted) // Chỉ lấy những loại độc giả chưa bị xóa
                .AsEnumerable() // Chuyển về IEnumerable để lọc trên máy khách
                .ToList();

            // Kiểm tra xem có tiêu chí tìm kiếm được chọn hay không
            if (string.IsNullOrEmpty(searchCriteria))
            {
                // Nếu không có tiêu chí, tìm kiếm trên tất cả các trường
                query = query.Where(r =>
                    NormalizeString(r.TenLoaiDocGia).Contains(searchTerm) ||
                    NormalizeString(r.SoSachMuonToiDa.ToString()).Contains(searchTerm)
                ).ToList();
            }
            else
            {
                // Nếu có tiêu chí, lọc theo tiêu chí được chọn
                switch (searchCriteria)
                {
                    case "Tên Loại Độc Giả":
                        query = query.Where(r => NormalizeString(r.TenLoaiDocGia).Contains(searchTerm)).ToList();
                        break;
                    case "Số Sách Mượn Tối Đa":
                        query = query.Where(r => NormalizeString(r.SoSachMuonToiDa.ToString()).Contains(searchTerm)).ToList();
                        break;
                }
            }

            // Cập nhật ItemsSource cho DataGrid
            ReaderTypes.Clear();
            foreach (var readerType in query)
            {
                ReaderTypes.Add(readerType);
            }
            ReaderTypesDataGrid.ItemsSource = ReaderTypes;
        }

        // Penalty Receipts 
        private void LoadPenaltyReceiptsData()
        {
            PenaltyReceipts.Clear();
            var receipts = _context.PHIEUTHUTIENPHAT.Include(p => p.IDDocGiaNavigation).ToList();
            foreach (var receipt in receipts)
            {
                PenaltyReceipts.Add(receipt);
            }
            PenaltyReceiptsDataGrid.ItemsSource = PenaltyReceipts;
        }

        private void UpdateReadersData()
        {
            // Tải lại dữ liệu độc giả từ database
            var updatedReaders = _context.DOCGIA.Include(d => d.IDTaiKhoanNavigation).ToList();

            // Cập nhật ObservableCollection<DOCGIA> Readers
            Readers.Clear();
            foreach (var reader in updatedReaders)
            {
                Readers.Add(reader);
            }
        }

        private void CreatePenaltyReceipt_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem cửa sổ thêm phiếu thu đã tồn tại chưa
            if (themPhieuThuWindow == null || !themPhieuThuWindow.IsVisible)
            {
                themPhieuThuWindow = new AWThemPhieuThuTienPhat(_context);
                themPhieuThuWindow.ShowDialog();

                // Load lại dữ liệu sau khi đóng cửa sổ thêm
                if (themPhieuThuWindow.DialogResult == true)
                {
                    LoadPenaltyReceiptsData();
                    UpdateReadersData();
                    ReadersDataGrid.ItemsSource = Readers;
                    ReadersDataGrid.Items.Refresh();
                }
            }
            else
            {
                // Nếu cửa sổ đang mở, chỉ cần focus vào cửa sổ đó
                themPhieuThuWindow.Focus();
            }
        }

        private void DeletePenalty_Click(object sender, RoutedEventArgs e)
        {
            if (PenaltyReceiptsDataGrid.SelectedItem is PHIEUTHUTIENPHAT selectedPenalty)
            {
                try
                {
                    var result = MessageBox.Show(
                        "Bạn có chắc chắn muốn xóa phiếu thu tiền phạt này?",
                        "Xác nhận xóa",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        // Tìm phiếu thu trong database
                        var penaltyToDelete = _context.PHIEUTHUTIENPHAT.Find(selectedPenalty.ID);
                        if (penaltyToDelete != null)
                        {
                            // Tìm độc giả dựa trên tên tài khoản được chọn trong ComboBox
                            string tenTaiKhoan = TenTaiKhoanPhatComboBox.Text;
                            var docGia = _context.DOCGIA
                                .Include(d => d.IDTaiKhoanNavigation)
                                .FirstOrDefault(d => d.IDTaiKhoanNavigation.TenTaiKhoan == tenTaiKhoan);

                            if (docGia != null)
                            {
                                // Hoàn lại số tiền đã thu vào tổng nợ
                                docGia.TongNo += penaltyToDelete.SoTienThu;
                            }

                            // Xóa thực sự phiếu thu khỏi database
                            _context.PHIEUTHUTIENPHAT.Remove(penaltyToDelete);
                            _context.SaveChanges();

                            UpdateReadersData();
                            ReadersDataGrid.Items.Refresh();
                            // Load lại dữ liệu
                            LoadPenaltyReceiptsData();
                            ClearPenaltyInputs();

                            MessageBox.Show("Xóa phiếu thu tiền phạt thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa phiếu thu tiền phạt: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phiếu thu tiền phạt để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditPenalty_Click(object sender, RoutedEventArgs e)
        {
            if (PenaltyReceiptsDataGrid.SelectedItem is PHIEUTHUTIENPHAT selectedPenalty)
            {
                try
                {
                    // Validate dữ liệu đầu vào
                    if (TenTaiKhoanPhatComboBox.SelectedItem == null)
                    {
                        icTenTaiKhoanPhatError.ToolTip = "Tên Tài Khoản không được để trống";
                        icTenTaiKhoanPhatError.Visibility = Visibility.Visible;
                    }
                    if (NgayThuPhat.SelectedDate == null)
                    {
                        icNgayThuPhatError.ToolTip = "Ngày Thu không được để trống";
                        icNgayThuPhatError.Visibility = Visibility.Visible;
                    }

                    // Kiểm tra còn lỗi không
                    if (HasError())
                    {
                        MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi cập nhật!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Tìm phiếu thu trong database
                    var penaltyToUpdate = _context.PHIEUTHUTIENPHAT.Find(selectedPenalty.ID);
                    if (penaltyToUpdate != null)
                    {
                        // Tìm độc giả dựa trên tên tài khoản được chọn trong ComboBox
                        string tenTaiKhoan = TenTaiKhoanPhatComboBox.Text;
                        var docGia = _context.DOCGIA
                            .Include(d => d.IDTaiKhoanNavigation)
                            .FirstOrDefault(d => d.IDTaiKhoanNavigation.TenTaiKhoan == tenTaiKhoan);

                        if (docGia == null)
                        {
                            MessageBox.Show("Không tìm thấy độc giả!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Lưu giá trị cũ để khôi phục nếu có lỗi
                        decimal oldSoTienThu = penaltyToUpdate.SoTienThu;

                        // Tính số tiền mới
                        decimal newSoTienThu = decimal.Parse(SoTienThu.Text);

                        // Kiểm tra số tiền thu không được lớn hơn tổng nợ
                        if (newSoTienThu > docGia.TongNo + oldSoTienThu)
                        {
                            MessageBox.Show("Số tiền thu không được lớn hơn tổng nợ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Cập nhật thông tin phiếu thu
                        penaltyToUpdate.NgayThu = NgayThuPhat.SelectedDate.Value;
                        penaltyToUpdate.SoTienThu = newSoTienThu;

                        // Điều chỉnh tổng nợ
                        docGia.TongNo = docGia.TongNo + oldSoTienThu - newSoTienThu;
                        penaltyToUpdate.ConLai = docGia.TongNo;

                        try
                        {
                            _context.SaveChanges();

                            UpdateReadersData();
                            ReadersDataGrid.Items.Refresh();
                            // Load lại dữ liệu
                            LoadPenaltyReceiptsData();
                            ClearPenaltyInputs();

                            MessageBox.Show("Cập nhật phiếu thu tiền phạt thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            // Khôi phục lại giá trị cũ nếu có lỗi
                            docGia.TongNo = docGia.TongNo + newSoTienThu - oldSoTienThu;
                            throw new Exception($"Lỗi khi lưu thay đổi: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật phiếu thu tiền phạt: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phiếu thu tiền phạt để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearReader_Click(object sender, RoutedEventArgs e)
        {
            ClearInputs();
            icGioiThieuError.Visibility = Visibility.Collapsed;
            icLoaiDocGiaError.Visibility = Visibility.Collapsed;
            icNgayHetHanError.Visibility = Visibility.Collapsed;
            icNgayLapTheError.Visibility = Visibility.Collapsed;
            icNgayThuPhatError.Visibility = Visibility.Collapsed;
            RefreshReadersTab();
        }

        private void ClearReaderType_Click(object sender, RoutedEventArgs e)
        {
            ClearReaderTypeInputs();
            icSoSachMuonToiDaError.Visibility = Visibility.Collapsed;
            icLoaiDocGiaError.Visibility = Visibility.Collapsed;
            RefreshReaderTypesTab();
        }

        private void ClearPenalty_Click(object sender, RoutedEventArgs e)
        {
            ClearPenaltyInputs();
            RefreshPenaltyReceiptsTab();
        }

        private void RefreshReadersTab()
        {
            Readers.Clear();
            LoadReadersData();
            ReadersDataGrid.Items.Refresh();
            TenTaiKhoanComboBox.SelectedItem = null;
            TenLoaiDocGiaComboBox.SelectedItem = null;
        }

        private void RefreshReaderTypesTab()
        {
            ReaderTypes.Clear();
            LoadReaderTypesData();
            ReaderTypesDataGrid.Items.Refresh();
        }

        private void RefreshPenaltyReceiptsTab()
        {
            PenaltyReceipts.Clear();
            LoadPenaltyReceiptsData();
            PenaltyReceiptsDataGrid.Items.Refresh();
        }

        private void PrintPenaltyReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (PenaltyReceiptsDataGrid.SelectedItem is PHIEUTHUTIENPHAT selectedPenalty)
            {
                // Mở cửa sổ AWPhieuThuTienPhat để hiển thị phiếu thu
                AWPhieuThuTienPhat printWindow = new AWPhieuThuTienPhat(selectedPenalty);
                printWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phiếu thu tiền phạt để in.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PenaltyReceiptsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PenaltyReceiptsDataGrid.SelectedItem is PHIEUTHUTIENPHAT selectedPenalty)
            {
                // Chọn tên tài khoản tương ứng trong ComboBox
                TenTaiKhoanPhatComboBox.SelectedItem = selectedPenalty.IDDocGiaNavigation.IDTaiKhoanNavigation.TenTaiKhoan;
                NgayThuPhat.SelectedDate = selectedPenalty.NgayThu;
                SoTienThu.Text = selectedPenalty.SoTienThu.ToString();
                TongNoPhat.Text = selectedPenalty.TongNo.ToString();
                ConLai.Text = selectedPenalty.ConLai.ToString();
            }
        }

        private void TTPSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = NormalizeString(TTPSearchTextBox.Text.Trim());
            string searchCriteria = (TTPSearchCriteriaComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var query = _context.PHIEUTHUTIENPHAT.Include(p => p.IDDocGiaNavigation).AsQueryable();

            switch (searchCriteria)
            {
                case "Tên Tài Khoản":
                    query = query.Where(p => p.IDDocGiaNavigation.IDTaiKhoanNavigation.TenTaiKhoan.ToLower().Contains(searchTerm));
                    break;
                case "Số Tiền Thu":
                    query = query.Where(p => p.SoTienThu.ToString().Contains(searchTerm));
                    break;
                default:
                    // Nếu không có tiêu chí nào được chọn, tìm kiếm trên tất cả các trường (bao gồm cả Tên tài khoản)
                    query = query.Where(p =>
                        p.IDDocGiaNavigation.IDTaiKhoanNavigation.TenTaiKhoan.ToLower().Contains(searchTerm) ||
                        p.SoTienThu.ToString().Contains(searchTerm)
                    );
                    break;
            }

            var filteredPenalties = query.ToList();
            PenaltyReceipts.Clear();
            foreach (var penalty in filteredPenalties)
            {
                PenaltyReceipts.Add(penalty);
            }
        }

        private void ClearPenaltyInputs()
        {
            TenTaiKhoanPhatComboBox.SelectedItem = null;
            NgayThuPhat.SelectedDate = null;
            SoTienThu.Text = string.Empty;
            TongNoPhat.Text = string.Empty;
            ConLai.Text = string.Empty;
        }


        // Import and Export 
        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Excel Files|*.xlsx";

                if (openFileDialog.ShowDialog() == true)
                {
                    var filePath = openFileDialog.FileName;
                    bool importSuccess = true; // Biến theo dõi trạng thái import

                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        var worksheet = package.Workbook.Worksheets[0]; // Lấy sheet đầu tiên

                        // Bỏ qua dòng tiêu đề
                        int row = 2;

                        while (worksheet.Cells[row, 1].Value != null) // Đọc đến khi gặp dòng trống
                        {
                            // Lấy dữ liệu từ các cột tương ứng, bắt đầu từ cột thứ 1 (không lấy mã độc giả)
                            string tenTaiKhoan = worksheet.Cells[row, 1].Value?.ToString();
                            string tenLoaiDocGia = worksheet.Cells[row, 2].Value?.ToString();

                            // Xử lý ngày tháng, kiểm tra giá trị "0" hoặc trống
                            DateTime ngayLapThe;
                            if (!DateTime.TryParseExact(worksheet.Cells[row, 3].Value?.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ngayLapThe))
                            {
                                MessageBox.Show($"Lỗi khi chuyển đổi ngày lập thẻ tại dòng {row}. Vui lòng kiểm tra lại định dạng (dd/MM/yyyy).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                importSuccess = false;
                                break; // Bỏ qua dòng này và tiếp tục với dòng tiếp theo
                            }

                            DateTime ngayHetHan;
                            if (!DateTime.TryParseExact(worksheet.Cells[row, 4].Value?.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ngayHetHan))
                            {
                                MessageBox.Show($"Lỗi khi chuyển đổi ngày hết hạn tại dòng {row}. Vui lòng kiểm tra lại định dạng (dd/MM/yyyy).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                importSuccess = false;
                                break;
                            }

                            decimal tongNo = decimal.Parse(worksheet.Cells[row, 5].Value?.ToString());
                            string gioiThieu = worksheet.Cells[row, 6].Value?.ToString();

                            // Tìm ID tương ứng trong database
                            var taiKhoan = _context.TAIKHOAN.FirstOrDefault(tk => tk.TenTaiKhoan == tenTaiKhoan);
                            var loaiDocGia = _context.LOAIDOCGIA.FirstOrDefault(ldg => ldg.TenLoaiDocGia == tenLoaiDocGia);

                            if (taiKhoan == null || loaiDocGia == null)
                            {
                                MessageBox.Show($"Tên tài khoản hoặc tên loại độc giả không hợp lệ tại dòng {row}. Vui lòng kiểm tra lại dữ liệu Excel.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                importSuccess = false;
                                break; // Dừng import nếu có lỗi
                            }

                            // Tạo độc giả mới (không cần truyền MaDocGia)
                            var newReader = new DOCGIA
                            {
                                IDTaiKhoan = taiKhoan.ID,
                                IDLoaiDocGia = loaiDocGia.ID,
                                TongNo = tongNo,
                                GioiThieu = gioiThieu
                            };

                            _context.DOCGIA.Add(newReader);
                            _context.SaveChanges();

                            taiKhoan.NgayMo = ngayLapThe;
                            taiKhoan.NgayDong = ngayHetHan;
                            _context.SaveChanges();

                            row++;
                        }
                    }
                    if (importSuccess)
                    {
                        LoadReadersData(); // Cập nhật lại DataGrid
                        MessageBox.Show("Nhập dữ liệu từ Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi nhập dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel Files|*.xlsx";

                if (saveFileDialog.ShowDialog() == true)
                {
                    var filePath = saveFileDialog.FileName;

                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Danh sách độc giả");

                        // Tạo tiêu đề cột 
                        worksheet.Cells[1, 1].Value = "Mã độc giả";
                        worksheet.Cells[1, 2].Value = "Tên tài khoản";
                        worksheet.Cells[1, 3].Value = "Tên loại độc giả";
                        worksheet.Cells[1, 4].Value = "Ngày lập thẻ";
                        worksheet.Cells[1, 5].Value = "Ngày hết hạn";
                        worksheet.Cells[1, 6].Value = "Tổng nợ";
                        worksheet.Cells[1, 7].Value = "Giới thiệu";

                        // Điền dữ liệu 
                        int row = 2;
                        foreach (var reader in Readers)
                        {
                            worksheet.Cells[row, 1].Value = reader.MaDocGia;
                            worksheet.Cells[row, 2].Value = reader.IDTaiKhoanNavigation?.TenTaiKhoan;
                            worksheet.Cells[row, 3].Value = reader.IDLoaiDocGiaNavigation?.TenLoaiDocGia;
                            worksheet.Cells[row, 6].Value = reader.TongNo;
                            worksheet.Cells[row, 7].Value = reader.GioiThieu;

                            var taiKhoan = _context.TAIKHOAN
                                .Where(tk => tk.ID == reader.IDTaiKhoan)
                                .FirstOrDefault();

                            worksheet.Cells[row, 4].Value = taiKhoan.NgayMo.ToString("dd/MM/yyyy");
                            worksheet.Cells[row, 5].Value = taiKhoan.NgayDong.ToString("dd/MM/yyyy");
                            row++;
                        }

                        // Lưu 
                        var fileInfo = new FileInfo(filePath);
                        package.SaveAs(fileInfo);
                        MessageBox.Show("Xuất dữ liệu ra Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF Files|*.pdf",
                    FileName = "DanhSachDocGia.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        Document document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
                        PdfWriter writer = PdfWriter.GetInstance(document, fs);
                        document.Open();

                        // Font 
                        BaseFont baseFont = BaseFont.CreateFont("c:/windows/fonts/arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        Font titleFont = new Font(baseFont, 16, Font.BOLD);
                        Font headerFont = new Font(baseFont, 10, Font.BOLD);
                        Font dataFont = new Font(baseFont, 10, Font.NORMAL);

                        // Tiêu đề
                        Paragraph title = new Paragraph("DANH SÁCH ĐỘC GIẢ", titleFont)
                        {
                            Alignment = Element.ALIGN_CENTER
                        };
                        document.Add(title);
                        document.Add(new Paragraph(" ")); // Khoảng trắng

                        // Tạo bảng với 7 cột
                        PdfPTable table = new PdfPTable(7)
                        {
                            WidthPercentage = 100,
                            SpacingBefore = 10f,
                            SpacingAfter = 10f
                        };
                        table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 3f });

                        // Tiêu đề cột
                        string[] headers = {
                    "Mã độc giả",
                    "Tên tài khoản",
                    "Tên loại độc giả",
                    "Ngày lập thẻ",
                    "Ngày hết hạn",
                    "Tổng nợ",
                    "Giới thiệu"
                };

                        foreach (string header in headers)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                            {
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                VerticalAlignment = Element.ALIGN_MIDDLE,
                                Padding = 5f
                            };
                            table.AddCell(cell);
                        }

                        // Dữ liệu
                        var readers = _context.DOCGIA
                            .Include(r => r.IDTaiKhoanNavigation)
                            .Include(r => r.IDLoaiDocGiaNavigation)
                            .ToList();

                        foreach (var reader in readers)
                        {
                            table.AddCell(new Phrase(reader.MaDocGia.ToString(), dataFont));
                            table.AddCell(new Phrase(reader.IDTaiKhoanNavigation?.TenTaiKhoan ?? "", dataFont));
                            table.AddCell(new Phrase(reader.IDLoaiDocGiaNavigation?.TenLoaiDocGia ?? "", dataFont));

                            var taiKhoan = _context.TAIKHOAN
                                .Where(tk => tk.ID == reader.IDTaiKhoan)
                                .FirstOrDefault();

                            table.AddCell(new Phrase(taiKhoan.NgayMo.ToString("dd/MM/yyyy"), dataFont));
                            table.AddCell(new Phrase(taiKhoan.NgayDong.ToString("dd/MM/yyyy") ?? "N/A", dataFont));

                            table.AddCell(new Phrase(reader.TongNo.ToString("N2"), dataFont));
                            table.AddCell(new Phrase(reader.GioiThieu?.ToString() ?? "", dataFont));
                        }

                        document.Add(table);
                        document.Close();

                        MessageBox.Show("Xuất PDF thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm kiểm tra còn lỗi không
        public bool HasError()
        {
            // Tìm tất cả các PackIcon
            foreach (var icon in FindVisualChildren<PackIcon>(this))
            {
                if (icon.Style == FindResource("ErrorIcon") && icon.Visibility == Visibility.Visible)
                {
                    return true;
                }
            }
            return false;
        }

        // Hàm tìm kiếm các control con
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
        // Xử lý sự kiện cho TenLoaiDocGiaTextBox
        private void TenLoaiDocGiaTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TenLoaiDocGiaTextBox.Text))
            {
                icLoaiDocGiaError.ToolTip = "Tên Loại Độc Giả không được để trống";
                icLoaiDocGiaError.Visibility = Visibility.Visible;
            }
            else
            {
                icLoaiDocGiaError.Visibility = Visibility.Collapsed;
            }

            // Kiểm tra tên loại độc giả chỉ chứa chữ cái và khoảng trắng
            if (!Regex.IsMatch(TenLoaiDocGiaTextBox.Text, @"^[a-zA-Z\p{L}\s]+$"))
            {
                icLoaiDocGiaError.ToolTip = "Tên loại độc giả chỉ được chứa chữ cái và khoảng trắng!";
                icLoaiDocGiaError.Visibility = Visibility.Visible;
            }
            else
            {
                icLoaiDocGiaError.Visibility = Visibility.Collapsed;
            }
        }

        // Xử lý sự kiện cho SoSachMuonToiDaTextBox
        private void SoSachMuonToiDaTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SoSachMuonToiDaTextBox.Text))
            {
                icSoSachMuonToiDaError.ToolTip = "Số Sách Mượn Tối Đa không được để trống";
                icSoSachMuonToiDaError.Visibility = Visibility.Visible;
            }
            else if (!int.TryParse(SoSachMuonToiDaTextBox.Text, out int soSach) || soSach <= 0)
            {
                icSoSachMuonToiDaError.ToolTip = "Số Sách Mượn Tối Đa phải là số nguyên dương";
                icSoSachMuonToiDaError.Visibility = Visibility.Visible;
            }
            else
            {
                icSoSachMuonToiDaError.Visibility = Visibility.Collapsed;
            }
        }
    }
}
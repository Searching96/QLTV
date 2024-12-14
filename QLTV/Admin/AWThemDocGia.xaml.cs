using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QLTV.Models;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace QLTV.Admin
{
    public partial class AWThemDocGia : Window
    {
        private QLTVContext _context; 
        private string _tenTaiKhoan;
        private string _tenLoaiDocGia;

        public AWThemDocGia(QLTVContext context, string tenTaiKhoan, string tenLoaiDocGia) 
        {
            InitializeComponent();
            _context = context; // Gán context
            _tenTaiKhoan = tenTaiKhoan;
            _tenLoaiDocGia = tenLoaiDocGia;
            LoadLoaiDocGia();
            LoadTenTaiKhoan(); 
            SetDefaultDates();

            cbbTenLoaiDocGia.SelectedItem = _tenLoaiDocGia;
        }

        private void LoadTenTaiKhoan()
        {
            // Sử dụng context nhận được từ cửa sổ chính
            var dsTenTaiKhoan = _context.TAIKHOAN
                .Where(tk => !_context.DOCGIA.Any(dg => dg.IDTaiKhoan == tk.ID))
                .Select(tk => tk.TenTaiKhoan)
                .ToList();

            cbbTenTaiKhoan.ItemsSource = dsTenTaiKhoan;
        }

        private void SetDefaultDates()
        {
            dpNgayLapThe.SelectedDate = DateTime.Now;
            dpNgayHetHan.SelectedDate = DateTime.Now.AddYears(1);
        }

        private void LoadLoaiDocGia()
        {
            using (var context = new QLTVContext())
            {
                var dsLoaiDocGia = _context.LOAIDOCGIA
                .Where(ldg => !ldg.IsDeleted)
                .Select(ldg => ldg.TenLoaiDocGia)
                .ToList();

                cbbTenLoaiDocGia.ItemsSource = dsLoaiDocGia;
            }
            // Đăng ký sự kiện TextChanged
        }

        private void cbbTenTaiKhoan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Kiểm tra đã chọn tên tài khoản chưa
            if (cbbTenTaiKhoan.SelectedItem != null)
            {
                icTenTaiKhoanError.Visibility = Visibility.Collapsed;
            }
        }

        private void cbbTenLoaiDocGia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Kiểm tra đã chọn loại độc giả chưa
            if (cbbTenLoaiDocGia.SelectedItem != null)
            {
                icTenLoaiDocGiaError.Visibility = Visibility.Collapsed;
            }
        }

        private void tbxGioiThieu_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Kiểm tra thông tin giới thiệu không quá dài
            if (!string.IsNullOrWhiteSpace(tbxGioiThieu.Text) && tbxGioiThieu.Text.Length > 200)
            {
                icGioiThieuError.ToolTip = "Thông Tin Giới Thiệu không được vượt quá 200 kí tự";
                icGioiThieuError.Visibility = Visibility.Visible;
                return;
            }

            icGioiThieuError.Visibility = Visibility.Collapsed;
        }

        private void tbxTongNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Kiểm tra tổng nợ không để trống
            if (string.IsNullOrWhiteSpace(tbxTongNo.Text))
            {
                icTongNoError.ToolTip = "Tổng Nợ không được để trống";
                icTongNoError.Visibility = Visibility.Visible;
                return;
            }

            // Kiểm tra tổng nợ là số nguyên
            if (!int.TryParse(tbxTongNo.Text, out int tongNo) || tongNo < 0)
            {
                icTongNoError.ToolTip = "Tổng Nợ phải là số nguyên không âm";
                icTongNoError.Visibility = Visibility.Visible;
                return;
            }

            // Giới hạn tổng nợ
            if (tongNo > 10000000)
            {
                icTongNoError.ToolTip = "Tổng Nợ không được vượt quá 10 triệu";
                icTongNoError.Visibility = Visibility.Visible;
                return;
            }

            icTongNoError.Visibility = Visibility.Collapsed;
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

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra các trường bắt buộc
                if (string.IsNullOrWhiteSpace(cbbTenTaiKhoan.Text))
                {
                    icTenTaiKhoanError.ToolTip = "Tên Tài Khoản không được để trống";
                    icTenTaiKhoanError.Visibility = Visibility.Visible;
                }

                if (cbbTenLoaiDocGia.SelectedItem == null)
                {
                    icTenLoaiDocGiaError.ToolTip = "Phải chọn Loại Độc Giả";
                    icTenLoaiDocGiaError.Visibility = Visibility.Visible;
                }

                if (string.IsNullOrWhiteSpace(tbxTongNo.Text))
                {
                    icTongNoError.ToolTip = "Tổng Nợ không được để trống";
                    icTongNoError.Visibility = Visibility.Visible;
                }

                // Kiểm tra còn lỗi không
                if (HasError())
                {
                    MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi thêm!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Lấy thông tin từ các trường
                string tenTaiKhoan = cbbTenTaiKhoan.SelectedItem.ToString(); // Lấy từ SelectedItem
                string loaiDocGia = cbbTenLoaiDocGia.SelectedItem.ToString();
                string gioiThieu = tbxGioiThieu.Text;
                decimal tongNo = decimal.Parse(tbxTongNo.Text);
                DateTime ngayLapThe = dpNgayLapThe.SelectedDate.Value;
                DateTime ngayHetHan = dpNgayHetHan.SelectedDate.Value;

                // Lấy ID của Loại Độc Giả
                int idLoaiDocGia = _context.LOAIDOCGIA
                    .Where(ldg => !ldg.IsDeleted && ldg.TenLoaiDocGia == loaiDocGia)
                    .Select(ldg => ldg.ID)
                    .FirstOrDefault();

                // Lấy ID của Tài Khoản
                int idTaiKhoan = _context.TAIKHOAN
                    .Where(tk => tk.TenTaiKhoan == tenTaiKhoan)
                    .Select(tk => tk.ID)
                    .FirstOrDefault();

                if (idTaiKhoan != 0) 
                {
                    // Create a new DOCGIA record
                    var newDocGia = new DOCGIA()
                    {
                        IDTaiKhoan = idTaiKhoan,
                        IDLoaiDocGia = idLoaiDocGia,
                        GioiThieu = gioiThieu,
                        TongNo = tongNo,
                    };

                    _context.DOCGIA.Add(newDocGia);
                    _context.SaveChanges();

                    var taiKhoan = _context.TAIKHOAN
                        .Where(tk => tk.ID == idTaiKhoan)
                        .FirstOrDefault();

                    taiKhoan.NgayMo = ngayLapThe;
                    taiKhoan.NgayDong = ngayHetHan;
                    _context.SaveChanges();

                    MessageBox.Show("Thêm độc giả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Đóng cửa sổ sau khi thêm thành công
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy tài khoản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm độc giả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
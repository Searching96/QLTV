using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QLTV.Models;
using System.Windows.Media;
using iTextSharp.text.pdf.codec;

namespace QLTV.Admin
{
    public partial class AWThemPhieuThuTienPhat : Window
    {
        private QLTVContext _context; 

        public AWThemPhieuThuTienPhat(QLTVContext context) //Truyền context
        {
            InitializeComponent();
            _context = context; 
            LoadTenTaiKhoan();
            SetDefaultDate();
        }

        private void SetDefaultDate()
        {
            dpNgayThu.SelectedDate = DateTime.Now;
        }

        private void LoadTenTaiKhoan()
        {
            var dsTenTaiKhoan = _context.DOCGIA
                .Include(dg => dg.IDTaiKhoanNavigation)
                .Select(dg => dg.IDTaiKhoanNavigation.TenTaiKhoan)
                .ToList();

            cbbTenTaiKhoan.ItemsSource = dsTenTaiKhoan;
        }

        private void cbbTenTaiKhoan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (var context = new QLTVContext())
            {
                string tenTK = cbbTenTaiKhoan.SelectedItem?.ToString() ?? string.Empty;

                var docGia = context.DOCGIA
                    .Where(dg => dg.IDTaiKhoanNavigation.TenTaiKhoan == tenTK)
                    .FirstOrDefault();

                tbxDangNo.Text = docGia.TongNo.ToString();
            }

            // Kiểm tra đã chọn tên tài khoản chưa
            if (cbbTenTaiKhoan.SelectedItem != null)
            {
                icTenTaiKhoanError.Visibility = Visibility.Collapsed;
            }
        }

        private void dpNgayThu_Loaded(object sender, RoutedEventArgs e)
        {
            // Tìm TextBox bên trong DatePicker
            var textBox = (dpNgayThu.Template.FindName("PART_TextBox", dpNgayThu) as TextBox);
            if (textBox != null)
            {
                textBox.TextChanged += NgayThu_TextChanged;
            }
        }

        private void NgayThu_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            // Danh sách định dạng hỗ trợ nhiều cách nhập ngày
            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy" };
            if (!DateTime.TryParseExact(textBox.Text, formats, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out DateTime ngayThu))
            {
                icNgayThuError.ToolTip = "Ngày Thu không hợp lệ (định dạng đúng: dd/MM/yyyy)";
                icNgayThuError.Visibility = Visibility.Visible;
                return;
            }

            // Kiểm tra ngày thu không được sau ngày hiện tại
            if (ngayThu > DateTime.Now)
            {
                icNgayThuError.ToolTip = "Ngày Thu không được sau ngày hiện tại";
                icNgayThuError.Visibility = Visibility.Visible;
                return;
            }

            // Nếu hợp lệ, ẩn thông báo lỗi
            icNgayThuError.Visibility = Visibility.Collapsed;
        }

        private void tbxSoTienThu_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Kiểm tra số tiền thu không để trống
            if (string.IsNullOrWhiteSpace(tbxSoTienThu.Text))
            {
                icSoTienThuError.ToolTip = "Số Tiền Thu không được để trống";
                icSoTienThuError.Visibility = Visibility.Visible;
                return;
            }

            // Kiểm tra số tiền thu là số nguyên không âm
            if (!int.TryParse(tbxSoTienThu.Text, out int soTienThu) || soTienThu < 0)
            {
                icSoTienThuError.ToolTip = "Số Tiền Thu phải là số nguyên không âm";
                icSoTienThuError.Visibility = Visibility.Visible;
                return;
            }

            icSoTienThuError.Visibility = Visibility.Collapsed;
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra các trường bắt buộc
                if (cbbTenTaiKhoan.SelectedItem == null)
                {
                    icTenTaiKhoanError.ToolTip = "Phải chọn Tên Tài Khoản";
                    icTenTaiKhoanError.Visibility = Visibility.Visible;
                }

                if (string.IsNullOrWhiteSpace(tbxSoTienThu.Text))
                {
                    icSoTienThuError.ToolTip = "Số Tiền Thu không được để trống";
                    icSoTienThuError.Visibility = Visibility.Visible;
                }

                using (var context = new QLTVContext())
                {
                    string tenTK = cbbTenTaiKhoan.SelectedItem?.ToString() ?? string.Empty;

                    var docGia = context.DOCGIA
                        .Where(dg => dg.IDTaiKhoanNavigation.TenTaiKhoan == tenTK)
                        .FirstOrDefault();

                    if (int.Parse(tbxSoTienThu.Text) > docGia.TongNo)
                    {
                        icSoTienThuError.ToolTip = "Số Tiền Thu không được lớn số đang nợ";
                        icSoTienThuError.Visibility = Visibility.Visible;
                    }
                }

                    // Kiểm tra còn lỗi không
                    if (HasError())
                    {
                        MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi thêm!", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                // Lấy thông tin từ các trường
                string tenTaiKhoan = cbbTenTaiKhoan.SelectedItem.ToString();
                DateTime ngayThu = dpNgayThu.SelectedDate.Value;
                decimal soTienThu = decimal.Parse(tbxSoTienThu.Text);

                var taiKhoan = _context.TAIKHOAN.FirstOrDefault(tk => tk.TenTaiKhoan == tenTaiKhoan);
                if (taiKhoan == null)
                {
                    icTenTaiKhoanError.ToolTip = "Tên tài khoản không tồn tại trong cơ sở dữ liệu!";
                    icTenTaiKhoanError.Visibility = Visibility.Visible;
                    return; 
                }

                // Lấy ID của Độc Giả
                int idDocGia = _context.DOCGIA
                    .Include(dg => dg.IDTaiKhoanNavigation)
                    .Where(dg => dg.IDTaiKhoanNavigation.TenTaiKhoan == tenTaiKhoan)
                    .Select(dg => dg.ID)
                    .FirstOrDefault();

                // Kiểm tra xem độc giả có tồn tại không
                if (idDocGia != 0)
                {
                    // Lấy tổng nợ hiện tại của độc giả
                    decimal tongNoHienTai = _context.DOCGIA
                        .Where(dg => dg.ID == idDocGia)
                        .Select(dg => dg.TongNo)
                        .FirstOrDefault();

                    // Tạo phiếu thu tiền phạt mới
                    var newPhieuThu = new PHIEUTHUTIENPHAT()
                    {
                        IDDocGia = idDocGia,
                        NgayThu = ngayThu,
                        TongNo = tongNoHienTai,
                        SoTienThu = soTienThu,
                        ConLai = tongNoHienTai - soTienThu,
                        IsDeleted = false
                    };

                    _context.PHIEUTHUTIENPHAT.Add(newPhieuThu);

                    // Cập nhật tổng nợ của độc giả
                    var docGia = _context.DOCGIA.Find(idDocGia);
                    docGia.TongNo = newPhieuThu.ConLai;

                    _context.SaveChanges();

                    MessageBox.Show("Thêm phiếu thu tiền phạt thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy độc giả!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm phiếu thu tiền phạt: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
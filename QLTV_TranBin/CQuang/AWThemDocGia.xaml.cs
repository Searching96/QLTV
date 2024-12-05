using MaterialDesignThemes.Wpf;
using QLTV_TranBin.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace QLTV_TranBin.CQuang
{
    /// <summary>
    /// Interaction logic for AWThemDocGia.xaml
    /// </summary>
    public partial class AWThemDocGia : Window
    {
        private string _tenTaiKhoan;
        private string _tenLoaiDocGia;

        public AWThemDocGia(string tenTaiKhoan, string tenLoaiDocGia)
        {
            InitializeComponent();
            _tenTaiKhoan = tenTaiKhoan;
            _tenLoaiDocGia = tenLoaiDocGia;
            LoadLoaiDocGia();

            tbxTenTaiKhoan.Text = _tenTaiKhoan;
            cbbTenLoaiDocGia.SelectedItem = _tenLoaiDocGia;
        }

        private void LoadLoaiDocGia()
        {
            using (var context = new QLTV2Context())
            {
                var dsLoaiDocGia = context.LOAIDOCGIA
                    .Where(ldg => !ldg.IsDeleted)
                    .Select(ldg => ldg.TenLoaiDocGia)
                    .ToList();

                cbbTenLoaiDocGia.ItemsSource = dsLoaiDocGia;
            }
            // Đăng ký sự kiện TextChanged
        }

        private void tbxTenTaiKhoan_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Kiểm tra tên tài khoản không để trống
            if (string.IsNullOrWhiteSpace(tbxTenTaiKhoan.Text))
            {
                icTenTaiKhoanError.ToolTip = "Tên Tài Khoản không được để trống";
                icTenTaiKhoanError.Visibility = Visibility.Visible;
                return;
            }

            icTenTaiKhoanError.Visibility = Visibility.Collapsed;
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
        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra các trường bắt buộc
                if (string.IsNullOrWhiteSpace(tbxTenTaiKhoan.Text))
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
                string tenTaiKhoan = tbxTenTaiKhoan.Text;
                string loaiDocGia = cbbTenLoaiDocGia.SelectedItem.ToString();
                string gioiThieu = tbxGioiThieu.Text;
                decimal tongNo = decimal.Parse(tbxTongNo.Text);

                using (var context = new QLTV2Context())
                {
                    // Lấy ID của Loại Độc Giả
                    int idLoaiDocGia = context.LOAIDOCGIA
                        .Where(ldg => !ldg.IsDeleted && ldg.TenLoaiDocGia == loaiDocGia)
                        .Select(ldg => ldg.ID)
                        .FirstOrDefault();

                    // Lấy ID của Tài Khoản
                    int idTaiKhoan = context.TAIKHOAN
                        .Where(tk => tk.TenTaiKhoan == tenTaiKhoan)
                        .Select(tk => tk.ID)
                        .FirstOrDefault();

                    if (idTaiKhoan != 0) // Kiểm tra xem tài khoản có tồn tại không
                    {
                        // Create a new DOCGIA record
                        var newDocGia = new DOCGIA()
                        {
                            IDTaiKhoan = idTaiKhoan,
                            IDLoaiDocGia = idLoaiDocGia,
                            GioiThieu = gioiThieu,
                            TongNo = tongNo
                        };

                        context.DOCGIA.Add(newDocGia);
                        context.SaveChanges();

                        // Hiển thị thông báo thành công
                        MessageBox.Show("Thêm độc giả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Đóng cửa sổ sau khi thêm thành công
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        // Xử lý trường hợp không tìm thấy tài khoản
                        MessageBox.Show("Không tìm thấy tài khoản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi
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

using MaterialDesignThemes.Wpf;
using QLTV.Models;
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

namespace QLTV.Admin
{
    /// <summary>
    /// Interaction logic for AWThemDocGia.xaml
    /// </summary>
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
                        TongNo = tongNo
                    };

                    _context.DOCGIA.Add(newDocGia);
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

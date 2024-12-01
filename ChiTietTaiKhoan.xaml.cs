using Microsoft.EntityFrameworkCore;
using QLTV_TranBin.GridViewModels;
using QLTV_TranBin.Models;
using QLTV_TranBin.Properties;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QLTV_TranBin
{
    /// <summary>
    /// Interaction logic for ChiTietTaiKhoan.xaml
    /// </summary>
    

    public partial class ChiTietTaiKhoan : UserControl
    {
        private AccountViewModel? _currentAccount; // Thuộc tính lưu DataContext

        public ChiTietTaiKhoan()
        {
            InitializeComponent();
            DataContextChanged += ChiTietTaiKhoan_DataContextChanged; // Gắn sự kiện DataContextChanged
            
           
        }
        public void LoadData()
        {
            
            if(_currentAccount?.IDPhanQuyen == 4)
            {
                txtNgayDong.Text = "Ngày hết hạn";
                txtNgayMo.Text = "Ngày lập thẻ";
                
            }
            else
            {
                txtNgayDong.Text = "Ngày kết thúc";
                txtNgayMo.Text = "Ngày vào làm";
                txtNgayDong.Text = "Ngày hết hạn";
                txtNgayMo.Text = "Ngày lập thẻ";

            }
            if (Settings.Default.CurrentUserPhanQuyen == 4)
            {
                btnEdit0.Visibility = Visibility.Visible;
                btnEdit1.Visibility = Visibility.Visible;
                btnEdit2.Visibility = Visibility.Visible;
                btnEdit3.Visibility = Visibility.Visible;
                btnEdit4.Visibility = Visibility.Visible;
                btnEdit5.Visibility = Visibility.Visible;
                btnEdit6.Visibility = Visibility.Collapsed;
                btnEdit7.Visibility = Visibility.Collapsed;
                btnEdit8.Visibility = Visibility.Collapsed;
            } 
            else
            {
                btnEdit0.Visibility = Visibility.Collapsed;
                btnEdit1.Visibility = Visibility.Collapsed;
                btnEdit2.Visibility = Visibility.Collapsed;
                btnEdit3.Visibility = Visibility.Collapsed;
                btnEdit4.Visibility = Visibility.Collapsed;
                btnEdit5.Visibility = Visibility.Collapsed;
                btnEdit6.Visibility = Visibility.Collapsed;
                btnEdit7.Visibility = Visibility.Visible;
                btnEdit8.Visibility = Visibility.Visible;
            }
              
        }

        private void ChiTietTaiKhoan_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Lấy DataContext mới khi nó thay đổi
            _currentAccount = DataContext as AccountViewModel;
            LoadData();
        }

        private void btnEdit0_Click(object sender, RoutedEventArgs e)
        {
            txtTenNguoiDung.IsReadOnly = false; // Cho phép chỉnh sửa
            txtTenNguoiDung.Focus(); // Đặt focus vào ô nhập liệu
        }
        private void btnEdit1_Click(object sender, RoutedEventArgs e)
        {
            txtGioiTinh.IsReadOnly = false; // Cho phép chỉnh sửa
            txtGioiTinh.Focus(); // Đặt focus vào ô nhập liệu
        }
        private void btnEdit2_Click(object sender, RoutedEventArgs e)
        {
            txtDiaChi.IsReadOnly = false; // Cho phép chỉnh sửa
            txtDiaChi.Focus(); // Đặt focus vào ô nhập liệu
        }
        private void btnEdit3_Click(object sender, RoutedEventArgs e)
        {
            dpNgaySinh.IsEnabled = true; // Cho phép chỉnh sửa
            dpNgaySinh.Focus(); // Đặt focus vào DatePicker
        }
        private void btnEdit4_Click(object sender, RoutedEventArgs e)
        {
            txtEmail.IsReadOnly = false; // Cho phép chỉnh sửa
            txtEmail.Focus(); // Đặt focus vào ô nhập liệu
        }
        private void btnEdit5_Click(object sender, RoutedEventArgs e)
        {
            txtSDT.IsReadOnly = false; // Cho phép chỉnh sửa
            txtSDT.Focus(); // Đặt focus vào ô nhập liệu
        }
        private void btnEdit6_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void btnEdit7_Click(object sender, RoutedEventArgs e)
        {
            dpNgayDangKy.IsEnabled = true; // Cho phép chỉnh sửa
            dpNgayDangKy.Focus(); // Đặt focus vào DatePicker
        }
        private void btnEdit8_Click(object sender, RoutedEventArgs e)
        {
            dpNgayHetHan.IsEnabled = true; // Cho phép chỉnh sửa
            dpNgayHetHan.Focus(); // Đặt focus vào DatePicker
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new QLTVContext())
                {
                    // Lấy thông tin từ các TextBox
                    
                    var taiKhoan = context.TAIKHOAN
                        .Include(t => t.IDPhanQuyenNavigation) // Tải bảng PHANQUYEN liên quan
                        .FirstOrDefault(u => u.MaTaiKhoan == _currentAccount.MaTaiKhoan && !u.IsDeleted);
                    if (taiKhoan == null)
                    {
                        MessageBox.Show("Không tìm thấy tài khoản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Cập nhật thông tin vào bảng TAIKHOAN
                    taiKhoan.DiaChi = txtDiaChi.Text;
                    taiKhoan.SinhNhat = dpNgaySinh.SelectedDate.HasValue ? dpNgaySinh.SelectedDate.Value : throw new Exception("Ngày sinh không hợp lệ!");
                    taiKhoan.Email = txtEmail.Text;
                    taiKhoan.SDT = txtSDT.Text;
                    taiKhoan.NgayMo = dpNgayDangKy.SelectedDate ?? DateTime.Now; // Nếu null thì dùng ngày hiện tại
                    taiKhoan.NgayDong = dpNgayHetHan.SelectedDate ?? DateTime.Now.AddYears(1); // Dùng ngày hiện tại + 1 năm nếu null
                    MessageBox.Show(taiKhoan.ID.ToString());
                    // Xử lý cập nhật thông tin TenNguoiDung và GioiTinh
                    if (txtLoaiTaiKhoan.Text == "Độc Giả")
                    {
                        var docGia = context.DOCGIA.FirstOrDefault(u => u.IDTaiKhoan == taiKhoan.ID);
                        if (docGia != null)
                        {
                            docGia.TenDocGia = txtTenNguoiDung.Text;
                            docGia.GioiTinh = txtGioiTinh.Text;
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy thông tin Độc Giả!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        var admin = context.ADMIN.FirstOrDefault(u => u.IDTaiKhoan == taiKhoan.ID);
                        if (admin != null)
                        {
                            admin.TenAdmin = txtTenNguoiDung.Text;
                            admin.GioiTinh = txtGioiTinh.Text;
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy thông tin ADMIN!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    // Lưu thay đổi
                    context.SaveChanges();
                    MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

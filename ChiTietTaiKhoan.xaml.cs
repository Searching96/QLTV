using Microsoft.EntityFrameworkCore;
using QLTV_TranBin.GridViewModels;
using QLTV_TranBin.Models;
using QLTV_TranBin.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<string> LoaiTaiKhoanItems { get; set; }
        public string SelectedLoaiTaiKhoan { get; set; }
        public ChiTietTaiKhoan()
        {
            InitializeComponent();
            DataContextChanged += ChiTietTaiKhoan_DataContextChanged; // Gắn sự kiện DataContextChanged
            
        }
        private void LoadLoaiTaiKhoanData()
        {
           
            // Khởi tạo danh sách
            LoaiTaiKhoanItems = new ObservableCollection<string>();

            using (var db = new QLTV2Context())
            {
                if (_currentAccount?.IDPhanQuyen == 4)
                {
                    // Lấy TenLoaiDocGia từ bảng LOAIDOCGIA
                    var loaiDocGiaList = db.LOAIDOCGIA
                                           .Where(ldg => !ldg.IsDeleted)
                                           .Select(ldg => ldg.TenLoaiDocGia)
                                           .ToList();

                    foreach (var item in loaiDocGiaList)
                    {
                        LoaiTaiKhoanItems.Add(item!);
                    }
                }
                else
                {
                    // Lấy MaHanhDong từ bảng PHANQUYEN
                    var phanQuyenList = db.PHANQUYEN
                                          .Where(pq => !pq.IsDeleted && pq.ID != 4)
                                          .Select(pq => pq.MoTa)
                                          .ToList();

                    foreach (var item in phanQuyenList)
                    {
                        LoaiTaiKhoanItems.Add(item);
                    }
                }
            }

            // Gán danh sách vào ComboBox
            cbLoaiTaiKhoan.ItemsSource = LoaiTaiKhoanItems;

            // Đặt giá trị mặc định
            if (LoaiTaiKhoanItems.Any())
            {
                cbLoaiTaiKhoan.SelectedIndex = 0;
                SelectedLoaiTaiKhoan = LoaiTaiKhoanItems[0];
            }
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
            }
            

        }

        private void ChiTietTaiKhoan_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Lấy DataContext mới khi nó thay đổi
            _currentAccount = DataContext as AccountViewModel;
            if (_currentAccount != null)
            {
                LoadData();
                LoadLoaiTaiKhoanData();
            }
        }

        

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new QLTV2Context())
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
                    taiKhoan.HoTen = txtTenNguoiDung.Text;
                    taiKhoan.GioiTinh = txtGioiTinh.Text;
                    // Xử lý cập nhật loại tài khoản dựa vào IDPhanQuyen
                    if (_currentAccount.IDPhanQuyen == 4)
                    {
                        // Trường hợp DocGia: Cập nhật thông tin vào bảng DOCGIA
                        var selectedLoaiDocGia = cbLoaiTaiKhoan.SelectedItem as string;
                        if (string.IsNullOrEmpty(selectedLoaiDocGia))
                        {
                            MessageBox.Show("Vui lòng chọn loại độc giả!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Lấy ID loại độc giả từ bảng LOAIDOCGIA
                        var loaiDocGia = context.LOAIDOCGIA
                            .FirstOrDefault(ldg => ldg.TenLoaiDocGia == selectedLoaiDocGia && !ldg.IsDeleted);

                        if (loaiDocGia == null)
                        {
                            MessageBox.Show("Loại độc giả không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Tìm đối tượng DOCGIA liên quan
                        var docGia = context.DOCGIA.FirstOrDefault(dg => dg.IDTaiKhoan == taiKhoan.ID);
                        if (docGia == null)
                        {
                            MessageBox.Show("Không tìm thấy thông tin độc giả liên quan!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Cập nhật thông tin DOCGIA
                        docGia.IDLoaiDocGia = loaiDocGia.ID;
                        
                    }
                    else
                    {
                        // Trường hợp Admin: Cập nhật thông tin IDPhanQuyen từ ComboBox
                        var selectedMoTa = cbLoaiTaiKhoan.SelectedItem as string;
                        if (string.IsNullOrEmpty(selectedMoTa))
                        {
                            MessageBox.Show("Vui lòng chọn mã hành động!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Lấy ID phân quyền từ bảng PHANQUYEN
                        var phanQuyen = context.PHANQUYEN
                            .FirstOrDefault(pq => pq.MoTa == selectedMoTa && !pq.IsDeleted);

                        if (phanQuyen == null)
                        {
                            MessageBox.Show("Mã hành động không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Cập nhật IDPhanQuyen
                        taiKhoan.IDPhanQuyen = phanQuyen.ID;
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

        private void btnEditBasicInfo_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra quyền truy cập
            if (Settings.Default.CurrentUserPhanQuyen == 4)
            {
                // Cho phép chỉnh sửa tất cả các trường
                txtTenNguoiDung.IsReadOnly = false;
                txtGioiTinh.IsReadOnly = false;
                txtDiaChi.IsReadOnly = false;
                dpNgaySinh.IsEnabled = true;
                txtEmail.IsReadOnly = false;
                txtSDT.IsReadOnly = false;
                cbLoaiTaiKhoan.IsEnabled = false;
                dpNgayDangKy.IsEnabled = false;
                dpNgayHetHan.IsEnabled = false;                
            }
            else
            {
                // Không cho phép chỉnh sửa nếu không đủ quyền
                txtTenNguoiDung.IsReadOnly = true;
                txtGioiTinh.IsReadOnly = true;
                txtDiaChi.IsReadOnly = true;
                dpNgaySinh.IsEnabled = true;
                txtEmail.IsReadOnly = true;
                txtSDT.IsReadOnly = true;
                cbLoaiTaiKhoan.IsEnabled = true;
                dpNgayDangKy.IsEnabled = true;
                dpNgayHetHan.IsEnabled = true;
            }
        }
    }
}

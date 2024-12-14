
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using QLTV.GridViewModels;
using QLTV.Models;
using QLTV.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

using System.IO;


namespace QLTV.User
{
    /// <summary>
    /// Interaction logic for ChiTietTaiKhoan.xaml
    /// </summary>
    

    public partial class ChiTietTaiKhoan : UserControl
    {
        private AccountViewModel? _currentAccount; // Thuộc tính lưu DataContext
        public ObservableCollection<string> LoaiTaiKhoanItems { get; set; }
        private string _selectedLoaiTaiKhoan;
        public string SelectedLoaiTaiKhoan
        {
            get => _selectedLoaiTaiKhoan;
            set
            {
                if (_selectedLoaiTaiKhoan != value)
                {
                    _selectedLoaiTaiKhoan = value;
                    OnPropertyChanged(nameof(SelectedLoaiTaiKhoan));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ChiTietTaiKhoan()
        {
            InitializeComponent();
            DataContextChanged += ChiTietTaiKhoan_DataContextChanged; // Gắn sự kiện DataContextChanged
            
        }

        

        private void LoadLoaiTaiKhoanData(AccountViewModel _currentAccount )
        {
            // Khởi tạo danh sách
            LoaiTaiKhoanItems = new ObservableCollection<string>();
            string temp = "";

            using (var db = new QLTVContext())
            {
                if (_currentAccount?.IDPhanQuyen == 4) // Trường hợp độc giả
                {
                    // Lấy danh sách TenLoaiDocGia từ bảng LOAIDOCGIA
                    var loaiDocGiaList = db.LOAIDOCGIA
                                           .Where(ldg => !ldg.IsDeleted)
                                           .Select(ldg => ldg.TenLoaiDocGia)
                                           .ToList();

                    foreach (var item in loaiDocGiaList)
                    {
                        LoaiTaiKhoanItems.Add(item!);
                    }

                    // Tìm loại độc giả hiện tại của tài khoản này từ bảng DOCGIA
                    
                    var tk = db.TAIKHOAN.FirstOrDefault(t => t.MaTaiKhoan == _currentAccount.MaTaiKhoan && !t.IsDeleted);
                    
                    var currentLoaiDocGia = db.DOCGIA
                                              .Where(dg => dg.IDTaiKhoan == tk.ID)
                                              .Select(dg => dg.IDLoaiDocGiaNavigation.TenLoaiDocGia)
                                              .FirstOrDefault();
                    
                    if (currentLoaiDocGia != null)
                    {
                        temp = currentLoaiDocGia;
                    }
                }
                else // Trường hợp không phải độc giả (Admin hoặc các quyền khác)
                {
                    // Lấy danh sách MoTa từ bảng PHANQUYEN, loại trừ ID = 4 (DOCGIA)
                    var phanQuyenList = db.PHANQUYEN
                                          .Where(pq => !pq.IsDeleted && pq.ID != 4)
                                          .Select(pq => pq.MoTa)
                                          .ToList();

                    foreach (var item in phanQuyenList)
                    {
                        LoaiTaiKhoanItems.Add(item);
                    }

                    // Tìm quyền hiện tại của tài khoản từ bảng PHANQUYEN
                    
                    var currentMoTa = db.PHANQUYEN
                                        .Where(pq => !pq.IsDeleted && pq.ID == _currentAccount.IDPhanQuyen)
                                        .Select(pq => pq.MoTa)
                                        .FirstOrDefault();

                    if (currentMoTa != null)
                    {
                        temp = currentMoTa;
                        
                    }
                }
            }

            // Gán danh sách vào ComboBox
            cbLoaiTaiKhoan.ItemsSource = LoaiTaiKhoanItems;
            SelectedLoaiTaiKhoan = temp;
            cbLoaiTaiKhoan.Text = SelectedLoaiTaiKhoan;
            
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
            // Gán DataContext mới vào _currentAccount
            _currentAccount = DataContext as AccountViewModel;
            
            if (_currentAccount != null)
            {
                
                AccountViewModel temp  = _currentAccount;
                LoadData();
                LoadLoaiTaiKhoanData(temp);
            }
        }

        private bool ValidateInputs()
        {
            // Kiểm tra các TextBox
            if (string.IsNullOrWhiteSpace(txtDiaChi.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtSDT.Text) ||
                string.IsNullOrWhiteSpace(txtTenNguoiDung.Text) ||
                string.IsNullOrWhiteSpace(txtGioiTinh.Text) ||
                !dpNgaySinh.SelectedDate.HasValue ||
                !dpNgayDangKy.SelectedDate.HasValue ||
                !dpNgayHetHan.SelectedDate.HasValue ||
                cbLoaiTaiKhoan.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                    return;
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

                    // Kiểm tra số điện thoại chỉ chứa số
                    if (string.IsNullOrEmpty(txtSDT.Text) || !txtSDT.Text.All(char.IsDigit))
                    {
                        MessageBox.Show("Số điện thoại chỉ được chứa số!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Kiểm tra giới tính chỉ được là "Nam" hoặc "Nữ"
                    if (txtGioiTinh.Text != "Nam" && txtGioiTinh.Text != "Nữ")
                    {
                        MessageBox.Show("Giới tính chỉ được chọn Nam hoặc Nữ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                txtTenNguoiDung.IsReadOnly = false;
                txtGioiTinh.IsReadOnly = false;
                txtDiaChi.IsReadOnly = false;
                dpNgaySinh.IsEnabled = false;
                txtEmail.IsReadOnly = true;
                txtSDT.IsReadOnly = false;
                cbLoaiTaiKhoan.IsEnabled = true;
                dpNgayDangKy.IsEnabled = true;
                dpNgayHetHan.IsEnabled = true;
            }
        }

        private void ChangeAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            // Hiển thị hộp thoại để chọn ảnh mới
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All Files (*.*)|*.*",
                Title = "Chọn ảnh mới"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Lấy đường dẫn file ảnh
                    string selectedImagePath = openFileDialog.FileName;

                    // Binding dữ liệu cho Avatar 


                    // Nếu cần, lưu dữ liệu vào cơ sở dữ liệu
                    using (var context = new QLTVContext())
                    {
                        var tk = context.TAIKHOAN.FirstOrDefault(cv => cv.MaTaiKhoan == _currentAccount.MaTaiKhoan);
                        if (tk != null)
                        {
                            tk.Avatar = selectedImagePath;
                            context.SaveChanges();
                        }
                    }
                    if (!string.IsNullOrEmpty(selectedImagePath))
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(selectedImagePath, UriKind.Absolute);
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        AvatarImage.ImageSource = bitmapImage; // Cập nhật hình ảnh hiển thị
                    }
                    MessageBox.Show("Cập nhật ảnh thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);


                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void txtTenNguoiDung_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenNguoiDung.Text))
            {
                icFullNameError.ToolTip = "Họ và tên không được để trống!";
                icFullNameError.Visibility = Visibility.Visible;
                return;
            }

            icFullNameError.Visibility = Visibility.Collapsed;
        }

        private void txtGioiTinh_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtGioiTinh.Text) || (txtGioiTinh.Text != "Nam" && txtGioiTinh.Text != "Nữ"))
            {
                icGioiTinhError.ToolTip = "Giới tính chỉ là Nam hoặc Nữ và không được bỏ trống!";
                icGioiTinhError.Visibility = Visibility.Visible;
                return;
            }

            icGioiTinhError.Visibility = Visibility.Collapsed;
        }

        private void txtDiaChi_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDiaChi.Text))
            {
                icAddressError.ToolTip = "Địa chỉ không được để trống!";
                icAddressError.Visibility = Visibility.Visible;
                return;
            }

            icAddressError.Visibility = Visibility.Collapsed;
        }

        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                icEmailError.ToolTip = "Email không được để trống!";
                icEmailError.Visibility = Visibility.Visible;
                return;
            }

            icEmailError.Visibility = Visibility.Collapsed;
        }

        private void txtSDT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSDT.Text) || !decimal.TryParse(txtSDT.Text, out decimal result))
            {
                icPhoneNumberError.ToolTip = "Số điện thoại chỉ bao gồm số và không được để trống!";
                icPhoneNumberError.Visibility = Visibility.Visible;
                return;
            }

            icPhoneNumberError.Visibility = Visibility.Collapsed;
        }
    }
}

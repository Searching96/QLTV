using HandyControl.Tools.Command;
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
using System.Windows.Shapes;

namespace QLTV_TranBin
{
    /// <summary>
    /// Interaction logic for UserScreen.xaml
    /// </summary>
    public partial class UserScreen : Window
    {
        public ICommand CloseTabCommand { get; }
        private readonly QLTVContext _context;

        public UserScreen()
        {
            InitializeComponent();
            _context = new QLTVContext();
            CloseTabCommand = new RelayCommand(CloseTab);
            DataContext = this; // Đặt DataContext cho Window
            RequireAccess();
        }
        
        public void RequireAccess()
        {
            // Lấy ID của người dùng hiện tại
            int currentUserID = Settings.Default.CurrentUserID;

            using (var context = new QLTVContext())
            {
                // Lấy đối tượng DOCGIA theo ID tài khoản
                var userDocGia = context.DOCGIA.FirstOrDefault(u => u.IDTaiKhoan == currentUserID);

                // Kiểm tra nếu không tìm thấy hoặc thông tin còn thiếu
                if (userDocGia == null ||
                    string.IsNullOrEmpty(userDocGia.TenDocGia) ||
                    string.IsNullOrEmpty(userDocGia.GioiTinh) ||
                    string.IsNullOrEmpty(userDocGia.GioiThieu))
                {
                    // Hiển thị cửa sổ yêu cầu điền thông tin
                    MessageBox.Show(
                        "Thông tin của bạn chưa đầy đủ. Vui lòng cập nhật thông tin trước khi tiếp tục!",
                        "Thông báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

                    // Mở cửa sổ để điền thông tin
                    var updateInfoWindow = new UpdateTTDG(userDocGia); // Cửa sổ cập nhật thông tin
                    updateInfoWindow.ShowDialog(); // Hiển thị và chờ người dùng hoàn tất việc điền thông tin

                    // Sau khi cửa sổ cập nhật đóng lại, kiểm tra lại thông tin
                    if (string.IsNullOrEmpty(userDocGia.TenDocGia) ||
                        string.IsNullOrEmpty(userDocGia.GioiTinh) ||
                        string.IsNullOrEmpty(userDocGia.GioiThieu))
                    {
                        // Nếu vẫn thiếu, thông báo và không cho phép truy cập
                        MessageBox.Show(
                            "Bạn chưa hoàn tất cập nhật thông tin. Không thể tiếp tục!",
                            "Thông báo",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );

                        // Đóng cửa sổ hiện tại (nếu cần)
                        Application.Current.Shutdown();
                    }
                }
            }

        }
        private void CloseTab(object parameter)
        {
            if (parameter is TabItem tabItem && tcUser.Items.Contains(tabItem))
            {
                tcUser.Items.Remove(tabItem); // Xóa tab
            }
        }
        public void OpenBookDetailTab(SACH selectedBook)
        {
            // Kiểm tra nếu đã có tab chi tiết cho sách này
            foreach (TabItem tabItem in tcUser.Items)
            {
                if (tabItem.Tag is SACH book && book.ID == selectedBook.ID)
                {
                    tcUser.SelectedItem = tabItem; // Chuyển đến tab này
                    return;
                }
            }

            // Tạo một instance của UCChiTietSach và gán dữ liệu sách vào DataContext
            var bookDetailControl = new QLTV_TranBin.UCChiTietSach
            {
                DataContext = selectedBook
            };

            // Tạo một TabItem mới để chứa UCChiTietSach
            var bookDetailTab = new TabItem
            {
                Header = selectedBook.IDTuaSachNavigation.TenTuaSach,
                Content = bookDetailControl,
                Tag = selectedBook
            };

            // Thêm tab mới vào TabControl
            tcUser.Items.Add(bookDetailTab);
            tcUser.SelectedItem = bookDetailTab; // Chuyển đến tab mới
        }
        private void btnTrangChu_Click(object sender, RoutedEventArgs e)
        {
            // Chuyển đến tab Trang Chủ
            tcUser.SelectedIndex = 0;
        }

        private void tbBookFilter_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btnThongBao_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSachMuon_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentUserID = Settings.Default.CurrentUserID;

                // Lấy AccountViewModel
                var accountViewModel = GetAccountViewModelByUserID(currentUserID);
                if (accountViewModel == null)
                {
                    MessageBox.Show("Không tìm thấy tài khoản hoặc thông tin độc giả!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Kiểm tra nếu TabControl không được tìm thấy
                if (tcUser == null)
                {
                    MessageBox.Show("TabControl không được tìm thấy!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Kiểm tra xem Tab đã tồn tại chưa
                var existingTab = tcUser.Items
                    .OfType<TabItem>()
                    .FirstOrDefault(tab => tab.Header?.ToString() == $"Profile - {accountViewModel.TenTaiKhoan}");

                if (existingTab != null)
                {
                    // Chuyển sang Tab nếu đã tồn tại
                    tcUser.SelectedItem = existingTab;
                }
                else
                {
                    // Tạo UserControl mới
                    var profileControl = new ChiTietTaiKhoan
                    {
                        DataContext = accountViewModel // Truyền dữ liệu
                    };

                    // Tạo TabItem mới
                    var newTab = new TabItem
                    {
                        Header = $"Profile - {accountViewModel.TenTaiKhoan}",
                        Content = profileControl
                    };

                    // Thêm Tab vào TabControl
                    tcUser.Items.Add(newTab);

                    // Chuyển sang Tab mới
                    tcUser.SelectedItem = newTab;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private AccountViewModel? GetAccountViewModelByUserID(int currentUserID)
        {
            using (var context = new QLTVContext())
            {
                // Truy xuất tài khoản từ bảng TAIKHOAN
                var taiKhoan = context.TAIKHOAN
                   .Include(t => t.IDPhanQuyenNavigation) // Tải bảng PHANQUYEN liên quan
                   .FirstOrDefault(t => t.ID == currentUserID);
                if (taiKhoan == null)
                {
                    return null; // Không tìm thấy tài khoản
                }
                MessageBox.Show(taiKhoan.IDPhanQuyenNavigation.MoTa);
                // Truy xuất thông tin từ bảng DOCGIA
                var docGia = context.DOCGIA.FirstOrDefault(dg => dg.IDTaiKhoan == currentUserID);

                // Tạo AccountViewModel và kết hợp dữ liệu từ hai bảng
                return new AccountViewModel
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    TenTaiKhoan = taiKhoan.TenTaiKhoan,
                    Email = taiKhoan.Email,
                    SDT = taiKhoan.SDT,
                    DiaChi = taiKhoan.DiaChi,
                    NgaySinh = taiKhoan.SinhNhat,
                    NgayDangKy = taiKhoan.NgayMo,
                    NgayHetHan = taiKhoan.NgayDong,
                    IDPhanQuyen = taiKhoan.IDPhanQuyen,
                    LoaiTaiKhoan = taiKhoan.IDPhanQuyenNavigation.MoTa,
                    TenNguoiDung = docGia?.TenDocGia, // Lấy từ bảng DOCGIA
                    GioiTinh = docGia?.GioiTinh,     // Lấy từ bảng DOCGIA
                         
                };
                
            }
        }

    }
    // RelayCommand Implementation
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

using Microsoft.EntityFrameworkCore;
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
    /// Interaction logic for MainScreen.xaml
    /// </summary>
    public partial class MainScreen : Window
    {
        private readonly QLTVContext _context;

        public MainScreen()
        {
            InitializeComponent();
            //ConfigureButtonsVisibility();
            _context = new QLTVContext();
            RequireAccess();// Khởi tạo context của bạn

        }
        public void RequireAccess()
        {
            // Lấy ID của người dùng hiện tại
            int currentUserID = Settings.Default.CurrentUserID;

            using (var context = new QLTVContext())
            {
                // Lấy đối tượng DOCGIA theo ID tài khoản
                var userAdmin = context.ADMIN.FirstOrDefault(u => u.IDTaiKhoan == currentUserID);
                
                
                // Kiểm tra nếu không tìm thấy hoặc thông tin còn thiếu
                if (userAdmin == null ||
                    string.IsNullOrEmpty(userAdmin.TenAdmin) ||
                    string.IsNullOrEmpty(userAdmin.GioiTinh))
                    
                {
                    // Hiển thị cửa sổ yêu cầu điền thông tin
                    MessageBox.Show(
                        "Thông tin của bạn chưa đầy đủ. Vui lòng cập nhật thông tin trước khi tiếp tục!",
                        "Thông báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

                    // Mở cửa sổ để điền thông tin
                    var updateInfoWindow = new UpdateTTAD(userAdmin); // Cửa sổ cập nhật thông tin
                    updateInfoWindow.ShowDialog(); // Hiển thị và chờ người dùng hoàn tất việc điền thông tin

                    // Sau khi cửa sổ cập nhật đóng lại, kiểm tra lại thông tin
                    if (string.IsNullOrEmpty(userAdmin.TenAdmin) ||
                        string.IsNullOrEmpty(userAdmin.GioiTinh))
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
        //private void ConfigureButtonsVisibility()
        //{
        //    // Lấy role hiện tại từ Settings
        //    int currentRole = Settings.Default.CurrentUserPhanQuyen;

        //    // Role: Superadmin
        //    if (currentRole == 1)
        //    {
        //        btn_BookManage.Visibility = Visibility.Visible;  // Hiện Quản lý sách
        //        btnQLDG.Visibility = Visibility.Visible;        // Hiện Quản lý độc giả
        //        btnQLTK.Visibility = Visibility.Visible;        // Hiện Quản lý tài khoản
        //    }
        //    // Role: Quản lý nhân sự
        //    else if (currentRole == 2)
        //    {
        //        btn_BookManage.Visibility = Visibility.Collapsed; // Ẩn Quản lý sách
        //        btnQLDG.Visibility = Visibility.Collapsed;       // Ẩn Quản lý độc giả
        //        btnQLTK.Visibility = Visibility.Visible;         // Hiện Quản lý tài khoản
        //    }
        //    // Role: Thủ thư
        //    else if (currentRole == 3)
        //    {
        //        btn_BookManage.Visibility = Visibility.Visible;  // Hiện Quản lý sách
        //        btnQLDG.Visibility = Visibility.Visible;        // Hiện Quản lý độc giả
        //        btnQLTK.Visibility = Visibility.Collapsed;      // Ẩn Quản lý tài khoản
        //    }
        //    // Các role khác (nếu có)
        //    else
        //    {
        //        btn_BookManage.Visibility = Visibility.Collapsed;
        //        btnQLDG.Visibility = Visibility.Collapsed;
        //        btnQLTK.Visibility = Visibility.Collapsed;
        //    }
        //}
        private void btnQLTK_Click(object sender, RoutedEventArgs e)
        {
            FrameMain.Content = new QuanLyTaiKhoan();
        }

        private void btn_DashBoard_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_QLMT_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_BookManage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnQLDG_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_LogOut(object sender, RoutedEventArgs e)
        {
            string sessionToken = Settings.Default.SessionToken;
            int userID = Settings.Default.CurrentUserID;
            var session = _context.ACTIVE_SESSION.FirstOrDefault(s => s.SessionToken == sessionToken);
            var user = _context.TAIKHOAN.FirstOrDefault(u => u.ID == userID);
            if (session != null)
            {
                _context.ACTIVE_SESSION.Remove(session);
                _context.SaveChanges();
            }
            if (user != null)
            {
                user.TrangThai = false;
                _context.SaveChanges();
            }

            Settings.Default.SessionToken = string.Empty;
            Settings.Default.Save();
            Settings.Default.CurrentUserID = -1;
            Settings.Default.Save();

            LoginScreen loginScreen = new LoginScreen();
            loginScreen.Show();
            this.Close();

        }

        
    }
}

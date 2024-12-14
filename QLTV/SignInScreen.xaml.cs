using Microsoft.EntityFrameworkCore;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QLTV.Properties;
using Org.BouncyCastle.Tls;
using QLTV.Admin;
using QLTV.User;

namespace QLTV
{
    /// <summary>
    /// Interaction logic for SignInScreen.xaml
    /// </summary>
    public partial class SignInScreen : UserControl
    {
        private readonly QLTVContext _context;

        public SignInScreen()
        {
            InitializeComponent();
            _context = new QLTVContext();
            
        }


        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        

        

        

        

        private void textEmail_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtUsername.Focus();
        }

        
        bool isLogin = false;
        private void Login_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !isLogin)
            {
                ((Storyboard)this.Resources["LoginShowAnimation"]).Begin();
                isLogin = true;
            }
        }
        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = pbPassword.Password;

            // Xác thực người dùng
            var user = ValidateUser(username, password);
            if (user != null)
            {
                user.TrangThai = true;
                _context.SaveChanges();
                // Nếu người dùng hợp lệ, kiểm tra session
                if (IsSessionValid(user.ID))
                {
                    // Nếu session hợp lệ, tự động đăng nhập và mở MainWindow
                    if (user.IDPhanQuyen < 4)
                    {
                        
                        AWTrangQuanLy ms = new AWTrangQuanLy();
                        ms.Show();
                    }
                    else
                    {
                        UWTrangDocGia us = new UWTrangDocGia();
                        us.Show();
                    }    
                    var parentWindow = Window.GetWindow(this) as LoginScreen;
                    if (parentWindow != null)
                    {
                        
                        parentWindow.Close();
                    }
                }
                else
                {
                    // Nếu chưa có session hoặc session hết hạn, tạo session mới
                    CreateSession(user);

                    // Lưu ID người dùng vào Properties.Settings
                    Settings.Default.CurrentUserID = user.ID;
                    Settings.Default.CurrentUserPhanQuyen = user.IDPhanQuyen;
                    Settings.Default.Save();
                    // Mở MainWindow và đóng cửa sổ đăng nhập
                    if (user.IDPhanQuyen < 4)
                    {

                        AWTrangQuanLy ms = new AWTrangQuanLy();
                        ms.Show();
                    }
                    else
                    {
                        UWTrangDocGia us = new UWTrangDocGia();
                        us.Show();
                    }
                    var parentWindow = Window.GetWindow(this) as LoginScreen;
                    if (parentWindow != null)
                    {
                        parentWindow.Close();
                    }
                }
            }
            else
            {
                // Hiển thị thông báo nếu tài khoản không hợp lệ hoặc mật khẩu sai
                MessageBox.Show("Invalid username or password!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool IsSessionValid(int userId)
        {
            // Kiểm tra xem người dùng có session hợp lệ không
            var session = _context.ACTIVE_SESSION
                .Where(s => s.IDTaiKhoan == userId && s.ExpiryTime > DateTime.Now)
                .OrderByDescending(s => s.ExpiryTime)
                .FirstOrDefault();

            return session != null;
        }
        private void CreateSession(TAIKHOAN user)
        {
            var session = new ACTIVE_SESSION
            {
                IDTaiKhoan = user.ID,
                SessionToken = Guid.NewGuid().ToString(),
                ExpiryTime = DateTime.Now.AddMinutes(30)
            };

            _context.ACTIVE_SESSION.Add(session);
            _context.SaveChanges();

            Settings.Default.SessionToken = session.SessionToken;
            Settings.Default.Save();
        }
        private TAIKHOAN ValidateUser(string username, string password)
        {
            // Tìm tài khoản người dùng từ cơ sở dữ liệu
            TAIKHOAN user = null;
            if (!username.Contains("@"))
                user = _context.TAIKHOAN.FirstOrDefault(u => u.TenTaiKhoan == username && u.MatKhau == password && !u.IsDeleted);
            else
                user = _context.TAIKHOAN.FirstOrDefault(u => u.Email == username && u.MatKhau == password && !u.IsDeleted);
            return user;
        }
        

        private void btnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as LoginScreen;
            if (parentWindow != null)
            {
                // Gán ResetPassword làm nội dung mới cho Frame LoginMain
                parentWindow.LoginMain.Content = new ResetPassword();
            }
        }
    }
}

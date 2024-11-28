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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QLTV_TranBin
{
    /// <summary>
    /// Interaction logic for SignUpScreen.xaml
    /// </summary>
    public partial class SignUpScreen : UserControl
    {
        public SignUpScreen()
        {
            InitializeComponent();
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            // Tìm cửa sổ cha (LoginScreen)
            var parentWindow = Window.GetWindow(this) as LoginScreen;
            if (parentWindow != null)
            {
                // Gán SignUpScreen làm nội dung mới cho Frame LoginMain
                parentWindow.LoginMain.Content = new SignInScreen();
            }

        }
        private void btnSignUp_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dữ liệu từ các TextBox và PasswordBox
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string address = txtAddress.Text.Trim();
            string phoneNumber = txtPhoneNumber.Text.Trim();
            DateTime? birthday = dpBirthday.SelectedDate;
            string password = pbPassword.Password.Trim();
            string repeatPassword = pbRepeatPassword.Password.Trim();

            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill in all required fields!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Kiểm tra mật khẩu
            if (password != repeatPassword)
            {
                MessageBox.Show("Passwords do not match!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new QLTVContext())
                {
                    // Kiểm tra email đã tồn tại chưa
                    var existingAccount = context.TAIKHOAN.FirstOrDefault(tk => tk.Email == email);
                    if (existingAccount != null)
                    {
                        MessageBox.Show("Email already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Tạo đối tượng tài khoản mới
                    var newAccount = new TAIKHOAN
                    {
                        TenTaiKhoan = username,
                        Email = email,
                        DiaChi = address,
                        SDT = phoneNumber,
                        SinhNhat = birthday.HasValue ? birthday.Value : default(DateTime),
                        MatKhau = password, // Lưu ý: Bạn nên mã hóa mật khẩu trước khi lưu
                        IsDeleted = false, // Cờ đánh dấu tài khoản còn hoạt động
                        Avatar = "hehe",
                        TrangThai = false,
                        IDPhanQuyen = 1
                    };

                    // Thêm tài khoản mới vào cơ sở dữ liệu
                    context.TAIKHOAN.Add(newAccount);
                    context.SaveChanges();

                    MessageBox.Show("Đăng ký thành công!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Reset các trường nhập liệu
                    ClearFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm reset các trường nhập liệu
        private void ClearFields()
        {
            txtUsername.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtAddress.Text = string.Empty;
            txtPhoneNumber.Text = string.Empty;
            dpBirthday.SelectedDate = null;
            pbPassword.Password = string.Empty;
            pbRepeatPassword.Password = string.Empty;
        }

        
    }
}

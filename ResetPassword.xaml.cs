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
using MailKit.Net.Smtp;
using MimeKit;
using QLTV_TranBin.Models;
using static MaterialDesignThemes.Wpf.Theme;

namespace QLTV_TranBin
{
    /// <summary>
    /// Interaction logic for ResetPassword.xaml
    /// </summary>
    public partial class ResetPassword : UserControl
    {
        private string verificationCode;
        
        
        public ResetPassword()
        {
            InitializeComponent();
            
        }

        private void btnSendCode_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text;

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please enter a valid email.");
                return;
            }

            verificationCode = GenerateVerificationCode();
            bool isSent = SendEmailUsingMailKit(email, verificationCode);
            if (isSent)
            {
                MessageBox.Show("Verification code has been sent to your email.");
            }
            else
            {
                MessageBox.Show("Failed to send verification code. Please try again.");
            }

        }
        private string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Mã 6 chữ số
        }
        public bool SendEmailUsingMailKit(string recipientEmail, string code)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("QLTV TranBin", "thunderstar848@gmail.com")); // Địa chỉ Gmail của bạn
                message.To.Add(new MailboxAddress("", recipientEmail));
                message.Subject = "Password Reset Verification Code";

                message.Body = new TextPart("plain")
                {
                    Text = $"Your verification code is: {code}"
                };

                using (var client = new SmtpClient())
                {
                    // Kết nối tới máy chủ Gmail
                    client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                    // Sử dụng App Password thay vì mật khẩu thông thường
                    client.Authenticate("thunderstar848@gmail.com", "arqd unir cttu vhgi");

                    client.Send(message);
                    client.Disconnect(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return false;
            }
        }


        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (txtCode.Text == verificationCode)
            {
                // Lấy mật khẩu từ hai ô PasswordBox
                string newPassword = passwordBox.Password;
                string repeatPassword = repeatPasswordBox.Password;

                // Kiểm tra xem mật khẩu nhập lại có khớp không
                if (newPassword != repeatPassword)
                {
                    MessageBox.Show("Passwords do not match. Please try again.");
                    return;
                }

                try
                {
                    using (var context = new QLTV2Context())
                    {
                        // Tìm tài khoản theo email
                        var account = context.TAIKHOAN.FirstOrDefault(tk => tk.Email == txtEmail.Text);

                        if (account != null)
                        {
                            // Cập nhật mật khẩu
                            account.MatKhau = newPassword;

                            // Lưu thay đổi vào cơ sở dữ liệu
                            context.SaveChanges();
                            MessageBox.Show("Password updated successfully!");

                            var parentWindow = Window.GetWindow(this) as LoginScreen;
                            if (parentWindow != null)
                            {
                                // Gán SignInScreen làm nội dung mới cho Frame LoginMain
                                parentWindow.LoginMain.Content = new SignInScreen();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Account not found. Please check your email.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Incorrect verification code. Please try again.");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as LoginScreen;
            if (parentWindow != null)
            {
                // Gán SignInScreen làm nội dung mới cho Frame LoginMain
                parentWindow.LoginMain.Content = new SignInScreen();
            }
        }
    }
}

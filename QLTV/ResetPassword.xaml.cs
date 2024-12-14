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
using QLTV.Models;
using static MaterialDesignThemes.Wpf.Theme;

namespace QLTV
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
                MessageBox.Show("Vui lòng điền một email phù hợp.");
                return;
            }

            verificationCode = GenerateVerificationCode();
            bool isSent = SendEmailUsingMailKit(email, verificationCode);
            if (isSent)
            {
                MessageBox.Show("Mã xác nhận đặt lại mật khẩu đã được gửi tới email của bạn.");
            }
            else
            {
                MessageBox.Show("Xảy ra lỗi khi gửi. Vui lòng thử lại!");
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
            // Tạo đối tượng email
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Quản lý thư viện LIMAN", "thuvienliman@gmail.com")); // Địa chỉ Gmail của bạn
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = "Yêu cầu đặt lại mật khẩu";

                // Nội dung HTML
                // Nội dung HTML
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Email OTP</title>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            line-height: 1.6;
                            margin: 0;
                            padding: 0;
                            background-color: #f4f4f4;
                        }}
                        .email-container {{
                            max-width: 600px;
                            margin: 20px auto;
                            background: #ffffff;
                            border: 1px solid #ddd;
                            border-radius: 8px;
                            padding: 20px;
                        }}
                        .email-header {{
                            text-align: center;
                            font-size: 24px;
                            font-weight: bold;
                            color: #333;
                            margin-bottom: 20px;
                        }}
                        .email-body {{
                            color: #555;
                            font-size: 16px;
                        }}
                        .otp-box {{
                            display: inline-block;
                            background-color: #007bff;
                            color: white;
                            font-size: 20px;
                            font-weight: bold;
                            padding: 10px 20px;
                            border-radius: 8px;
                            margin: 20px 0;
                            text-align: center;
                        }}
                        .email-footer {{
                            margin-top: 20px;
                            font-size: 14px;
                            color: #888;
                            text-align: center;
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='email-header'>
                            Quản lý thư viện LIMAN
                        </div>
                        <div class='email-body'>
                            <p>Chào bạn,</p>
                            <p>Hệ thống đã nhận được yêu cầu đặt lại mật khẩu của bạn. Vui lòng nhập mã OTP dưới đây để tiếp tục. Lưu ý, mã OTP chỉ có hiệu lực trong 5 phút. Nếu đây không phải là yêu cầu của bạn, xin vui lòng bỏ qua email này.</p>
                            <div class='otp-box'>
                                {code}
                            </div>
                        </div>
                        <div class='email-footer'>
                            Trân trọng,<br>
                            QLTV Development Team
                        </div>
                    </div>
                </body>
                </html>"
                };

                // Gán nội dung vào email
                message.Body = bodyBuilder.ToMessageBody();

            // Kết nối tới SMTP Server và gửi email
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                // Lưu ý: Sử dụng App Password của Gmail thay vì mật khẩu thông thường
                client.Authenticate("thuvienliman@gmail.com", "ujsk gxba hfgo cgzi");

                client.Send(message);
                client.Disconnect(true);
            }

            return true;
        }
        catch (Exception ex)
        {
            // Hiển thị lỗi nếu xảy ra vấn đề
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
                    MessageBox.Show("Mật khẩu không khớp. Thử lại.");
                    return;
                }

                try
                {
                    using (var context = new QLTVContext())
                    {
                        // Tìm tài khoản theo email
                        var account = context.TAIKHOAN.FirstOrDefault(tk => tk.Email == txtEmail.Text);

                        if (account != null)
                        {
                            // Cập nhật mật khẩu
                            account.MatKhau = newPassword;

                            // Lưu thay đổi vào cơ sở dữ liệu
                            context.SaveChanges();
                            MessageBox.Show("Cập nhật mật khẩu thành công!");

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
                MessageBox.Show("Mã yêu cầu đặt lại mật khẩu không đúng. Thử lại.");
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

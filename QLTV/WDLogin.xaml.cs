using System.Windows;
using System.Linq;
using QLTV.Models;
using System.Windows.Controls;
using System;
using QLTV;

namespace QLTV
{
    public partial class WDLogin : Window
    {
        private readonly QLTVContext _context;

        public WDLogin()
        {
            InitializeComponent();
            _context = new QLTVContext();

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;

            var user = _context.TAIKHOAN.FirstOrDefault(u => u.TenTaiKhoan == username && u.MatKhau == password);

            if (user != null)
            {
                this.Hide();
                UCInterface mainInterface = new UCInterface();
                mainInterface.Show();
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            var signUpWindow = new WDSignUp();
            signUpWindow.ShowDialog();
        }

        private void ForgotPasswordButton_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessageBox.Show("Invalid username or password.");
        }

        private void UsernameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
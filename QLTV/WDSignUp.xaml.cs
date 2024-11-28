using System;
using System.Windows;
using System.Windows.Controls;
using QLTV.Models;

namespace QLTV
{
    public partial class WDSignUp : Window
    {
        private readonly QLTVContext _context;

        public WDSignUp()
        {
            InitializeComponent();
            _context = new QLTVContext();
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            var tenTaiKhoan = UsernameTextBox.Text;
            var email = EmailTextBox.Text;
            var matKhau = PasswordBox.Password;
            var confirmPassword = ConfirmPasswordBox.Password;
            var sinhNhat = BirthdayPicker.SelectedDate;
            var diaChi = AddressTextBox.Text;
            var sdt = PhoneNumberTextBox.Text;

            if (string.IsNullOrWhiteSpace(tenTaiKhoan) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(matKhau) || string.IsNullOrWhiteSpace(confirmPassword) ||
                string.IsNullOrWhiteSpace(diaChi) || string.IsNullOrWhiteSpace(sdt) ||
                sinhNhat == null)
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (matKhau != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Invalid email format.");
                return;
            }

            var newTaiKhoan = new TAIKHOAN
            {
                TenTaiKhoan = tenTaiKhoan,
                Email = email,
                MatKhau = matKhau,
                SinhNhat = sinhNhat.Value,
                DiaChi = diaChi,
                SDT = sdt,
                Avatar = "Your Avatar URL", 
                TrangThai = true,
                IDPhanQuyen = 1 
            };

            _context.TAIKHOAN.Add(newTaiKhoan);
            _context.SaveChanges();

            MessageBox.Show("User registered successfully!");
            Close();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
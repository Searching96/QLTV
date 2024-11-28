using System.Windows;
using LoginApp.Data;
using LoginApp.Models;

namespace LoginApp
{
    public partial class WDSignUp : Window
    {
        private readonly AppDbContext _context;

        public WDSignUp()
        {
            InitializeComponent();
            _context = new AppDbContext();
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var email = EmailTextBox.Text;
            var password = PasswordBox.Password;
            var confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            var newUser = new User { Username = username, Email = email, Password = password };
            _context.Users.Add(newUser);
            _context.SaveChanges();

            MessageBox.Show("User registered successfully!");
            Close();
        }
    }
}
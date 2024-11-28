using System.Windows;
using System.Linq;
using LoginApp.Data;

namespace LoginApp
{
    public partial class WDForgotPassword : Window
    {
        private readonly AppDbContext _context;

        public WDForgotPassword()
        {
            InitializeComponent();
            _context = new AppDbContext();
        }

        private void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailTextBox.Text;

            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                MessageBox.Show("Password reset instructions have been sent to your email.");
                Close();
            }
            else
            {
                MessageBox.Show("No user found with this email address.");
            }
        }
    }
}
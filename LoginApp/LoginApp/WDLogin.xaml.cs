using System.Windows;
using System.Linq;
using LoginApp.Data;

namespace LoginApp
{
    public partial class WDLogin : Window
    {
        private readonly AppDbContext _context;

        public WDLogin()
        {
            InitializeComponent();
            _context = new AppDbContext();
          
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;

            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

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
            var forgotPassword = new WDForgotPassword();
            forgotPassword.ShowDialog();
        }

        private void UsernameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
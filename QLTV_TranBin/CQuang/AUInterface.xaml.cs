using QLTV_TranBin.Models;
using QLTV_TranBin.Properties;
using QLTV_TranBin.ViewModels;
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

namespace QLTV_TranBin.CQuang
{
    /// <summary>
    /// Interaction logic for AUInterface.xaml
    /// </summary>
    public partial class AUInterface : Window
    {
        private readonly QLTV2Context _context;
        public AUInterface()
        {
            InitializeComponent();
            DataContext = new CLClockViewModel();

            _context = new QLTV2Context();
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Dashboard");
        }

        private void ReaderManagement_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new UQuanLyDocGia();
        }

        private void BookManagement_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("BookManagement");
        }

        private void LoanManagement_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("LoanManagement");
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Report");
        }

        private void btnQLTK_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new QuanLyTaiKhoan();
        }

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
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

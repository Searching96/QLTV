using QLTV.Models;
using QLTV.Properties;
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

namespace QLTV.User
{
    /// <summary>
    /// Interaction logic for UWTrangDocGia.xaml
    /// </summary>
    public partial class UWTrangDocGia : Window
    {
        public List<UserControl> OpeningUC;

        public UWTrangDocGia()
        {
            InitializeComponent();
            OpeningUC = new List<UserControl>();
            USMainContent.Content = new UTrangChu();
        }

        private void OpenUC(UserControl uc)
        {
            UserControl existingUC = OpeningUC.FirstOrDefault(x => x.GetType() == uc.GetType());

            if (existingUC == null)
            {
                OpeningUC.Add(uc);
                USMainContent.Content = uc;
            }
            else
            {
                USMainContent.Content = existingUC;
            }
        }

        private void btnFnTrangChu_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new UTrangChu());
        }

        private void btnFnMuonSach_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new UUFnMuonSach());
        }

        private void btnFnDisplaySach_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new UUFnDisplaySach());
        }

        private void btnFnSachDangMuon_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new UUFnSachDangMuon());
        }

        private void DangXuat_Click(object sender, RoutedEventArgs e)
        {
            var _context = new QLTVContext();
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

using Microsoft.EntityFrameworkCore;
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
using QLTV.Models;
using QLTV.Admin;
using QLTV.User;

namespace QLTV
{
    /// <summary>
    /// Interaction logic for LoginScreen.xaml
    /// </summary>
    public partial class LoginScreen : Window
    {
        private readonly QLTVContext _context;

        public LoginScreen()
        {
            InitializeComponent();
            _context = new QLTVContext();
            CheckSession();
            LoginMain.Content = new SignInScreen();
        }
        private void CheckSession()
        {
            string sessionToken = Settings.Default.SessionToken;
            if (!string.IsNullOrEmpty(sessionToken))
            {
                var activeSession = _context.ACTIVE_SESSION
                    .FirstOrDefault(s => s.SessionToken == sessionToken && s.ExpiryTime > DateTime.Now);
                if (activeSession != null)
                {
                    int currentRoleID = Settings.Default.CurrentUserPhanQuyen;
                    if (currentRoleID < 4)
                    {

                        AWTrangQuanLy tql = new AWTrangQuanLy();
                        tql.Show();
                    }
                    else
                    {
                        UWTrangDocGia tdg = new UWTrangDocGia();
                        tdg.Show();
                    }
                    this.Close();
                }
            }
        }
    }
}

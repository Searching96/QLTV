using MaterialDesignColors;
using QLTV.Models;
using QLTV.Properties;
using System.ComponentModel;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto.Generators;

namespace QLTV.Admin
{
    /// <summary>
    /// Interaction logic for AWTrangQuanLy.xaml
    /// </summary>
    public partial class AWTrangQuanLy : Window
    {
        public List<UserControl> OpeningUC;

        public class TaiKhoanViewModel
        {
            public string TenTaiKhoan { get; set; }
            public string PhanQuyen { get; set; }
            public string Avatar { get; set; }
        }

        public void LoadTaiKhoan()
        {
            using (var context = new QLTVContext())
            {
                var taiKhoan = context.TAIKHOAN
                    .Include(tk => tk.IDPhanQuyenNavigation)
                    .Where(tk => tk.ID == Settings.Default.CurrentUserID)
                    .Select(tk => new TaiKhoanViewModel
                    {
                        TenTaiKhoan = tk.TenTaiKhoan,
                        PhanQuyen = tk.IDPhanQuyenNavigation.MoTa,
                        Avatar = tk.Avatar,
                    })
                    .FirstOrDefault();

                if (taiKhoan == null)
                {
                    MessageBox.Show("Không tìm thấy thông tin tài khoản.");
                    return;
                }

                spTaiKhoan.DataContext = taiKhoan;
            }
        }

        public AWTrangQuanLy()
        {
            InitializeComponent();
            OpeningUC = new List<UserControl>();
            spClock.DataContext = new ClockViewModel();
            LoadTaiKhoan();
        }

        private void OpenUC(UserControl uc)
        {
            UserControl existingUC = OpeningUC.FirstOrDefault(x => x.GetType() == uc.GetType());

            if (existingUC == null)
            {
                OpeningUC.Add(uc);
                ADMainContent.Content = uc;
            }
            else
            {
                ADMainContent.Content = existingUC;
            }
        }

        private void btnFnQuanLySach_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new AUFnQuanLySach());
            //// Lấy mã màu từ SolidColorBrush
            //var primaryBrush = (SolidColorBrush)App.AdminPriColor;
            //string colorCode = primaryBrush.Color.ToString(); // Trả về mã màu dạng #RRGGBB hoặc #AARRGGBB

            //MessageBox.Show($"PrimaryColor: {colorCode}");
        }

        private void btnFnQuanLyTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new QuanLyTaiKhoan());
        }

        private void btnFnQuanLyDocGia_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new AUQuanLyDocGia());
        }

        private void btnFnQuanLyMuonTra_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new UcQLMuonTra());
        }

        private void btnFnBaoCaoThongKe_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new UcBCMuonTra());
        }

        private void btnFnQuyDinh_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new UcQuyDinh());
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

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            if (dpMenu.Visibility == Visibility.Collapsed)
                dpMenu.Visibility = Visibility.Visible;
            else if (dpMenu.Visibility == Visibility.Visible)
                dpMenu.Visibility = Visibility.Collapsed;
        }

        private void btnCloseAllPage_Click(object sender, RoutedEventArgs e)
        {
            ADMainContent.Content = null;
            OpeningUC.Clear();
        }

        private void btnNotification_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class ClockViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timer;
        private string _currentTime;
        private string _currentDate;

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                if (_currentTime != value)
                {
                    _currentTime = value;
                    OnPropertyChanged(nameof(CurrentTime));
                }
            }
        }

        public string CurrentDate
        {
            get => _currentDate;
            set
            {
                if (_currentDate != value)
                {
                    _currentDate = value;
                    OnPropertyChanged(nameof(CurrentDate));
                }
            }
        }

        public ClockViewModel()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            CurrentDate = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
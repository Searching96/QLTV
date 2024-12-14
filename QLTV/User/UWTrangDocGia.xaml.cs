using Microsoft.EntityFrameworkCore;
using QLTV.Admin;
using QLTV.GridViewModels;
using QLTV.Models;
using QLTV.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace QLTV.User
{
    /// <summary>
    /// Interaction logic for UWTrangDocGia.xaml
    /// </summary>
    public partial class UWTrangDocGia : Window
    {
        public List<UserControl> OpeningUC;

        public class TaiKhoanViewModel
        {
            public string TenTaiKhoan { get; set; }
            public string LoaiDocGia { get; set; }
            public string Avatar { get; set; }
        }

        public void LoadTaiKhoan()
        {
            using (var context = new QLTVContext())
            {
                var docGia = context.DOCGIA
                    .Include(dg => dg.IDTaiKhoanNavigation)
                    .Include(dg => dg.IDLoaiDocGiaNavigation)
                    .Where(dg => dg.IDTaiKhoanNavigation.ID == Settings.Default.CurrentUserID)
                    .Select(dg => new TaiKhoanViewModel
                    {
                        TenTaiKhoan = dg.IDTaiKhoanNavigation.TenTaiKhoan,
                        LoaiDocGia = dg.IDLoaiDocGiaNavigation.TenLoaiDocGia,
                        Avatar = dg.IDTaiKhoanNavigation.Avatar
                    })
                    .FirstOrDefault();

                if (docGia == null)
                {
                    MessageBox.Show("Không tìm thấy thông tin tài khoản.");
                    return;
                }

                spTaiKhoan.DataContext = docGia;
            }
        }

        public UWTrangDocGia()
        {
            InitializeComponent();
            spClock.DataContext = new ClockViewModel();
            LoadTaiKhoan();
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

        private void btnFnLichSuMuonTra_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new UUFnLichSuMuonTra());
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
            USMainContent.Content = null;
            OpeningUC.Clear();
        }

        private void btnThongBao_Click(object sender, RoutedEventArgs e)
        {
            popupThongBao.IsOpen = true;
        }

        private void btnSachMuon_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            popupTaiKhoan.IsOpen = !popupTaiKhoan.IsOpen;
        }
        private void ChiTiet_Click(object sender, RoutedEventArgs e)
        {
            int currentUserId = Properties.Settings.Default.CurrentUserID;

            using (var dbContext = new QLTVContext())
            {
                var taiKhoan = dbContext.TAIKHOAN.Include(tk => tk.IDPhanQuyenNavigation) // Bao gồm bảng PHANQUYEN
                            .FirstOrDefault(tk => tk.ID == currentUserId);

                if (taiKhoan == null)
                {
                    MessageBox.Show("Không tìm thấy tài khoản!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var accountViewModel = new AccountViewModel
                {
                    ID = taiKhoan.ID,
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    TenTaiKhoan = taiKhoan.TenTaiKhoan,
                    TenNguoiDung = taiKhoan.HoTen,
                    GioiTinh = taiKhoan.GioiTinh,
                    DiaChi = taiKhoan.DiaChi,
                    Avatar = taiKhoan.Avatar,
                    Email = taiKhoan.Email,
                    SDT = taiKhoan.SDT,
                    NgaySinh = taiKhoan.SinhNhat,
                    NgayDangKy = taiKhoan.NgayMo,
                    NgayHetHan = taiKhoan.NgayDong,
                    IDPhanQuyen = taiKhoan.IDPhanQuyen,
                    LoaiTaiKhoan = taiKhoan.IDPhanQuyenNavigation.MoTa,

                };

                // Mở tab tài khoản chi tiết
                OpenAccountTab(accountViewModel);
            }
        }

        private void OpenAccountTab(AccountViewModel account)
        {
            if (USMainContent != null)
            {
                // Tạo mới UserControl ChiTietTaiKhoan
                var chiTietTaiKhoanControl = new ChiTietTaiKhoan
                {
                    DataContext = account
                };

                // Gán ChiTietTaiKhoan vào Content của USMainContent
                USMainContent.Content = chiTietTaiKhoanControl;
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
}

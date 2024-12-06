using Microsoft.EntityFrameworkCore;
using QLTV_TranBin.GridViewModels;
using QLTV_TranBin.Models;
using QLTV_TranBin.Properties;
using QLTV_TranBin.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace QLTV_TranBin
{
    /// <summary>
    /// Interaction logic for WUserWindow.xaml
    /// </summary>
    public partial class WUserWindow : INotifyPropertyChanged
    {
        private ThongBaoList _thongBaoList;

        public ThongBaoList ThongBaoList
        {
            get => _thongBaoList;
            set
            {
                _thongBaoList = value;
                OnPropertyChanged();
            }
        }
        public WUserWindow()
        {
            InitializeComponent();
            ThongBaoList = new ThongBaoList();

            int currentUserId = Properties.Settings.Default.CurrentUserID;

            // Tải dữ liệu thông báo khi khởi động
            ThongBaoList.LoadThongBao(currentUserId);
            DataContext = this; // Hoặc một instance của ViewModel
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            // Get the button that was clicked
            var button = sender as System.Windows.Controls.Button;

            // Find the parent TabItem
            if (button != null)
            {
                var tabItem = FindParent<TabItem>(button);

                // Remove the TabItem from the TabControl
                if (tabItem != null)
                {
                    MainTabControl.Items.Remove(tabItem);
                }
            }
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            // Search up the visual tree to find the parent of type T
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        private void btnTrangChu_Click(object sender, RoutedEventArgs e)
        {

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

            using (var dbContext = new QLTV2Context())
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
        private void DangXuat_Click(object sender, RoutedEventArgs e)
        {
            var _context = new QLTV2Context();
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
        private void OpenAccountTab(AccountViewModel account)
        {
            if (MainTabControl != null)
            {
                var existingTab = MainTabControl.Items
                                    .OfType<TabItem>()
                                    .FirstOrDefault(tab => tab.Header?.ToString() == $"Profile - {account.TenTaiKhoan}");

                if (existingTab != null)
                {
                    MainTabControl.SelectedItem = existingTab;
                }
                else
                {
                    var profileTab = new TabItem
                    {
                        Header = $"Profile - {account.TenTaiKhoan}",
                        Content = new ChiTietTaiKhoan
                        {
                            DataContext = account
                        }
                    };

                    MainTabControl.Items.Add(profileTab);
                    MainTabControl.SelectedItem = profileTab;
                }
            }
            else
            {
                MessageBox.Show("TabControl không được tìm thấy!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void tbBookFilter_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}

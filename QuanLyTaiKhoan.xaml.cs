using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using QLTV_TranBin.GridViewModels;
using QLTV_TranBin.Models;
using QLTV_TranBin.Properties;
using QLTV_TranBin.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace QLTV_TranBin
{
    /// <summary>
    /// Interaction logic for QuanLyTaiKhoan.xaml
    /// </summary>
    public partial class QuanLyTaiKhoan : UserControl
    {
        public ObservableCollection<AccountViewModel> Accounts { get; set; } = new ObservableCollection<AccountViewModel>();
        private static Random random = new Random(); // Khởi tạo Random duy nhất

        public QuanLyTaiKhoan()
        {
            InitializeComponent();
            DataContext = this; // Gán DataContext để binding
            LoadData();
        }
        
        public void LoadData()
        {
            try
            {
                using (var context = new QLTVContext())
                {
                    // Tải dữ liệu từ TAIKHOAN và liên kết với DOCGIA hoặc ADMIN dựa trên IDPhanQuyen
                    var accounts = context.TAIKHOAN
                        .Where(tk => !tk.IsDeleted)
                        .Select(tk => new AccountViewModel
                        {
                            MaTaiKhoan = tk.MaTaiKhoan,
                            TenTaiKhoan = tk.TenTaiKhoan,
                            Email = tk.Email,
                            SDT = tk.SDT,
                            DiaChi = tk.DiaChi,
                            NgaySinh = tk.SinhNhat,  // Sửa lỗi tại đây: Sử dụng ?? để xử lý null
                            IDPhanQuyen = tk.IDPhanQuyen, 
                            LoaiTaiKhoan = tk.IDPhanQuyenNavigation.MoTa,
                            NgayDangKy = tk.NgayMo,
                            NgayHetHan = tk.NgayDong,
                            BgColor = GenerateRandomColor(),

                            // Lấy dữ liệu từ DOCGIA nếu IDPhanQuyen == 4, nếu không thì lấy từ ADMIN
                            TenNguoiDung = tk.IDPhanQuyen == 4
                                ? tk.DOCGIA.FirstOrDefault(dg => dg.IDTaiKhoan == tk.ID).TenDocGia
                                : tk.ADMIN.FirstOrDefault(ad => ad.IDTaiKhoan == tk.ID).TenAdmin,

                            GioiTinh = tk.IDPhanQuyen == 4
                                ? tk.DOCGIA.FirstOrDefault(dg => dg.IDTaiKhoan == tk.ID).GioiTinh
                                : tk.ADMIN.FirstOrDefault(ad => ad.IDTaiKhoan == tk.ID).GioiTinh,

                            
                        })
                        .ToList();

                    // Thêm dữ liệu vào ObservableCollection
                    Accounts.Clear(); // Xóa dữ liệu cũ (nếu có)
                    foreach (var account in accounts)
                    {
                        Accounts.Add(account);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string GenerateRandomColor()
        { 
            int red = random.Next(0, 256);
            int green = random.Next(0, 256);
            int blue = random.Next(0, 256);

            // Chuyển đổi các giá trị RGB thành mã màu hex
            return $"#{red:X2}{green:X2}{blue:X2}";
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            LoadData(); 
        }

        private void btnTaoTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            var createAccountWindow = new CreateAccount();
            createAccountWindow.ReloadRequested += LoadData; // Gọi lại hàm LoadData
            createAccountWindow.ShowDialog();
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox?.DataContext is AccountViewModel account)
            {
                account.IsChecked = true;

                // Thêm dòng vào SelectedItems nếu chưa có
                if (!dgAccount.SelectedItems.Contains(account))
                {
                    dgAccount.SelectedItems.Add(account);
                }
            }

            // Đảm bảo SelectedItems không bị xóa
            foreach (var item in dgAccount.Items)
            {
                if (item is AccountViewModel accountInList && accountInList.IsChecked && !dgAccount.SelectedItems.Contains(accountInList))
                {
                    dgAccount.SelectedItems.Add(accountInList);
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Lấy checkbox bị bỏ tick
            var checkbox = sender as CheckBox;
            if (checkbox != null)
            {
                // Lấy item (dòng) tương ứng với checkbox
                var item = checkbox.DataContext as AccountViewModel;
                if (item != null)
                {
                    // Cập nhật thuộc tính IsChecked của AccountViewModel
                    item.IsChecked = false;

                    // Loại bỏ dòng khỏi danh sách SelectedItems
                    if (dgAccount.SelectedItems.Contains(item))
                    {
                        dgAccount.SelectedItems.Remove(item); // Loại bỏ dòng
                    }
                }
            }
        }

        private void HeaderCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in dgAccount.Items)
            {
                if (item is AccountViewModel account)
                {
                    account.IsChecked = true;

                    // Thêm tất cả các dòng vào SelectedItems
                    if (!dgAccount.SelectedItems.Contains(account))
                    {
                        dgAccount.SelectedItems.Add(account);
                    }
                }
            }
        }

        private void HeaderCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in dgAccount.Items)
            {
                if (item is AccountViewModel account)
                {
                    account.IsChecked = false;

                    // Xóa tất cả các dòng khỏi SelectedItems
                    if (dgAccount.SelectedItems.Contains(account))
                    {
                        dgAccount.SelectedItems.Remove(account);
                    }
                }
            }
        }
        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Kiểm tra nếu phím bấm là Delete
            if (e.Key == Key.Delete)
            {
                DeleteSelectedItems();
            }
        }

        private void DeleteSelectedItems()
        {
            // Lấy danh sách các dòng đang được chọn
            var selectedAccounts = new List<AccountViewModel>();
            foreach (var selectedItem in dgAccount.SelectedItems)
            {
                
                if (selectedItem is AccountViewModel account)
                {
                    
                    selectedAccounts.Add(account);
                    // Thêm mục vào danh sách xóa
                }
            }

            if (selectedAccounts.Any())
            {
                try
                {
                    using (var context = new QLTVContext())
                    {
                        // Dùng Transaction để đảm bảo tính toàn vẹn khi thao tác với cơ sở dữ liệu
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {
                                // Lặp qua danh sách các tài khoản được chọn
                                foreach (var account in selectedAccounts)
                                {
                                    
                                    // Tìm tài khoản trong cơ sở dữ liệu
                                    var accountToDelete = context.TAIKHOAN.FirstOrDefault(a => a.MaTaiKhoan == account.MaTaiKhoan);

                                    if (accountToDelete != null)
                                    {
                                        // Đánh dấu tài khoản là đã xóa
                                        accountToDelete.IsDeleted = true;
                                    }
                                }

                                // Lưu thay đổi vào cơ sở dữ liệu
                                context.SaveChanges();

                                // Commit Transaction
                                transaction.Commit();

                                // Xóa các dòng khỏi ObservableCollection để cập nhật giao diện
                                foreach (var account in selectedAccounts)
                                {
                                    Accounts.Remove(account);
                                }

                                MessageBox.Show("Đã xóa các tài khoản được chọn thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                // Nếu có lỗi trong quá trình xóa, rollback transaction
                                transaction.Rollback();
                                MessageBox.Show($"Đã xảy ra lỗi khi xóa dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Đã xảy ra lỗi khi kết nối với cơ sở dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn ít nhất một tài khoản để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Lấy danh sách các dòng đang được chọn
            var selectedAccounts = new List<AccountViewModel>();
            foreach (var selectedItem in dgAccount.SelectedItems)
            {

                if (selectedItem is AccountViewModel account)
                {

                    selectedAccounts.Add(account);
                    // Thêm mục vào danh sách xóa
                }
            }

            if (selectedAccounts.Any())
            {
                try
                {
                    using (var context = new QLTVContext())
                    {
                        // Dùng Transaction để đảm bảo tính toàn vẹn khi thao tác với cơ sở dữ liệu
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {
                                // Lặp qua danh sách các tài khoản được chọn
                                foreach (var account in selectedAccounts)
                                {
                                    
                                    // Tìm tài khoản trong cơ sở dữ liệu
                                    var accountToDelete = context.TAIKHOAN.FirstOrDefault(a => a.MaTaiKhoan == account.MaTaiKhoan);

                                    if (accountToDelete != null)
                                    {
                                        // Đánh dấu tài khoản là đã xóa
                                        accountToDelete.IsDeleted = true;
                                    }
                                }

                                // Lưu thay đổi vào cơ sở dữ liệu
                                context.SaveChanges();

                                // Commit Transaction
                                transaction.Commit();

                                // Xóa các dòng khỏi ObservableCollection để cập nhật giao diện
                                foreach (var account in selectedAccounts)
                                {
                                    Accounts.Remove(account);
                                }

                                MessageBox.Show("Đã xóa tài khoản được chọn thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                // Nếu có lỗi trong quá trình xóa, rollback transaction
                                transaction.Rollback();
                                MessageBox.Show($"Đã xảy ra lỗi khi xóa dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Đã xảy ra lỗi khi kết nối với cơ sở dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn ít nhất một tài khoản để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void btnThemFile_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            // Lấy tài khoản hiện tại từ DataContext
            var button = sender as Button;
            if (button?.DataContext is AccountViewModel selectedAccount)
            {
                // Truy cập TabControl trong UserControl
                if (tcQLTK != null)
                {
                    // Kiểm tra xem Tab với tài khoản này đã tồn tại chưa
                    var existingTab = tcQLTK.Items
                                        .OfType<TabItem>()
                                        .FirstOrDefault(tab => tab.Header?.ToString() == $"Profile - {selectedAccount.TenTaiKhoan}");

                    if (existingTab != null)
                    {
                        // Nếu Tab đã tồn tại, chuyển sang Tab đó
                        tcQLTK.SelectedItem = existingTab;
                    }
                    else
                    {
                        // Tạo UI mới từ UserControl
                        var profileTab = new TabItem
                        {
                            Header = $"Profile - {selectedAccount.TenTaiKhoan}", // Đặt tiêu đề tab
                            Content = new ChiTietTaiKhoan
                            {
                                DataContext = selectedAccount // Truyền dữ liệu vào DataContext
                            }
                        };

                        // Thêm Tab vào TabControl
                        tcQLTK.Items.Add(profileTab);

                        // Chuyển sang Tab vừa tạo
                        tcQLTK.SelectedItem = profileTab;
                    }
                }
                else
                {
                    MessageBox.Show("TabControl không được tìm thấy trong UserControl!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using QLTV.GridViewModels;
using QLTV.Models;
using QLTV.Properties;
using QLTV.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using OfficeOpenXml;
using System.IO;
using MimeKit;
using MailKit.Net.Smtp;
using System.Globalization;
using MaterialDesignThemes.Wpf;


namespace QLTV.Admin
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
                            ID = tk.ID,
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
                            TenNguoiDung = tk.HoTen,
                            GioiTinh = tk.GioiTinh,
                            Avatar = tk.Avatar
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

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                using (var context = new QLTVContext())
                {
                    // Lấy giá trị nhập từ TextBox
                    string searchValue = txtSearch.Text.Trim();

                    // Lấy loại tìm kiếm từ ComboBox
                    var selectedSearchType = (cbSearchType.SelectedItem as ComboBoxItem)?.Tag?.ToString();

                    // Nếu chưa chọn loại tìm kiếm hoặc ô tìm kiếm trống
                    if (string.IsNullOrEmpty(selectedSearchType) || string.IsNullOrEmpty(searchValue))
                    {
                        LoadData(); // Hiển thị toàn bộ dữ liệu
                        return;
                    }

                    // Lọc dữ liệu theo loại tìm kiếm
                    var query = context.TAIKHOAN
                        .Where(tk => !tk.IsDeleted)
                        .Select(tk => new AccountViewModel
                        {
                            MaTaiKhoan = tk.MaTaiKhoan,
                            TenTaiKhoan = tk.TenTaiKhoan,
                            Email = tk.Email,
                            SDT = tk.SDT,
                            DiaChi = tk.DiaChi,
                            NgaySinh = tk.SinhNhat,
                            IDPhanQuyen = tk.IDPhanQuyen,
                            LoaiTaiKhoan = tk.IDPhanQuyenNavigation.MoTa,
                            BgColor = GenerateRandomColor(),
                        });

                    if (selectedSearchType == "TenTaiKhoan")
                    {
                        query = query.Where(tk => tk.TenTaiKhoan.Contains(searchValue));

                    }
                    else if (selectedSearchType == "IDPhanQuyen")
                    {
                        query = query.Where(tk => tk.LoaiTaiKhoan.Contains(searchValue));
                    }

                    // Cập nhật ObservableCollection
                    var filteredAccounts = query.ToList();
                    Debug.WriteLine($"Số lượng tài khoản tìm được: {filteredAccounts.Count}");

                    Accounts.Clear();
                    foreach (var account in filteredAccounts)
                    {
                        Accounts.Add(account);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi khi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            // Lấy tài khoản hiện tại từ DataContext
            var button = sender as Button;
            if (button?.DataContext is AccountViewModel selectedAccount)
            {
                // Kiểm tra nếu TabControl không null
                if (tcQLTK != null)
                {
                    // Kiểm tra nếu Tab với tài khoản đã tồn tại
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
                        // Tạo Tab mới
                        var profileTab = new TabItem
                        {
                            Header = $"Profile - {selectedAccount.TenTaiKhoan}", // Tiêu đề tab
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
                    MessageBox.Show("TabControl không được tìm thấy!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
                    tcQLTK.Items.Remove(tabItem);
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


        private void btnXoa_Click(object sender, RoutedEventArgs e)
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
        public bool SendEmailUsingMailKit(string recipientEmail, string code)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("QLTV TranBin", "thunderstar848@gmail.com")); // Địa chỉ Gmail của bạn
                message.To.Add(new MailboxAddress("", recipientEmail));
                message.Subject = "Password for your account";

                message.Body = new TextPart("plain")
                {
                    Text = $"Your password is: {code}"
                };

                using (var client = new SmtpClient())
                {
                    // Kết nối tới máy chủ Gmail
                    client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                    // Sử dụng App Password thay vì mật khẩu thông thường
                    client.Authenticate("thunderstar848@gmail.com", "arqd unir cttu vhgi");

                    client.Send(message);
                    client.Disconnect(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return false;
            }
        }
        private void btnThemFile_Click(object sender, RoutedEventArgs e)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Random random = new Random();

            string password = random.Next(100000, 999999).ToString(); // Tạo mật khẩu tạm thời

            // Hiển thị hộp thoại chọn file
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Chọn file Excel"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Đọc file Excel
                    using (var package = new ExcelPackage(new FileInfo(openFileDialog.FileName)))
                    {
                        var worksheet = package.Workbook.Worksheets[0]; // Lấy sheet đầu tiên
                        var rowCount = worksheet.Dimension.Rows;       // Số hàng dữ liệu

                        int soDongThanhCong = 0;
                        int soDongBiLoi = 0;
                        List<string> danhSachLoi = new List<string>(); // Ghi nhận dòng lỗi

                        using (var context = new QLTVContext())
                        {
                            for (int row = 2; row <= rowCount; row++) // Bỏ qua hàng tiêu đề
                            {
                                try
                                {
                                    var tenTaiKhoan = worksheet.Cells[row, 1].Text; // Cột 1: Tên tài khoản
                                    var email = worksheet.Cells[row, 2].Text;       // Cột 2: Email

                                    // Kiểm tra xem TenTaiKhoan hoặc Email đã tồn tại trong cơ sở dữ liệu chưa
                                    var accountExists = context.TAIKHOAN.Any(a => a.TenTaiKhoan == tenTaiKhoan || a.Email == email);
                                    if (accountExists)
                                    {
                                        throw new Exception($"Tên tài khoản hoặc Email đã tồn tại: TenTaiKhoan='{tenTaiKhoan}', Email='{email}'");
                                    }

                                    var account = new TAIKHOAN
                                    {
                                        TenTaiKhoan = worksheet.Cells[row, 1].Text, // Cột 1: Tên tài khoản
                                        Email = worksheet.Cells[row, 2].Text,       // Cột 2: Email
                                        SDT = worksheet.Cells[row, 3].Text,         // Cột 3: Số điện thoại
                                        DiaChi = worksheet.Cells[row, 4].Text,      // Cột 4: Địa chỉ
                                        SinhNhat = worksheet.Cells[row, 5].Value is DateTime dateValue
                                                ? dateValue
                                                : throw new Exception($"Ngày sinh không hợp lệ: {worksheet.Cells[row, 5].Value}"),
                                        IDPhanQuyen = int.TryParse(worksheet.Cells[row, 6].Text, out var idPhanQuyen) ? idPhanQuyen : throw new Exception("ID phân quyền không hợp lệ"),
                                        NgayMo = worksheet.Cells[row, 7].Value is DateTime dateNgayMo
                                                ? dateNgayMo.Date
                                                : throw new Exception($"Ngày mở không hợp lệ: {worksheet.Cells[row, 7].Value}"),

                                        NgayDong = worksheet.Cells[row, 8].Value is DateTime dateNgayDong
                                                ? dateNgayDong.Date
                                                : throw new Exception($"Ngày đóng không hợp lệ: {worksheet.Cells[row, 8].Value}"),
                                        IsDeleted = false, // Đảm bảo không bị đánh dấu đã xóa
                                        MatKhau = password,
                                        TrangThai = false

                                    };
                                    SendEmailUsingMailKit(account.Email, password);
                                    // Thêm tài khoản mới vào cơ sở dữ liệu
                                    context.TAIKHOAN.Add(account);
                                    soDongThanhCong++;
                                }
                                catch (Exception ex)
                                {
                                    soDongBiLoi++;
                                    danhSachLoi.Add($"Dòng {row}: {ex.Message}");
                                }
                            }

                            // Lưu tất cả thay đổi vào cơ sở dữ liệu
                            context.SaveChanges();
                        }

                        // Hiển thị kết quả
                        string ketQua = $"Thêm dữ liệu từ file Excel hoàn tất!\n" +
                                        $"Số dòng thêm thành công: {soDongThanhCong}\n" +
                                        $"Số dòng bị lỗi: {soDongBiLoi}";

                        if (soDongBiLoi > 0)
                        {
                            // Ghi danh sách lỗi ra file hoặc hiển thị
                            string fileLog = "LogLoiImport.txt";
                            File.WriteAllLines(fileLog, danhSachLoi);
                            ketQua += $"\nChi tiết lỗi được ghi tại: {fileLog}";
                            System.Diagnostics.Process.Start("notepad.exe", fileLog);
                        }

                        MessageBox.Show(ketQua, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Đã xảy ra lỗi khi thêm dữ liệu từ file Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private void btnXuatFile_Click(object sender, RoutedEventArgs e)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Hiển thị hộp thoại lưu file
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Lưu file Excel",
                FileName = "DanhSachTaiKhoan.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Tạo file Excel
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Danh sách tài khoản");

                        // Thêm tiêu đề cột
                        worksheet.Cells[1, 1].Value = "Mã Tài Khoản";
                        worksheet.Cells[1, 2].Value = "Tên Tài Khoản";
                        worksheet.Cells[1, 3].Value = "Email";
                        worksheet.Cells[1, 4].Value = "SĐT";
                        worksheet.Cells[1, 5].Value = "Địa Chỉ";
                        worksheet.Cells[1, 6].Value = "Ngày Sinh";
                        worksheet.Cells[1, 7].Value = "Loại Tài Khoản";
                        worksheet.Cells[1, 8].Value = "Ngày Đăng Ký";
                        worksheet.Cells[1, 9].Value = "Ngày Hết Hạn";

                        // Định dạng tiêu đề
                        using (var range = worksheet.Cells[1, 1, 1, 9])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }

                        // Thêm dữ liệu từ ObservableCollection
                        int row = 2;
                        foreach (var account in Accounts)
                        {
                            worksheet.Cells[row, 1].Value = account.MaTaiKhoan;
                            worksheet.Cells[row, 2].Value = account.TenTaiKhoan;
                            worksheet.Cells[row, 3].Value = account.Email;
                            worksheet.Cells[row, 4].Value = account.SDT;
                            worksheet.Cells[row, 5].Value = account.DiaChi;
                            worksheet.Cells[row, 6].Value = account.NgaySinh.ToString("dd/MM/yyyy");
                            worksheet.Cells[row, 7].Value = account.LoaiTaiKhoan;
                            worksheet.Cells[row, 8].Value = account.NgayDangKy.ToString("dd/MM/yyyy");
                            worksheet.Cells[row, 9].Value = account.NgayHetHan.ToString("dd/MM/yyyy");
                            row++;
                        }

                        // Tự động điều chỉnh kích thước cột
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        // Lưu file
                        var filePath = saveFileDialog.FileName;
                        package.SaveAs(new FileInfo(filePath));

                        MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Đã xảy ra lỗi khi xuất file Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

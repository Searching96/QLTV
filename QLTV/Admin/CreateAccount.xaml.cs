using QLTV.Models;
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
using MailKit.Net.Smtp;
using MimeKit;
using System.Collections.ObjectModel;
using QLTV.Properties;

namespace QLTV.Admin
{
    /// <summary>
    /// Interaction logic for CreateAccount.xaml
    /// </summary>
    public partial class CreateAccount : Window
    {
        public ObservableCollection<PHANQUYEN> AvailableRoles { get; set; } = new ObservableCollection<PHANQUYEN>();
        public event Action ReloadRequested;

        public int? SelectedRoleID { get; set; } // Giá trị role được chọn
        public CreateAccount()
        {
            InitializeComponent();
            DataContext = this;
            icSinhNhatError.Visibility = Visibility.Collapsed;
            icNgayMoError.Visibility = Visibility.Collapsed;
            icNgayDongError.Visibility = Visibility.Collapsed;
            LoadRoles();
        }
        private void cbDocGia_Checked(object sender, RoutedEventArgs e)
        {
            // Tái tải lại các quyền khi checkbox "Độc giả" được chọn
            LoadRoles();
            cbAdmin.IsChecked = false;
        }

        private void cbDocGia_Unchecked(object sender, RoutedEventArgs e)
        {
            // Tái tải lại các quyền khi checkbox "Độc giả" bị bỏ chọn
            LoadRoles();
        }

        private void cbAdmin_Checked(object sender, RoutedEventArgs e)
        {
            // Tái tải lại các quyền khi checkbox "Admin" được chọn
            LoadRoles();
            cbDocGia.IsChecked = false;
        }

        private void cbAdmin_Unchecked(object sender, RoutedEventArgs e)
        {
            // Tái tải lại các quyền khi checkbox "Admin" bị bỏ chọn
            LoadRoles();
        }
        private void cbNam_Checked(object sender, RoutedEventArgs e)
        {
            // Tái tải lại các quyền khi checkbox "Độc giả" được chọn
            
            cbNu.IsChecked = false;
        }

        private void cbNam_Unchecked(object sender, RoutedEventArgs e)
        {

        }
        private void cbNu_Checked(object sender, RoutedEventArgs e)
        {
            // Tái tải lại các quyền khi checkbox "Admin" được chọn
            
            cbNam.IsChecked = false;
        }

        private void cbNu_Unchecked(object sender, RoutedEventArgs e)
        {
            
        }

        public bool SendEmailUsingMailKit(string recipientEmail, string code)
        {
            try
            {
                // Tạo đối tượng email
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Quản lý thư viện LIMAN", "thuvienliman@gmail.com")); // Địa chỉ Gmail của bạn
                message.To.Add(new MailboxAddress("", recipientEmail));
                message.Subject = "Mật khẩu cho tài khoản của bạn";

                // Nội dung HTML
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Email OTP</title>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            line-height: 1.6;
                            margin: 0;
                            padding: 0;
                            background-color: #f4f4f4;
                        }}
                        .email-container {{
                            max-width: 600px;
                            margin: 20px auto;
                            background: #ffffff;
                            border: 1px solid #ddd;
                            border-radius: 8px;
                            padding: 20px;
                        }}
                        .email-header {{
                            text-align: center;
                            font-size: 24px;
                            font-weight: bold;
                            color: #333;
                            margin-bottom: 20px;
                        }}
                        .email-body {{
                            color: #555;
                            font-size: 16px;
                        }}
                        .otp-box {{
                            display: inline-block;
                            background-color: #007bff;
                            color: white;
                            font-size: 20px;
                            font-weight: bold;
                            padding: 10px 20px;
                            border-radius: 8px;
                            margin: 20px 0;
                            text-align: center;
                        }}
                        .email-footer {{
                            margin-top: 20px;
                            font-size: 14px;
                            color: #888;
                            text-align: center;
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='email-header'>
                            Quản lý thư viện LIMAN
                        </div>
                        <div class='email-body'>
                            <p>Chào bạn,</p>
                            <p>Tài khoản của bạn đã được tạo thành công trên hệ thống. Dưới đây là mật khẩu của bạn để đăng nhập. Vui lòng thay đổi mật khẩu ngay sau khi đăng nhập để đảm bảo an toàn. Nếu bạn không yêu cầu tạo tài khoản, xin vui lòng bỏ qua email này.</p>
                            <div class='otp-box'>
                                {code}
                            </div>
                        </div>
                        <div class='email-footer'>
                            Trân trọng,<br>
                            QLTV Development Team
                        </div>
                    </div>
                </body>
                </html>"
                };

                // Gán nội dung vào email
                message.Body = bodyBuilder.ToMessageBody();

                // Kết nối tới SMTP Server và gửi email
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                    // Lưu ý: Sử dụng App Password của Gmail thay vì mật khẩu thông thường
                    client.Authenticate("thuvienliman@gmail.com", "ujsk gxba hfgo cgzi");

                    client.Send(message);
                    client.Disconnect(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Hiển thị lỗi nếu xảy ra vấn đề
                MessageBox.Show($"Error: {ex.Message}");
                return false;
            }
        }
        private void LoadRoles()
        {
            using (var context = new QLTVContext())
            {
                // Lấy quyền hiện tại của người dùng
                int currentRoleID = Settings.Default.CurrentUserPhanQuyen;

                var roles = new List<PHANQUYEN>();
                

                // Kiểm tra nếu "Độc giả" được chọn
                if (cbDocGia.IsChecked == true)
                {
                    // Lấy danh sách các loại độc giả từ bảng LOAIDOCGIA
                    var loaiDocGia = context.LOAIDOCGIA
                        .Where(ldg => !ldg.IsDeleted) // Lọc các loại không bị xóa
                        .ToList();

                    // Tạo các quyền cho các loại độc giả
                    foreach (var item in loaiDocGia)
                    {
                        roles.Add(new PHANQUYEN
                        {
                            ID = item.ID,
                            MoTa = item.TenLoaiDocGia // Bạn có thể sử dụng thuộc tính `TenLoaiDocGia` để hiển thị trong combo box
                        });
                    }
                }

                // Kiểm tra nếu "Admin" được chọn
                if (cbAdmin.IsChecked == true)
                {
                    var phanquyenadmin = context.PHANQUYEN
                        .Where(role =>
                            (currentRoleID == 1 && (role.ID == 1 || role.ID == 2 || role.ID == 3)) || // SuperAdmin: chỉ hiện SuperAdmin, Quản lý nhân sự và Thủ thư
                            (currentRoleID == 2 && (role.ID == 3)) // Quản lý nhân sự: chỉ hiện Thủ thư và Độc giả
                        )
                        .ToList();
                    roles.AddRange(phanquyenadmin);
                }

                // Nếu không có checkbox nào được chọn, có thể lấy tất cả quyền (hoặc các quyền mặc định)
                if (!roles.Any())
                {
                    roles = context.PHANQUYEN.ToList();
                }
                if(!cbAdmin.IsChecked == true && !cbDocGia.IsChecked == true)
                {
                    roles.Clear();
                }    
                // Cập nhật danh sách quyền
                AvailableRoles.Clear();
                foreach (var role in roles)
                {
                    AvailableRoles.Add(role);
                }

                // Nếu danh sách quyền có phần tử, chọn quyền đầu tiên
                if (AvailableRoles.Any())
                {
                    SelectedRoleID = AvailableRoles.First().ID;
                }
            }
        }
        private void btnSignUp_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dữ liệu từ các TextBox và PasswordBox
            string hoten = txtHoVaTen.Text;
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string address = txtAddress.Text.Trim();
            string phoneNumber = txtPhoneNumber.Text.Trim();
            DateTime? birthday = dpBirthday.SelectedDate;
            DateTime? ngaymo = dpNgayMo.SelectedDate;
            DateTime? ngaydong = dpNgayDong.SelectedDate;
            Random random = new Random();
            string password = random.Next(100000, 999999).ToString(); // Tạo mật khẩu tạm thời

            if (ngaymo > ngaydong)
            {
                icNgayMoError.ToolTip = "Ngày mở không được lớn hơn Ngày đóng";
                icNgayMoError.Visibility = Visibility.Visible;
                icNgayDongError.ToolTip = "Ngày đóng không được nhỏ hơn Ngày mở";
                icNgayDongError.Visibility = Visibility.Visible;
                return;
            }

            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Hãy điền tất cả các trường đang trống!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Kiểm tra số điện thoại chỉ chứa số
            if (string.IsNullOrEmpty(phoneNumber) || !phoneNumber.All(char.IsDigit))
            {
                icPhoneNumberError.ToolTip = "SDT không được bỏ trống và chỉ chứa số";
                return;
            }

            try
            {
                using (var context = new QLTVContext())
                {

                    // Kiểm tra email đã tồn tại chưa
                    var existingAccount = context.TAIKHOAN.FirstOrDefault(tk => tk.Email == email);
                    if (existingAccount != null)
                    {
                        MessageBox.Show("Email đã tồn tài!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }



                    // Kiểm tra nếu là "Độc giả"
                    int phanQuyen = 0;
                    int? idLoaiDocGia = null;
                    string gioitinh = "";
                    if (cbNam.IsChecked == true)
                    {
                        gioitinh = "Nam";
                    }
                    else
                    {
                        gioitinh = "Nữ";
                    }

                    if (cbDocGia.IsChecked == true) // Nếu chọn "Độc giả"
                    {
                        phanQuyen = 4; // ID phân quyền Độc giả
                        if (cbPhanQuyen.SelectedItem is PHANQUYEN selectedRole)
                        {
                            idLoaiDocGia = selectedRole.ID; // Lấy ID phân quyền làm IDLoaiDocGia
                        }
                    }
                    else if (cbAdmin.IsChecked == true) // Nếu chọn "Admin" và chọn từ ComboBox
                    {
                        // Kiểm tra ComboBox để chọn phân quyền cho Admin
                        if (cbPhanQuyen.SelectedItem is PHANQUYEN selectedRole)
                        {
                            phanQuyen = selectedRole.ID; // Lấy ID phân quyền từ ComboBox
                        }
                        else
                        {
                            MessageBox.Show("Please select a role from the ComboBox for Admin!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a role!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Tạo đối tượng tài khoản mới
                    var newAccount = new TAIKHOAN
                    {
                        HoTen = hoten,
                        TenTaiKhoan = username,
                        Email = email,
                        DiaChi = address,
                        SDT = phoneNumber,
                        SinhNhat = birthday.HasValue ? birthday.Value : default(DateTime),
                        MatKhau = password, // Lưu ý: Bạn nên mã hóa mật khẩu trước khi lưu
                        IsDeleted = false, // Cờ đánh dấu tài khoản còn hoạt động
                        GioiTinh = gioitinh,
                        TrangThai = false, // Ví dụ: trạng thái tài khoản
                        IDPhanQuyen = phanQuyen, // Cập nhật phân quyền
                        NgayMo = ngaymo.HasValue ? ngaymo.Value : default(DateTime),
                        NgayDong = ngaydong.HasValue ? ngaydong.Value : default(DateTime),
                    };

                    // Thêm tài khoản mới vào cơ sở dữ liệu
                    context.TAIKHOAN.Add(newAccount);

                    context.SaveChanges();
                    if (phanQuyen != 4)
                    {
                        var newAdmin = new ADMIN
                        {
                            IDTaiKhoan = newAccount.ID
                        };
                        context.ADMIN.Add(newAdmin);
                    }
                    else
                    {
                        var newDocGia = new DOCGIA
                        {
                            IDTaiKhoan = newAccount.ID,
                            IDLoaiDocGia = idLoaiDocGia ?? 0,  // Gán IDLoaiDocGia từ phân quyền
                            GioiThieu = string.Empty
                        };
                        context.DOCGIA.Add(newDocGia);
                    }
                    context.SaveChanges();
                    // Thông báo đăng ký thành công
                    MessageBox.Show("Đăng ký tài khoản thành công!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Gửi email cho người dùng (sử dụng MailKit hoặc phương thức khác)
                    SendEmailUsingMailKit(email, password);
                    ClearFields();
                    // Reset các trường nhập liệu
                    ReloadRequested?.Invoke();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm reset các trường nhập liệu
        private void ClearFields()
        {
            txtHoVaTen.Text = string.Empty;
            cbNam.IsChecked = false;
            cbNu.IsChecked = false;
            txtUsername.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtAddress.Text = string.Empty;
            txtPhoneNumber.Text = string.Empty;
            dpBirthday.SelectedDate = null;
            cbPhanQuyen.SelectedItem = null;
            cbAdmin.IsChecked = false;  
            cbDocGia.IsChecked = false;
            dpNgayDong.SelectedDate = null;
            dpNgayMo.SelectedDate = null;
        }

        private void cbPhanQuyen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Lấy giá trị đã chọn trong ComboBox
            var selectedRole = cbPhanQuyen.SelectedValue;

            if (selectedRole != null && int.TryParse(selectedRole.ToString(), out int roleID))
            {
                // Kiểm tra ID của role
                if (roleID == 4) // ID của độc giả
                {
                    // Độc giả
                    dpNgayMo.SetValue(MaterialDesignThemes.Wpf.HintAssist.HintProperty, "Ngày lập thẻ");
                    dpNgayDong.SetValue(MaterialDesignThemes.Wpf.HintAssist.HintProperty, "Ngày hết hạn");
                }
                else // Các role khác
                {
                    dpNgayMo.SetValue(MaterialDesignThemes.Wpf.HintAssist.HintProperty, "Ngày vào làm");
                    dpNgayDong.SetValue(MaterialDesignThemes.Wpf.HintAssist.HintProperty, "Ngày kết thúc");
                }

                // Bật chỉnh sửa cho DatePicker
                dpNgayMo.IsEnabled = true;
                dpNgayDong.IsEnabled = true;
            }
            else
            {
                // Không có lựa chọn hợp lệ -> vô hiệu hóa DatePicker
                dpNgayMo.IsEnabled = false;
                dpNgayDong.IsEnabled = false;

                // Đặt tiêu đề mặc định
                dpNgayMo.SetValue(MaterialDesignThemes.Wpf.HintAssist.HintProperty, "Ngày mở");
                dpNgayDong.SetValue(MaterialDesignThemes.Wpf.HintAssist.HintProperty, "Ngày đóng");
            }
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                icUsernameError.ToolTip = "Tên tài khoản không được để trống!";
                icUsernameError.Visibility = Visibility.Visible;
                return;
            }

            icUsernameError.Visibility = Visibility.Collapsed;
        }
        private void txtFullName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHoVaTen.Text))
            {
                icFullNameError.ToolTip = "Họ và tên không được để trống!";
                icFullNameError.Visibility = Visibility.Visible;
                return;
            }

            icFullNameError.Visibility = Visibility.Collapsed;
        }


        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                icEmailError.ToolTip = "Email không được để trống!";
                icEmailError.Visibility = Visibility.Visible;
                return;
            }

            icEmailError.Visibility = Visibility.Collapsed;
        }

        private void txtAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                icAddressError.ToolTip = "Địa chỉ không được để trống!";
                icAddressError.Visibility = Visibility.Visible;
                return;
            }

            icAddressError.Visibility = Visibility.Collapsed;
        }

        private void txtPhoneNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text) || !decimal.TryParse(txtPhoneNumber.Text, out decimal result))
            {
                icPhoneNumberError.ToolTip = "Số điện thoại chỉ bao gồm số và không được để trống!";
                icPhoneNumberError.Visibility = Visibility.Visible;
                return;
            }
            
            icPhoneNumberError.Visibility = Visibility.Collapsed;
        }

        private void dpNgayDong_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dpNgayMo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpNgayDong.SelectedDate != null && dpNgayMo.SelectedDate != null)
            {
                if (dpNgayDong.SelectedDate < dpNgayMo.SelectedDate)
                {
                    icNgayMoError.ToolTip = "Ngày Mở không được lớn hơn Ngày Đóng";
                    icNgayMoError.Visibility = Visibility.Visible;
                    icNgayDongError.ToolTip = "Ngày Đóng không được nhỏ hơn Ngày Mở";
                    icNgayDongError.Visibility = Visibility.Visible;
                }
            }

            icNgayMoError.Visibility = Visibility.Collapsed;
            icNgayDongError.Visibility = Visibility.Collapsed;
        }
    }
}

using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QLTV
{
    /// <summary>
    /// Interaction logic for AWThemSach.xaml
    /// </summary>
    public partial class AWThemSach : Window
    {
        private CollectionViewSource viewSource;
        public SACH NewSach;
        public int SoLuong;

        public AWThemSach()
        {
            InitializeComponent();
            LoadTuaSach();
            LoadTinhTrang();
        }

        private string ConvertToUnsigned(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return new string(
                text.Normalize(NormalizationForm.FormD)
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    .ToArray()
            ).Normalize(NormalizationForm.FormC);
        }

        private void LoadTuaSach()
        {
            using (var context = new QLTVContext())
            {
                var dsTuaSach = context.TUASACH
                    .Where(ts => !ts.IsDeleted)
                    .Select(ts => ts.TenTuaSach)
                    .ToList();

                viewSource = new CollectionViewSource();
                viewSource.Source = dsTuaSach;
                cbbTuaSach.ItemsSource = viewSource.View;

                cbbTuaSach.Loaded += (s, e) =>
                {
                    var textBox = cbbTuaSach.Template.FindName("PART_EditableTextBox", cbbTuaSach) as TextBox;
                    if (textBox != null)
                    {
                        textBox.TextChanged += (sender, args) =>
                        {
                            var searchText = ConvertToUnsigned(textBox.Text);
                            viewSource.View.Filter = item =>
                            {
                                if (string.IsNullOrEmpty(searchText))
                                    return true;
                                var itemText = ConvertToUnsigned(item.ToString());
                                return itemText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                            };
                            cbbTuaSach.IsDropDownOpen = true;
                        };
                    }
                };
            }
        }

        private void LoadTinhTrang()
        {
            using (var context = new QLTVContext())
            {
                var dsTinhTrang = context.TINHTRANG
                    .Where(tt => !tt.IsDeleted && tt.TenTinhTrang != "Mất" 
                        && tt.TenTinhTrang != "Hỏng nặng" && tt.TenTinhTrang != "Hỏng hoàn toàn")
                    .Select(tt => tt.TenTinhTrang)
                    .ToList();

                cbbTinhTrang.ItemsSource = dsTinhTrang;
                cbbTinhTrang.SelectedIndex = 0;
            }
        }

        public bool HasError()
        {
            // Tìm tất cả các PackIcon trong UserControl
            foreach (var icon in FindVisualChildren<PackIcon>(this))
            {
                if (icon.Style == FindResource("ErrorIcon") && icon.Visibility == Visibility.Visible)
                {
                    return true;
                }
            }
            return false;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T t)
                    {
                        yield return t;
                    }
                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private async void btnThem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxNhaXuatBan.Text))
            {
                icNhaXuatBanError.ToolTip = "Nhà Xuất Bản không được để trống";
                icNhaXuatBanError.Visibility = Visibility.Visible;
            }

            if (string.IsNullOrWhiteSpace(tbxNamXuatBan.Text))
            {
                icNamXuatBanError.ToolTip = "Năm Xuất Bản không được để trống";
                icNamXuatBanError.Visibility = Visibility.Visible;
            }

            if (string.IsNullOrWhiteSpace(tbxTriGia.Text))
            {
                icTriGiaError.ToolTip = "Trị Giá không được để trống";
                icTriGiaError.Visibility = Visibility.Visible;
            }

            if (string.IsNullOrWhiteSpace(tbxSoLuong.Text))
            {
                icSoLuongError.ToolTip = "Số Lượng không được để trống";
                icSoLuongError.Visibility = Visibility.Visible;
            }

            if (cbbTuaSach.SelectedItem == null)
            {
                icTuaSachError.ToolTip = "Phải chọn một tựa sách";
                icTuaSachError.Visibility = Visibility.Visible;
            }

            if (HasError())
            {
                MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi sửa!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string tuaSach = cbbTuaSach.SelectedItem?.ToString() ?? string.Empty;
            string nhaXuatBan = tbxNhaXuatBan.Text;
            string viTri = tbxViTri.Text;
            int namXuatBan = int.Parse(tbxNamXuatBan.Text);
            DateTime ngayNhap = DateTime.ParseExact(
                dpNgayNhap.Text,
                "dd/MM/yyyy",  // Định dạng ngày dmy
                System.Globalization.CultureInfo.InvariantCulture
            );
            decimal triGia = decimal.Parse(tbxTriGia.Text);
            string tinhTrang = cbbTinhTrang.SelectedItem?.ToString() ?? string.Empty;
            int soLuong = int.Parse(tbxSoLuong.Text);

            using (var context = new QLTVContext())
            {
                // Get the ID of the selected TuaSach and TinhTrang
                int idTuaSach = context.TUASACH
                    .Where(ts => !ts.IsDeleted && ts.TenTuaSach == tuaSach)
                    .Select(ts => ts.ID)
                    .FirstOrDefault();

                int idTinhTrang = context.TINHTRANG
                    .Where(tt => !tt.IsDeleted && tt.TenTinhTrang == tinhTrang)
                    .Select(tt => tt.ID)
                    .FirstOrDefault();

                // Add the new SACH records
                for (int i = 0; i < soLuong; i++)
                {
                    var newSach = new SACH()
                    {
                        IDTuaSach = idTuaSach,
                        ViTri = viTri,
                        NhaXuatBan = nhaXuatBan,
                        NamXuatBan = namXuatBan,
                        NgayNhap = ngayNhap,
                        TriGia = triGia,
                        IDTinhTrang = idTinhTrang
                    };
                    context.SACH.Add(newSach);
                    NewSach = newSach;
                }

                // Save the changes to the SACH table
                await context.SaveChangesAsync();

                // After inserting, update the SoLuong in TUASACH
                context.TUASACH
                    .Where(ts => ts.ID == idTuaSach)
                    .ExecuteUpdate(ts => ts.SetProperty(t => t.SoLuong, t => t.SoLuong + soLuong));
            }

            SoLuong = soLuong;
            this.DialogResult = true;
            this.Close();
        }

        private void dpNgayNhap_Loaded(object sender, RoutedEventArgs e)
        {
            // Tìm TextBox bên trong DatePicker
            var textBox = (dpNgayNhap.Template.FindName("PART_TextBox", dpNgayNhap) as TextBox);
            if (textBox != null)
            {
                textBox.TextChanged += TextBox_TextChanged;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            // Danh sách định dạng hỗ trợ nhiều cách nhập ngày
            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy" };
            if (!DateTime.TryParseExact(textBox.Text, formats, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out DateTime ngayNhap))
            {
                icNgayNhapError.ToolTip = "Ngày Nhập không hợp lệ (định dạng đúng: dd/MM/yyyy)";
                icNgayNhapError.Visibility = Visibility.Visible;
                return;
            }

            // Kiểm tra giới hạn ngày từ 1/1/2000 đến hiện tại
            DateTime minDate = new DateTime(2000, 1, 1);
            if (ngayNhap < minDate)
            {
                icNgayNhapError.ToolTip = "Ngày Nhập không được trước ngày 1/1/2000";
                icNgayNhapError.Visibility = Visibility.Visible;
                return;
            }

            if (ngayNhap > DateTime.Now)
            {
                icNgayNhapError.ToolTip = "Ngày Nhập không được sau ngày hiện tại";
                icNgayNhapError.Visibility = Visibility.Visible;
                return;
            }

            // Nếu hợp lệ, ẩn thông báo lỗi
            icNgayNhapError.Visibility = Visibility.Collapsed;
        }

        private void tbxNhaXuatBan_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxNhaXuatBan.Text))
            {
                icNhaXuatBanError.ToolTip = "Nhà Xuất Bản không được để trống";
                icNhaXuatBanError.Visibility = Visibility.Visible;
                return;
            }

            foreach (char c in tbxNhaXuatBan.Text)
            {
                if (!char.IsLetter(c) && !char.IsDigit(c) && !char.IsWhiteSpace(c))
                {
                    icNhaXuatBanError.ToolTip = "Nhà Xuất Bản không được có kí tự đặc biệt";
                    icNhaXuatBanError.Visibility = Visibility.Visible;
                    return;
                }
            }

            icNhaXuatBanError.Visibility = Visibility.Collapsed;
        }

        private void tbxNamXuatBan_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxNamXuatBan.Text))
            {
                icNamXuatBanError.ToolTip = "Năm Xuất Bản không được để trống";
                icNamXuatBanError.Visibility = Visibility.Visible;
                return;
            }

            if (!int.TryParse(tbxNamXuatBan.Text, out int nxb))
            {
                icNamXuatBanError.ToolTip = "Năm Xuất Bản phải là số nguyên";
                icNamXuatBanError.Visibility = Visibility.Visible;
                return;
            }

            if (nxb <= 1900 || nxb > DateTime.Now.Year)
            {
                icNamXuatBanError.ToolTip = "Năm Xuất Bản phải sau 1900 và không quá năm hiện tại";
                icNamXuatBanError.Visibility = Visibility.Visible;
                return;
            }

            icNamXuatBanError.Visibility = Visibility.Collapsed;
        }

        private void tbxTriGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxTriGia.Text))
            {
                icTriGiaError.ToolTip = "Trị Giá không được để trống";
                icTriGiaError.Visibility = Visibility.Visible;
                return;
            }

            if (!int.TryParse(tbxTriGia.Text, out int tg) || tg < 5000 || tg > 10000000)
            {
                icTriGiaError.ToolTip = "Trị Giá phải là lớn hơn 5 ngàn và nhỏ hơn 10 triệu";
                icTriGiaError.Visibility = Visibility.Visible;
                return;
            }

            icTriGiaError.Visibility = Visibility.Collapsed;
        }

        private void tbxSoLuong_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxSoLuong.Text))
            {
                icSoLuongError.ToolTip = "Số Lượng không được để trống";
                icSoLuongError.Visibility = Visibility.Visible;
                return;
            }

            if (!int.TryParse(tbxSoLuong.Text, out int sl) || sl <= 0)
            {
                icSoLuongError.ToolTip = "Số Lượng phải là số nguyên dương";
                icSoLuongError.Visibility = Visibility.Visible;
                return;
            }

            if (icSoLuongError != null) 
                icSoLuongError.Visibility = Visibility.Collapsed;
        }

        private void cbbTuaSach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbTuaSach.SelectedItem != null)
                icTuaSachError.Visibility = Visibility.Collapsed;
        }

        private void btnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

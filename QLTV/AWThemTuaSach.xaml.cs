using MaterialDesignThemes.Wpf;
using Microsoft.IdentityModel.Tokens;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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
    /// Interaction logic for AWThemTuaSach.xaml
    /// </summary>
    public partial class AWThemTuaSach : Window
    {
        public static readonly Thickness DisplayElementMargin = new Thickness(10, 0, 10, 10);
        public static readonly Thickness ErrorIconMargin = new Thickness(10, 0, 15, 10);

        public AWThemTuaSach()
        {
            InitializeComponent();
        }

        private void btnSuaTacGia_Click(object sender, RoutedEventArgs e)
        {
            // Chuyển DSTacGia trong TextBox thành List<TacGia>
            var currentAuthors = ParseDSTacGia(tbxDSTacGia.Text);

            // Lấy danh sách tất cả các tác giả từ cơ sở dữ liệu
            List<TACGIA> allAuthors;
            using (var context = new QLTVContext())
            {
                allAuthors = context.TACGIA
                                    .Where(tg => !tg.IsDeleted)
                                    .ToList();
            }

            // Mở cửa sổ WDChonTacGia
            var wdChonTacGia = new AWChonTacGia(allAuthors, currentAuthors);

            if (wdChonTacGia.ShowDialog() == true)
            {
                // Cập nhật DSTacGia từ danh sách tác giả mới
                var newSelectedAuthors = wdChonTacGia.SelectedAuthors;
                tbxDSTacGia.Text = string.Join(", ", newSelectedAuthors.Select(a => a.TenTacGia));
            }
        }

        private List<TACGIA> ParseDSTacGia(string DSTacGia)
        {
            if (string.IsNullOrWhiteSpace(DSTacGia)) return new List<TACGIA>();

            // Tách DSTacGia thành các tên tác giả dựa vào dấu phẩy
            var lstTenTacGia = DSTacGia.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(tg => tg.Trim())
                                       .ToList();

            // Lấy danh sách từ cơ sở dữ liệu khớp với tên
            using (var context = new QLTVContext())
            {
                return context.TACGIA.Where(tg => lstTenTacGia
                              .Contains(tg.TenTacGia)).ToList();
            }
        }

        private void btnSuaTheLoai_Click(object sender, RoutedEventArgs e)
        {
            var currentCategories = ParseDSTheLoai(tbxDSTheLoai.Text);

            List<THELOAI> allCategories;
            using (var context = new QLTVContext())
            {
                allCategories = context.THELOAI
                                       .Where(tl => !tl.IsDeleted)
                                       .ToList();
            }

            var awChonTheLoai = new AWChonTheLoai(allCategories, currentCategories);

            if (awChonTheLoai.ShowDialog() == true)
            {
                var newSelectedCategories = awChonTheLoai.SelectedCategories;
                tbxDSTheLoai.Text = string.Join(", ", newSelectedCategories.Select(c => c.TenTheLoai));
            }
        }

        private List<THELOAI> ParseDSTheLoai(string DSTheLoai)
        {
            if (string.IsNullOrWhiteSpace(DSTheLoai)) return new List<THELOAI>();

            var lstTenTheLoai = DSTheLoai.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(tl => tl.Trim())
                                         .ToList();

            using (var context = new QLTVContext())
            {
                return context.THELOAI.Where(tl => lstTenTheLoai
                              .Contains(tl.TenTheLoai)).ToList();
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

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxTenTuaSach.Text))
            {
                icTenTuaSachError.ToolTip = "Tên Tựa Sách không được để trống";
                icTenTuaSachError.Visibility = Visibility.Visible;
            }

            if (string.IsNullOrWhiteSpace(tbxDSTacGia.Text))
            {
                icDSTacGiaError.ToolTip = "Tác Giả không được để trống";
                icDSTacGiaError.Visibility = Visibility.Visible;
            }

            if (string.IsNullOrWhiteSpace(tbxDSTheLoai.Text))
            {
                icDSTheLoaiError.ToolTip = "Thể Loại không được để trống";
                icDSTheLoaiError.Visibility = Visibility.Visible;
            }

            if (string.IsNullOrWhiteSpace(tbxHanMuonToiDa.Text))
            {
                icHanMuonToiDaError.ToolTip = "Hạn Mượn Tối Đa không được để trống";
                icHanMuonToiDaError.Visibility = Visibility.Visible;
            }

            if (HasError())
            {
                MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi sửa!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string tenTuaSach = tbxTenTuaSach.Text;
            int hanMuonToiDa = int.Parse(tbxHanMuonToiDa.Text);
            var lstTenTacGia = tbxDSTacGia.Text.Split(", ").Select(n => n.Trim()).ToList();
            var lstTenTheLoai = tbxDSTheLoai.Text.Split(", ").Select(n => n.Trim()).ToList();

            using (var context = new QLTVContext())
            {
                var newTuaSach = new TUASACH()
                {
                    TenTuaSach = tenTuaSach,
                    HanMuonToiDa = hanMuonToiDa
                };

                context.TUASACH.Add(newTuaSach);
                context.SaveChanges();

                // Thêm tác giả
                foreach (var tenTacGia in lstTenTacGia)
                {
                    var tacGia = context.TACGIA.FirstOrDefault(tg => tg.TenTacGia == tenTacGia);
                    if (tacGia != null)
                    {
                        // Liên kết tác giả với tựa sách
                        var newTSTG = new TUASACH_TACGIA()
                        {
                            IDTuaSach = newTuaSach.ID,  // Dùng ID của tựa sách vừa tạo
                            IDTacGia = tacGia.ID
                        };
                        context.TUASACH_TACGIA.Add(newTSTG);
                    }
                }

                // Thêm thể loại
                foreach (var tenTheLoai in lstTenTheLoai)
                {
                    var theLoai = context.THELOAI.FirstOrDefault(tl => tl.TenTheLoai == tenTheLoai);
                    if (theLoai != null)
                    {
                        // Liên kết thể loại với tựa sách
                        var newTSTL = new TUASACH_THELOAI()
                        {
                            IDTuaSach = newTuaSach.ID,  // Dùng ID của tựa sách vừa tạo
                            IDTheLoai = theLoai.ID
                        };
                        context.TUASACH_THELOAI.Add(newTSTL);
                    }
                }

                context.SaveChanges(); // Lưu tất cả các thay đổi
            }


            this.DialogResult = true;
            this.Close();
        }

        private void tbxTenTuaSach_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxTenTuaSach.Text))
            {
                icTenTuaSachError.ToolTip = "Tên Tựa Sách không được để trống!";
                icTenTuaSachError.Visibility = Visibility.Visible;
                return;
            }

            icTenTuaSachError.Visibility = Visibility.Collapsed;
        }

        private void tbxHanMuonToiDa_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxHanMuonToiDa.Text))
            {
                icHanMuonToiDaError.ToolTip = "Hạn Mượn Tối Đa không được để trống";
                icHanMuonToiDaError.Visibility = Visibility.Visible;
                return;
            }

            if (!int.TryParse(tbxHanMuonToiDa.Text, out int hmtd) || hmtd <= 0 || hmtd > 16)
            {
                icHanMuonToiDaError.ToolTip = "Hạn Mượn Tối Đa phải là số nguyên dương không quá 16";
                icHanMuonToiDaError.Visibility = Visibility.Visible;
                return;
            }

            if (icHanMuonToiDaError != null) 
                icHanMuonToiDaError.Visibility = Visibility.Collapsed;
        }

        private void tbxDSTheLoai_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxDSTheLoai.Text))
            {
                icDSTheLoaiError.ToolTip = "Thể Loại không được để trống";
                icDSTheLoaiError.Visibility = Visibility.Visible;
                return;
            }

            icDSTheLoaiError.Visibility = Visibility.Collapsed;
        }

        private void tbxDSTacGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxDSTacGia.Text))
            {
                icDSTacGiaError.ToolTip = "Tác Giả không được để trống";
                icDSTacGiaError.Visibility = Visibility.Visible;
                return;
            }

            icDSTacGiaError.Visibility = Visibility.Collapsed;
        }

        private void btnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

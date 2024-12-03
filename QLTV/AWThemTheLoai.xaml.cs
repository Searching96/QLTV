using MaterialDesignThemes.Wpf;
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

namespace QLTV
{
    /// <summary>
    /// Interaction logic for AWThemTheLoai.xaml
    /// </summary>
    public partial class AWThemTheLoai : Window
    {
        public AWThemTheLoai()
        {
            InitializeComponent();
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
            if (string.IsNullOrWhiteSpace(tbxTenTheLoai.Text))
            {
                icTenTheLoaiError.ToolTip = "Tên Thể Loại không được để trống";
                icTenTheLoaiError.Visibility = Visibility.Visible;
            }

            if (HasError())
            {
                MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi sửa!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string tenTheLoai = tbxTenTheLoai.Text;
            string moTa = tbxMoTa.Text;

            using (var context = new QLTVContext())
            {
                var newTheLoai = new THELOAI()
                {
                    TenTheLoai = tenTheLoai,
                    MoTa = moTa
                };

                context.THELOAI.Add(newTheLoai);
                context.SaveChanges();
            }

            this.DialogResult = true;
            this.Close();
        }

        private void tbxTenTheLoai_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxTenTheLoai.Text))
            {
                icTenTheLoaiError.ToolTip = "Tên Thể Loại không được để trống";
                icTenTheLoaiError.Visibility = Visibility.Visible;
                return;
            }

            if (tbxTenTheLoai.Text.Length > 100)
            {
                icTenTheLoaiError.ToolTip = "Tên Thể Loại không được quá 100 ký tự";
                icTenTheLoaiError.Visibility = Visibility.Visible;
                return;
            }

            foreach (char c in tbxTenTheLoai.Text)
            {
                if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
                {
                    icTenTheLoaiError.ToolTip = "Tên Thể Loại không được có số hay kí tự đặc biệt";
                    icTenTheLoaiError.Visibility = Visibility.Visible;
                    return;
                }
            }

            icTenTheLoaiError.Visibility = Visibility.Collapsed;
        }

        private void tbxMoTa_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbxMoTa.Text.Length > 500)
            {
                icMoTaError.ToolTip = "Mô Tả không được quá 500 ký tự";
                icMoTaError.Visibility = Visibility.Visible;
                return;
            }

            icMoTaError.Visibility = Visibility.Collapsed;
        }
    }
}

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

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
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

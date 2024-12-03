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
    /// Interaction logic for AWThemTacGia.xaml
    /// </summary>
    public partial class AWThemTacGia : Window
    {
        public AWThemTacGia()
        {
            InitializeComponent();
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            string tenTacGia = tbxTenTacGia.Text;
            int namSinh = int.Parse(tbxNamSinh.Text);
            string quocTich = tbxQuocTich.Text;

            using (var context = new QLTVContext())
            {
                var newTacGia = new TACGIA()
                {
                    TenTacGia = tenTacGia,
                    NamSinh = namSinh,
                    QuocTich = quocTich
                };

                context.TACGIA.Add(newTacGia);
                context.SaveChanges();
            }

            this.DialogResult = true;
            this.Close();
        }

        private void tbxTenTacGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxTenTacGia.Text))
            {
                icTenTacGiaError.ToolTip = "Tên Tác Giả không được để trống";
                icTenTacGiaError.Visibility = Visibility.Visible;
                return;
            }

            foreach (char c in tbxTenTacGia.Text)
            {
                if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
                {
                    icTenTacGiaError.ToolTip = "Tên Tác Giả không được có số hay kí tự đặc biệt";
                    icTenTacGiaError.Visibility = Visibility.Visible;
                    return;
                }
            }

            icTenTacGiaError.Visibility = Visibility.Collapsed;
        }

        private void tbxNamSinh_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxNamSinh.Text))
            {
                icNamSinhError.ToolTip = "Năm Sinh không được để trống";
                icNamSinhError.Visibility = Visibility.Visible;
                return;
            }

            if (!int.TryParse(tbxNamSinh.Text, out int ns) || ns <= 0)
            {
                icNamSinhError.ToolTip = "Năm Sinh phải là số nguyên dương";
                icNamSinhError.Visibility = Visibility.Visible;
                return;
            }

            icNamSinhError.Visibility = Visibility.Collapsed;
        }

        private void tbxQuocTich_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxQuocTich.Text))
            {
                icQuocTichError.ToolTip = "Quốc Tịch không được để trống";
                icQuocTichError.Visibility = Visibility.Visible;
                return;
            }

            foreach (char c in tbxQuocTich.Text)
            {
                if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
                {
                    icQuocTichError.ToolTip = "Quốc Tịch không được có số hay kí tự đặc biệt";
                    icQuocTichError.Visibility = Visibility.Visible;
                    return;
                }
            }

            icQuocTichError.Visibility = Visibility.Collapsed;
        }
    }
}

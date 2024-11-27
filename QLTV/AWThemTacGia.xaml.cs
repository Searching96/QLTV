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
    }
}

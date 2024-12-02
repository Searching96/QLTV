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
    }
}

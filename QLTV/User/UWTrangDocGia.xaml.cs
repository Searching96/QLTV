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

namespace QLTV.User
{
    /// <summary>
    /// Interaction logic for UWTrangDocGia.xaml
    /// </summary>
    public partial class UWTrangDocGia : Window
    {
        public UWTrangDocGia()
        {
            InitializeComponent();
        }

        private void btnFnMuonSach_Click(object sender, RoutedEventArgs e)
        {
            UUFnMuonSach ms = new UUFnMuonSach();
            USMainContent.Content = ms;
        }

        private void btnFnDisplaySach_Click(object sender, RoutedEventArgs e)
        {
            UUFnDisplaySach ds = new UUFnDisplaySach();
            USMainContent.Content = ds;
        }

        private void btnFnSachDangMuon_Click(object sender, RoutedEventArgs e)
        {
            UUFnSachDangMuon sdm = new UUFnSachDangMuon();
            USMainContent.Content = sdm;
        }


    }
}

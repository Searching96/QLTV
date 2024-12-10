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
        public List<UserControl> OpeningUC;

        public UWTrangDocGia()
        {
            InitializeComponent();
            OpeningUC = new List<UserControl>();
        }

        private void OpenUC(UserControl uc)
        {
            UserControl existingUC = OpeningUC.FirstOrDefault(x => x.GetType() == uc.GetType());

            if (existingUC == null)
            {
                OpeningUC.Add(uc);
                USMainContent.Content = uc;
            }
            else
            {
                USMainContent.Content = existingUC;
            }
        }

        private void btnFnMuonSach_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new UUFnMuonSach());
        }

        private void btnFnDisplaySach_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new UUFnDisplaySach());
        }

        private void btnFnSachDangMuon_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new UUFnSachDangMuon());
        }
    }
}

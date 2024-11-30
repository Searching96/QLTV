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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QLTV
{
    /// <summary>
    /// Interaction logic for ADUCQuanLySach.xaml
    /// </summary>
    public partial class AUFnQuanLySach : UserControl
    {
        List<UserControl> OpeningUC;

        public AUFnQuanLySach()
        {
            InitializeComponent();
            OpeningUC = new List<UserControl>();
        }

        private void btnQuanLyTuaSach_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new AUQuanLyTuaSach());
        }

        private void btnQuanLyTacGia_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new AUQuanLyTacGia());
        }

        private void btnQuanLyTheLoai_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new AUQuanLyTheLoai());
        }

        private void btnQuanLySach_Click(object sender, RoutedEventArgs e)
        {
            OpenUC(new AUQuanLySach());
        }

        private void OpenUC(UserControl uc)
        {
            UserControl existingUC = OpeningUC.FirstOrDefault(x => x.GetType() == uc.GetType());

            if (existingUC == null)
            {
                OpeningUC.Add(uc);
                frFnQuanLySach.Content = uc;
            }
            else
            {
                frFnQuanLySach.Content = existingUC;
            }
        }
    }
}

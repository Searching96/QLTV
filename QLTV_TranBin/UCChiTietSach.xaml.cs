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

namespace QLTV_TranBin
{
    /// <summary>
    /// Interaction logic for UCChiTietSach.xaml
    /// </summary>
    public partial class UCChiTietSach : UserControl
    {
        public UCChiTietSach()
        {
            InitializeComponent();
        }
        private void ImageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageListBox.SelectedItem is Image selectedImage)
            {
                PopupImage.Source = selectedImage.Source;
                ImagePopup.IsOpen = true;
            }
        }

        private void dgViTriSach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

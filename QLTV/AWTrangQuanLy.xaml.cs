using MaterialDesignColors;
using System.Text;
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
    /// Interaction logic for AWTrangQuanLy.xaml
    /// </summary>
    public partial class AWTrangQuanLy : Window
    {
        public AWTrangQuanLy()
        {
            InitializeComponent();
        }

        private void btnFnQuanLySach_Click(object sender, RoutedEventArgs e)
        {
            AUFnQuanLySach qls = new AUFnQuanLySach();
            ADMainContent.Content = qls;
            //// Lấy mã màu từ SolidColorBrush
            //var primaryBrush = (SolidColorBrush)App.AdminPriColor;
            //string colorCode = primaryBrush.Color.ToString(); // Trả về mã màu dạng #RRGGBB hoặc #AARRGGBB

            //MessageBox.Show($"PrimaryColor: {colorCode}");
        }
    }
}
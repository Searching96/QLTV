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

namespace UI_Chung
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string username;
        private string password;
        public MainWindow()
        {
            InitializeComponent();

        }

        private void KeyDown_ESC(object sender, KeyEventArgs e)
        {
            // Kiểm tra nếu phím Esc được nhấn
            if (e.Key == Key.Escape)
            {
                this.Close(); // Đóng cửa sổ
            }
        }
        
        
        private bool IsMaximize = false;

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (IsMaximize)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1080;
                    this.Height = 720;

                    IsMaximize = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;

                    IsMaximize = true;
                }
            }
        }
        

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnStatisticalReport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnReaderManage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBookManage_Click(object sender, RoutedEventArgs e)
        {
            UCQuanLySach qls = new UCQuanLySach();
            AdminMain.Content = qls;
        }

        private void btnQLMT_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDashBoard_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
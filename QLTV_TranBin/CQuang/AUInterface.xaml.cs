using QLTV_TranBin.Models;
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

namespace QLTV_TranBin.CQuang
{
    /// <summary>
    /// Interaction logic for AUInterface.xaml
    /// </summary>
    public partial class AUInterface : Window
    {
        private readonly QLTV2Context _context;
        public AUInterface()
        {
            InitializeComponent();
            _context = new QLTV2Context();
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Dashboard");
        }

        private void ReaderManagement_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new AUQuanLyDocGia();
            AUQuanLyDocGia readerManagementPage = new AUQuanLyDocGia();
            readerManagementPage.Show();
        }

        private void BookManagement_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("BookManagement");
        }

        private void LoanManagement_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("LoanManagement");
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Report");
        }

        private void btnQLTK_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new QuanLyTaiKhoan();
        }
    }
}

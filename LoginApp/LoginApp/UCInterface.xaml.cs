using LoginApp.Data;
using System.Windows;
using System.Windows.Controls;

namespace LoginApp
{
    public partial class UCInterface : Window
    {
        private readonly AppDbContext _context;
        public UCInterface()
        {
            InitializeComponent();
            _context = new AppDbContext();
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Dashboard");
        }

        private void ReaderManagement_Click(object sender, RoutedEventArgs e)
        {
            UCReaderManagement readerManagementPage = new UCReaderManagement();
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
            UCThongKe thongKe = new UCThongKe();
            thongKe.Show();
        }
    }
}
using QLTV;
using System;
using System.Windows;
using System.Windows.Controls;
using QLTV.Models;

namespace QLTV
{
    public partial class UCInterface : Window
    {
        private readonly DBContext _context;
        public UCInterface()
        {
            InitializeComponent();
            _context = new DBContext();
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
            MessageBox.Show("Report");
        }
    }
}
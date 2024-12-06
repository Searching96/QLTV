using QLTV;
using System;
using System.Windows;
using System.Windows.Controls;
using QLTV.Models;

namespace QLTV
{
    public partial class AUInterface : Window
    {
        private readonly QLTVContext _context;
        public AUInterface()
        {
            InitializeComponent();
            _context = new QLTVContext();
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            AWRatingWindow ratingWindow = new AWRatingWindow(4);
            ratingWindow.Show();
        }

        private void ReaderManagement_Click(object sender, RoutedEventArgs e)
        {
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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QLTV.Models;
using System.IO;
using System.Windows.Media;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using OfficeOpenXml;

namespace QLTV
{
    public partial class UCReaderManagement : Window
    {
        private QLTVContext _context = new QLTVContext();

        public UCReaderManagement()
        {
            InitializeComponent();
            LoadAccountsData();
            LoadReadersData();
            LoadReaderTypesData();
            LoadPenaltyReceiptsData();
        }

        // Accounts 
        private void LoadAccountsData()
        {
            var accounts = _context.TAIKHOAN.ToList();
            AccountsDataGrid.ItemsSource = accounts;
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // TAIKHOAN 
                var newAccount = new TAIKHOAN
                {
                    TenTaiKhoan = TenTaiKhoanTextBox.Text,
                    MaTaiKhoan = MaTaiKhoanTextBox.Text,
                    MatKhau = MatKhauTextBox.Text,
                    Email = EmailTaiKhoanTextBox.Text,
                    SinhNhat = SinhNhatTaiKhoanDatePicker.SelectedDate.Value,
                    DiaChi = DiaChiTaiKhoanTextBox.Text,
                    SDT = SDTTaiKhoanTextBox.Text,
                    TrangThai = bool.Parse(TrangThaiTaiKhoanTextBox.Text), 
                    IDPhanQuyen = int.Parse(IDPhanQuyenTaiKhoanTextBox.Text)
                };

                _context.TAIKHOAN.Add(newAccount);

                _context.SaveChanges();

                LoadAccountsData();

                TenTaiKhoanTextBox.Clear();
                MaTaiKhoanTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding account: " + ex.Message);
            }
        }

        private void UpdateAccount_Click(object sender, RoutedEventArgs e)
        {

            LoadAccountsData(); 
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {

            LoadAccountsData(); 
        }

        private void AccountsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        // Readers 
        private void LoadReadersData()
        {
            var readers = _context.DOCGIA.ToList();
            ReadersDataGrid.ItemsSource = readers;
        }

        private void AddReader_Click(object sender, RoutedEventArgs e)
        {

            LoadReadersData();
        }

        private void UpdateReader_Click(object sender, RoutedEventArgs e)
        {

            LoadReadersData(); 
        }

        private void DeleteReader_Click(object sender, RoutedEventArgs e)
        {

            LoadReadersData(); 
        }

        private void ReadersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        // Reader Types 
        private void LoadReaderTypesData()
        {
            var readerTypes = _context.LOAIDOCGIA.ToList();
            ReaderTypesDataGrid.ItemsSource = readerTypes;
        }

        private void AddReaderType_Click(object sender, RoutedEventArgs e)
        {

            LoadReaderTypesData();
        }

        private void UpdateReaderType_Click(object sender, RoutedEventArgs e)
        {

            LoadReaderTypesData(); 
        }

        private void DeleteReaderType_Click(object sender, RoutedEventArgs e)
        {

            LoadReaderTypesData(); 
        }

        private void ReaderTypesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        // Penalty Receipts 
        private void LoadPenaltyReceiptsData()
        {
            var penaltyReceipts = _context.PHIEUTHUTIENPHAT.ToList();
            PenaltyReceiptsDataGrid.ItemsSource = penaltyReceipts;
        }

        private void CreatePenaltyReceipt_Click(object sender, RoutedEventArgs e)
        {

            LoadPenaltyReceiptsData(); 
        }

        private void DeletePenalty_Click(object sender, RoutedEventArgs e)
        {

            LoadPenaltyReceiptsData(); 
        }

        private void EditPenalty_Click(object sender, RoutedEventArgs e)
        {

            LoadPenaltyReceiptsData(); 
        }

        private void ClearPenalty_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PrintPenaltyReceipt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PenaltyReceiptsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MaDocGiaPhat_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        // Search 
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchCriteria = SearchCriteriaComboBox.SelectedItem.ToString();
            string searchText = SearchTextBox.Text;

            switch (searchCriteria)
            {
                case "Mã Độc Giả":
                    break;
                case "Họ Tên":
                    break;
                case "Email":
                    break;
                case "Số Điện Thoại":
                    break;
                case "Loại Độc Giả":
                    break;
            }
        }

        // Import and Export Functionality
        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var package = new ExcelPackage(new FileInfo(openFileDialog.FileName));
                    var worksheet = package.Workbook.Worksheets[0]; 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error importing data: " + ex.Message);
                }
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files|*.xlsx";
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var package = new ExcelPackage();
                    var worksheet = package.Workbook.Worksheets.Add("Readers");
                    var excelFile = new FileInfo(saveFileDialog.FileName);
                    package.SaveAs(excelFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting data: " + ex.Message);
                }
            }
        }

        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Files|*.pdf";
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    Document document = new Document(PageSize.A4);
                    PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(saveFileDialog.FileName, FileMode.Create));

                    document.Open();
                    document.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting data: " + ex.Message);
                }
            }
        }
    }
}
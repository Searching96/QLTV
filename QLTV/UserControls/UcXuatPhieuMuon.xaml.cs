using QLTV.Models;
using System.IO.Packaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace QLTV.UserControls
{
    /// <summary>
    /// Interaction logic for UcXuatPhieuMuon.xaml
    /// </summary>
    /// 

    public partial class UcXuatPhieuMuon : UserControl
    {
        PHIEUMUON phieuMuon;

        public UcXuatPhieuMuon(PHIEUMUON _phieuMuon)
        {
            InitializeComponent();
            phieuMuon = _phieuMuon;
            int count = phieuMuon.CTPHIEUMUON.Count();
            tb_SLSach.Text = $"Số lượng sách : {count}";
            DataContext = _phieuMuon;
        }

        private void btnInPhieu_Click(object sender, RoutedEventArgs e)
        {
            // Show save file dialog
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = phieuMuon.MaPhieuMuon, // Default file name
                DefaultExt = ".pdf",  // Default file extension
                Filter = "PDF documents (.pdf)|*.pdf" // Filter files by extension
            };

            // Process save file dialog box results
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                // Get the selected file path
                string filename = dlg.FileName;

                try
                {
                    // Convert WPF visual to XPS and then to PDF
                    using (MemoryStream lMemoryStream = new MemoryStream())
                    {
                        using (Package package = Package.Open(lMemoryStream, FileMode.Create))
                        {
                            using (XpsDocument doc = new XpsDocument(package, CompressionOption.Maximum))
                            {
                                XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);

                                // This is your WPF Window or Visual to be converted
                                writer.Write(PhieuMuonContent);
                            }
                        }

                        // Convert XPS to PDF
                        using (MemoryStream outStream = new MemoryStream())
                        {
                            PdfSharp.Xps.XpsConverter.Convert(lMemoryStream, outStream, false);

                            // Write the PDF file to the selected location
                            using (FileStream fileStream = new FileStream(filename, FileMode.Create))
                            {
                                outStream.CopyTo(fileStream);
                            }
                        }
                    }

                    MessageBox.Show("Đã lưu phiếu mượn.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnThoat_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}

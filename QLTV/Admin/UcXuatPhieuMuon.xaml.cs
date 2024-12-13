using QLTV.Models;
using System;
using System.IO.Packaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace QLTV.Admin
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
            int count = phieuMuon.CTPHIEUMUON.Count;
            tb_SLSach.Text = $"Số lượng sách: {count}";
            DataContext = phieuMuon;
        }

        private void btnInPhieu_Click(object sender, RoutedEventArgs e)
        {
            // Mở hộp thoại lưu file
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = phieuMuon.MaPhieuMuon, // Tên file mặc định
                DefaultExt = ".pdf",  // Loại file mặc định
                Filter = "PDF documents (.pdf)|*.pdf" // Lọc theo loại file
            };

            // Xử lí kết quả hộp thoại save file 
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                // Lấy đường dẫn của file đã chọn
                string filename = dlg.FileName;

                try
                {
                    // Xuất thuộc tính WPF thành file XPS và chuyển thành file PDF
                    using (MemoryStream lMemoryStream = new MemoryStream())
                    {
                        using (Package package = Package.Open(lMemoryStream, FileMode.Create))
                        {
                            using (XpsDocument doc = new XpsDocument(package, CompressionOption.Maximum))
                            {
                                XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);

                                // Nội dung thuộc tính WPF cần xuất
                                writer.Write(PhieuMuonContent);
                            }
                        }

                        // Chuyển file XPS thành file PDF
                        using (MemoryStream outStream = new MemoryStream())
                        {
                            PdfSharp.Xps.XpsConverter.Convert(lMemoryStream, outStream, false);

                            // Ghi nội dung vào file PDF ở địa chỉ đã lưu
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

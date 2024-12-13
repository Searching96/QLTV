using QLTV.Models;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.IO;
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
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace QLTV.Admin
{
    /// <summary>
    /// Interaction logic for UcXuatPhieuTra.xaml
    /// </summary>
    public partial class UcXuatPhieuTra : UserControl
    {
        PHIEUTRA phieuTra;

        public UcXuatPhieuTra(PHIEUTRA _phieuTra)
        {
            InitializeComponent();
            phieuTra = _phieuTra;
            int count = phieuTra.CTPHIEUTRA.Count();
            decimal TongTienPhat = phieuTra.CTPHIEUTRA.Sum(pt => pt.TienPhat);
            string DocGia = _phieuTra.CTPHIEUTRA.First().IDPhieuMuonNavigation.IDDocGiaNavigation.IDTaiKhoanNavigation.TenTaiKhoan;
            tb_DocGia.Text = $"Độc giả : {DocGia}";
            tb_SLSach.Text = $"Số lượng sách : {count}";
            tb_TongTienPhat.Text = $"Tổng tiền phạt : {TongTienPhat} (VND)";
            DataContext = phieuTra;
        }
        private void btnInPhieu_Click(object sender, RoutedEventArgs e)
        {
            // Mở hộp thoại lưu file
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = phieuTra.MaPhieuTra, // Tên file mặc định
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
                                writer.Write(PhieuTraContent);
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

                    MessageBox.Show("Đã lưu phiếu trả.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
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

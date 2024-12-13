using Microsoft.EntityFrameworkCore;
using QLTV;
using QLTV.Models;
using System.Windows;
using System.Windows.Controls;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing.Imaging;
using System.Drawing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Linq;

namespace QLTV.Admin
{
    /// <summary>
    /// Interaction logic for AWPhieuThuTienPhat.xaml
    /// </summary>
    public partial class AWPhieuThuTienPhat : Window
    {
        private readonly QLTVContext _context = new QLTVContext(); 

        public AWPhieuThuTienPhat(PHIEUTHUTIENPHAT receipt)
        {
            InitializeComponent();
            _context = new QLTVContext(); // Khởi tạo _context
            LoadPenaltyReceipt(receipt);
        }

        private void LoadPenaltyReceipt(PHIEUTHUTIENPHAT receipt)
        {
            // Lấy thông tin độc giả
            var docGia = _context.DOCGIA
                .Include(d => d.IDTaiKhoanNavigation) // Include related data
                .FirstOrDefault(d => d.ID == receipt.IDDocGia); // Execute the query

            if (docGia == null)
            {
                MessageBox.Show("Không tìm thấy độc giả!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Gán dữ liệu vào controls
            tbxMaPhieuThu.Text = receipt.MaPTTP;
            tbxNgayThu.Text = receipt.NgayThu.ToString("dd/MM/yyyy");
            tbxMaDocGia.Text = docGia.MaDocGia;
            tbxTenDocGia.Text = docGia.IDTaiKhoanNavigation.TenTaiKhoan;
            tbxSoTienThu.Text = receipt.SoTienThu.ToString("N2");
            tbxConNo.Text = receipt.ConLai.ToString("N2");
            tbxTongNo.Text = receipt.TongNo.ToString("N2");
        }
        private void btnIn_Click(object sender, RoutedEventArgs e)
        {
            // Ẩn nút IN trước khi in
            btnIn.Visibility = Visibility.Collapsed;

            // Tạo render bitmap của window
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)ActualWidth,
                (int)ActualHeight,
                96, 96,
                PixelFormats.Pbgra32);

            renderBitmap.Render(this);

            // Chuyển đổi bitmap sang ảnh
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            // Lưu file PDF
            string pdfPath = $"PhieuThuTienPhat_{tbxMaPhieuThu.Text}_{DateTime.Now:yyyyMMdd}.pdf";

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Position = 0;

                Document document = new Document();
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(pdfPath, FileMode.Create));

                document.Open();

                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(stream.ToArray());
                image.ScaleToFit(document.PageSize.Width - 50, document.PageSize.Height - 50);
                image.Alignment = Element.ALIGN_CENTER;

                document.Add(image);
                document.Close();
                writer.Close();
            }

            // Hiện lại nút IN
            btnIn.Visibility = Visibility.Visible;

            // Mở file PDF sau khi tạo
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(pdfPath) { UseShellExecute = true });
        }
    }
}

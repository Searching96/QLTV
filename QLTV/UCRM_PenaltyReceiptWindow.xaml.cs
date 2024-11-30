using Microsoft.EntityFrameworkCore;
using QLTV;
using QLTV.Models;
using System.Windows;
using System.Windows.Controls;

namespace QLTV
{
    public partial class UCRM_PenaltyReceiptWindow : Window
    {
        private readonly QLTVContext _context;

        public UCRM_PenaltyReceiptWindow(PHIEUTHUTIENPHAT receipt)
        {
            InitializeComponent();
            LoadPenaltyReceipt(receipt);
        }

        private void LoadPenaltyReceipt(PHIEUTHUTIENPHAT receipt)
        {
            // Lấy thông tin độc giả
            var docGia = _context.DOCGIA.Include(d => d.IDTaiKhoanNavigation)
                                         .FirstOrDefault(d => d.ID == receipt.IDDocGia);

            if (docGia == null)
            {
                MessageBox.Show("Không tìm thấy độc giả!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Gán dữ liệu vào controls
            MaPhieuThuTextBlock.Text = receipt.MaPTTP; 
            NgayThuTextBlock.Text = receipt.NgayThu.ToString("dd/MM/yyyy");
            MaDocGiaTextBlock.Text = docGia.MaDocGia;
            TenDocGiaTextBlock.Text = docGia.IDTaiKhoanNavigation.TenTaiKhoan; 
            SoTienThuTextBlock.Text = receipt.SoTienThu.ToString("N2");
            ConNoTextBlock.Text = receipt.ConLai.ToString("N2");  
        }
    }
}

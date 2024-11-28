using System.Windows;
using System.Windows.Controls;

namespace LoginApp
{
    public partial class UCRM_PenaltyReceiptWindow : Window
    {
        public UCRM_PenaltyReceiptWindow(PenaltyReceipt receipt) 
        {
            InitializeComponent();
            LoadPenaltyReceipt(receipt);
        }

        private void LoadPenaltyReceipt(PenaltyReceipt receipt)
        {
            // Gán dữ liệu từ phiếu thu vào controls 
            MaPhieuThuTextBlock.Text = receipt.MaPhieuThu;
            NgayThuTextBlock.Text = receipt.NgayThu.ToString("dd/MM/yyyy");
            MaDocGiaTextBlock.Text = receipt.MaDocGia;
            TenDocGiaTextBlock.Text = receipt.TenDocGia;
            SoTienThuTextBlock.Text = receipt.SoTienThu.ToString("N2"); 
            ConNoTextBlock.Text = receipt.ConNo.ToString("N2");
            NguoiThuTextBlock.Text = receipt.NguoiThu;
            GhiChuTextBlock.Text = receipt.GhiChu;
        }
    }
}
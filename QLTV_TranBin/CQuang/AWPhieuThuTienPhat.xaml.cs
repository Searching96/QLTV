using Microsoft.EntityFrameworkCore;
using QLTV_TranBin.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace QLTV_TranBin.CQuang
{
    /// <summary>
    /// Interaction logic for AWPhieuThuTienPhat.xaml
    /// </summary>
    public partial class AWPhieuThuTienPhat : Window
    {
        private readonly QLTV2Context _context = new QLTV2Context(); // Khởi tạo _context

        public AWPhieuThuTienPhat(PHIEUTHUTIENPHAT receipt)
        {
            InitializeComponent();
            _context = new QLTV2Context(); // Khởi tạo _context
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
            TongNoTextBlock.Text = receipt.TongNo.ToString("N2");
        }
    }
}

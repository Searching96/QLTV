using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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

namespace QLTV.User
{
    /// <summary>
    /// Interaction logic for UUFnSachDangMuon.xaml
    /// </summary>
    public partial class UUFnSachDangMuon : UserControl
    {
        public class SachDangMuonViewModel
        {
            public string MaSach { get; set; }
            public string TenTuaSach { get; set; }
            public string MaPhieuMuon { get; set; }
            public string ViTri { get; set; }
            public string TinhTrang { get; set; }
            public string NgayConLai { get; set; }
            public string HanTra { get; set; }
            public decimal DanhGia { get; set; }  // Assuming DanhGia is a numeric rating
        }


        public UUFnSachDangMuon()
        {
            InitializeComponent();
            LoadSachDangMuon();
        }

        private void LoadSachDangMuon()
        {
            using (var context = new QLTVContext())
            {
                // Lấy DOCGIA hiện tại (giả sử bạn đã có ID của độc giả hiện tại)
                var docGiaId = 1; // Giả sử lấy ID độc giả đầu tiên, bạn có thể thay đổi theo yêu cầu thực tế

                // Truy vấn các sách chưa trả của độc giả này
                var dsSachDangMuon = context.CTPHIEUMUON
                    .Where(ct => ct.IDPhieuMuonNavigation.IDDocGia == docGiaId 
                        && !ct.IDPhieuMuonNavigation.IsPending
                        && !ct.IDPhieuMuonNavigation.CTPHIEUTRA.Any()) // Chưa có phiếu trả
                    .Include(ct => ct.IDSachNavigation) // Bao gồm SACH
                    .ThenInclude(s => s.IDTuaSachNavigation) // Bao gồm TUASACH liên quan
                    .Include(ct => ct.IDTinhTrangMuonNavigation) // Bao gồm TINHTRANGMuon
                    .Include(ct => ct.IDPhieuMuonNavigation) // Bao gồm PHIEUMUON
                    .ThenInclude(p => p.DANHGIA) // Bao gồm DANHGIA của PHIEUMUON
                    .Select(ct => new SachDangMuonViewModel
                    {
                        MaSach = ct.IDSachNavigation.MaSach,
                        TenTuaSach = ct.IDSachNavigation.IDTuaSachNavigation.TenTuaSach,
                        MaPhieuMuon = ct.IDPhieuMuonNavigation.MaPhieuMuon,
                        ViTri = ct.IDSachNavigation.ViTri,
                        TinhTrang = ct.IDTinhTrangMuonNavigation.TenTinhTrang, // Tình trạng mượn
                        NgayConLai = (ct.HanTra - DateTime.Now).Days.ToString() + " ngày", // Ngày còn lại
                        HanTra = ct.HanTra.ToString("dd/MM/yyyy"),
                        DanhGia = ct.IDSachNavigation.DANHGIA
                            .Where(dg => dg.IDPhieuMuon == ct.IDPhieuMuon)
                            .Select(dg => dg.DanhGia)
                            .FirstOrDefault()
                    })
                    .ToList();

                dgSachDangMuon.ItemsSource = dsSachDangMuon; // Gán nguồn dữ liệu cho DataGrid
            }
        }

        private void RatingBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var row = ((FrameworkElement)sender).DataContext as SachDangMuonViewModel;

            if (row == null)
            {
                return;
            }

            var newRating = e.NewValue;

            using (var context = new QLTVContext())
            {
                var existingDanhGia = context.DANHGIA
                    .FirstOrDefault(dg =>
                        dg.IDPhieuMuonNavigation.MaPhieuMuon == row.MaPhieuMuon &&
                        dg.IDSachNavigation.MaSach == row.MaSach);

                if (existingDanhGia != null)
                {
                    existingDanhGia.DanhGia = Convert.ToDecimal(newRating);
                }
                else
                {
                    var sachEntity = context.SACH.FirstOrDefault(s => s.MaSach == row.MaSach);
                    var phieuMuonEntity = context.PHIEUMUON.FirstOrDefault(pm => pm.MaPhieuMuon == row.MaPhieuMuon);

                    if (sachEntity != null && phieuMuonEntity != null)
                    {
                        var newDanhGia = new DANHGIA
                        {
                            IDSach = sachEntity.ID,  // Use the actual ID, not MaSach
                            IDPhieuMuon = phieuMuonEntity.ID,
                            DanhGia = Convert.ToDecimal(newRating)
                        };
                        context.DANHGIA.Add(newDanhGia);
                    }
                }

                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    // Log error if needed, you can also use a logger here
                    MessageBox.Show($"Lỗi khi lưu dữ liệu: {ex.Message}");
                }
            }
        }
    }
}

using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using QLTV.Properties;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.Data;

namespace QLTV.Admin
{

    public class NgayConLaiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int daysRemainingInt)
            {
                if (daysRemainingInt < 0)
                {
                    return $"Hết hạn {Math.Abs(daysRemainingInt)} ngày";
                }
                if (daysRemainingInt <= 7)
                {
                    return $"Còn lại {daysRemainingInt} ngày";
                }
                return $"{daysRemainingInt} ngày";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Interaction logic for UUFnSachDangMuon.xaml
    /// </summary>
    public partial class UUFnSachDangMuon : UserControl
    {
        DOCGIA docGiaHT = new DOCGIA();

        public class SachDangMuonViewModel
        {
            public string MaSach { get; set; }
            public string TenTuaSach { get; set; }
            public string MaPhieuMuon { get; set; }
            public string ViTri { get; set; }
            public string TinhTrang { get; set; }
            public int NgayConLai { get; set; }
            public string HanTra { get; set; }
            public decimal DanhGia { get; set; }  // Assuming DanhGia is a numeric rating
        }

        private CollectionViewSource viewSource;

        public UUFnSachDangMuon()
        {
            InitializeComponent();
            LoadDocGia();
        }

        private async void LoadDocGia()
        {
            using (var context = new QLTVContext())
            {
                var docGia = await context.DOCGIA
                    .Include(d => d.IDTaiKhoanNavigation)
                    .Include(d => d.IDLoaiDocGiaNavigation)
                    .Include(d => d.PHIEUMUON)
                        .ThenInclude(pm => pm.CTPHIEUMUON)
                    .Include(d => d.PHIEUMUON)
                        .ThenInclude(pm => pm.CTPHIEUTRA)
                    .ToListAsync();

                viewSource = new CollectionViewSource();
                viewSource.Source = docGia;
                cboDocGia.ItemsSource = viewSource.View;
            }
        }

        private void cboDocGia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var docGia = (sender as ComboBox).SelectedItem as DOCGIA;
            if (docGia == null) return;
            docGiaHT = docGia;
            LoadSachDangMuon(docGia.ID);
        }

        private void LoadSachDangMuon(int docGiaId, int Dieu_Kien_Min = int.MinValue, int Dieu_Kien_Max = int.MaxValue)
        {
            using (var context = new QLTVContext())
            {
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
                        NgayConLai = (ct.HanTra - DateTime.Now).Days, // Ngày còn lại
                        HanTra = ct.HanTra.ToString("dd/MM/yyyy"),
                        DanhGia = ct.IDSachNavigation.DANHGIA
                            .Where(dg => dg.IDPhieuMuon == ct.IDPhieuMuon)
                            .Select(dg => dg.DanhGia)
                            .FirstOrDefault()
                    })
                    .ToList();

                dgSachDangMuon.ItemsSource = dsSachDangMuon.Where(ct => ct.NgayConLai >= Dieu_Kien_Min && ct.NgayConLai <= Dieu_Kien_Max); // Gán nguồn dữ liệu cho DataGrid
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

        private void cboLoc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSachDangMuon != null && docGiaHT != null)
            {
                var selectedFilter = (cboLoc.SelectedItem as ComboBoxItem)?.Content.ToString();
                int dieuKienMax = selectedFilter switch
                {
                    "Đã hết hạn" => -1,
                    "Sắp hết hạn" => 7,
                    _ => int.MaxValue // "Tất cả" or any other case
                };
                int dieuKienMin = selectedFilter switch
                {
                    "Chưa hết hạn" => 0,
                    "Sắp hết hạn" => 0,
                    _ => int.MinValue // "Tất cả" or any other case
                };

                LoadSachDangMuon(docGiaHT.ID, dieuKienMin, dieuKienMax);
            }
        }
    }
}
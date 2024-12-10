using Microsoft.CSharp.RuntimeBinder;
using QLTV.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QLTV.User
{
    /// <summary>
    /// Interaction logic for UUChiTietSach.xaml
    /// </summary>
    public partial class UUChiTietSach : UserControl
    {
        public string MaTuaSach;
        public UUChiTietSach(string maTuaSach)
        {
            InitializeComponent();
            MaTuaSach = maTuaSach;
            LoadSach();
        }

        public class SachViewModel
        {
            public string MaSach { get; set; }
            public string TuaSach { get; set; }
            public string ViTri { get; set; }
            public decimal TriGia { get; set; }
            public string TinhTrang { get; set; }
        }

        private void LoadSach()
        {
            try
            {
                using (var dbcontext = new QLTVContext())
                {
                    // Lấy ID Tựa Sách
                    var idTuaSach = dbcontext.TUASACH
                        .Where(ts => ts.MaTuaSach == MaTuaSach && !ts.IsDeleted)
                        .Select(ts => ts.ID)
                        .FirstOrDefault();

                    if (idTuaSach == 0)
                    {
                        MessageBox.Show("Không tìm thấy ID Tựa Sách tương ứng.", "Thông báo");
                        return;
                    }

                    // Lấy danh sách Sách
                    var data = dbcontext.SACH
                        .Where(s => s.IDTuaSach == idTuaSach && s.IsAvailable == true && s.IsDeleted == false)
                        .Select(s => new SachViewModel
                        {
                            MaSach = s.MaSach,
                            TuaSach = s.IDTuaSachNavigation.TenTuaSach,
                            ViTri = "abc",
                            TriGia = s.TriGia,
                            TinhTrang = s.IDTinhTrangNavigation.TenTinhTrang
                        })
                        .ToList();

                    if (data.Count == 0)
                    {
                        dgSach.Visibility = Visibility.Collapsed;
                        tblTenDg.Text = "Tựa này đã hết sách có sẵn!";
                    }

                    // Gán dữ liệu vào DataGrid
                    dgSach.ItemsSource = data;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnMuonNgay_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dữ liệu sách từ DataContext của button
            var button = sender as Button;
            var sach = button?.DataContext as SachViewModel; // DataContext chứa dữ liệu row của DataGrid
            if (sach != null)
            {
                var maSach = sach.MaSach;

                using (var context = new QLTVContext())
                {
                    var sachSeMuon = context.SACH
                        .Where(s => s.MaSach == maSach)
                        .FirstOrDefault();

                    sachSeMuon.IsAvailable = false;
                    context.SaveChanges();
                    LoadSach();
                }

                    // Tìm ra UWTrangDocGia đang mở và kiểm tra OpeningUC
                var parentWindow = Window.GetWindow(this) as UWTrangDocGia;
                if (parentWindow != null)
                {
                    // Kiểm tra nếu UUFnMuonSach đã có trong danh sách OpeningUC
                    var existingUC = parentWindow.OpeningUC.FirstOrDefault(x => x.GetType() == typeof(UUFnMuonSach));

                    if (existingUC == null)
                    {
                        // Nếu chưa có, tạo mới và thêm vào
                        var newUC = new UUFnMuonSach();
                        parentWindow.OpeningUC.Add(newUC);

                        // Cập nhật Content
                        parentWindow.USMainContent.Content = newUC;
                        existingUC = newUC;
                    }
                    // Tiếp theo truyền MaSach vào UUFnMuonSach
                    (existingUC as UUFnMuonSach)?.MuonNgaySach(maSach);
                }
            }
        }
    }
}

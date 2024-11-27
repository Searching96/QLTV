using Microsoft.EntityFrameworkCore;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace QLTV
{
    /// <summary>
    /// Interaction logic for AUQuanLySach.xaml
    /// </summary>
    public partial class AUQuanLySach : UserControl
    {
        public AUQuanLySach()
        {
            InitializeComponent();
            LoadSach();
        }

        private void LoadSach()
        {
            using (var context = new QLTVContext())
            {
                var dsSach = context.SACH
                    .Where(s => !s.IsDeleted && !s.IDTuaSachNavigation.IsDeleted)
                    .Select(s => new
                    {
                        s.MaSach,
                        TuaSach = s.IDTuaSachNavigation.TenTuaSach,
                        DSTacGia = string.Join(", ", s.IDTuaSachNavigation.TUASACH_TACGIA
                            .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                        DSTheLoai = string.Join(", ", s.IDTuaSachNavigation.TUASACH_THELOAI
                            .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai)),
                        s.NhaXuatBan,
                        s.NamXuatBan,
                        NgayNhap = s.NgayNhap.ToString("dd/MM/yyyy"),
                        s.TriGia,
                        TinhTrang = s.IDTinhTrangNavigation.TenTinhTrang
                    })
                    .ToList();

                dgSach.ItemsSource = dsSach;
            }
        }

        private void dgSach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = dgSach.SelectedItem;

            if (selectedItem != null)
            {
                dynamic selectedBook = selectedItem;
                tbxMaSach.Text = selectedBook.MaSach;
                tbxTuaSach.Text = selectedBook.TuaSach;
                tbxDSTacGia.Text = selectedBook.DSTacGia;
                tbxDSTheLoai.Text = selectedBook.DSTheLoai;
                tbxNhaXuatBan.Text = selectedBook.NhaXuatBan;
                tbxNamXuatBan.Text = selectedBook.NamXuatBan.ToString();
                tbxNgayNhap.Text = selectedBook.NgayNhap;
                tbxTriGia.Text = selectedBook.TriGia.ToString();
                tbxTinhTrang.Text = selectedBook.TinhTrang;
            }
            else
            {
                tbxMaSach.Text = "";
                tbxTuaSach.Text = "";
                tbxDSTacGia.Text = "";
                tbxDSTheLoai.Text = "";
                tbxNhaXuatBan.Text = "";
                tbxNamXuatBan.Text = "";
                tbxNgayNhap.Text = "";
                tbxTriGia.Text = "";
                tbxTinhTrang.Text = "";
            }
        }

        private void btnThemSach_Click(object sender, RoutedEventArgs e)
        {
            AWThemSach awThemSach = new AWThemSach();
            if (awThemSach.ShowDialog() == true)
                LoadSach();
        }

        private void btnSuaSach_Click(object sender, RoutedEventArgs e)
        {
            if (dgSach.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn sách cần sửa!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new QLTVContext())
            {
                // Lấy MaTuaSach từ item được chọn
                dynamic selectedItem = dgSach.SelectedItem;
                string maSach = selectedItem.MaSach;

                // Tìm tựa sách cần sửa
                var sachToUpdate = context.SACH
                    .FirstOrDefault(s => s.MaSach == maSach);

                if (sachToUpdate != null)
                {
                    // Cập nhật thông tin cơ bản
                    sachToUpdate.NhaXuatBan = tbxNhaXuatBan.Text;
                    sachToUpdate.NamXuatBan = int.Parse(tbxNamXuatBan.Text);
                    sachToUpdate.NgayNhap = DateTime.Parse(tbxNgayNhap.Text);
                    sachToUpdate.TriGia = decimal.Parse(tbxTriGia.Text);

                    // Lưu tất cả thay đổi
                    context.SaveChanges();

                    MessageBox.Show("Cập nhật tựa sách thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh lại DataGrid
                    LoadSach();
                }
            }
        }

        private void btnXoaSach_Click(object sender, RoutedEventArgs e)
        {
            if (dgSach.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn sách cần xóa!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selectedItem = dgSach.SelectedItem;
            string maSach = selectedItem.MaSach;

            MessageBoxResult mbrXacNhan = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa sách có mã: {maSach}?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (mbrXacNhan == MessageBoxResult.Yes)
            {
                using (var context = new QLTVContext())
                {
                    var sachToDelete = context.SACH
                        .FirstOrDefault(s => s.MaSach == maSach);

                    // Truong hop bat dong bo?
                    if (sachToDelete != null)
                    {
                        sachToDelete.IsDeleted = true;
                        var tuaSach = context.TUASACH
                            .FirstOrDefault(ts => ts.ID == sachToDelete.IDTuaSach);
                        if (tuaSach != null)
                            tuaSach.SoLuong--;
                        context.SaveChanges();
                        MessageBox.Show($"Sách có mã {maSach} đã được xóa.", "Thông báo",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadSach();
                    }
                }
            }
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            LoadSach();
        }

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

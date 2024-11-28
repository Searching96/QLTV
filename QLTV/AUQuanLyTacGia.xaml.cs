using Microsoft.EntityFrameworkCore;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace QLTV
{
    /// <summary>
    /// Interaction logic for AUQuanLyTacGia.xaml
    /// </summary>
    public partial class AUQuanLyTacGia : UserControl
    {
        public AUQuanLyTacGia()
        {
            InitializeComponent();
            LoadTacGia();
        }

        private void LoadTacGia()
        {
            using (var context = new QLTVContext())
            {
                var dsTacGia = context.TACGIA
                                      .Where(tg => !tg.IsDeleted)
                                      .Select(tg => new
                                      {
                                          tg.MaTacGia,
                                          tg.TenTacGia,
                                          tg.NamSinh,
                                          tg.QuocTich,
                                      })
                                      .ToList();

                dgTacGia.ItemsSource = dsTacGia;
            }
        }

        private void btnThemTacGia_Click(object sender, RoutedEventArgs e)
        {
            AWThemTacGia awThemTacGia = new AWThemTacGia();
            if (awThemTacGia.ShowDialog() == true)
                LoadTacGia();
        }

        private void btnSuaTacGia_Click(object sender, RoutedEventArgs e)
        {
            if (dgTacGia.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn tác giả cần sửa!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new QLTVContext())
            {
                // Lấy MaTuaSach từ item được chọn
                dynamic selectedItem = dgTacGia.SelectedItem;
                string maTacGia = selectedItem.MaTacGia;

                // Tìm tựa sách cần sửa
                var tacGiaToUpdate = context.TACGIA
                                            .FirstOrDefault(tg => tg.MaTacGia == maTacGia);

                if (tacGiaToUpdate != null)
                {
                    // Cập nhật thông tin cơ bản
                    tacGiaToUpdate.TenTacGia = tbxTenTacGia.Text;
                    tacGiaToUpdate.NamSinh = int.Parse(tbxNamSinh.Text);
                    tacGiaToUpdate.QuocTich = tbxQuocTich.Text;

                    // Lưu tất cả thay đổi
                    context.SaveChanges();

                    MessageBox.Show("Cập nhật tác giả thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh lại DataGrid
                    LoadTacGia();
                }
            }
        }

        private void dgTacGia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = dgTacGia.SelectedItem;

            if (selectedItem != null)
            {
                dynamic selectedAuthor = selectedItem;
                tbxMaTacGia.Text = selectedAuthor.MaTacGia;
                tbxTenTacGia.Text = selectedAuthor.TenTacGia;
                tbxNamSinh.Text = selectedAuthor.NamSinh.ToString();
                tbxQuocTich.Text = selectedAuthor.QuocTich;
            }
            else
            {
                tbxMaTacGia.Text = "";
                tbxTenTacGia.Text = "";
                tbxNamSinh.Text = "";
                tbxQuocTich.Text = "";
            }
        }

        private void btnXoaTacGia_Click(object sender, RoutedEventArgs e)
        {
            if (dgTacGia.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn tác giả cần xóa!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selectedItem = dgTacGia.SelectedItem;
            string maTacGia = selectedItem.MaTacGia;

            MessageBoxResult mbrXacNhan = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa tác giả có mã: {maTacGia}?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (mbrXacNhan == MessageBoxResult.Yes)
            {
                using (var context = new QLTVContext())
                {
                    var tacGiaToDelete = context.TACGIA
                        .Include(tg => tg.TUASACH_TACGIA)
                        .FirstOrDefault(tg => tg.MaTacGia == maTacGia);

                    // Truong hop bat dong bo?
                    if (tacGiaToDelete != null)
                    {
                        context.TUASACH_TACGIA.RemoveRange(tacGiaToDelete.TUASACH_TACGIA);
                        tacGiaToDelete.IsDeleted = true;
                        context.SaveChanges();
                        MessageBox.Show($"Tác giả có mã {maTacGia} đã được xóa.", "Thông báo", 
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTacGia();
                    }
                }
            }
        }
        
        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            LoadTacGia();
        }

        private string NormalizeString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return new string(
                text.Normalize(NormalizationForm.FormD)
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    .ToArray()
            ).Normalize(NormalizationForm.FormC).ToLower();
        }

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = NormalizeString(tbxThongTinTimKiem.Text.Trim().ToLower());
            string selectedProperty = ((ComboBoxItem)cbbThuocTinhTimKiem.SelectedItem)?.Content.ToString();

            // Kiểm tra nếu không có gì được chọn
            if (string.IsNullOrEmpty(selectedProperty))
            {
                MessageBox.Show("Vui lòng chọn thuộc tính tìm kiếm", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new QLTVContext())
            {
                var query = context.TACGIA
                    .Where(tg => !tg.IsDeleted)
                    .Select(tg => new
                    {
                        tg.MaTacGia,
                        tg.TenTacGia,
                        tg.NamSinh,
                        tg.QuocTich,
                    })
                    .AsEnumerable() // Chuyển về IEnumerable để lọc trên máy khách
                    .ToList();

                // Lọc theo thuộc tính tìm kiếm được chọn
                if (selectedProperty == "Tên Tác Giả")
                {
                    query = query.Where(tg => NormalizeString(tg.TenTacGia).Contains(NormalizeString(searchTerm))).ToList();
                }
                else if (selectedProperty == "Năm Sinh")
                {
                    int namSinh;
                    if (!int.TryParse(searchTerm, out namSinh))
                        MessageBox.Show("Năm sinh phải là số nguyên", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    query = query.Where(tg => tg.NamSinh == namSinh).ToList();
                }
                else if (selectedProperty == "Quốc Tịch")
                {
                    query = query.Where(tg => NormalizeString(tg.QuocTich).Contains(NormalizeString(searchTerm))).ToList();
                }

                // Cập nhật ItemsSource cho DataGrid
                dgTacGia.ItemsSource = query;
            }
        }

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnImportExcel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

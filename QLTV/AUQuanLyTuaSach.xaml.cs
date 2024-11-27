using Microsoft.EntityFrameworkCore;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace QLTV
{
    /// <summary>
    /// Interaction logic for ADUCQuanLySach.xaml
    /// </summary>
    public partial class AUQuanLyTuaSach : UserControl
    {

        public AUQuanLyTuaSach()
        {
            InitializeComponent();
            LoadTuaSach();
        }

        private void LoadTuaSach()
        {
            using (var context = new QLTVContext())
            {
                var dsTuaSach = context.TUASACH
                                       .Where(ts => !ts.IsDeleted)
                                       .Select(ts => new
                                       {
                                           ts.MaTuaSach,
                                           ts.TenTuaSach,
                                           ts.SoLuong,
                                           ts.HanMuonToiDa,
                                           DSTacGia = string.Join(", ", ts.TUASACH_TACGIA
                                                            .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                                           DSTheLoai = string.Join(", ", ts.TUASACH_THELOAI
                                                             .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai))
                                       })
                                       .ToList();

                dgTuaSach.ItemsSource = dsTuaSach;
            }
        }

        private void btnThemTuaSach_Click(object sender, RoutedEventArgs e)
        {
            AWThemTuaSach awThemTuaSach = new AWThemTuaSach();
            if (awThemTuaSach.ShowDialog() == true)
                LoadTuaSach();
        }

        private void btnSuaTuaSach_Click(object sender, RoutedEventArgs e)
        {
            if (dgTuaSach.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn tựa sách cần sửa!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new QLTVContext())
            {
                // Lấy MaTuaSach từ item được chọn
                dynamic selectedItem = dgTuaSach.SelectedItem;
                string maTuaSach = selectedItem.MaTuaSach;

                // Tìm tựa sách cần sửa
                var tuaSachToUpdate = context.TUASACH
                                             .Include(ts => ts.TUASACH_TACGIA)
                                             .Include(ts => ts.TUASACH_THELOAI)
                                             .FirstOrDefault(ts => ts.MaTuaSach == maTuaSach);

                if (tuaSachToUpdate != null)
                {
                    // Cập nhật thông tin cơ bản
                    tuaSachToUpdate.TenTuaSach = tbxTenTuaSach.Text;
                    tuaSachToUpdate.SoLuong = int.Parse(tbxSoLuong.Text);
                    tuaSachToUpdate.HanMuonToiDa = int.Parse(tbxHanMuonToiDa.Text.Replace(" tuần", ""));

                    // Cập nhật quan hệ với Tác giả
                    // Xóa các quan hệ cũ
                    context.TUASACH_TACGIA.RemoveRange(tuaSachToUpdate.TUASACH_TACGIA);

                    // Thêm quan hệ mới từ danh sách tác giả đã chọn
                    var selectedAuthors = ParseDSTacGia(tbxDSTacGia.Text);
                    foreach (var author in selectedAuthors)
                    {
                        tuaSachToUpdate.TUASACH_TACGIA.Add(new TUASACH_TACGIA
                        {
                            IDTuaSach = tuaSachToUpdate.ID,
                            IDTacGia = author.ID
                        });
                    }

                    // Cập nhật quan hệ với Thể loại
                    // Xóa các quan hệ cũ
                    context.TUASACH_THELOAI.RemoveRange(tuaSachToUpdate.TUASACH_THELOAI);

                    // Thêm quan hệ mới từ danh sách thể loại
                    var selectedCategories = ParseDSTheLoai(tbxDSTheLoai.Text);
                    foreach (var category in selectedCategories)
                    {
                        tuaSachToUpdate.TUASACH_THELOAI.Add(new TUASACH_THELOAI
                        {
                            IDTuaSach = tuaSachToUpdate.ID,
                            IDTheLoai = category.ID
                        });
                    }

                    // Lưu tất cả thay đổi
                    context.SaveChanges();

                    MessageBox.Show("Cập nhật tựa sách thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh lại DataGrid
                    LoadTuaSach();
                }
            }
        }

        private void dgTuaSach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = dgTuaSach.SelectedItem;
        
            if (selectedItem != null)
            {
                dynamic selectedBook = selectedItem;
                tbxMaTuaSach.Text = selectedBook.MaTuaSach;
                tbxTenTuaSach.Text = selectedBook.TenTuaSach;
                tbxDSTacGia.Text = selectedBook.DSTacGia;
                tbxDSTheLoai.Text = selectedBook.DSTheLoai;
                tbxSoLuong.Text = selectedBook.SoLuong.ToString();
                tbxHanMuonToiDa.Text = selectedBook.HanMuonToiDa.ToString() + " tuần";
            }
            else
            {
                tbxMaTuaSach.Text = "";
                tbxTenTuaSach.Text = "";
                tbxDSTacGia.Text = "";
                tbxDSTheLoai.Text = "";
                tbxSoLuong.Text = "";
                tbxHanMuonToiDa.Text = "";
            }
        }

        private void btnSuaTacGia_Click(object sender, RoutedEventArgs e)
        {
            // Chuyển DSTacGia trong TextBox thành List<TacGia>
            var currentAuthors = ParseDSTacGia(tbxDSTacGia.Text);

            // Lấy danh sách tất cả các tác giả từ cơ sở dữ liệu
            List<TACGIA> allAuthors;
            using (var context = new QLTVContext())
            {
                allAuthors = context.TACGIA
                                    .Where(tg => !tg.IsDeleted)
                                    .ToList();
            }

            // Mở cửa sổ WDChonTacGia
            var wdChonTacGia = new AWChonTacGia(allAuthors, currentAuthors);

            if (wdChonTacGia.ShowDialog() == true)
            {
                // Cập nhật DSTacGia từ danh sách tác giả mới
                var newSelectedAuthors = wdChonTacGia.SelectedAuthors;
                tbxDSTacGia.Text = string.Join(", ", newSelectedAuthors.Select(a => a.TenTacGia));
            }
        }

        private List<TACGIA> ParseDSTacGia(string DSTacGia)
        {
            if (string.IsNullOrWhiteSpace(DSTacGia)) return new List<TACGIA>();

            // Tách DSTacGia thành các tên tác giả dựa vào dấu phẩy
            var lstTenTacGia = DSTacGia.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(tg => tg.Trim())
                                       .ToList();

            // Lấy danh sách từ cơ sở dữ liệu khớp với tên
            using (var context = new QLTVContext())
            {
                return context.TACGIA.Where(tg => lstTenTacGia
                              .Contains(tg.TenTacGia)).ToList();
            }
        }

        private void btnSuaTheLoai_Click(object sender, RoutedEventArgs e)
        {
            // Chuyển DSTacGia trong TextBox thành List<TacGia>
            var currentCategories = ParseDSTheLoai(tbxDSTheLoai.Text);

            // Lấy danh sách tất cả các tác giả từ cơ sở dữ liệu
            List<THELOAI> allCategories;
            using (var context = new QLTVContext())
            {
                allCategories = context.THELOAI
                                       .Where(tl => !tl.IsDeleted)
                                       .ToList();
            }

            // Mở cửa sổ WDChonTacGia
            var awChonTheLoai= new AWChonTheLoai(allCategories, currentCategories);

            if (awChonTheLoai.ShowDialog() == true)
            {
                // Cập nhật DSTacGia từ danh sách tác giả mới
                var newSelectedCategories = awChonTheLoai.SelectedCategories;
                tbxDSTheLoai.Text = string.Join(", ", newSelectedCategories.Select(c => c.TenTheLoai));
            }
        }

        private List<THELOAI> ParseDSTheLoai(string DSTheLoai)
        {
            if (string.IsNullOrWhiteSpace(DSTheLoai)) return new List<THELOAI>();

            var lstTenTheLoai = DSTheLoai.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(tl => tl.Trim())
                                         .ToList();

            using (var context = new QLTVContext())
            {
                return context.THELOAI.Where(tl => lstTenTheLoai
                              .Contains(tl.TenTheLoai)).ToList();
            }
        }

        private void btnXoaTuaSach_Click(object sender, RoutedEventArgs e)
        {
            if (dgTuaSach.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn tựa sách cần xóa!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selectedItem = dgTuaSach.SelectedItem;
            string maTuaSach = selectedItem.MaTuaSach;

            MessageBoxResult mbrXacNhan = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa tựa sách có mã: {maTuaSach}?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (mbrXacNhan == MessageBoxResult.Yes)
            {
                using (var context = new QLTVContext())
                {
                    var tuaSachToDelete = context.TUASACH
                        .Include(ts => ts.TUASACH_TACGIA)
                        .Include(ts => ts.TUASACH_THELOAI)
                        .FirstOrDefault(tg => tg.MaTuaSach == maTuaSach);

                    // Truong hop bat dong bo?
                    if (tuaSachToDelete != null)
                    {
                        context.TUASACH_TACGIA.RemoveRange(tuaSachToDelete.TUASACH_TACGIA);
                        context.TUASACH_THELOAI.RemoveRange(tuaSachToDelete.TUASACH_THELOAI);
                        tuaSachToDelete.IsDeleted = true;
                        context.SaveChanges();
                        MessageBox.Show($"Tựa sách có mã {maTuaSach} đã được xóa.", "Thông báo",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTuaSach();
                    }
                }
            }
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            LoadTuaSach();
        }

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

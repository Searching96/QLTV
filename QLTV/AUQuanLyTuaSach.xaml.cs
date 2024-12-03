using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Globalization;
using MaterialDesignThemes.Wpf;
using System.Windows.Data;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using Azure.Core;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace QLTV
{
    /// <summary>
    /// Interaction logic for ADUCQuanLySach.xaml
    /// </summary>
    public partial class AUQuanLyTuaSach : UserControl
    {
        public static readonly Thickness DisplayElementMargin = new Thickness(0, 0, 0, 10);
        public static readonly Thickness ErrorIconMargin = new Thickness(0, 0, 5, 10);

        public AUQuanLyTuaSach()
        {
            InitializeComponent();
            LoadTuaSach();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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

        public bool HasError()
        {
            // Tìm tất cả các PackIcon trong UserControl
            foreach (var icon in FindVisualChildren<PackIcon>(this))
            {
                if (icon.Style == FindResource("ErrorIcon") && icon.Visibility == Visibility.Visible)
                {
                    return true;
                }
            }
            return false;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T t)
                    {
                        yield return t;
                    }
                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
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

            if (HasError())
            {
                MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi sửa!", "Thông báo",
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
                tbxHanMuonToiDa.Text = selectedBook.HanMuonToiDa.ToString();

                string maTuaSach = selectedBook.MaTuaSach;

                using (var context = new QLTVContext())
                {
                    var tuaSach = context.TUASACH
                        .Where(ts => ts.MaTuaSach == maTuaSach)
                        .FirstOrDefault();

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(tuaSach.BiaSach, UriKind.Absolute);
                    bitmap.EndInit();

                    imgBiaSach.Source = bitmap;
                    bdBiaSach.Visibility = Visibility.Visible;
                }
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
            var awChonTheLoai = new AWChonTheLoai(allCategories, currentCategories);

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
                        var lstSachToDelete = context.SACH
                            .Where(s => s.IDTuaSach == tuaSachToDelete.ID)
                            .ToList();

                        foreach (var sach in lstSachToDelete)
                            sach.IsDeleted = true;

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

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportDataGridToExcel();
        }

        private void ExportDataGridToExcel()
        {
            // Cấu hình đường dẫn lưu file Excel
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Lưu file Excel",
                FileName = "DanhSachTuaSach.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var filePath = saveFileDialog.FileName;

                // Tạo file Excel mới
                using (ExcelPackage package = new ExcelPackage())
                {
                    // Tạo một sheet mới
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Danh Sách Tựa Sách");

                    // Đặt tiêu đề cho các cột trong Excel
                    worksheet.Cells[1, 1].Value = "Mã Tựa Sách";
                    worksheet.Cells[1, 2].Value = "Tên Tựa Sách";
                    worksheet.Cells[1, 3].Value = "Tác Giả";
                    worksheet.Cells[1, 4].Value = "Thể Loại";
                    worksheet.Cells[1, 5].Value = "Số Lượng";
                    worksheet.Cells[1, 6].Value = "Hạn Mượn Tối Đa (Tuần)";

                    // Áp dụng style cho tiêu đề
                    using (var range = worksheet.Cells[1, 1, 1, 6])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Duyệt qua dữ liệu trong DataGrid và ghi vào Excel
                    var items = dgTuaSach.ItemsSource as System.Collections.IEnumerable;
                    int rowIndex = 2;

                    foreach (var item in items)
                    {
                        dynamic data = item;
                        worksheet.Cells[rowIndex, 1].Value = data.MaTuaSach;
                        worksheet.Cells[rowIndex, 2].Value = data.TenTuaSach;
                        worksheet.Cells[rowIndex, 3].Value = data.DSTacGia;
                        worksheet.Cells[rowIndex, 4].Value = data.DSTheLoai;
                        worksheet.Cells[rowIndex, 5].Value = data.SoLuong;
                        worksheet.Cells[rowIndex, 6].Value = data.HanMuonToiDa;
                        rowIndex++;
                    }

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Cells.AutoFitColumns();

                    // Lưu file Excel
                    FileInfo excelFile = new FileInfo(filePath);
                    package.SaveAs(excelFile);
                }

                MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ImportExcelToDb(string filePath)
        {
            var context = new QLTVContext();
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) return;

                var existingAuthors = context.TACGIA.ToDictionary(tg => tg.TenTacGia);
                var existingCategories = context.THELOAI.ToDictionary(tl => tl.TenTheLoai);

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    string tenTuaSach = worksheet.Cells[row, 1].Text;
                    string hanMuonToiDaText = worksheet.Cells[row, 4].Text;
                    if (string.IsNullOrWhiteSpace(tenTuaSach) || !int.TryParse(hanMuonToiDaText, out int hanMuonToiDa))
                        continue;  // Skip invalid rows

                    var newTuaSach = new TUASACH
                    {
                        TenTuaSach = tenTuaSach,
                        HanMuonToiDa = hanMuonToiDa
                    };
                    context.TUASACH.Add(newTuaSach);
                    context.SaveChanges();

                    var lstTenTacGia = worksheet.Cells[row, 2].Text.Split(", ").Select(n => n.Trim()).ToList();
                    var lstTenTheLoai = worksheet.Cells[row, 3].Text.Split(", ").Select(n => n.Trim()).ToList();

                    // Add authors
                    foreach (var tenTacGia in lstTenTacGia)
                    {
                        if (!existingAuthors.TryGetValue(tenTacGia, out var tacGia))
                        {
                            tacGia = new TACGIA { TenTacGia = tenTacGia, NamSinh = -1, QuocTich = "Chưa Có" };
                            context.TACGIA.Add(tacGia);
                            context.SaveChanges();
                            existingAuthors[tenTacGia] = tacGia;
                        }
                        context.TUASACH_TACGIA.Add(new TUASACH_TACGIA { IDTuaSach = newTuaSach.ID, IDTacGia = tacGia.ID });
                    }

                    // Add categories
                    foreach (var tenTheLoai in lstTenTheLoai)
                    {
                        if (!existingCategories.TryGetValue(tenTheLoai, out var theLoai))
                        {
                            theLoai = new THELOAI { TenTheLoai = tenTheLoai };
                            context.THELOAI.Add(theLoai);
                            context.SaveChanges();
                            existingCategories[tenTheLoai] = theLoai;
                        }
                        context.TUASACH_THELOAI.Add(new TUASACH_THELOAI { IDTuaSach = newTuaSach.ID, IDTheLoai = theLoai.ID });
                    }
                }

                // Save all changes at the end for efficiency
                context.SaveChanges();
            }
        }

        private void btnImportExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ImportExcelToDb(openFileDialog.FileName);
                MessageBox.Show("Nhập Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTuaSach();
            }
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
                // Truy vấn cơ sở dữ liệu để lấy tất cả các tựa sách
                var query = context.TUASACH
                    .Where(ts => !ts.IsDeleted)
                    .Select(ts => new
                    {
                        ts.MaTuaSach,
                        ts.TenTuaSach,
                        ts.SoLuong,
                        ts.HanMuonToiDa,
                        DSTacGia = string.Join(", ", ts.TUASACH_TACGIA.Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                        DSTheLoai = string.Join(", ", ts.TUASACH_THELOAI.Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai))
                    })
                    .AsEnumerable() // Chuyển về IEnumerable để lọc trên máy khách
                    .ToList();

                // Lọc theo thuộc tính tìm kiếm được chọn
                if (selectedProperty == "Tên Tựa Sách")
                {
                    query = query.Where(ts => NormalizeString(ts.TenTuaSach).Contains(NormalizeString(searchTerm))).ToList();
                }
                else if (selectedProperty == "Tác Giả")
                {
                    query = query.Where(ts => NormalizeString(ts.DSTacGia).Contains(NormalizeString(searchTerm))).ToList();
                }
                else if (selectedProperty == "Thể Loại")
                {
                    query = query.Where(ts => NormalizeString(ts.DSTheLoai).Contains(NormalizeString(searchTerm))).ToList();
                }

                // Cập nhật ItemsSource cho DataGrid
                dgTuaSach.ItemsSource = query;
            }
        }

        private void tbxTenTuaSach_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgTuaSach.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxTenTuaSach.Text))
            {
                icTenTuaSachError.ToolTip = "Tên Tựa Sách không được để trống!";
                icTenTuaSachError.Visibility = Visibility.Visible;
                return;
            }

            icTenTuaSachError.Visibility = Visibility.Collapsed;
        }

        private void tbxHanMuonToiDa_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgTuaSach.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxHanMuonToiDa.Text))
            {
                icHanMuonToiDaError.ToolTip = "Hạn Mượn Tối Đa không được để trống";
                icHanMuonToiDaError.Visibility = Visibility.Visible;
                return;
            }

            if (!int.TryParse(tbxHanMuonToiDa.Text, out int hmtd) || hmtd <= 0 || hmtd > 16)
            {
                icHanMuonToiDaError.ToolTip = "Hạn Mượn Tối Đa phải là số nguyên dương không quá 16";
                icHanMuonToiDaError.Visibility = Visibility.Visible;
                return;
            }

            icHanMuonToiDaError.Visibility = Visibility.Collapsed;
        }

        private void tbxDSTheLoai_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgTuaSach.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxDSTheLoai.Text))
            {
                icDSTheLoaiError.ToolTip = "Thể Loại không được để trống";
                icDSTheLoaiError.Visibility = Visibility.Visible;
                return;
            }

            icDSTheLoaiError.Visibility = Visibility.Collapsed;
        }

        private void tbxDSTacGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgTuaSach.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxDSTacGia.Text))
            {
                icDSTacGiaError.ToolTip = "Tác Giả không được để trống";
                icDSTacGiaError.Visibility = Visibility.Visible;
                return;
            }

            icDSTacGiaError.Visibility = Visibility.Collapsed;
        }

        private async void btnTimBiaSach_Click(object sender, RoutedEventArgs e)
        {
            dynamic tuaSach = dgTuaSach.SelectedItem;
            if (tuaSach == null)
            {
                MessageBox.Show("Hãy chọn tựa sách muốn sửa bìa.", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string maTuaSach = string.Empty;
            if (tuaSach != null) 
                maTuaSach = tuaSach.MaTuaSach;

            using (var context = new QLTVContext())
            {
                var TuaSach = context.TUASACH
                    .Where(ts => ts.MaTuaSach == maTuaSach)
                    .FirstOrDefault();

                AWSuaBiaSach awSuaBiaSach = new AWSuaBiaSach(TuaSach);
                if (awSuaBiaSach.ShowDialog() == true)
                {
                    LoadTuaSach();

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(awSuaBiaSach.TuaSach.BiaSach, UriKind.Absolute);
                    bitmap.EndInit();
                    imgBiaSach.Source = bitmap;
                }
            }
        }
    }
}

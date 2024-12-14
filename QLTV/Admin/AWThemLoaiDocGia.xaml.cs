using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QLTV.Models;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace QLTV.Admin
{
    public partial class AWThemLoaiDocGia : Window
    {
        private QLTVContext _context; 
        public AWThemLoaiDocGia(QLTVContext context) // Thêm tham số context
        {
            InitializeComponent();
            _context = context; // Gán context
        }

        private void tbxTenLoaiDocGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Kiểm tra tên loại độc giả không để trống
            if (string.IsNullOrWhiteSpace(tbxTenLoaiDocGia.Text))
            {
                icTenLoaiDocGiaError.ToolTip = "Tên Loại Độc Giả không được để trống";
                icTenLoaiDocGiaError.Visibility = Visibility.Visible;
                return;
            }
            icTenLoaiDocGiaError.Visibility = Visibility.Collapsed;

            if (!Regex.IsMatch(tbxTenLoaiDocGia.Text, @"^[a-zA-Z\p{L}\s]+$"))
            {
                icTenLoaiDocGiaError.ToolTip = "Tên loại độc giả chỉ được chứa chữ cái và khoảng trắng!";
                icTenLoaiDocGiaError.Visibility = Visibility.Visible;
            }
            else
            {
                icTenLoaiDocGiaError.Visibility = Visibility.Collapsed;
            }
        }

        private void tbxSoSachMuonToiDa_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Kiểm tra số sách mượn tối đa không để trống
            if (string.IsNullOrWhiteSpace(tbxSoSachMuonToiDa.Text))
            {
                icSoSachMuonToiDaError.ToolTip = "Số Sách Mượn Tối Đa không được để trống";
                icSoSachMuonToiDaError.Visibility = Visibility.Visible;
                return;
            }

            // Kiểm tra số sách mượn tối đa là số nguyên dương
            if (!int.TryParse(tbxSoSachMuonToiDa.Text, out int soSachMuonToiDa) || soSachMuonToiDa <= 0)
            {
                icSoSachMuonToiDaError.ToolTip = "Số Sách Mượn Tối Đa phải là số nguyên dương";
                icSoSachMuonToiDaError.Visibility = Visibility.Visible;
                return;
            }

            icSoSachMuonToiDaError.Visibility = Visibility.Collapsed;
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra các trường bắt buộc
                if (string.IsNullOrWhiteSpace(tbxTenLoaiDocGia.Text))
                {
                    icTenLoaiDocGiaError.ToolTip = "Tên Loại Độc Giả không được để trống";
                    icTenLoaiDocGiaError.Visibility = Visibility.Visible;
                }

                if (string.IsNullOrWhiteSpace(tbxSoSachMuonToiDa.Text))
                {
                    icSoSachMuonToiDaError.ToolTip = "Số Sách Mượn Tối Đa không được để trống";
                    icSoSachMuonToiDaError.Visibility = Visibility.Visible;
                }

                // Kiểm tra còn lỗi không
                if (HasError())
                {
                    MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi thêm!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Lấy thông tin từ các trường
                string tenLoaiDocGia = tbxTenLoaiDocGia.Text;
                int soSachMuonToiDa = int.Parse(tbxSoSachMuonToiDa.Text);

                // Create a new LOAIDOCGIA record
                var newLoaiDocGia = new LOAIDOCGIA()
                {
                    TenLoaiDocGia = tenLoaiDocGia,
                    SoSachMuonToiDa = soSachMuonToiDa,
                    IsDeleted = false // Đảm bảo IsDeleted được đặt là false
                };

                _context.LOAIDOCGIA.Add(newLoaiDocGia);
                _context.SaveChanges();

                MessageBox.Show("Thêm loại độc giả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm loại độc giả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm kiểm tra còn lỗi không
        public bool HasError()
        {
            // Tìm tất cả các PackIcon
            foreach (var icon in FindVisualChildren<PackIcon>(this))
            {
                if (icon.Style == FindResource("ErrorIcon") && icon.Visibility == Visibility.Visible)
                {
                    return true;
                }
            }
            return false;
        }

        // Hàm tìm kiếm các control con
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
    }
}
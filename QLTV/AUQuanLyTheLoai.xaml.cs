﻿using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
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

namespace QLTV
{
    /// <summary>
    /// Interaction logic for AUQuanLyTheLoai.xaml
    /// </summary>
    public partial class AUQuanLyTheLoai : UserControl
    {
        public AUQuanLyTheLoai()
        {
            InitializeComponent();
            LoadTheLoai();
        }

        private void LoadTheLoai()
        {
            using (var context = new QLTVContext())
            {
                var dsTacGia = context.THELOAI
                    .Where(tl => !tl.IsDeleted)
                    .Select(tl => new
                    {
                        tl.MaTheLoai,
                        tl.TenTheLoai,
                        tl.MoTa
                    })
                    .ToList();
                    
                dgTheLoai.ItemsSource = dsTacGia;
            }
        }

        private void btnThemTheLoai_Click(object sender, RoutedEventArgs e)
        {
            AWThemTheLoai awThemTheLoai = new AWThemTheLoai();
            if (awThemTheLoai.ShowDialog() == true)
                LoadTheLoai();
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

        private void btnSuaTheLoai_Click(object sender, RoutedEventArgs e)
        {
            if (dgTheLoai.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn thể loại cần sửa!", "Thông báo",
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
                dynamic selectedItem = dgTheLoai.SelectedItem;
                string maTheLoai = selectedItem.MaTheLoai;

                // Tìm tựa sách cần sửa
                var theLoaiToUpdate = context.THELOAI
                    .FirstOrDefault(tl => tl.MaTheLoai == maTheLoai);

                if (theLoaiToUpdate != null)
                {
                    // Cập nhật thông tin cơ bản
                    theLoaiToUpdate.TenTheLoai = tbxTenTheLoai.Text;
                    theLoaiToUpdate.MoTa = tbxMoTa.Text;

                    // Lưu tất cả thay đổi
                    context.SaveChanges();

                    MessageBox.Show("Cập nhật thể loại thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh lại DataGrid
                    LoadTheLoai();
                }
            }
        }

        private void dgTheLoai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = dgTheLoai.SelectedItem;

            if (selectedItem != null)
            {
                dynamic selectedCategory = selectedItem;
                tbxMaTheLoai.Text = selectedCategory.MaTheLoai;
                tbxTenTheLoai.Text = selectedCategory.TenTheLoai;
                tbxMoTa.Text = selectedCategory.MoTa;
            }
            else
            {
                tbxMaTheLoai.Text = "";
                tbxTenTheLoai.Text = "";
                tbxMoTa.Text = "";
            }
        }

        private void btnXoaTheLoai_Click(object sender, RoutedEventArgs e)
        {
            if (dgTheLoai.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn thể loại cần xóa!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selectedItem = dgTheLoai.SelectedItem;
            string maTheLoai = selectedItem.MaTheLoai;

            MessageBoxResult mbrXacNhan = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa thể loại có mã: {maTheLoai}?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (mbrXacNhan == MessageBoxResult.Yes)
            {
                using (var context = new QLTVContext())
                {
                    var theLoaiToDelete = context.THELOAI
                        .Include(tl => tl.TUASACH_THELOAI)
                        .FirstOrDefault(tl => tl.MaTheLoai == maTheLoai);

                    // Truong hop bat dong bo?
                    if (theLoaiToDelete != null)
                    {
                        context.TUASACH_THELOAI.RemoveRange(theLoaiToDelete.TUASACH_THELOAI);
                        theLoaiToDelete.IsDeleted = true;
                        context.SaveChanges();
                        MessageBox.Show($"Thể loại có mã {maTheLoai} đã được xóa.", "Thông báo",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTheLoai();
                    }
                }
            }
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            LoadTheLoai();
        }

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbxTenTheLoai_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgTheLoai.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxTenTheLoai.Text))
            {
                icTenTheLoai.ToolTip = "Tên Thể Loại không được để trống";
                icTenTheLoai.Visibility = Visibility.Visible;
                return;
            }

            if (tbxTenTheLoai.Text.Length > 100)
            {
                icTenTheLoai.ToolTip = "Tên Thể Loại không được quá 100 ký tự";
                icTenTheLoai.Visibility = Visibility.Visible;
                return;
            }

            foreach (char c in tbxTenTheLoai.Text)
            {
                if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
                {
                    icTenTheLoai.ToolTip = "Tên Thể Loại không được có số hay kí tự đặc biệt";
                    icTenTheLoai.Visibility = Visibility.Visible;
                    return;
                }
            }

            icTenTheLoai.Visibility = Visibility.Collapsed;
        }

        private void tbxMoTa_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgTheLoai.SelectedItem == null)
                return;

            if (tbxMoTa.Text.Length > 500)
            {
                icMoTa.ToolTip = "Mô Tả không được quá 500 ký tự";
                icMoTa.Visibility = Visibility.Visible;
                return;
            }

            icMoTa.Visibility = Visibility.Collapsed;
        }
    }
}

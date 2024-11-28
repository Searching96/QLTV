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

namespace QLTV_TranBin
{
    /// <summary>
    /// Interaction logic for UpdateTTAD.xaml
    /// </summary>
    public partial class UpdateTTAD : Window
    {
        private ADMIN _admin;
        public UpdateTTAD(ADMIN admin)
        {
            InitializeComponent();
            _admin = admin;
        }
        private bool IsMaximize = false;

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (IsMaximize)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1080;
                    this.Height = 720;

                    IsMaximize = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;

                    IsMaximize = true;
                }
            }
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Lấy thông tin từ các TextBox
            _admin.TenAdmin = txtTenAdmin.Text.Trim();
            _admin.GioiTinh = txtGioiTinh.Text.Trim();
            

            // Lưu thông tin vào cơ sở dữ liệu
            using (var context = new QLTVContext())
            {
                context.ADMIN.Update(_admin);
                context.SaveChanges();
            }

            MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close(); // Đóng cửa sổ sau khi lưu
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QLTV
{
    /// <summary>
    /// Interaction logic for AWThemSach.xaml
    /// </summary>
    public partial class AWThemSach : Window
    {
        private CollectionViewSource viewSource;

        public AWThemSach()
        {
            InitializeComponent();
            LoadTuaSach();
            LoadTinhTrang();
        }

        private string ConvertToUnsigned(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return new string(
                text.Normalize(NormalizationForm.FormD)
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    .ToArray()
            ).Normalize(NormalizationForm.FormC);
        }

        private void LoadTuaSach()
        {
            using (var context = new QLTVContext())
            {
                var dsTuaSach = context.TUASACH
                    .Where(ts => !ts.IsDeleted)
                    .Select(ts => ts.TenTuaSach)
                    .ToList();

                viewSource = new CollectionViewSource();
                viewSource.Source = dsTuaSach;
                cbbTuaSach.ItemsSource = viewSource.View;

                cbbTuaSach.Loaded += (s, e) =>
                {
                    var textBox = cbbTuaSach.Template.FindName("PART_EditableTextBox", cbbTuaSach) as TextBox;
                    if (textBox != null)
                    {
                        textBox.TextChanged += (sender, args) =>
                        {
                            var searchText = ConvertToUnsigned(textBox.Text);
                            viewSource.View.Filter = item =>
                            {
                                if (string.IsNullOrEmpty(searchText))
                                    return true;
                                var itemText = ConvertToUnsigned(item.ToString());
                                return itemText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                            };
                            cbbTuaSach.IsDropDownOpen = true;
                        };
                    }
                };
            }
        }

        private void LoadTinhTrang()
        {
            using (var context = new QLTVContext())
            {
                var dsTinhTrang = context.TINHTRANG
                    .Where(tt => !tt.IsDeleted)
                    .Select(tt => tt.TenTinhTrang)
                    .ToList();

                cbbTinhTrang.ItemsSource = dsTinhTrang;
            }
        }

        private async void btnThem_Click(object sender, RoutedEventArgs e)
        {
            string tuaSach = cbbTuaSach.SelectedItem?.ToString() ?? string.Empty;
            string nhaXuatBan = tbxNhaXuatBan.Text;
            int namXuatBan = int.Parse(tbxNamXuatBan.Text);
            DateTime ngayNhap = DateTime.ParseExact(
                dpNgayNhap.Text,
                "dd/MM/yyyy",  // Định dạng ngày dmy
                System.Globalization.CultureInfo.InvariantCulture
            );
            decimal triGia = decimal.Parse(tbxTriGia.Text);
            string tinhTrang = cbbTinhTrang.SelectedItem?.ToString() ?? string.Empty;
            int soLuong = int.Parse(tbxSoLuong.Text);

            using (var context = new QLTVContext())
            {
                // Get the ID of the selected TuaSach and TinhTrang
                int idTuaSach = context.TUASACH
                    .Where(ts => !ts.IsDeleted && ts.TenTuaSach == tuaSach)
                    .Select(ts => ts.ID)
                    .FirstOrDefault();

                int idTinhTrang = context.TINHTRANG
                    .Where(tt => !tt.IsDeleted && tt.TenTinhTrang == tinhTrang)
                    .Select(tt => tt.ID)
                    .FirstOrDefault();

                // Add the new SACH records
                for (int i = 0; i < soLuong; i++)
                {
                    var newSach = new SACH()
                    {
                        IDTuaSach = idTuaSach,
                        NhaXuatBan = nhaXuatBan,
                        NamXuatBan = namXuatBan,
                        NgayNhap = ngayNhap,
                        TriGia = triGia,
                        IDTinhTrang = idTinhTrang
                    };
                    context.SACH.Add(newSach);
                }

                // Save the changes to the SACH table
                await context.SaveChangesAsync();

                // After inserting, update the SoLuong in TUASACH
                context.TUASACH
                    .Where(ts => ts.ID == idTuaSach)
                    .ExecuteUpdate(ts => ts.SetProperty(t => t.SoLuong, t => t.SoLuong + soLuong));
            }

            this.DialogResult = true;
            this.Close();
        }

        private void dpNgayNhap_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}

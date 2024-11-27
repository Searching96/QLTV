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
using System.Windows.Shapes;

namespace QLTV
{
    /// <summary>
    /// Interaction logic for AWThemTuaSach.xaml
    /// </summary>
    public partial class AWThemTuaSach : Window
    {
        public AWThemTuaSach()
        {
            InitializeComponent();
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
            var wdChonTacGia = new ADWDChonTacGia(allAuthors, currentAuthors);

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

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            string tenTuaSach = tbxTenTuaSach.Text;
            int hanMuonToiDa = int.Parse(tbxHanMuonToiDa.Text);
            var lstTenTacGia = tbxDSTacGia.Text.Split(", ").Select(n => n.Trim()).ToList();
            var lstTenTheLoai = tbxDSTheLoai.Text.Split(", ").Select(n => n.Trim()).ToList();

            using (var context = new QLTVContext())
            {
                var newTuaSach = new TUASACH()
                {
                    TenTuaSach = tenTuaSach,
                    HanMuonToiDa = hanMuonToiDa
                };

                context.TUASACH.Add(newTuaSach);
                context.SaveChanges();

                // Thêm tác giả
                foreach (var tenTacGia in lstTenTacGia)
                {
                    var tacGia = context.TACGIA.FirstOrDefault(tg => tg.TenTacGia == tenTacGia);
                    if (tacGia != null)
                    {
                        // Liên kết tác giả với tựa sách
                        var newTSTG = new TUASACH_TACGIA()
                        {
                            IDTuaSach = newTuaSach.ID,  // Dùng ID của tựa sách vừa tạo
                            IDTacGia = tacGia.ID
                        };
                        context.TUASACH_TACGIA.Add(newTSTG);
                    }
                }

                // Thêm thể loại
                foreach (var tenTheLoai in lstTenTheLoai)
                {
                    var theLoai = context.THELOAI.FirstOrDefault(tl => tl.TenTheLoai == tenTheLoai);
                    if (theLoai != null)
                    {
                        // Liên kết thể loại với tựa sách
                        var newTSTL = new TUASACH_THELOAI()
                        {
                            IDTuaSach = newTuaSach.ID,  // Dùng ID của tựa sách vừa tạo
                            IDTheLoai = theLoai.ID
                        };
                        context.TUASACH_THELOAI.Add(newTSTL);
                    }
                }

                context.SaveChanges(); // Lưu tất cả các thay đổi
            }


            this.DialogResult = true;
            this.Close();
        }
    }
}

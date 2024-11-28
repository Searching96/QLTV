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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;

namespace QLTV_TranBin
{
    /// <summary>
    /// Interaction logic for UCTrangChuUser.xaml
    /// </summary>
    public partial class UCTrangChuUser : UserControl
    {
        public string TheLoaiRandom { get; set; }
        public UCTrangChuUser()
        {
            InitializeComponent();
            LoadAllGrids();
        }
        private void LoadAllGrids()
        {
            usedTheLoaiIds.Clear();  // Đảm bảo danh sách thể loại đã chọn được xóa khi bắt đầu random lại
            LoadDataForGrid(1);
            LoadDataForGrid(2);
            LoadDataForGrid(3);
            LoadDataForGrid(4);
        }

        private List<int> usedTheLoaiIds = new List<int>();

        private void LoadDataForGrid(int gridIndex)
        {
            using (var context = new QLTVContext())
            {
                var randomTheLoai = context.THELOAI
                    .Where(t => !t.IsDeleted && !usedTheLoaiIds.Contains(t.ID))
                    .OrderBy(r => Guid.NewGuid())
                    .FirstOrDefault();

                if (randomTheLoai != null)
                {
                    usedTheLoaiIds.Add(randomTheLoai.ID);

                    Button btnTheLoai;
                    ItemsControl itemsSach;

                    switch (gridIndex)
                    {
                        case 1:
                            btnTheLoai = btnTheLoai1;
                            itemsSach = ItemsSach1;
                            break;
                        case 2:
                            btnTheLoai = btnTheLoai2;
                            itemsSach = ItemsSach2;
                            break;
                        case 3:
                            btnTheLoai = btnTheLoai3;
                            itemsSach = ItemsSach3;
                            break;
                        case 4:
                            btnTheLoai = btnTheLoai4;
                            itemsSach = ItemsSach4;
                            break;
                        default:
                            throw new ArgumentException("Grid index không hợp lệ");
                    }

                    btnTheLoai.Content = randomTheLoai.TenTheLoai;

                    // Lấy danh sách SACH có đầy đủ thuộc tính, lọc theo thể loại và lấy ngẫu nhiên 4 sách
                    var sachList = context.SACH
                        .Include(s => s.IDTuaSachNavigation) // Tải trước liên kết TUASACH
                        .Where(s => s.IDTuaSachNavigation.IDTheLoai.Any(tl => tl.ID == randomTheLoai.ID))
                        .OrderBy(r => Guid.NewGuid())
                        .Take(4)
                        .ToList();

                    // Binding danh sách sách vào ItemsControl
                    itemsSach.ItemsSource = sachList;
                }

            }
        }

        private void btnTheLoai_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSach_Click(object sender, RoutedEventArgs e)
        {
            // Lấy thông tin sách từ Tag của button
            var button = sender as Button;
            var selectedBook = button?.Tag as SACH; // selectedBook sẽ là một đối tượng SACH

            if (selectedBook == null)
            {
                MessageBox.Show("Selected book is null!");
                return;
            }

            // Gửi thông tin sách lên Window để mở tab chi tiết
            var mainWindow = Application.Current.MainWindow as UserScreen;
            mainWindow?.OpenBookDetailTab(selectedBook);
        }
    }
}

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
    /// Interaction logic for AWChonTheLoai.xaml
    /// </summary>
    public partial class AWChonTheLoai : Window
    {
        public List<THELOAI> SelectedCategories { get; private set; }
        private List<THELOAI> generalSelectedCategories = new List<THELOAI>();
        private List<THELOAI> removedCategories = new List<THELOAI>();

        public AWChonTheLoai(List<THELOAI> allCategories, List<THELOAI> selectedCategories)
        {
            InitializeComponent();
            lbTheLoai.ItemsSource = allCategories;
            SelectedCategories = selectedCategories;
            // Lưu trữ tác giả ban đầu vào generalSelectedAuthors
            generalSelectedCategories = new List<THELOAI>(SelectedCategories);
            UpdateTheLoaiDaChonSan(allCategories, SelectedCategories);
        }

        private void lbTheLoai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Lưu danh sách tác giả đã chọn trước khi thay đổi
            var previousSelectedAuthors = new List<THELOAI>(SelectedCategories);

            // Cập nhật lại danh sách SelectedAuthors từ ListBox
            SelectedCategories = lbTheLoai.SelectedItems.Cast<THELOAI>().ToList();

            // Xác định các tác giả bị bỏ chọn (tức là có trong previousSelectedAuthors nhưng không có trong SelectedAuthors)
            var removedAuthorsThisTime = previousSelectedAuthors.Except(SelectedCategories).ToList();

            // Thêm các tác giả bị bỏ chọn vào removedAuthors
            removedCategories.AddRange(removedAuthorsThisTime);

            // Đồng bộ generalSelectedAuthors với SelectedAuthors sau mỗi lần thay đổi
            generalSelectedCategories = generalSelectedCategories.Concat(SelectedCategories).Distinct().ToList();
        }

        private void tbxTenTheLoaiTim_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Trước khi lọc, lưu lại các tác giả đã chọn vào generalSelectedAuthors và loại bỏ các tác giả đã bị bỏ chọn
            generalSelectedCategories = generalSelectedCategories.Concat(SelectedCategories).Distinct().ToList();
            generalSelectedCategories = generalSelectedCategories.Except(SelectedCategories).ToList();
            removedCategories.Clear(); // Xóa các tác giả đã bị bỏ chọn trước đó

            // Tiến hành lọc theo input
            string searchTerm = tbxTenTheLoaiTim.Text.Trim();
            using (var context = new QLTVContext())
            {
                var filteredCategories = context.THELOAI
                                                .Where(tl => !tl.IsDeleted && tl.TenTheLoai.Contains(searchTerm))
                                                .ToList();

                lbTheLoai.ItemsSource = filteredCategories;
                UpdateTheLoaiDaChonSan(filteredCategories, generalSelectedCategories);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCategories == null || SelectedCategories.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một tác giả!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true; // Đóng cửa sổ
        }

        private void tbxTenTheLoaiTim_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Lọc tác giả theo tên từ input
            string searchTerm = tbxTenTheLoaiTim.Text.Trim();
            using (var context = new QLTVContext())
            {
                var filteredCategories = context.THELOAI
                                                .Where(tl => !tl.IsDeleted && tl.TenTheLoai.Contains(searchTerm))
                                                .ToList();

                lbTheLoai.ItemsSource = filteredCategories;
                UpdateTheLoaiDaChonSan(filteredCategories, generalSelectedCategories);
            }
        }

        private void UpdateTheLoaiDaChonSan(List<THELOAI> allCategories, List<THELOAI> selectedCategories)
        {
            // Cập nhật lại danh sách các thể loại đã được chọn trong ListBox
            foreach (var selectedCategory in selectedCategories)
            {
                var categoryToSelect = allCategories.FirstOrDefault(c => c.MaTheLoai == selectedCategory.MaTheLoai);
                if (categoryToSelect != null)
                {
                    lbTheLoai.SelectedItems.Add(categoryToSelect);
                }
            }
        }
    }
}

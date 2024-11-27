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
    /// Interaction logic for ADWDChonTacGia.xaml
    /// </summary>
    public partial class ADWDChonTacGia : Window
    {
        public List<TACGIA> SelectedAuthors { get; private set; }
        private List<TACGIA> generalSelectedAuthors = new List<TACGIA>();
        private List<TACGIA> removedAuthors = new List<TACGIA>();

        public ADWDChonTacGia(List<TACGIA> allAuthors, List<TACGIA> selectedAuthors)
        {
            InitializeComponent();
            lbTacGia.ItemsSource = allAuthors;
            SelectedAuthors = selectedAuthors;
            // Lưu trữ tác giả ban đầu vào generalSelectedAuthors
            generalSelectedAuthors = new List<TACGIA>(SelectedAuthors);
            UpdateTacGiaDaChonSan(allAuthors, SelectedAuthors);
        }

        private void lbTacGia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Lưu danh sách tác giả đã chọn trước khi thay đổi
            var previousSelectedAuthors = new List<TACGIA>(SelectedAuthors);

            // Cập nhật lại danh sách SelectedAuthors từ ListBox
            SelectedAuthors = lbTacGia.SelectedItems.Cast<TACGIA>().ToList();

            // Xác định các tác giả bị bỏ chọn (tức là có trong previousSelectedAuthors nhưng không có trong SelectedAuthors)
            var removedAuthorsThisTime = previousSelectedAuthors.Except(SelectedAuthors).ToList();

            // Thêm các tác giả bị bỏ chọn vào removedAuthors
            removedAuthors.AddRange(removedAuthorsThisTime);

            // Đồng bộ generalSelectedAuthors với SelectedAuthors sau mỗi lần thay đổi
            generalSelectedAuthors = generalSelectedAuthors.Concat(SelectedAuthors).Distinct().ToList();
        }

        private void tbxTenTacGiaTim_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Trước khi lọc, lưu lại các tác giả đã chọn vào generalSelectedAuthors và loại bỏ các tác giả đã bị bỏ chọn
            generalSelectedAuthors = generalSelectedAuthors.Concat(SelectedAuthors).Distinct().ToList();
            generalSelectedAuthors = generalSelectedAuthors.Except(removedAuthors).ToList();
            removedAuthors.Clear(); // Xóa các tác giả đã bị bỏ chọn trước đó

            // Tiến hành lọc theo input
            string searchTerm = tbxTenTacGiaTim.Text.Trim();
            using (var context = new QLTVContext())
            {
                var filteredAuthors = context.TACGIA
                                             .Where(tg => !tg.IsDeleted && tg.TenTacGia.Contains(searchTerm))
                                             .ToList();

                lbTacGia.ItemsSource = filteredAuthors;
                UpdateTacGiaDaChonSan(filteredAuthors, generalSelectedAuthors);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedAuthors == null || SelectedAuthors.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một tác giả!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true; // Đóng cửa sổ
        }

        private void tbxTenTacGiaTim_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Lọc tác giả theo tên từ input
            string searchTerm = tbxTenTacGiaTim.Text.Trim();
            using (var context = new QLTVContext())
            {
                var filteredAuthors = context.TACGIA
                                             .Where(tg => !tg.IsDeleted && tg.TenTacGia.Contains(searchTerm))
                                             .ToList();

                lbTacGia.ItemsSource = filteredAuthors;
                UpdateTacGiaDaChonSan(filteredAuthors, generalSelectedAuthors);
            }
        }

        private void UpdateTacGiaDaChonSan(List<TACGIA> allAuthors, List<TACGIA> selectedAuthors)
        {
            // Cập nhật lại danh sách các tác giả đã được chọn trong ListBox
            foreach (var selectedAuthor in selectedAuthors)
            {
                var authorToSelect = allAuthors.FirstOrDefault(a => a.MaTacGia == selectedAuthor.MaTacGia);
                if (authorToSelect != null)
                {
                    lbTacGia.SelectedItems.Add(authorToSelect);
                }
            }
        }
    }
}

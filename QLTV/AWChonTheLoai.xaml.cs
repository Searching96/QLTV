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
        public List<THELOAI> AllCategories { get; set; }
        public List<THELOAI> SelectedCategories { get; set; }
        private List<THELOAI> DisplayedCategories { get; set; }
        private bool isUpdatingSelection = false;

        public AWChonTheLoai(List<THELOAI> allCategories, List<THELOAI> selectedCategories = null)
        {
            InitializeComponent();

            // Filter out deleted authors
            AllCategories = allCategories;
            SelectedCategories = selectedCategories ?? new List<THELOAI>();

            // Include both displayed and selected authors to maintain selection
            DisplayedCategories = AllCategories
                .Union(SelectedCategories, new THELOAIComparer())
                .Distinct(new THELOAIComparer())
                .ToList();

            lbTheLoai.ItemsSource = DisplayedCategories;

            isUpdatingSelection = true;
            foreach (var category in SelectedCategories)
            {
                var listBoxItem = DisplayedCategories
                    .FirstOrDefault(c => c.ID == category.ID);

                if (listBoxItem != null)
                {
                    lbTheLoai.SelectedItems.Add(listBoxItem);
                }
            }
            isUpdatingSelection = false;
        }

        private void tbxTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = tbxTimKiem.Text.ToLower();

            // Store currently selected authors
            var currentSelectedCategories = SelectedCategories.ToList();

            // Filter authors based on search text
            var filteredCategories = AllCategories
                .Where(c => c.TenTheLoai.ToLower().Contains(searchText.ToLower())
                    || c.MaTheLoai.ToLower().Contains(searchText.ToLower()))
                .ToList();

            // Combine filtered authors with currently selected authors
            DisplayedCategories = filteredCategories
                .Union(currentSelectedCategories, new THELOAIComparer())
                .Distinct(new THELOAIComparer())
                .ToList();

            isUpdatingSelection = true;

            // Update ItemsSource
            lbTheLoai.ItemsSource = DisplayedCategories;

            // Clear and reselect items
            lbTheLoai.SelectedItems.Clear();
            foreach (var category in currentSelectedCategories)
            {
                var matchingCategory = DisplayedCategories.FirstOrDefault(a => a.ID == category.ID);
                if (matchingCategory != null)
                {
                    lbTheLoai.SelectedItems.Add(matchingCategory);
                }
            }

            isUpdatingSelection = false;
        }

        private void lbTheLoai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Prevent recursive updates
            if (isUpdatingSelection) return;

            // Update SelectedAuthors list when selection changes
            SelectedCategories = lbTheLoai.SelectedItems.Cast<THELOAI>().ToList();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        // Custom comparer to ensure unique authors based on ID
        private class THELOAIComparer : IEqualityComparer<THELOAI>
        {
            public bool Equals(THELOAI x, THELOAI y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return x.ID == y.ID;
            }

            public int GetHashCode(THELOAI obj)
            {
                return obj.ID.GetHashCode();
            }
        }

        private void btnThemTheLoai_Click(object sender, RoutedEventArgs e)
        {
            AWThemTheLoai awThemTheLoai = new AWThemTheLoai();
            if (awThemTheLoai.ShowDialog() == true)
                LoadTheLoai();
        }

        private void btnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

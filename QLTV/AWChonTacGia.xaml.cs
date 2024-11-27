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
    /// Interaction logic for AWChonTacGia.xaml
    /// </summary>
    public partial class AWChonTacGia : Window
    {
        public List<TACGIA> AllAuthors { get; set; }
        public List<TACGIA> SelectedAuthors { get; set; }
        private List<TACGIA> DisplayedAuthors { get; set; }
        private bool isUpdatingSelection = false;

        public AWChonTacGia(List<TACGIA> allAuthors, List<TACGIA> selectedAuthors = null)
        {
            InitializeComponent();

            // Filter out deleted authors
            AllAuthors = allAuthors;
            SelectedAuthors = selectedAuthors ?? new List<TACGIA>();

            // Include both displayed and selected authors to maintain selection
            DisplayedAuthors = AllAuthors
                .Union(SelectedAuthors, new TACGIAComparer())
                .Distinct(new TACGIAComparer())
                .ToList();

            lbTacGia.ItemsSource = DisplayedAuthors;

            isUpdatingSelection = true;
            foreach (var author in SelectedAuthors)
            {
                var listBoxItem = DisplayedAuthors
                    .FirstOrDefault(a => a.ID == author.ID);

                if (listBoxItem != null)
                {
                    lbTacGia.SelectedItems.Add(listBoxItem);
                }
            }
            isUpdatingSelection = false;
        }

        private void tbxTimTen_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = tbxTimTen.Text.ToLower();

            // Store currently selected authors
            var currentSelectedAuthors = SelectedAuthors.ToList();

            // Filter authors based on search text
            var filteredAuthors = AllAuthors
                .Where(a => a.TenTacGia.ToLower().Contains(searchText.ToLower()) 
                    || a.MaTacGia.ToLower().Contains(searchText.ToLower()))
                .ToList();

            // Combine filtered authors with currently selected authors
            DisplayedAuthors = filteredAuthors
                .Union(currentSelectedAuthors, new TACGIAComparer())
                .Distinct(new TACGIAComparer())
                .ToList();

            isUpdatingSelection = true;

            // Update ItemsSource
            lbTacGia.ItemsSource = DisplayedAuthors;

            // Clear and reselect items
            lbTacGia.SelectedItems.Clear();
            foreach (var author in currentSelectedAuthors)
            {
                var matchingAuthor = DisplayedAuthors.FirstOrDefault(a => a.ID == author.ID);
                if (matchingAuthor != null)
                {
                    lbTacGia.SelectedItems.Add(matchingAuthor);
                }
            }

            isUpdatingSelection = false;
        }

        private void lbTacGia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Prevent recursive updates
            if (isUpdatingSelection) return;

            // Update SelectedAuthors list when selection changes
            SelectedAuthors = lbTacGia.SelectedItems.Cast<TACGIA>().ToList();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        // Custom comparer to ensure unique authors based on ID
        private class TACGIAComparer : IEqualityComparer<TACGIA>
        {
            public bool Equals(TACGIA x, TACGIA y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return x.ID == y.ID;
            }

            public int GetHashCode(TACGIA obj)
            {
                return obj.ID.GetHashCode();
            }
        }
    }
}

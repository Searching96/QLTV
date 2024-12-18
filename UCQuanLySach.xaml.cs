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

namespace UI_Chung
{
    /// <summary>
    /// Interaction logic for UCQuanLySach.xaml
    /// </summary>
    public partial class UCQuanLySach : UserControl
    {
        public UCQuanLySach()
        {
            InitializeComponent();
        }
        private void btnQuanLySach_Click(object sender, RoutedEventArgs e)
        {
            OpenTab("Quản lí sách", new UCQuanLyThongTinSach());
        }

        // Hàm mở tab "Quản lí tác giả"
        private void btnQuanLyTacGia_Click(object sender, RoutedEventArgs e)
        {
            OpenTab("Quản lí tác giả", new UCQuanLyTacGia());
        }

        // Hàm mở tab "Quản lí thể loại"
        private void btnQuanLyTheLoai_Click(object sender, RoutedEventArgs e)
        {
            OpenTab("Quản lí thể loại", new UCQuanLyTheLoai());
        }

        // Hàm để mở tab với tiêu đề và nội dung cho trước
        private void OpenTab(string header, UserControl content)
        {
            // Kiểm tra nếu tab đã tồn tại
            foreach (TabItem item in MainTabControl.Items)
            {
                // Lấy tiêu đề của tab dưới dạng TextBlock
                if (item.Header is StackPanel stackPanel && stackPanel.Children[0] is TextBlock textBlock && textBlock.Text == header)
                {
                    MainTabControl.SelectedItem = item;
                    return;
                }
            }

            // Tạo TabItem mới
            var tabItem = new TabItem
            {
                Content = content
            };

            // Tạo StackPanel để chứa tiêu đề và nút đóng
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            var headerText = new TextBlock { Text = header };
            var closeButton = new Button { Content = "X", Width = 20, Height = 20, Margin = new Thickness(5, 0, 0, 0) };
            closeButton.Click += (s, e) => MainTabControl.Items.Remove(tabItem);

            // Thêm TextBlock và nút đóng vào StackPanel
            headerPanel.Children.Add(headerText);
            headerPanel.Children.Add(closeButton);

            // Gán StackPanel làm tiêu đề của TabItem
            tabItem.Header = headerPanel;

            // Thêm TabItem vào TabControl
            MainTabControl.Items.Add(tabItem);
            MainTabControl.SelectedItem = tabItem;
        }

        
    }
}

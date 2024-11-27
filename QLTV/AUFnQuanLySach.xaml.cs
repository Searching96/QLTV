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
    /// Interaction logic for AUFnQuanLySach.xaml
    /// </summary>
    public partial class AUFnQuanLySach : UserControl
    {
        public AUFnQuanLySach()
        {
            InitializeComponent();
        }

        private void btnQuanLyTuaSach_Click(object sender, RoutedEventArgs e)
        {
            OpenTab("Quản lý tựa sách", new AUQuanLyTuaSach());
        }

        private void btnQuanLyTacGia_Click(object sender, RoutedEventArgs e)
        {
            OpenTab("Quản lý tác giả", new AUQuanLyTacGia());
        }

        private void btnQuanLyTheLoai_Click(object sender, RoutedEventArgs e)
        {
            OpenTab("Quản lý thể loại", new AUQuanLyTheLoai());
        }

        private void OpenTab(string header, UserControl content)
        {
            // Nếu tab đã tồn tài thì di chuyển tới tab đó
            foreach (TabItem item in tcFnQuanLySach.Items)
            {
                if (item.Header is StackPanel sp && sp.Children[0] is TextBlock tbl && tbl.Text == header)
                {
                    tcFnQuanLySach.SelectedItem = item;
                    return;
                }
            }

            // Tạo tab mới
            var tabItem = new TabItem { Content = content };

            // Stackpanel (template) chứa tiêu đề và nút đóng
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            var headerText = new TextBlock { Text = header, VerticalAlignment = VerticalAlignment.Center };
            var closeButton = new Button { Content = "x", Margin = new Thickness(2) };
            closeButton.Click += (s, e) => tcFnQuanLySach.Items.Remove(tabItem);

            // Thêm header và close button vào stack panel
            headerPanel.Children.Add(headerText); 
            headerPanel.Children.Add(closeButton);

            // Gán headerPanel cho header (cục chọn tab) của 1 tab
            tabItem.Header = headerPanel;

            // Thêm tab vào tab control
            tcFnQuanLySach.Items.Add(tabItem);
            tcFnQuanLySach.SelectedItem = tabItem;
        }
    }
}

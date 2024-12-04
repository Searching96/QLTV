using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
    /// Interaction logic for AWSuaBiaSach.xaml
    /// </summary>
    public partial class AWSuaBiaSach : Window
    {
        public TUASACH TuaSach { get; set; }
        public bool IsValidImage = false;

        public AWSuaBiaSach(TUASACH tuaSach)
        {
            InitializeComponent();
            TuaSach = tuaSach;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(TuaSach.BiaSach, UriKind.Absolute);
            bitmap.EndInit();

            imgBiaSach.Source = bitmap;
        }

        public async Task<string> GetBookCoverUrlAsync(string tuaSach)
        {
            string apiKey = "uapik";     
            string url = $"https://www.googleapis.com/books/v1/volumes?q=intitle:{Uri.EscapeDataString(tuaSach)}&key={apiKey}";

            using HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(url);
            using JsonDocument document = JsonDocument.Parse(response);
            var root = document.RootElement;

            if (root.TryGetProperty("items", out JsonElement items) && items.GetArrayLength() > 0)
            {
                var volumeInfo = items[0].GetProperty("volumeInfo");
                if (volumeInfo.TryGetProperty("imageLinks", out JsonElement imageLinks) &&
                    imageLinks.TryGetProperty("thumbnail", out JsonElement thumbnail))
                {
                    return thumbnail.GetString() ?? "No cover available";
                }
            }

            return "No cover available";
        }

        private async Task<bool> IsImageUrlValid(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Gửi yêu cầu GET đến URL
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Kiểm tra xem mã trạng thái HTTP có phải là 200 (OK)
                    return response.IsSuccessStatusCode && response.Content.Headers.ContentType.MediaType.StartsWith("image");
                }
            }
            catch
            {
                // Nếu xảy ra lỗi trong quá trình yêu cầu (ví dụ: không thể kết nối), trả về false
                return false;
            }
        }

        private async void btnKiemTra_Click(object sender, RoutedEventArgs e)
        {
            string imageUrl = tbxLinkBia.Text; // Lấy liên kết từ TextBox
            if (await IsImageUrlValid(imageUrl))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageUrl, UriKind.Absolute);
                bitmap.EndInit();

                imgBiaSach.Source = bitmap;
                IsValidImage = true;
            }
            else
            {
                MessageBox.Show("Ảnh không tồn tại hoặc URL không hợp lệ.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnTimAPI_Click(object sender, RoutedEventArgs e)
        {
            string biaSach = await GetBookCoverUrlAsync(TuaSach.TenTuaSach);
            if (biaSach == "No cover available")
            {
                MessageBox.Show("Không tìm được bìa sách qua API!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            tbxLinkBia.Text = biaSach;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValidImage)
            {
                MessageBox.Show("Link ảnh không hợp lệ", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            using (var context = new QLTVContext())
            {
                var tuaSach = context.TUASACH
                    .Where(ts => ts.MaTuaSach == TuaSach.MaTuaSach)
                    .FirstOrDefault();

                tuaSach.BiaSach = tbxLinkBia.Text;
                context.SaveChanges();
            }

            TuaSach.BiaSach = tbxLinkBia.Text;
            this.DialogResult = true;
            this.Close();
        }
    }
}

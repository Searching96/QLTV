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
using RestSharp;
using Newtonsoft.Json;
using QLTV.ViewModels;
using System.IO;
using iTextSharp.text.pdf.codec.wmf;
using Org.BouncyCastle.Asn1.Crmf;
namespace QLTV.User
{
    /// <summary>
    /// Interaction logic for UWThanhToanNo.xaml
    /// </summary>
    public partial class UWThanhToanNo : Window
    {
        public UWThanhToanNo(int debt)
        {
            InitializeComponent();
            LoadQR(debt);
        }
        public void LoadQR(int tongno)
        {
            var apiRequest = new ApiRequest();
            apiRequest.acqId = 970418;
            apiRequest.accountNo = 5622889955;
            apiRequest.accountName = "THƯ VIỆN LIMAN";
            apiRequest.amount = tongno;
            apiRequest.format = "text";
            apiRequest.template = "print";
            var jsonRequest = JsonConvert.SerializeObject(apiRequest);
            // use restsharp for request api.
            var client = new RestClient("https://api.vietqr.io/v2/generate");
            var request = new RestRequest();
            request.Method = Method.Post;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", jsonRequest, ParameterType.RequestBody);
            var response = client.Execute(request);
            var content = response.Content;
            var dataResult = JsonConvert.DeserializeObject<ApiResponse>(content);
            string base64Image = dataResult.data.qrDataURL.Replace("data:image/png;base64,", "");
            // Chuyển đổi và gán vào ImageBrush
            var qrImage = Base64ToBitmapImage(base64Image);
            // Gán hình ảnh QR vào ImageBrush trong XAML
            Dispatcher.Invoke(() =>
            {
                var imageBrush = new ImageBrush();
                imageBrush.ImageSource = qrImage;
                // Tìm Rectangle theo tên và gán ImageBrush
                QRBox.Fill = imageBrush;
            });
        }
        public BitmapImage Base64ToBitmapImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            bitmap.Freeze(); // Đảm bảo hình ảnh có thể sử dụng trên UI thread
            return bitmap;
        }
    }
}
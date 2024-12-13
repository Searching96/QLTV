using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLTV.ViewModels
{
    public class ThongBao
    {
        public string Title { get; set; } // Tiêu đề thông báo
        public string Message { get; set; } // Nội dung thông báo
        public string Icon { get; set; } // Đường dẫn hoặc kiểu biểu tượng

        public ThongBao(string title, string message, string icon = null)
        {
            Title = title;
            Message = message;
            Icon = icon ?? "default_icon"; // Đặt biểu tượng mặc định nếu không có
        }
    }
}

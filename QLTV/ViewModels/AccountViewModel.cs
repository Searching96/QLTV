using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
namespace QLTV.GridViewModels
{
    public class AccountViewModel : INotifyPropertyChanged
    {
        public int ID { get; set; }
        public string? MaTaiKhoan { get; set; }
        public string? TenNguoiDung {  get; set; }
        public string? GioiTinh {  get; set; }   
        public string TenTaiKhoan { get; set; } = null!;
        public string? DiaChi { get; set; }
        public string? Avatar {  get; set; }
        public string Email { get; set; } = null!;
        public string SDT { get; set; } = null!;
        public DateTime NgaySinh { get; set; }
        public DateTime NgayDangKy { get; set; }
        public DateTime NgayHetHan { get; set; }
        public int IDPhanQuyen { get; set; }
        public string? LoaiTaiKhoan { get; set; }
        public string BgColor { get; set; } = null!;
        public string Character => string.IsNullOrEmpty(TenTaiKhoan) ? "" : TenTaiKhoan[0].ToString().ToUpper();

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

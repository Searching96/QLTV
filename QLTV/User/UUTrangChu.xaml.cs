using Microsoft.EntityFrameworkCore;
using QLTV.Models;
using QLTV.Properties;
using QLTV.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace QLTV.User
{
    /// <summary>
    /// Interaction logic for UTrangChu.xaml
    /// </summary>
    public partial class UTrangChu : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<THELOAI> _theLoaiList;

        public ObservableCollection<THELOAI> TheLoaiList

        {
            get => _theLoaiList;
            set
            {
                _theLoaiList = value;
                OnPropertyChanged(nameof(TheLoaiList));
            }
        }

        private Dictionary<int, ObservableCollection<TUASACH>> _tuaSachByTheLoai;

        public Dictionary<int, ObservableCollection<TUASACH>> TuaSachByTheLoai
        {
            get => _tuaSachByTheLoai;
            set
            {
                _tuaSachByTheLoai = value;
                OnPropertyChanged(nameof(TuaSachByTheLoai));
            }
        }

        private void LoadTuaSachByTheLoai()
        {
            using (var context = new QLTVContext())
            {
                TuaSachByTheLoai = new Dictionary<int, ObservableCollection<TUASACH>>();

                foreach (var theLoai in TheLoaiList)
                {
                    var tuaSachs = context.TUASACH
                    .Where(t => t.TUASACH_THELOAI
                        .Any(ttl => ttl.IDTheLoai == theLoai.ID) && !t.IsDeleted) // Lọc theo thể loại từ bảng TUASACH_THELOAI
                    .OrderBy(t => Guid.NewGuid()) // Lấy tựa sách ngẫu nhiên
                    .Take(4) // Lấy 4 tựa sách ngẫu nhiên
                    .ToList();
                    //foreach (var tuasach in tuaSachs)
                    //    MessageBox.Show(tuasach.TenTuaSach);
                    TuaSachByTheLoai[theLoai.ID] = new ObservableCollection<TUASACH>(tuaSachs);

                }
            }
        }
        public ObservableCollection<TUASACH> TuaSachList { get; set; }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl.SelectedItem is THELOAI selectedTheLoai && TuaSachByTheLoai.ContainsKey(selectedTheLoai.ID))
            {

                TuaSachList = TuaSachByTheLoai[selectedTheLoai.ID];
                OnPropertyChanged(nameof(TuaSachList));
            }

        }
        public ObservableCollection<TUASACH> TuaSachMuonNhieuList { get; set; }

        private void LoadTheLoai()
        {
            using (var context = new QLTVContext())
            {
                // Lấy 4 thể loại ngẫu nhiên
                var randomTheLoai = context.THELOAI
                    .Where(t => !t.IsDeleted)
                    .OrderBy(t => Guid.NewGuid())
                    .Take(4)
                    .ToList();

                TheLoaiList = new ObservableCollection<THELOAI>(randomTheLoai);
            }
        }




        private ObservableCollection<TUASACH> _sachMuonNhieuTuan;
        private ObservableCollection<TUASACH> _sachMuonNhieuThang;
        private ObservableCollection<TUASACH> _sachMuonNhieuNam;

        public ObservableCollection<TUASACH> SachMuonNhieuTuan
        {
            get => _sachMuonNhieuTuan;
            set
            {
                _sachMuonNhieuTuan = value;
                OnPropertyChanged(nameof(SachMuonNhieuTuan));
            }
        }

        public ObservableCollection<TUASACH> SachMuonNhieuThang
        {
            get => _sachMuonNhieuThang;
            set
            {
                _sachMuonNhieuThang = value;
                OnPropertyChanged(nameof(SachMuonNhieuThang));
            }
        }
        public ObservableCollection<TUASACH> SachMuonNhieuNam
        {
            get => _sachMuonNhieuNam;
            set
            {
                _sachMuonNhieuNam = value;
                OnPropertyChanged(nameof(SachMuonNhieuNam));
            }
        }
        private ObservableCollection<PHIEUMUON> _danhSachPhieuMuon;
        public ObservableCollection<PHIEUMUON> DanhSachPhieuMuon
        {
            get => _danhSachPhieuMuon;
            set
            {
                _danhSachPhieuMuon = value;
                OnPropertyChanged(nameof(DanhSachPhieuMuon));
            }
        }
        public ObservableCollection<PHIEUMUON> GetAllPhieuMuon()
        {
            using (var context = new QLTVContext()) // Khởi tạo DbContext
            {
                // Lấy tất cả các phiếu mượn từ bảng PHIEUMUON
                var phieuMuons = context.PHIEUMUON
                                        .Where(pm => !pm.IsDeleted) // Lọc các phiếu mượn chưa bị xóa
                                        .OrderBy(pm => pm.NgayMuon) // Sắp xếp theo ngày mượn
                                        .Include(pm => pm.CTPHIEUMUON) // Bao gồm các chi tiết phiếu mượn nếu cần
                                        .ThenInclude(ct => ct.IDSachNavigation) // Bao gồm thông tin sách nếu cần
                                        .ToList(); // Thực thi truy vấn và lấy kết quả

                // Chuyển đổi danh sách phiếu mượn thành ObservableCollection để binding trong UI
                return new ObservableCollection<PHIEUMUON>(phieuMuons);
            }
        }
        public void LoadSachPhoBien()
        {
            using (var context = new QLTVContext())
            {
                DanhSachPhieuMuon = GetAllPhieuMuon();
                SachMuonNhieuTuan = ThongKeSach.GetSachMuonNhieuTheoThoiGian(context.PHIEUMUON, "tuan"); // Truyền DbSet vào phương thức

                SachMuonNhieuThang = ThongKeSach.GetSachMuonNhieuTheoThoiGian(context.PHIEUMUON, "thang");

                SachMuonNhieuNam = ThongKeSach.GetSachMuonNhieuTheoThoiGian(context.PHIEUMUON, "nam");

            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public UTrangChu()
        {
            InitializeComponent();

            LoadTheLoai();
            LoadTuaSachByTheLoai(); // Thêm phương thức tải tựa sách theo thể loại
            LoadSachPhoBien();
            tcSachMuonNhieu.DataContext = this;
            DataContext = this; // Gán DataContext chính là UserControl
        }
        private void btnSach_Click(object sender, RoutedEventArgs e)
        {
            // Lấy thông tin sách từ Tag của button
            var button = sender as Button;
            var selectedBook = button?.Tag as TUASACH; // selectedBook sẽ là một đối tượng SACH

            if (selectedBook == null)
            {
                MessageBox.Show("Selected book is null!");
                return;
            }
            // else MessageBox.Show("Co Sach");
            //// Gửi thông tin sách lên Window để mở tab chi tiết
            //var mainWindow = Application.Current.MainWindow as WUserWindow;
            //mainWindow?.OpenBookDetailTab(selectedBook);
        }
        private TUASACH _selectedTuaSach;
        public TUASACH SelectedTuaSach
        {
            get => _selectedTuaSach;
            set
            {
                _selectedTuaSach = value;
                OnPropertyChanged(nameof(SelectedTuaSach));
            }
        }
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button && button.Tag is TUASACH tuaSach)
            {
                SelectedTuaSach = tuaSach; // Cập nhật trực tiếp vào property SelectedTuaSach
            }
        }



    }
}
using Microsoft.EntityFrameworkCore;
using QLTV.Models;
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
        private ObservableCollection<THELOAI> _theLoaiMuonNhieuList;

        public ObservableCollection<THELOAI> TheLoaiMuonNhieuList

        {
            get => _theLoaiMuonNhieuList;
            set
            {
                _theLoaiMuonNhieuList = value;
                OnPropertyChanged(nameof(TheLoaiMuonNhieuList));
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
        private Dictionary<int, ObservableCollection<TUASACH>> _tuaSachByTheLoaiMuonNhieu;

        public Dictionary<int, ObservableCollection<TUASACH>> TuaSachByTheLoaiMuonNhieu
        {
            get => _tuaSachByTheLoaiMuonNhieu;
            set
            {
                _tuaSachByTheLoaiMuonNhieu = value;
                OnPropertyChanged(nameof(TuaSachByTheLoaiMuonNhieu));
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
        private void tcMuonNhieu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as TabControl)?.SelectedItem is THELOAI SelectedTheLoaiMuonNhieu)
            {

                if (TuaSachByTheLoaiMuonNhieu.ContainsKey(SelectedTheLoaiMuonNhieu.ID))
                {

                    TuaSachMuonNhieuList = TuaSachByTheLoaiMuonNhieu[SelectedTheLoaiMuonNhieu.ID];
                    OnPropertyChanged(nameof(TuaSachMuonNhieuList));  // Quan trọng để cập nhật giao diện
                }
            }

        }
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
        private void LoadTheLoaiMuonNhieu()
        {
            try
            {
                using (var context = new QLTVContext())
                {

                    // Truy vấn lấy danh sách thể loại được mượn nhiều nhất
                    var topTheLoai = context.CTPHIEUMUON
                        .Include(ctpm => ctpm.IDSachNavigation)
                        .ThenInclude(sach => sach.IDTuaSachNavigation)
                        .ThenInclude(tuaSach => tuaSach.TUASACH_THELOAI) // Bao gồm bảng TUASACH_THELOAI
                        .ThenInclude(tuasachTheLoai => tuasachTheLoai.IDTheLoaiNavigation) // Bao gồm thông tin Thể loại
                        .ToList() // Lấy toàn bộ dữ liệu ra trước
                        .SelectMany(ctpm => ctpm.IDSachNavigation.IDTuaSachNavigation.TUASACH_THELOAI) // Lấy tất cả các thể loại của tựa sách
                        .Select(tuasachTheLoai => tuasachTheLoai.IDTheLoaiNavigation) // Chỉ lấy thông tin thể loại
                        .GroupBy(theloai => theloai.ID) // Nhóm theo ID thể loại
                        .Select(g => new
                        {
                            TheLoai = g.FirstOrDefault(),
                            SoLuotMuon = g.Count()
                        })
                        .OrderByDescending(x => x.SoLuotMuon)
                        .Take(5)
                        .Select(x => x.TheLoai)
                        .ToList(); // Chuyển kết quả cuối cùng thành danh sách
                                   // Gán danh sách vào ObservableCollection để hiển thị trên giao diện
                    TheLoaiMuonNhieuList = new ObservableCollection<THELOAI>(topTheLoai);

                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có
                MessageBox.Show($"Lỗi khi tải thể loại mượn nhiều: {ex.Message}");
            }
        }
        private void LoadTuaSachByTheLoaiMuonNhieu()
        {
            using (var context = new QLTVContext())
            {
                TuaSachByTheLoaiMuonNhieu = new Dictionary<int, ObservableCollection<TUASACH>>();

                foreach (var theLoai in TheLoaiMuonNhieuList)
                {

                    // Truy vấn 5 cuốn sách mượn nhiều nhất trong thể loại
                    var tuaSachs = context.CTPHIEUMUON
                        .Include(ctpm => ctpm.IDSachNavigation) // Bao gồm thông tin Sách
                        .ThenInclude(sach => sach.IDTuaSachNavigation) // Bao gồm thông tin Tựa sách
                        .ThenInclude(tuasach => tuasach.TUASACH_THELOAI)
                        .ThenInclude(tuasachTheLoai => tuasachTheLoai.IDTheLoaiNavigation) // Bao gồm thông tin Thể loại
                        .Where(ctpm => ctpm.IDSachNavigation.IDTuaSachNavigation.TUASACH_THELOAI
                        .Any(tl => tl.IDTheLoai == theLoai.ID)) // Lọc theo thể loại
                        .GroupBy(ctpm => ctpm.IDSachNavigation.IDTuaSachNavigation) // Nhóm theo Tựa sách
                        .Select(g => new
                        {
                            TuaSach = g.Key,
                            SoLuotMuon = g.Count() // Đếm số lần mượn
                        })
                        .OrderByDescending(x => x.SoLuotMuon) // Sắp xếp giảm dần theo số lượt mượn
                        .Take(5) // Lấy 5 tựa sách mượn nhiều nhất
                        .Select(x => x.TuaSach) // Chỉ lấy đối tượng Tựa sách
                        .ToList();
                    // Gán danh sách tựa sách vào Dictionary
                    TuaSachByTheLoaiMuonNhieu[theLoai.ID] = new ObservableCollection<TUASACH>(tuaSachs);
                }
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
            LoadTheLoaiMuonNhieu();
            LoadTuaSachByTheLoaiMuonNhieu();
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
            else MessageBox.Show("Co Sach");
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
using QLTV_TranBin.Models;
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

namespace QLTV_TranBin
{
    /// <summary>
    /// Interaction logic for UTrangChu.xaml
    /// </summary>
    public partial class UTrangChu : INotifyPropertyChanged
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
            using (var context = new QLTV2Context())
            {
                TuaSachByTheLoai = new Dictionary<int, ObservableCollection<TUASACH>>();

                foreach (var theLoai in TheLoaiList)
                {
                    var tuaSachs = context.TUASACH
                        .Where(t => t.IDTheLoai.Any(tl => tl.ID == theLoai.ID) && !t.IsDeleted) // Lọc theo thể loại
                        .OrderBy(t => Guid.NewGuid())
                        .Take(4) // Lấy 4 tựa sách ngẫu nhiên
                        .ToList();
                    foreach (var tuasach in tuaSachs)
                        MessageBox.Show(tuasach.TenTuaSach);
                    TuaSachByTheLoai[theLoai.ID] = new ObservableCollection<TUASACH>(tuaSachs);
                }
            }
        }
        public ObservableCollection<TUASACH> TuaSachList { get; set; }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl.SelectedItem is THELOAI selectedTheLoai)
            {
                // Cập nhật TuaSachList khi chọn Tab
                TuaSachList = TuaSachByTheLoai[selectedTheLoai.ID];
                OnPropertyChanged(nameof(TuaSachList));
            }
        }
        private void LoadTheLoai()
        {
            using (var context = new QLTV2Context())
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
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public UTrangChu()
        {
            InitializeComponent();
            DataContext = this; // Gán DataContext chính là UserControl
            LoadTheLoai();
            LoadTuaSachByTheLoai(); // Thêm phương thức tải tựa sách theo thể loại
        }
        

    }
}

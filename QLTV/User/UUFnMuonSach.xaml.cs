using MaterialDesignColors;
using Microsoft.EntityFrameworkCore;
using QLTV.Models;
using QLTV.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
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
using System.Xml.Linq;

namespace QLTV.User
{
    /// <summary>
    /// Interaction logic for UUFnMuonSach.xaml
    /// </summary>
    public partial class UUFnMuonSach : UserControl
    {
        private readonly QLTVContext _context;
        private ObservableCollection<SACH> _allBooks;
        private ObservableCollection<SachCoSanViewModel> dsSach;
        private IEnumerable<SachCoSanViewModel> filteredBooks;
        private ObservableCollection<SachDaChonViewModel> _selectedBooks;
        private CollectionViewSource viewSource;
        private DOCGIA docGia = new DOCGIA();
        
        private class SachCoSanViewModel
        {
            public SACH? OSach { get; set; }
            public string MaSach { get; set; }
            public string TuaSach { get; set; }
            public string DSTacGia { get; set; }
            public string DSTheLoai { get; set; }
            public int HanMuonToiDa { get; set; }
        }
        
        private class SachDaChonViewModel : INotifyPropertyChanged, IDataErrorInfo
        {
            private SachCoSanViewModel _oSachVm;
            private string _soTuanMuon;
            private DateTime _ngayTra;

            public SachCoSanViewModel OSachVM
            {
                get => _oSachVm;
                set
                {
                    _oSachVm = value;
                    OnPropertyChanged(nameof(_oSachVm));
                    OnPropertyChanged(nameof(SachCoSanViewModel.DSTacGia));
                    OnPropertyChanged(nameof(SachCoSanViewModel.DSTheLoai));
                }
            }

            public SACH OSach
            {
                get => _oSachVm.OSach;
                set
                {
                    _oSachVm.OSach = value;
                    OnPropertyChanged(nameof(OSachVM));
                    OnPropertyChanged(nameof(MaSach));
                    OnPropertyChanged(nameof(IDTuaSachNavigation));
                }
            }

            public string DSTheLoai
            {
                get => _oSachVm.DSTheLoai;
            }

            public string DSTacGia
            {
                get => _oSachVm.DSTacGia;
            }

            public string SoTuanMuon
            {
                get => _soTuanMuon;
                set
                {
                    _soTuanMuon = value;
                    UpdateNgayTra();
                    OnPropertyChanged(nameof(SoTuanMuon));
                }
            }

            public DateTime NgayTra
            {
                get => _ngayTra;
                set
                {
                    _ngayTra = value;
                    OnPropertyChanged(nameof(NgayTra));
                }
            }

            private bool _isValid = true;

            public bool isValid
            {
                get => _isValid;
                set
                {
                    _isValid = value;
                    OnPropertyChanged(nameof(isValid));
                }
            }


            string IDataErrorInfo.this[string columnName]
            {
                get
                {
                    if (columnName == nameof(SoTuanMuon))
                    {
                        if (string.IsNullOrWhiteSpace(SoTuanMuon))
                        {
                            isValid = false;
                            return "Không được trống.";
                        }

                        if (!int.TryParse(SoTuanMuon, out int a))
                        {
                            isValid = false;
                            return "Nhập số nguyên.";
                        }
                        if (a <= 0)
                        {
                            isValid = false;
                            return "Tối thiểu 1.";
                        }

                        if (a > OSach.IDTuaSachNavigation.HanMuonToiDa)
                        {
                            isValid = false;
                            return $"Tối đa {OSach.IDTuaSachNavigation.HanMuonToiDa}.";
                        }
                        isValid = true;
                        return null;
                    }
                    return null;
                }
            }

            public string MaSach => OSach.MaSach;
            public TUASACH IDTuaSachNavigation => OSach.IDTuaSachNavigation;

            public int ID => OSach.ID;

            public string Error => throw new NotImplementedException();

            private void UpdateNgayTra()
            {
                if (!int.TryParse(SoTuanMuon, out int a))
                {
                    NgayTra = DateTime.Now.AddDays(OSach.IDTuaSachNavigation.HanMuonToiDa * 7);
                    return;
                }
                NgayTra = DateTime.Now.AddDays(a * 7);
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public UUFnMuonSach()
        {
            InitializeComponent();
            _context = new();
            _selectedBooks = new();
            dgSelectedBooks.ItemsSource = _selectedBooks;
            LoadData();
        }

        private string ConvertToUnsigned(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return new string(
                text.Trim()
                    .ToLower()
                    .Normalize(NormalizationForm.FormD)
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    .ToArray()
            ).Normalize(NormalizationForm.FormC);
        }

        private async void LoadData()
        {
            try
            {
                _allBooks = new ObservableCollection<SACH>(await _context.SACH
                     .Include(s => s.IDTuaSachNavigation)
                         .ThenInclude(ts => ts.TUASACH_THELOAI)
                             .ThenInclude(ts_tl => ts_tl.IDTheLoaiNavigation)
                     .Include(s => s.IDTuaSachNavigation)
                         .ThenInclude(ts => ts.TUASACH_TACGIA)
                             .ThenInclude(ts_tg => ts_tg.IDTacGiaNavigation)
                     .Include(s => s.IDTinhTrangNavigation)
                     .Where(s => !s.IsDeleted && s.IsAvailable == true)
                     .ToListAsync());

                dsSach = new ObservableCollection<SachCoSanViewModel>(_allBooks.Select(s => new SachCoSanViewModel
                {
                    OSach = s,
                    MaSach = s.MaSach,
                    TuaSach = s.IDTuaSachNavigation.TenTuaSach,
                    DSTacGia = string.Join(", ", s.IDTuaSachNavigation.TUASACH_TACGIA
                        .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                    DSTheLoai = string.Join(", ", s.IDTuaSachNavigation.TUASACH_THELOAI
                        .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai)),
                    HanMuonToiDa = s.IDTuaSachNavigation.HanMuonToiDa
                }).ToList());

                dgAvailableBooks.ItemsSource = dsSach;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TimSach(string search)
        {
            if (_allBooks == null) return;

            var searchText = ConvertToUnsigned(search);
            var searchType = ((ComboBoxItem)cboSearchType.SelectedItem).Content.ToString();

            filteredBooks = dsSach;

            if (!string.IsNullOrEmpty(searchText))
            {
                switch (searchType)
                {
                    case "Mã sách":
                        filteredBooks = filteredBooks.Where(s =>
                            ConvertToUnsigned(s.OSach.MaSach).Contains(searchText));
                        break;

                    case "Tên sách":
                        filteredBooks = filteredBooks.Where(s =>
                            ConvertToUnsigned(s.OSach.IDTuaSachNavigation.TenTuaSach).Contains(searchText));
                        break;

                    case "Thể loại":
                        filteredBooks = filteredBooks.Where(s =>
                            s.OSach.IDTuaSachNavigation.TUASACH_THELOAI
                                .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai)
                                .Any(tenTheLoai => ConvertToUnsigned(tenTheLoai).Contains(searchText)));
                        break;

                    case "Tác giả":
                        filteredBooks = filteredBooks.Where(s =>
                            s.OSach.IDTuaSachNavigation.TUASACH_TACGIA
                                .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia.ToLower())
                                .Any(tenTacGia => ConvertToUnsigned(tenTacGia).Contains(searchText)));
                        break;

                    default: // "Tất cả"
                        filteredBooks = filteredBooks.Where(s =>
                            ConvertToUnsigned(s.OSach.MaSach).Contains(searchText) ||
                            ConvertToUnsigned(s.OSach.IDTuaSachNavigation.TenTuaSach).Contains(searchText) ||
                            s.OSach.IDTuaSachNavigation.TUASACH_THELOAI
                                .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai)
                                .Any(tenTheLoai => ConvertToUnsigned(tenTheLoai).Contains(searchText)) ||
                            s.OSach.IDTuaSachNavigation.TUASACH_TACGIA
                                .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia.ToLower())
                                .Any(tenTacGia => ConvertToUnsigned(tenTacGia).Contains(searchText)));
                        break;
                }
            }

            // Convert filtered books to display format using the concrete type

            dgAvailableBooks.ItemsSource = filteredBooks;
        }

        private void txtSearchBook_TextChanged(object sender, TextChangedEventArgs e)
        {
            TimSach(txtSearchBook.Text);
        }

        private void btnSelectBook_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new QLTVContext())
            {
                docGia = context.DOCGIA
                    .Include(dg => dg.IDLoaiDocGiaNavigation)
                    .Include(dg => dg.PHIEUMUON)
                        .ThenInclude(pm => pm.CTPHIEUMUON)
                    .Include(dg => dg.PHIEUMUON)
                        .ThenInclude(pm => pm.CTPHIEUTRA)
                    .Where(dg => dg.IDTaiKhoanNavigation.ID == Settings.Default.CurrentUserID)
                    .FirstOrDefault();

                MessageBox.Show(Settings.Default.CurrentUserID.ToString());

                int daMuon = docGia.PHIEUMUON.Sum(dg => dg.CTPHIEUMUON.Count - dg.CTPHIEUTRA.Count);

                if (daMuon + _selectedBooks.Count + 1 > docGia.IDLoaiDocGiaNavigation.SoSachMuonToiDa)
                {
                    MessageBox.Show("Quá số sách tối đa bạn có thể mượn!", "Thông Báo", 
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                var sachDuocChon = ((Button)sender).DataContext as SachCoSanViewModel;
                if (sachDuocChon?.OSach == null) return;

                var maSach = sachDuocChon.OSach.MaSach;
                var sach = context.SACH
                    .Where(s => s.MaSach == maSach)
                    .FirstOrDefault();

                if (sach == null) return;

                // Kiểm tra trạng thái sách trước khi chọn
                if (!sach.IsAvailable ?? false)
                {
                    MessageBox.Show("Sách này đã được chọn hoặc mượn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LoadData();
                    return;
                }

                try
                {
                    // Sử dụng lock để đảm bảo tính đồng bộ
                    lock (context.SACH)
                    {
                        if (!_selectedBooks.Any(sb => sb.OSach.ID == sachDuocChon.OSach.ID))
                        {
                            // Đánh dấu sách là không khả dụng
                            sach.IsAvailable = false;
                            context.Update(sach);
                            context.SaveChangesAsync();

                            var sachDaChon = new SachDaChonViewModel
                            {
                                OSachVM = sachDuocChon,
                                SoTuanMuon = sachDuocChon.OSach.IDTuaSachNavigation.HanMuonToiDa.ToString()
                            };

                            // Thay đổi trạng thái một cách an toàn
                            var tempDsSach = new ObservableCollection<SachCoSanViewModel>(dsSach);
                            tempDsSach.Remove(sachDuocChon);
                            dsSach = tempDsSach;

                            var tempAllBooks = new ObservableCollection<SACH>(_allBooks);
                            tempAllBooks.Remove(sachDuocChon.OSach);
                            _allBooks = tempAllBooks;

                            TimSach(txtSearchBook.Text);
                            _selectedBooks.Add(sachDaChon);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Khôi phục trạng thái nếu có lỗi
                    context.SACH.Find(sachDuocChon.OSach.ID).IsAvailable = true;
                    context.SaveChangesAsync();
                    MessageBox.Show($"Lỗi khi chọn sách: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRemoveBook_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new QLTVContext())
            {
                var sachDuocChon = ((Button)sender).DataContext as SachDaChonViewModel;
                if (sachDuocChon?.OSachVM?.OSach == null) return;

                var maSach = sachDuocChon.OSach.MaSach;
                var sach = context.SACH
                    .Where(s => s.MaSach == maSach)
                    .FirstOrDefault();

                if (sach == null) return;

                try
                {
                    // Sử dụng lock để đảm bảo tính đồng bộ
                    lock (context.SACH)
                    {
                        // Khôi phục trạng thái sách
                        sach.IsAvailable = true;
                        context.Update(sach);
                        context.SaveChangesAsync();
                        _selectedBooks.Remove(sachDuocChon);

                        // Thay đổi trạng thái một cách an toàn
                        var tempDsSach = new ObservableCollection<SachCoSanViewModel>(dsSach);
                        tempDsSach.Add(sachDuocChon.OSachVM);
                        dsSach = tempDsSach;

                        var tempAllBooks = new ObservableCollection<SACH>(_allBooks);
                        tempAllBooks.Add(sachDuocChon.OSachVM.OSach);
                        _allBooks = tempAllBooks;

                        TimSach(txtSearchBook.Text);
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu có
                    context.SACH.Find(sachDuocChon.OSach.ID).IsAvailable = false;
                    context.SaveChangesAsync();
                    MessageBox.Show($"Lỗi khi xóa sách: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new QLTVContext())
            {
                if (_selectedBooks.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một cuốn sách.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedBooks.Any(b => !b.isValid))
                {
                    MessageBox.Show("Vui lòng chọn số ngày mượn hợp lệ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    // Tạo mới phiếu mượn
                    var phieuMuon = new PHIEUMUON
                    {
                        IDDocGia=  context.DOCGIA
                            .Include(dg => dg.IDTaiKhoanNavigation)
                            .Where(dg => dg.IDTaiKhoanNavigation.ID == Settings.Default.CurrentUserID)
                            .Select(dg => dg.ID)
                            .FirstOrDefault(),
                        NgayMuon = DateTime.Now,
                        IsPending = true
                    };

                    context.PHIEUMUON.Add(phieuMuon);
                    await context.SaveChangesAsync();  // Lưu phiếu mượn

                    var ctPhieuMuonList = new List<CTPHIEUMUON>();

                    foreach (var sachDaChon in _selectedBooks)
                    {
                        var ctPhieuMuon = new CTPHIEUMUON
                        {
                            IDPhieuMuon = phieuMuon.ID,
                            IDSach = sachDaChon.ID,
                            HanTra = sachDaChon.NgayTra,
                            IDTinhTrangMuon = sachDaChon.OSach.IDTinhTrang
                        };
                        ctPhieuMuonList.Add(ctPhieuMuon);

                        sachDaChon.OSach.IsAvailable = false;
                    }

                    context.CTPHIEUMUON.AddRange(ctPhieuMuonList);  // Thêm các chi tiết phiếu mượn
                    await context.SaveChangesAsync();  // Lưu thay đổi chi tiết phiếu mượn

                    // Update BCMUONSACH
                    var Today = DateTime.Now;
                    var bcMuonSach = await context.BCMUONSACH
                        .FirstOrDefaultAsync(bc => bc.Thang == Today);

                    if (bcMuonSach == null)
                    {
                        bcMuonSach = new BCMUONSACH
                        {
                            Thang = Today,
                            TongSoLuotMuon = _selectedBooks.Count
                        };
                        context.BCMUONSACH.Add(bcMuonSach);
                    }
                    else
                    {
                        bcMuonSach.TongSoLuotMuon += _selectedBooks.Count;
                    }

                    await context.SaveChangesAsync();  // Lưu báo cáo mượn sách

                    // Update CTBCMUONSACH
                    var theLoaiGroups = _selectedBooks
                        .SelectMany(b => b.OSach.IDTuaSachNavigation.TUASACH_THELOAI.Select(ts_tl => ts_tl.IDTheLoaiNavigation))
                        .GroupBy(tl => tl.ID)
                        .Select(g => new { TheLoaiId = g.Key, Count = g.Count() });

                    var ctBcMuonSachList = new List<CTBCMUONSACH>();

                    foreach (var group in theLoaiGroups)
                    {
                        var ctBcMuonSach = await context.CTBCMUONSACH
                            .FirstOrDefaultAsync(ct => ct.IDBCMuonSach == bcMuonSach.ID && ct.IDTheLoai == group.TheLoaiId);

                        if (ctBcMuonSach == null)
                        {
                            ctBcMuonSach = new CTBCMUONSACH
                            {
                                IDBCMuonSach = bcMuonSach.ID,
                                IDTheLoai = group.TheLoaiId,
                                SoLuotMuon = group.Count
                            };
                            ctBcMuonSachList.Add(ctBcMuonSach);
                        }
                        else
                        {
                            ctBcMuonSach.SoLuotMuon += group.Count;
                        }
                    }

                    context.CTBCMUONSACH.AddRange(ctBcMuonSachList);  // Thêm chi tiết báo cáo mượn sách theo thể loại
                    await context.SaveChangesAsync();  // Lưu thay đổi báo cáo mượn sách theo thể loại

                    // Update TiLe for all CTBCMUONSACH entries of this report
                    var allCtBcMuonSach = await context.CTBCMUONSACH
                        .Where(ct => ct.IDBCMuonSach == bcMuonSach.ID)
                        .ToListAsync();

                    foreach (var ct in allCtBcMuonSach)
                    {
                        ct.TiLe = (float)ct.SoLuotMuon / bcMuonSach.TongSoLuotMuon;
                    }

                    await context.SaveChangesAsync();  // Lưu tỷ lệ cho các chi tiết báo cáo mượn sách

                    MessageBox.Show("Thêm phiếu mượn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    _selectedBooks.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu phiếu mượn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new QLTVContext())
            {
                try
                {
                    // Đặt lại trạng thái IsAvailable của sách đã chọn về true
                    foreach (var idSach in _selectedBooks.Select(sb => sb.OSachVM.OSach.ID))
                    {
                        var sach = context.SACH.Find(idSach);
                        if (sach.IsAvailable == false)
                        {
                            sach.IsAvailable = true;
                        }
                    }

                    // Cập nhật trạng thái trong cơ sở dữ liệu (nếu cần)
                    await context.SaveChangesAsync();

                    // Đóng cửa sổ
                    _selectedBooks.Clear();
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi hủy: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void MuonNgaySach(string maSach)
        {
            try
            {
                using (var context = new QLTVContext())
                {
                    // Lấy thông tin sách từ database
                    var sachDuocChon = context.SACH
                        .Where(s => s.MaSach == maSach)
                        .Include(s => s.IDTuaSachNavigation)
                        .Include(s => s.IDTinhTrangNavigation)
                        .Select(s => new SachCoSanViewModel
                        {
                            OSach = s,
                            MaSach = s.MaSach,
                            TuaSach = s.IDTuaSachNavigation.TenTuaSach,
                            DSTacGia = string.Join(", ", s.IDTuaSachNavigation.TUASACH_TACGIA
                                .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                            DSTheLoai = string.Join(", ", s.IDTuaSachNavigation.TUASACH_THELOAI
                                .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai)),
                            HanMuonToiDa = s.IDTuaSachNavigation.HanMuonToiDa
                        })
                        .FirstOrDefault();

                    if (sachDuocChon == null)
                    {
                        return; // Không tìm thấy sách với mã này
                    }

                    // Safe null handling
                    string soTuanMuon = "0"; // Default value
                    if (sachDuocChon.OSach?.IDTuaSachNavigation != null)
                    {
                        soTuanMuon = sachDuocChon.OSach.IDTuaSachNavigation.HanMuonToiDa.ToString();
                    }

                    var sachDaChon = new SachDaChonViewModel
                    {
                        OSachVM = sachDuocChon,
                        SoTuanMuon = soTuanMuon
                    };

                    // Thay đổi trạng thái một cách an toàn
                    var tempDsSach = new ObservableCollection<SachCoSanViewModel>(dsSach);
                    tempDsSach.Remove(sachDuocChon);
                    dsSach = tempDsSach;

                    var tempAllBooks = new ObservableCollection<SACH>(_allBooks);
                    tempAllBooks.Remove(sachDuocChon.OSach);
                    _allBooks = tempAllBooks;

                    // Cập nhật lại danh sách sách sau khi tìm kiếm lại
                    TimSach(txtSearchBook.Text);

                    // Thêm sách đã chọn vào danh sách
                    _selectedBooks.Add(sachDaChon);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chi tiết lỗi: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Lỗi Chi Tiết", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

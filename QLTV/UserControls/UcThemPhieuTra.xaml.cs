using QLTV.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Windows.Data;
using System.Globalization;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QLTV.UserControls
{

    public class ReturnDetailModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private CTPHIEUMUON ctPhieuMuon;
        public CTPHIEUMUON CTPhieuMuon
        {
            get => ctPhieuMuon;
            set
            {
                ctPhieuMuon = value;
                OnPropertyChanged();
            }
        }

        private CTPHIEUTRA ctPhieuTra;
        public CTPHIEUTRA CTPhieuTra
        {
            get => ctPhieuTra;
            set
            {
                ctPhieuTra = value;
                OnPropertyChanged();
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }

        private bool isValid;
        public bool IsValid
        {
            get => isValid;
            set
            {
                isValid = value;
                OnPropertyChanged();
            }
        }

        public TINHTRANG TinhTrangMuon => ctPhieuMuon.IDTinhTrangMuonNavigation;
        public TINHTRANG TinhTrangTra
        {
            get => ctPhieuTra.IDTinhTrangTraNavigation;
            set
            {
                ctPhieuTra.IDTinhTrangTraNavigation = value;
                OnPropertyChanged();
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (columnName == nameof(TinhTrangTra))
                {
                    if (TinhTrangTra == null)
                    {
                        isValid = false;
                        return "Vui lòng chọn tình trạng trả.";
                    }

                    if (TinhTrangTra.MucHuHong < TinhTrangMuon.MucHuHong)
                    {
                        isValid = false;
                        return "Tình trạng trả không được tốt hơn tình trạng mượn.";
                    }
                    isValid = true;
                    return null;
                }
                return null;
            }
        }

        string IDataErrorInfo.Error => throw new NotImplementedException();

        public string MaSach => ctPhieuTra.IDSachNavigation?.MaSach ?? "Không tồn tại";
        public string TenSach => ctPhieuTra.IDSachNavigation?.IDTuaSachNavigation?.TenTuaSach ?? "Không tồn tại";
        public DateTime HanTra => ctPhieuMuon.HanTra;
        public string GhiChu
        {
            get => ctPhieuTra.GhiChu;
            set
            {
                ctPhieuTra.GhiChu = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class UcThemPhieuTra : UserControl
    {
        private readonly QLTVContext _context;
        private ObservableCollection<ReturnDetailModel> _returnDetails;

        public UcThemPhieuTra()
        {
            InitializeComponent();
            _context = new QLTVContext();
            _returnDetails = new ObservableCollection<ReturnDetailModel>();
            LoadData();
        }

        private string ConvertToUnsigned(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return new string(
                text.Normalize(NormalizationForm.FormD)
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    .ToArray()
            ).Normalize(NormalizationForm.FormC);
        }

        private async void LoadData()
        {
            try
            {
                //Load tất cả độc giả và các phiếu mượn của độc giả đó

                var pendingBorrows = await _context.DOCGIA
                    .Include(dg => dg.PHIEUMUON.Where(pm => pm.CTPHIEUMUON.Count != pm.CTPHIEUTRA.Count))
                        .ThenInclude(pm => pm.CTPHIEUMUON)
                            .ThenInclude(ct => ct.IDSachNavigation)
                                .ThenInclude(s => s.IDTuaSachNavigation)
                    .Include(dg => dg.PHIEUMUON.Where(pm => pm.CTPHIEUMUON.Count != pm.CTPHIEUTRA.Count))
                        .ThenInclude(pm => pm.CTPHIEUTRA)
                    .Include(dg => dg.PHIEUMUON.Where(pm => pm.CTPHIEUMUON.Count != pm.CTPHIEUTRA.Count))
                        .ThenInclude(pm => pm.CTPHIEUMUON)
                            .ThenInclude(ct => ct.IDSachNavigation)
                                .ThenInclude(s => s.IDTinhTrangNavigation)
                    .Include(dg => dg.IDTaiKhoanNavigation)
                    .ToListAsync();

                //Chọn các phiếu mượn chưa hoàn tất trả

                var availableBorrows = pendingBorrows
                    .Where(dg => dg.PHIEUMUON.Any(pm =>
                        pm.CTPHIEUMUON.Any(ct =>
                            !pm.CTPHIEUTRA.Any(ptr => ptr.IDPhieuMuonNavigation == pm && ptr.IDSach == ct.IDSach)
                        )
                    ))
                    .ToList();

                //Load tất cả các tình trạng
                var tinhTrangList = _context.TINHTRANG.ToList();
                colTinhTrangTra.ItemsSource = tinhTrangList;


                var viewSource = new CollectionViewSource();
                viewSource.Source = availableBorrows;
                cboDocGia.ItemsSource = viewSource.View;

                var textBox = cboDocGia.Template.FindName("PART_EditableTextBox", cboDocGia) as TextBox;
                if (textBox != null)
                {
                    textBox.TextChanged += (sender, args) =>
                    {
                        var searchText = ConvertToUnsigned(textBox.Text);
                        viewSource.View.Filter = item =>
                        {
                            if (string.IsNullOrEmpty(searchText))
                                return true;
                            var docGia = item as DOCGIA;
                            if (docGia != null)
                            {
                                var itemText = ConvertToUnsigned(docGia.IDTaiKhoanNavigation.TenTaiKhoan.ToString());
                                return itemText.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                            }
                            return false;
                        };
                        cboDocGia.IsDropDownOpen = true;
                    };
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cboDocGia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedReader = cboDocGia.SelectedItem as DOCGIA;
            if (selectedReader == null)
            {
                _returnDetails.Clear();
                return;
            }

            //Khởi tạo các chi tiết phiếu mượn tương ứng
            _returnDetails.Clear();
            foreach (var ctpm in selectedReader.PHIEUMUON.Where(pm => pm.CTPHIEUMUON.Count != pm.CTPHIEUTRA.Count).SelectMany(pm => pm.CTPHIEUMUON).ToList())
            {
                var ctpt = new CTPHIEUTRA
                {
                    IDPhieuMuon = ctpm.IDPhieuMuon,
                    IDPhieuMuonNavigation = ctpm.IDPhieuMuonNavigation,
                    IDSach = ctpm.IDSach,
                    IDSachNavigation = ctpm.IDSachNavigation,
                    SoNgayMuon = (int)(DateTime.Now - ctpm.IDPhieuMuonNavigation.NgayMuon).TotalDays,
                    IDTinhTrangTra = ctpm.IDTinhTrangMuon,
                    IDTinhTrangTraNavigation = ctpm.IDTinhTrangMuonNavigation,
                    GhiChu = "",
                    TienPhat = 0
                };

                var returnDetail = new ReturnDetailModel {
                    IsSelected = true,
                    CTPhieuTra = ctpt,
                    CTPhieuMuon = ctpm
                };
                
                _returnDetails.Add(returnDetail);
            }
            dgBooks.ItemsSource = _returnDetails;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var selectedReader = cboDocGia.SelectedItem as DOCGIA;
            if (selectedReader == null)
            {
                MessageBox.Show("Vui lòng chọn độc giả", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedBooks = _returnDetails.Where(r => r.IsSelected).ToList();
            if (!selectedBooks.Any())
            {
                MessageBox.Show("Vui lòng chọn ít nhất một cuốn sách để trả", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(selectedBooks.Any(b => !b.IsValid))
            {
                MessageBox.Show("Vui lòng nhập các tình trạng trả hợp lệ", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var phieuTra = new PHIEUTRA
                {
                    NgayTra = DateTime.Now
                };

                _context.PHIEUTRA.Add(phieuTra);
                await _context.SaveChangesAsync();

                // Check for late returns and update BCTRATRE if necessary

                var lateReturns = selectedBooks
                    .Where(r => DateTime.Now > r.HanTra)
                    .ToList();

                if (lateReturns.Any())
                {
                    var today = DateTime.Now.Date;
                    var bcTraTre = await _context.BCTRATRE
                        .FirstOrDefaultAsync(bc => bc.Ngay == today);

                    if (bcTraTre == null)
                    {
                        bcTraTre = new BCTRATRE
                        {
                            Ngay = today
                        };
                        _context.BCTRATRE.Add(bcTraTre);
                        await _context.SaveChangesAsync();
                    }

                    foreach (var lateReturn in lateReturns)
                    {
                        var ctBcTraTre = new CTBCTRATRE
                        {
                            IDBCTraTre = bcTraTre.ID,
                            IDPhieuTra = phieuTra.ID,
                            SoNgayTraTre = (int)(DateTime.Now - lateReturn.HanTra).TotalDays
                        };

                        _context.CTBCTRATRE.Add(ctBcTraTre);
                    }
                }

                await _context.SaveChangesAsync();

                foreach (var returnBook in selectedBooks)
                {
                    CTPHIEUTRA ctpt = returnBook.CTPhieuTra;
                    ctpt.IDPhieuTra = phieuTra.ID;

                    //ctpt.IDTinhTrangTra = returnBook.IDTinhTrangTra;
                    //ctpt.IDTinhTrangTraNavigation = _context.TINHTRANG.Find(returnBook.IDTinhTrangTra);

                    var book = await _context.SACH.FindAsync(ctpt.IDSach);
                    var borrowDetail = await _context.CTPHIEUMUON
                        .FirstOrDefaultAsync(ct => ct.IDPhieuMuon == ctpt.IDPhieuMuon && ct.IDSach == ctpt.IDSach);

                    if (borrowDetail != null)
                    {

                        // Calculate fines
                        if (DateTime.Now > borrowDetail.HanTra)
                        {
                            int daysLate = (int)(DateTime.Now - borrowDetail.HanTra).TotalDays;
                            ctpt.TienPhat = daysLate * _context.THAMSO.OrderBy(ts => ts.ID).Last().TienPhatTraTreMotNgay;
                        }

                        if (returnBook.TinhTrangTra != returnBook.TinhTrangMuon && book != null)
                        {
                            ctpt.TienPhat += book.TriGia * _context.THAMSO.OrderBy(ts => ts.ID).Last().TiLeDenBu * (returnBook.TinhTrangTra.MucHuHong - returnBook.TinhTrangMuon.MucHuHong) / 100;
                        }


                        selectedReader.TongNo += ctpt.TienPhat;
                        ctpt.GhiChu = returnBook.GhiChu;

                        _context.CTPHIEUTRA.Add(ctpt);
                        if (book != null)
                        {
                            if (ctpt.IDTinhTrangTraNavigation.MucHuHong != 100)
                                book.IsAvailable = true;
                            book.IDTinhTrang = ctpt.IDTinhTrangTra;
                            _context.SACH.Update(book);
                        }
                    }
                }

                // Update borrow ticket status

                foreach(var pm in selectedReader.PHIEUMUON.ToList())

                if (pm.CTPHIEUMUON.All(ct => _returnDetails.Select(ctpm => ctpm.CTPhieuMuon).ToList().Contains(ct) && 
                                                    pm.CTPHIEUMUON.All(r => _returnDetails.First(rd => rd.CTPhieuMuon == ct).IsSelected)))
                {
                    pm.IsPending = false;
                    _context.PHIEUMUON.Update(pm);
                }

                await _context.SaveChangesAsync();
                MessageBox.Show("Thêm phiếu trả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                Window.GetWindow(this).DialogResult = true;
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu phiếu trả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
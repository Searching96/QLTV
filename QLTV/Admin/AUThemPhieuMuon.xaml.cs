using Microsoft.EntityFrameworkCore;
using Microsoft.Win32.SafeHandles;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace QLTV.Admin
{
    public class BookViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private SACH book;
        public SACH Book
        {
            get => book;
            set
            {
                book = value;
                OnPropertyChanged();
            }
        }

        public string DSTheLoai => string.Join(", ", book.IDTuaSachNavigation.TUASACH_TACGIA
                    .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia));

        public string DSTacGia => string.Join(", ", book.IDTuaSachNavigation.TUASACH_THELOAI
                    .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai));

        private string _CustomBorrowWeeks;
        public string CustomBorrowWeeks
        {
            get => _CustomBorrowWeeks;
            set
            {
                _CustomBorrowWeeks = value;
                UpdateCustomReturnDate();
                OnPropertyChanged(nameof(CustomBorrowWeeks));
            }
        }

        private DateTime _CustomReturnDate;
        public DateTime CustomReturnDate
        {
            get => _CustomReturnDate;
            set
            {
                _CustomReturnDate = value;
                OnPropertyChanged(nameof(CustomReturnDate));
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
                if (columnName == nameof(CustomBorrowWeeks))
                {
                    if (string.IsNullOrWhiteSpace(CustomBorrowWeeks))
                    {
                        isValid = false;
                        return "Thầy Dũng đẹp trai";
                    }

                    if (!int.TryParse(CustomBorrowWeeks, out int a))
                    {
                        isValid = false;
                        return "Nhập số nguyên";
                    }
                    if (a <= 0)
                    {
                        isValid = false;
                        return "Tối thiểu 1.";
                    }

                    if (a > Book.IDTuaSachNavigation.HanMuonToiDa)
                    {
                        isValid = false;
                        return $"Tối đa {Book.IDTuaSachNavigation.HanMuonToiDa}.";
                    }
                }
                isValid = true;
                return null;
            }
        }

        public string MaSach => Book.MaSach;
        public string TuaSach => IDTuaSachNavigation.TenTuaSach;
        public int HanMuonToiDa => IDTuaSachNavigation.HanMuonToiDa;

        public TUASACH IDTuaSachNavigation => Book.IDTuaSachNavigation;

        public int ID => Book.ID;

        public string Error => throw new NotImplementedException();

        private void UpdateCustomReturnDate()
        {
            if (!int.TryParse(CustomBorrowWeeks, out int a))
            {
                CustomReturnDate = DateTime.Now.AddDays(Book.IDTuaSachNavigation.HanMuonToiDa * 7);
                return;
            }
            CustomReturnDate = DateTime.Now.AddDays(a * 7);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class UcThemPhieuMuon : UserControl
    {
        private ObservableCollection<BookViewModel> dsSach;
        private IEnumerable<BookViewModel> filteredBooks;
        private ObservableCollection<BookViewModel> _selectedBooks;
        private CollectionViewSource viewSource;
        public bool isClosing = false;

        public UcThemPhieuMuon()
        {
            InitializeComponent();
            _selectedBooks = new();
            dgSelectedBooks.ItemsSource = _selectedBooks;
            LoadData();
            this.Loaded += UcThemPhieuMuon_Loaded;
        }

        private void UcThemPhieuMuon_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Closing += UcThemPhieuMuon_Closing;
            }
        }

        private void UcThemPhieuMuon_Closing(object sender, CancelEventArgs e)
        {
            isClosing = true;
            btnCancel_Click(sender, new RoutedEventArgs());
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

        private async Task LoadBookData()
        {
            try
            {
                using (var context = new QLTVContext())
                {
                    dsSach = new ObservableCollection<BookViewModel>(
                        await context.SACH
                        .Include(s => s.IDTuaSachNavigation)
                            .ThenInclude(ts => ts.TUASACH_THELOAI)
                                .ThenInclude(ts_tl => ts_tl.IDTheLoaiNavigation)
                        .Include(s => s.IDTuaSachNavigation)
                            .ThenInclude(ts => ts.TUASACH_TACGIA)
                                .ThenInclude(ts_tg => ts_tg.IDTacGiaNavigation)
                        .Include(s => s.IDTinhTrangNavigation)
                        .Where(s => !s.IsDeleted && s.IsAvailable == true)
                        .Select(s => new BookViewModel
                        {
                            Book = s,
                            CustomBorrowWeeks = s.IDTuaSachNavigation.HanMuonToiDa.ToString()
                        }).ToListAsync());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                dgAvailableBooks.ItemsSource = dsSach;
            }
        }

        private async void LoadData()
        {
            try
            {
                using (var context = new QLTVContext())
                {
                    LoadBookData();

                    var docGia = await context.DOCGIA
                        .Include(d => d.IDTaiKhoanNavigation)
                        .Include(d => d.IDLoaiDocGiaNavigation)
                        .Include(d => d.PHIEUMUON)
                            .ThenInclude(pm => pm.CTPHIEUMUON)
                        .Include(d => d.PHIEUMUON)
                            .ThenInclude(pm => pm.CTPHIEUTRA)
                        .ToListAsync();

                    viewSource = new CollectionViewSource();
                    viewSource.Source = docGia;
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BookSearch(string search)
        {
            if (dsSach == null) return;

            var searchText = ConvertToUnsigned(search);
            var searchType = ((ComboBoxItem)cboSearchType.SelectedItem).Content.ToString();

            filteredBooks = dsSach;

            if (!string.IsNullOrEmpty(searchText))
            {
                switch (searchType)
                {
                    case "Mã sách":
                        filteredBooks = filteredBooks.Where(s =>
                            ConvertToUnsigned(s.MaSach).Contains(searchText));
                        break;

                    case "Tên sách":
                        filteredBooks = filteredBooks.Where(s =>
                            ConvertToUnsigned(s.IDTuaSachNavigation.TenTuaSach).Contains(searchText));
                        break;

                    case "Thể loại":
                        filteredBooks = filteredBooks.Where(s =>
                            s.IDTuaSachNavigation.TUASACH_THELOAI
                                .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai)
                                .Any(tenTheLoai => ConvertToUnsigned(tenTheLoai).Contains(searchText)));
                        break;

                    case "Tác giả":
                        filteredBooks = filteredBooks.Where(s =>
                            s.IDTuaSachNavigation.TUASACH_TACGIA
                                .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia.ToLower())
                                .Any(tenTacGia => ConvertToUnsigned(tenTacGia).Contains(searchText)));
                        break;

                    default: // "Tất cả"
                        filteredBooks = filteredBooks.Where(s =>
                            ConvertToUnsigned(s.MaSach).Contains(searchText) ||
                            ConvertToUnsigned(s.IDTuaSachNavigation.TenTuaSach).Contains(searchText) ||
                            s.IDTuaSachNavigation.TUASACH_THELOAI
                                .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai)
                                .Any(tenTheLoai => ConvertToUnsigned(tenTheLoai).Contains(searchText)) ||
                            s.IDTuaSachNavigation.TUASACH_TACGIA
                                .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia.ToLower())
                                .Any(tenTacGia => ConvertToUnsigned(tenTacGia).Contains(searchText)));
                        break;
                }
            }

            // Liên kết kết quả hàm lọc với DataGrid

            dgAvailableBooks.ItemsSource = filteredBooks;
        }

        private void txtSearchBook_TextChanged(object sender, TextChangedEventArgs e)
        {
            BookSearch(txtSearchBook.Text);
        }

        private async void btnSelectBook_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new QLTVContext())
                {
                    var book = ((Button)sender).DataContext as BookViewModel;
                    if (book == null) return;

                    if (!DocGiaValidation(cboDocGia.SelectedItem as DOCGIA)) return;

                    if (!_selectedBooks.Any(b => b.Book.ID == book.Book.ID) && book.Book.IsAvailable == true)
                    {
                        var sach = context.SACH
                            .Where(s => s.ID == book.Book.ID)
                            .FirstOrDefault();

                        if (sach == null)
                            return;

                        sach.IsAvailable = false;
                        context.SaveChanges();
                        _selectedBooks.Add(book);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chọn sách: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await LoadBookData();
                BookSearch(txtSearchBook.Text);
            }

        }

        private async void btnRemoveBook_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new QLTVContext())
                {
                    var book = ((Button)sender).DataContext as BookViewModel;
                    if (book != null && book.Book != null)
                    {
                        var sach = context.SACH
                            .Where(s => s.ID == book.Book.ID)
                            .FirstOrDefault();

                        if (sach == null)
                            return;

                        sach.IsAvailable = true;
                        context.SaveChanges();
                        _selectedBooks.Remove(book);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa sách: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await LoadBookData();
                BookSearch(txtSearchBook.Text);
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cboDocGia.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn độc giả.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                using (var context = new QLTVContext())
                {
                    context.ChangeTracker.Clear();
                    var phieuMuon = new PHIEUMUON
                    {
                        IDDocGia = (int)cboDocGia.SelectedValue,
                        NgayMuon = DateTime.Now
                    };

                    context.PHIEUMUON.Add(phieuMuon);
                    await context.SaveChangesAsync();

                    foreach (var bookWithDate in _selectedBooks)
                    {
                        var ctPhieuMuon = new CTPHIEUMUON
                        {
                            IDPhieuMuon = phieuMuon.ID,
                            IDSach = bookWithDate.ID,
                            HanTra = bookWithDate.CustomReturnDate,
                            IDTinhTrangMuon = bookWithDate.Book.IDTinhTrang
                        };
                        context.CTPHIEUMUON.Add(ctPhieuMuon);

                        bookWithDate.Book.IsAvailable = false;
                    }

                    // Cập nhật BCMUONSACH
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

                    await context.SaveChangesAsync();

                    // Cập nhật CTBCMUONSACH
                    var theLoaiGroups = _selectedBooks
                        .SelectMany(b => b.Book.IDTuaSachNavigation.TUASACH_THELOAI.Select(ts_tl => ts_tl.IDTheLoaiNavigation))
                        .GroupBy(tl => tl.ID)
                        .Select(g => new { TheLoaiId = g.Key, Count = g.Count() });

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
                            context.CTBCMUONSACH.Add(ctBcMuonSach);
                        }
                        else
                        {
                            ctBcMuonSach.SoLuotMuon += group.Count;
                        }
                    }

                    await context.SaveChangesAsync();

                    // Cập nhật TiLe cho tất cả các bản ghi CTBCMUONSACH có trong báo cáo
                    var allCtBcMuonSach = await context.CTBCMUONSACH
                        .Where(ct => ct.IDBCMuonSach == bcMuonSach.ID)
                        .ToListAsync();

                    foreach (var ct in allCtBcMuonSach)
                    {
                        ct.TiLe = (float)ct.SoLuotMuon / bcMuonSach.TongSoLuotMuon;
                    }

                    await context.SaveChangesAsync();
                    MessageBox.Show("Thêm phiếu mượn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    XuatPhieuMuon(phieuMuon);
                    Window.GetWindow(this).DialogResult = true;
                    Window.GetWindow(this)?.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu phiếu mượn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XuatPhieuMuon(PHIEUMUON phieuMuon)
        {
            using (var context = new QLTVContext())
            {
                phieuMuon = context.PHIEUMUON
                    .Include(pm => pm.IDDocGiaNavigation)
                        .ThenInclude(dg => dg.IDTaiKhoanNavigation)
                    .Include(pm => pm.CTPHIEUMUON)
                        .ThenInclude(ct => ct.IDSachNavigation)
                            .ThenInclude(s => s.IDTinhTrangNavigation)
                    .Include(pm => pm.CTPHIEUMUON)
                        .ThenInclude(ct => ct.IDSachNavigation)
                            .ThenInclude(s => s.IDTuaSachNavigation)
                    .First(pm => pm.ID == phieuMuon.ID);

                var window = new Window
                {
                    Title = "In phiếu mượn",
                    Content = new UcXuatPhieuMuon(phieuMuon),
                    Width = 350,
                    Height = 600,
                    WindowStyle = WindowStyle.None,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.CanResizeWithGrip
                };
                window.ShowDialog();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this).DialogResult == true)
            {
                return;
            }
            try
            {
                using (var context = new QLTVContext())
                {
                    foreach (var book in _selectedBooks)
                    {
                        context.ChangeTracker.Clear();
                        book.Book.IsAvailable = true;
                        context.Update(book.Book);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hủy bỏ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (!isClosing)
                {
                    Window.GetWindow(this).DialogResult = true;
                    Window.GetWindow(this)?.Close();
                }
            }
        }

        private void cboDocGia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var docGia = (sender as ComboBox).SelectedItem as DOCGIA;
            if (docGia == null) return;
            DocGiaValidation(docGia);
        }

        private bool DocGiaValidation(DOCGIA _docGia)
        {
            if (_docGia == null)
            {
                icDocGiaError.ToolTip = $"Vui lòng chọn 1 độc giả.";
                icDocGiaError.Visibility = Visibility.Visible;
                return false;
            }
            if (_docGia.PHIEUMUON.Sum(pm => pm.CTPHIEUMUON.Count - pm.CTPHIEUTRA.Count) + _selectedBooks.Count >= _docGia.IDLoaiDocGiaNavigation.SoSachMuonToiDa)
            {
                icDocGiaError.ToolTip = $"Độc giả này không thể mượn quá {_docGia.IDLoaiDocGiaNavigation.SoSachMuonToiDa} quyển sách.";
                icDocGiaError.Visibility = Visibility.Visible;
                return false;
            }
            icDocGiaError.Visibility = Visibility.Collapsed;
            return true;
        }

        private async void GenerateBorrowTickets_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var random = new Random();

                // STEP 1: Load readers and available books (AsNoTracking is okay since we're just reading)
                List<DOCGIA> readers;
                List<SACH> availableBooks;
                using (var context = new QLTVContext())
                {
                    readers = await context.DOCGIA
                        .Include(d => d.IDLoaiDocGiaNavigation)
                        .Include(d => d.PHIEUMUON)
                            .ThenInclude(pm => pm.CTPHIEUMUON)
                        .Include(d => d.PHIEUMUON)
                            .ThenInclude(pm => pm.CTPHIEUTRA)
                        .AsNoTracking()
                        .ToListAsync();

                    availableBooks = await context.SACH
                        .Include(s => s.IDTuaSachNavigation)
                        .Where(s => s.IsAvailable == true && !s.IsDeleted)
                        .AsNoTracking()
                        .ToListAsync();
                }

                if (!readers.Any() || !availableBooks.Any())
                {
                    ResultTextBlock.Text = "No readers or books available.";
                    return;
                }

                // STEP 2: Generate Borrow Tickets In Memory
                var borrowTickets = new List<PHIEUMUON>();
                var startDate = new DateTime(2023, 12, 1);
                var endDate = DateTime.Now;

                for (var currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
                {
                    // Skip weekends
                    if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    // Generate 1 to 5 tickets per day
                    int ticketsToday = random.Next(1, 6);
                    for (int i = 0; i < ticketsToday; i++)
                    {
                        var reader = readers[random.Next(readers.Count)];

                        // Calculate how many books currently borrowed but not returned
                        int currentBorrowedBooks = reader.PHIEUMUON.Sum(pm => pm.CTPHIEUMUON.Count - pm.CTPHIEUTRA.Count);
                        int maxBooksAllowed = reader.IDLoaiDocGiaNavigation.SoSachMuonToiDa;

                        if (currentBorrowedBooks >= maxBooksAllowed)
                            continue;

                        int canBorrow = maxBooksAllowed - currentBorrowedBooks;
                        int booksToBorrow = random.Next(1, Math.Min(canBorrow, 3) + 1);

                        var selectedBooks = availableBooks
                            .Where(s => s.IDTinhTrang != 5 && s.IDTinhTrang != 6)
                            .OrderBy(_ => random.Next())
                            .Take(booksToBorrow)
                            .ToList();

                        if (!selectedBooks.Any())
                            continue;

                        var pm = new PHIEUMUON
                        {
                            IDDocGia = reader.ID,
                            NgayMuon = currentDate,
                            CTPHIEUMUON = new List<CTPHIEUMUON>()
                        };

                        foreach (var book in selectedBooks)
                        {
                            int maxLoanWeeks = book.IDTuaSachNavigation.HanMuonToiDa;
                            int loanDurationDays = random.Next(1, maxLoanWeeks + 1) * 7;
                            var ctpm = new CTPHIEUMUON
                            {
                                IDSach = book.ID,
                                HanTra = currentDate.AddDays(loanDurationDays),
                                IDTinhTrangMuon = book.IDTinhTrang
                            };
                            pm.CTPHIEUMUON.Add(ctpm);
                        }

                        borrowTickets.Add(pm);
                    }
                }

                if (!borrowTickets.Any())
                {
                    ResultTextBlock.Text = "No borrow tickets were generated.";
                    return;
                }

                // STEP 3: Insert Borrow Tickets into DB and update book availability
                using (var context = new QLTVContext())
                {
                    context.PHIEUMUON.AddRange(borrowTickets);
                    await context.SaveChangesAsync();

                    // Update book availability
                    foreach (var pm in borrowTickets)
                    {
                        foreach (var ctpm in pm.CTPHIEUMUON)
                        {
                            var bookToUpdate = await context.SACH.FindAsync(ctpm.IDSach);
                            if (bookToUpdate != null)
                            {
                                bookToUpdate.IsAvailable = false;
                            }
                        }
                    }

                    await context.SaveChangesAsync();
                }

                // Keep a list of ticket IDs for re-query
                var borrowTicketIds = borrowTickets.Select(x => x.ID).ToList();

                // STEP 4: Update BCMUONSACH & CTBCMUONSACH
                // Re-query from DB with includes, and sort by NgayMuon to ensure chronological order
                using (var context = new QLTVContext())
                {
                    var borrowTicketsFromDb = await context.PHIEUMUON
                        .Where(pm => borrowTicketIds.Contains(pm.ID))
                        .Include(pm => pm.CTPHIEUMUON)
                            .ThenInclude(ct => ct.IDSachNavigation.IDTuaSachNavigation.TUASACH_THELOAI)
                                .ThenInclude(ts_tl => ts_tl.IDTheLoaiNavigation)
                        .ToListAsync();

                    // Sort by NgayMuon to ensure chronological updates
                    borrowTicketsFromDb = borrowTicketsFromDb.OrderBy(pm => pm.NgayMuon).ToList();

                    foreach (var pm in borrowTicketsFromDb)
                    {
                        var borrowDate = pm.NgayMuon.Date;
                        var bcMuonSach = await context.BCMUONSACH.FirstOrDefaultAsync(bc => bc.Thang == borrowDate);
                        if (bcMuonSach == null)
                        {
                            bcMuonSach = new BCMUONSACH
                            {
                                Thang = borrowDate,
                                TongSoLuotMuon = pm.CTPHIEUMUON.Count
                            };
                            context.BCMUONSACH.Add(bcMuonSach);
                        }
                        else
                        {
                            bcMuonSach.TongSoLuotMuon += pm.CTPHIEUMUON.Count;
                        }

                        await context.SaveChangesAsync();

                        // Group by TheLoai
                        var theLoaiGroups = pm.CTPHIEUMUON
                            .SelectMany(ct => ct.IDSachNavigation.IDTuaSachNavigation.TUASACH_THELOAI
                                .Select(ts_tl => ts_tl.IDTheLoaiNavigation))
                            .GroupBy(tl => tl.ID)
                            .Select(g => new { TheLoaiId = g.Key, Count = g.Count() });

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
                                context.CTBCMUONSACH.Add(ctBcMuonSach);
                            }
                            else
                            {
                                ctBcMuonSach.SoLuotMuon += group.Count;
                            }
                        }

                        await context.SaveChangesAsync();

                        // Update TiLe
                        var allCtBcMuonSach = await context.CTBCMUONSACH
                            .Where(ct => ct.IDBCMuonSach == bcMuonSach.ID)
                            .ToListAsync();
                        foreach (var ct in allCtBcMuonSach)
                        {
                            ct.TiLe = (float)ct.SoLuotMuon / bcMuonSach.TongSoLuotMuon;
                        }

                        await context.SaveChangesAsync();
                    }
                }

                // STEP 5: Generate Return Tickets
                // Instead of processing them immediately, let's store them in memory with their returnDate
                var returnsToProcess = new List<(DateTime returnDate, PHIEUMUON pm, PHIEUTRA phieuTra, List<(CTPHIEUMUON, int daysLate, int tinhTrangTra, decimal tienPhat)>)>();

                using (var context = new QLTVContext())
                {
                    var borrowTicketsForReturn = await context.PHIEUMUON
                        .Where(pm => borrowTicketIds.Contains(pm.ID))
                        .Include(pm => pm.CTPHIEUMUON)
                            .ThenInclude(ct => ct.IDSachNavigation.IDTuaSachNavigation.TUASACH_THELOAI)
                        .Include(pm => pm.CTPHIEUMUON)
                            .ThenInclude(ct => ct.IDTinhTrangMuonNavigation)
                        .ToListAsync();

                    // Prepare returns
                    foreach (var pm in borrowTicketsForReturn)
                    {
                        var returnDate = pm.NgayMuon.AddDays(random.Next(1, 30));
                        if (returnDate > DateTime.Now)
                            continue; // Skip if the return date is in the future

                        var phieuTra = new PHIEUTRA { NgayTra = returnDate };

                        var ctpTraDetails = new List<(CTPHIEUMUON, int daysLate, int tinhTrangTra, decimal tienPhat)>();

                        foreach (var ctPhieuMuon in pm.CTPHIEUMUON)
                        {
                            int tinhTrangTra = Math.Min(ctPhieuMuon.IDTinhTrangMuon + (int)Math.Round(random.NextDouble()), 6);
                            var daysLate = (returnDate - ctPhieuMuon.HanTra).Days;

                            var latestQuyDinh = await context.THAMSO
                                .Where(ts => ts.ThoiGian <= returnDate)
                                .OrderByDescending(ts => ts.ID)
                                .FirstOrDefaultAsync();

                            var MucHuHongBanDau = ctPhieuMuon.IDTinhTrangMuonNavigation.MucHuHong;
                            var tinhTrangTraNav = await context.TINHTRANG.FindAsync(tinhTrangTra);
                            var MucHuHongSauKhiTra = tinhTrangTraNav.MucHuHong;

                            decimal TiLeDenBu = latestQuyDinh?.TiLeDenBu ?? 1.2m;
                            decimal TienPhatTraTre = latestQuyDinh?.TienPhatTraTreMotNgay ?? 5000m;
                            decimal triGia = ctPhieuMuon.IDSachNavigation.TriGia;

                            decimal tienPhat = ((MucHuHongSauKhiTra - MucHuHongBanDau) / 100m) * TiLeDenBu * triGia;
                            if (daysLate > 0)
                            {
                                tienPhat += daysLate * TienPhatTraTre;
                            }

                            ctpTraDetails.Add((ctPhieuMuon, daysLate, tinhTrangTra, tienPhat));
                        }

                        if (ctpTraDetails.Any())
                        {
                            returnsToProcess.Add((returnDate, pm, phieuTra, ctpTraDetails));
                        }
                    }
                }

                // Sort returns by returnDate to ensure chronological update of late return reports
                returnsToProcess = returnsToProcess.OrderBy(r => r.returnDate).ToList();

                // Now insert returns in chronological order
                using (var context = new QLTVContext())
                {
                    foreach (var (returnDate, pm, phieuTra, details) in returnsToProcess)
                    {
                        context.PHIEUTRA.Add(phieuTra);
                        await context.SaveChangesAsync(); // Ensure phieuTra.ID is generated

                        foreach (var (ctPhieuMuon, daysLate, tinhTrangTra, tienPhat) in details)
                        {
                            var ctPhieuTra = new CTPHIEUTRA
                            {
                                IDPhieuTra = phieuTra.ID,
                                IDPhieuMuon = ctPhieuMuon.IDPhieuMuon,
                                IDSach = ctPhieuMuon.IDSach,
                                SoNgayMuon = (returnDate - pm.NgayMuon).Days,
                                IDTinhTrangTra = tinhTrangTra,
                                TienPhat = tienPhat,
                                GhiChu = ""
                            };
                            context.CTPHIEUTRA.Add(ctPhieuTra);

                            // Update book status
                            var bookToUpdate = await context.SACH.FindAsync(ctPhieuMuon.IDSach);
                            if (bookToUpdate != null)
                            {
                                var tinhTrangTraNav = await context.TINHTRANG.FindAsync(tinhTrangTra);
                                bookToUpdate.IsAvailable = (tinhTrangTraNav.MucHuHong < 100);
                                bookToUpdate.IDTinhTrang = tinhTrangTra;
                            }

                            // Update late returns report if needed
                            if (daysLate > 0)
                            {
                                var bcTraTre = await context.BCTRATRE.FirstOrDefaultAsync(bc => bc.Ngay == returnDate.Date);
                                if (bcTraTre == null)
                                {
                                    bcTraTre = new BCTRATRE { Ngay = returnDate.Date };
                                    context.BCTRATRE.Add(bcTraTre);
                                    await context.SaveChangesAsync(); // Ensure bcTraTre.ID is generated
                                }

                                // Check if CTBCTRATRE already exists
                                var existingCtBcTraTre = await context.CTBCTRATRE
                                    .FirstOrDefaultAsync(ct => ct.IDBCTraTre == bcTraTre.ID && ct.IDPhieuTra == phieuTra.ID);

                                if (existingCtBcTraTre == null)
                                {
                                    // No entry found, create new
                                    var ctBcTraTre = new CTBCTRATRE
                                    {
                                        IDBCTraTre = bcTraTre.ID,
                                        IDPhieuTra = phieuTra.ID,
                                        SoNgayTraTre = daysLate
                                    };
                                    context.CTBCTRATRE.Add(ctBcTraTre);
                                }
                                else
                                {
                                    // Entry already exists, update SoNgayTraTre if the new value is greater
                                    if (daysLate > existingCtBcTraTre.SoNgayTraTre)
                                    {
                                        existingCtBcTraTre.SoNgayTraTre = daysLate;
                                    }
                                }

                                await context.SaveChangesAsync();
                            }
                        }

                        await context.SaveChangesAsync();
                    }
                }

                ResultTextBlock.Text = "Sinh Dữ Liệu Thành Công";
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating borrow and return tickets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
using QLTV.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Globalization;
using System.Text;
using System.Windows.Media;

namespace QLTV.UserControls
{
    public class MaxBorrowDaysValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // Explicitly get the text from the TextBox
            string textValue = value.Target is TextBox textBox
                ? textBox.Text
                : value?.ToString();

            if (int.TryParse(textValue, out int customBorrowDays))
            {
                DependencyObject currentObj = value as DependencyObject;
                while (currentObj != null)
                {
                    if (currentObj is DataGridRow parent)
                    {
                        var book = (SACH)parent.Item;
                        if (customBorrowDays > book.IDTuaSachNavigation.HanMuonToiDa * 7)
                        {
                            return new ValidationResult(false, $"Số ngày mượn không được vượt quá {book.IDTuaSachNavigation.HanMuonToiDa * 7}.");
                        }
                        break;
                    }
                    currentObj = VisualTreeHelper.GetParent(currentObj);
                }
            }
            return ValidationResult.ValidResult;
        }
    }

        public partial class UcThemPhieuMuon : UserControl
    {
        private readonly QLTVContext _context;
        private ObservableCollection<SACH> _allBooks;
        private ObservableCollection<BookDisplayItem> dsSach;
        private IEnumerable<BookDisplayItem> filteredBooks;
        private ObservableCollection<BookWithCustomDate> _selectedBooks;
        private CollectionViewSource viewSource;

        private class BookWithCustomDate : INotifyPropertyChanged
        {
            private BookDisplayItem _bookItem;
            private int _customBorrowDays;
            private DateTime _customReturnDate;

            public SACH Book
            {
                get => _bookItem.Book;
                set
                {
                    _bookItem.Book = value;
                    OnPropertyChanged(nameof(Book));
                    OnPropertyChanged(nameof(MaSach));
                    OnPropertyChanged(nameof(IDTuaSachNavigation));
                }
            }

            public BookDisplayItem BookItem
            {
                get => _bookItem;
                set
                {
                    _bookItem = value;
                    OnPropertyChanged(nameof(_bookItem));
                    OnPropertyChanged(nameof(BookDisplayItem.DSTacGia));
                    OnPropertyChanged(nameof(BookDisplayItem.DSTheLoai));
                }
            }

            public string DSTheLoai
            {
                get => _bookItem.DSTheLoai;
            }

            public string DSTacGia
            {
                get => _bookItem.DSTacGia;
            }

            public int CustomBorrowDays
            {
                get => _customBorrowDays;
                set
                {
                    _customBorrowDays = value;
                    UpdateCustomReturnDate();
                    OnPropertyChanged(nameof(CustomBorrowDays));
                }
            }

            public DateTime CustomReturnDate
            {
                get => _customReturnDate;
                set
                {
                    _customReturnDate = value;
                    OnPropertyChanged(nameof(CustomReturnDate));
                }
            }

            public string MaSach => Book.MaSach;
            public TUASACH IDTuaSachNavigation => Book.IDTuaSachNavigation;

            public int ID => Book.ID;

            private void UpdateCustomReturnDate()
            {
                CustomReturnDate = DateTime.Now.AddDays(CustomBorrowDays > 0 ? CustomBorrowDays : Book.IDTuaSachNavigation.HanMuonToiDa);
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public static ObservableCollection<BookWithCustomDate> FromBookWithGenresList(List<SACH> books)
            {
                return new ObservableCollection<BookWithCustomDate>(
                    books.Select(book => new BookWithCustomDate
                    {
                        Book = book,
                        CustomBorrowDays = book.IDTuaSachNavigation.HanMuonToiDa
                    })
                );
            }
        }

        private class BookDisplayItem
        {
            public SACH Book { get; set; }
            public string MaSach { get; set; }
            public string TuaSach { get; set; }
            public string DSTacGia { get; set; }
            public string DSTheLoai { get; set; }
            public int HanMuonToiDa { get; set; }
        }

        public UcThemPhieuMuon()
        {
            InitializeComponent();
            _context = new ();
            _selectedBooks = new ();
            dgSelectedBooks.ItemsSource = _selectedBooks;
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
                _allBooks = new ObservableCollection<SACH>(await _context.SACH
                    .Include(s => s.IDTuaSachNavigation)
                        .ThenInclude(ts => ts.TUASACH_THELOAI)
                            .ThenInclude(ts_tl => ts_tl.IDTheLoaiNavigation)
                    .Include(s => s.IDTuaSachNavigation)
                        .ThenInclude(ts => ts.TUASACH_TACGIA)
                            .ThenInclude(ts_tg => ts_tg.IDTacGiaNavigation)
                    .Where(s => !s.IsDeleted && s.IsAvailable == true)
                    .ToListAsync());

                dsSach = new ObservableCollection<BookDisplayItem>( _allBooks.Select(s => new BookDisplayItem
                {
                    Book = s,
                    MaSach = s.MaSach,
                    TuaSach = s.IDTuaSachNavigation.TenTuaSach,
                    DSTacGia = string.Join(", ", s.IDTuaSachNavigation.TUASACH_TACGIA
                        .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                    DSTheLoai = string.Join(", ", s.IDTuaSachNavigation.TUASACH_THELOAI
                        .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai)),
                    HanMuonToiDa = s.IDTuaSachNavigation.HanMuonToiDa
                }).ToList());

                var docGia = await _context.DOCGIA
                    .Include(d => d.IDTaiKhoanNavigation)
                    .ToListAsync();
                
                dgAvailableBooks.ItemsSource = dsSach;

                viewSource = new CollectionViewSource();
                viewSource.Source = docGia;
                cboDocGia.ItemsSource = viewSource.View;

                cboDocGia.Loaded += (s, e) =>
                {
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
                                var itemText = ConvertToUnsigned(item.ToString());
                                return itemText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                            };
                            cboDocGia.IsDropDownOpen = true;
                        };
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BookSearch(string search)
        {
            if (_allBooks == null) return;

            var searchText = search.Trim().ToLower();
            var searchType = ((ComboBoxItem)cboSearchType.SelectedItem).Content.ToString();

            filteredBooks = dsSach;

            if (!string.IsNullOrEmpty(searchText))
            {
                switch (searchType)
                {
                    case "Mã sách":
                        filteredBooks = filteredBooks.Where(s =>
                            s.Book.MaSach.ToLower().Contains(searchText));
                        break;

                    case "Tên sách":
                        filteredBooks = filteredBooks.Where(s =>
                            s.Book.IDTuaSachNavigation.TenTuaSach.ToLower().Contains(searchText));
                        break;

                    case "Thể loại":
                        filteredBooks = filteredBooks.Where(s =>
                            s.Book.IDTuaSachNavigation.TUASACH_THELOAI
                                .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai.ToLower())
                                .Any(tenTheLoai => tenTheLoai.Contains(searchText)));
                        break;

                    case "Tác giả":
                        filteredBooks = filteredBooks.Where(s =>
                            s.Book.IDTuaSachNavigation.TUASACH_TACGIA
                                .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia.ToLower())
                                .Any(tenTacGia => tenTacGia.Contains(searchText)));
                        break;

                    default: // "Tất cả"
                        filteredBooks = filteredBooks.Where(s =>
                            s.Book.MaSach.ToLower().Contains(searchText) ||
                            s.Book.IDTuaSachNavigation.TenTuaSach.ToLower().Contains(searchText) ||
                            s.Book.IDTuaSachNavigation.TUASACH_THELOAI
                                .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai.ToLower())
                                .Any(tenTheLoai => tenTheLoai.Contains(searchText)) ||
                            s.Book.IDTuaSachNavigation.TUASACH_TACGIA
                                .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia.ToLower())
                                .Any(tenTacGia => tenTacGia.Contains(searchText)));
                        break;
                }
            }

            // Convert filtered books to display format using the concrete type

            dgAvailableBooks.ItemsSource = filteredBooks;
        }

        private void txtSearchBook_TextChanged(object sender, TextChangedEventArgs e)
        {
            BookSearch(txtSearchBook.Text);
        }

        private void btnSelectBook_Click(object sender, RoutedEventArgs e)
        {
            var book = ((Button)sender).DataContext as BookDisplayItem;
            if (book == null) return;

            if (!_selectedBooks.Any(b => b.Book.ID == book.Book.ID))
            {
                var bookWithDate = new BookWithCustomDate 
                { 
                    BookItem = book,
                    CustomBorrowDays = book.Book.IDTuaSachNavigation.HanMuonToiDa
                };
                dsSach.Remove(book);
                _allBooks.Remove(book.Book);
                BookSearch(txtSearchBook.Text);
                _selectedBooks.Add(bookWithDate);
            }
        }

        private void btnRemoveBook_Click(object sender, RoutedEventArgs e)
        {
            var bookWithDate = ((Button)sender).DataContext as BookWithCustomDate;
            if (bookWithDate != null && bookWithDate.Book != null)
            {
                _selectedBooks.Remove(bookWithDate);
                dsSach.Add(bookWithDate.BookItem);
                BookSearch(txtSearchBook.Text);
                _allBooks.Add(bookWithDate.BookItem.Book);
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cboDocGia.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn độc giả", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cboDocGia.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn độc giả", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_selectedBooks.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một cuốn sách", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var phieuMuon = new PHIEUMUON
                {
                    IDDocGia = (int)cboDocGia.SelectedValue,
                    NgayMuon = DateTime.Now,
                    MaPhieuMuon = GenerateNewBorrowCode(),
                    IsPending = true
                };

                _context.PHIEUMUON.Add(phieuMuon);
                await _context.SaveChangesAsync();

                foreach (var bookWithDate in _selectedBooks)
                {
                    var ctPhieuMuon = new CTPHIEUMUON
                    {
                        IDPhieuMuon = phieuMuon.ID,
                        IDSach = bookWithDate.ID,
                        HanTra = bookWithDate.CustomReturnDate,
                        TinhTrangMuon = "Tốt"
                    };
                    _context.CTPHIEUMUON.Add(ctPhieuMuon);

                    bookWithDate.Book.IsAvailable = false;
                }

                // Update BCMUONSACH
                var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var bcMuonSach = await _context.BCMUONSACH
                    .FirstOrDefaultAsync(bc => bc.Thang == currentMonth);

                if (bcMuonSach == null)
                {
                    bcMuonSach = new BCMUONSACH
                    {
                        Thang = currentMonth,
                        TongSoLuotMuon = _selectedBooks.Count
                    };
                    _context.BCMUONSACH.Add(bcMuonSach);
                }
                else
                {
                    bcMuonSach.TongSoLuotMuon += _selectedBooks.Count;
                }

                await _context.SaveChangesAsync();

                // Update CTBCMUONSACH
                var theLoaiGroups = _selectedBooks
                    .SelectMany(b => b.Book.IDTuaSachNavigation.TUASACH_THELOAI.Select(ts_tl => ts_tl.IDTheLoaiNavigation))
                    .GroupBy(tl => tl.ID)
                    .Select(g => new { TheLoaiId = g.Key, Count = g.Count() });

                foreach (var group in theLoaiGroups)
                {
                    var ctBcMuonSach = await _context.CTBCMUONSACH
                        .FirstOrDefaultAsync(ct => ct.IDBCMuonSach == bcMuonSach.ID && ct.IDTheLoai == group.TheLoaiId);

                    if (ctBcMuonSach == null)
                    {
                        ctBcMuonSach = new CTBCMUONSACH
                        {
                            IDBCMuonSach = bcMuonSach.ID,
                            IDTheLoai = group.TheLoaiId,
                            SoLuotMuon = group.Count
                        };
                        _context.CTBCMUONSACH.Add(ctBcMuonSach);
                    }
                    else
                    {
                        ctBcMuonSach.SoLuotMuon += group.Count;
                    }
                }

                await _context.SaveChangesAsync();

                // Update TiLe for all CTBCMUONSACH entries of this report
                var allCtBcMuonSach = await _context.CTBCMUONSACH
                    .Where(ct => ct.IDBCMuonSach == bcMuonSach.ID)
                    .ToListAsync();

                foreach (var ct in allCtBcMuonSach)
                {
                    ct.TiLe = (float)ct.SoLuotMuon / bcMuonSach.TongSoLuotMuon;
                }

                await _context.SaveChangesAsync();
                MessageBox.Show("Thêm phiếu mượn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                Window.GetWindow(this).DialogResult = true;
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu phiếu mượn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateNewBorrowCode()
        {
            var lastCode = _context.PHIEUMUON
                .OrderByDescending(p => p.MaPhieuMuon)
                .Select(p => p.MaPhieuMuon)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(lastCode))
            {
                return "PM0001";
            }

            int number = int.Parse(lastCode.Substring(2)) + 1;
            return $"PM{number:D4}";
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void txtBorrowDays_TextChanged(object sender, TextChangedEventArgs e)
        {
            //UpdateReturnDates();
        }

        private void txtCustomDays_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
} 
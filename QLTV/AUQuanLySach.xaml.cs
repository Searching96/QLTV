using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;
using OfficeOpenXml.Drawing.Style.Coloring;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using System.Globalization;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.IdentityModel.Tokens;
using System.Windows.Controls.Primitives;
using static QLTV.AUQuanLySach;
using System.Windows.Markup;
using System.Security.Policy;

namespace QLTV
{
    /// <summary>
    /// Interaction logic for AUQuanLySach.xaml
    /// </summary>
    public partial class AUQuanLySach : UserControl
    {
        private bool isUpdatingText = false;
        public List<string> lstSelectedMaSach = new List<string>();
        private ObservableCollection<SachViewModel> _dsSach;
        private ObservableCollection<SachViewModel> _fullDataSource;
        private ObservableCollection<SearchCondition> _searchConditions = new ObservableCollection<SearchCondition>();
        private int _currentPage = 1;
        private int _itemsPerPage = 10;
        private int _totalItems = 0;
        private bool _isSearchMode = false;
        private bool _useAns = true;
        private string _searchMode = "AND";

        public class SearchCondition
        {
            public string ConditionText { get; set; }
        }

        public class SachViewModel : INotifyPropertyChanged
        {
            public string MaSach { get; set; }
            public string TuaSach { get; set; }
            public string DSTacGia { get; set; }
            public string DSTheLoai { get; set; }
            public string NhaXuatBan { get; set; }
            public int NamXuatBan { get; set; }
            public string NgayNhap { get; set; }
            public decimal TriGia { get; set; }
            public string TinhTrang { get; set; }
            private bool _isSelected;

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected != value)
                    {
                        _isSelected = value;
                        OnPropertyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ToggleSearchMode_Checked(object sender, RoutedEventArgs e)
        {
            _searchMode = "AND";
            var toggleButton = sender as ToggleButton;
            if (toggleButton != null)
            {
                toggleButton.Content = "AND";
            }
        }

        private void ToggleSearchMode_Unchecked(object sender, RoutedEventArgs e)
        {
            _searchMode = "OR";
            var toggleButton = sender as ToggleButton;
            if (toggleButton != null)
            {
                toggleButton.Content = "OR";
            }
        }

        private void tgbUseAns_Unchecked(object sender, RoutedEventArgs e)
        {
            _useAns = false;
            tgbUseAns.Content = "NoAns";
            if (_searchConditions.Count > 0)
                _searchConditions.RemoveAt(0);
        }

        private void tgbUseAns_Checked(object sender, RoutedEventArgs e)
        {
            _useAns = true;
            tgbUseAns.Content = "UseAns";
            _searchConditions.Insert(0, new SearchCondition { ConditionText = "ANS" });
        }

        // Mở/tắt Popup
        private void btnOpenQuery_Click(object sender, RoutedEventArgs e)
        {
            puAdvancedSearch.IsOpen = !puAdvancedSearch.IsOpen;
        }

        // Xóa điều kiện
        private void DeleteCondition_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var condition = button?.Tag as SearchCondition;

            if (condition != null && _searchConditions.Contains(condition))
            {
                _searchConditions.Remove(condition);
            }
        }

        private void btnAddQuery_Click(object sender, RoutedEventArgs e)
        {
            string conditionText = tbxThongTinTim.Text.Trim();
            string selectedProperty = ((ComboBoxItem)cbbThuocTinhTim.SelectedItem)?.Content.ToString();

            if (string.IsNullOrEmpty(conditionText) || string.IsNullOrEmpty(selectedProperty))
            {
                MessageBox.Show("Vui lòng điền từ khóa và thuộc tính!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newCondition = $"{selectedProperty}: {conditionText}";

            // Add the new condition to the collection
            _searchConditions.Add(new SearchCondition { ConditionText = newCondition });

            // Optionally, clear the TextBox and ComboBox for next input
            tbxThongTinTim.Clear();
        }

        public void PerformAdvanceSearch()
        {
            if (_searchConditions.IsNullOrEmpty())
            {
                MessageBox.Show("Hãy thêm truy vấn", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;  // Exit if there are no search conditions
            }

            using (var context = new QLTVContext())
            {
                IQueryable<SachViewModel> querySource;
                List<SachViewModel> result;
                int startIndex;

                // Choose the query source based on whether we use the in-memory data or the database
                MessageBox.Show($"_useAns value: {_useAns}", "Debug Information");
                if (_useAns)
                {
                    querySource = _fullDataSource.AsQueryable();
                    startIndex = 1;
                }
                else
                {
                    querySource = context.SACH
                        .Where(s => !s.IsDeleted && !s.IDTuaSachNavigation.IsDeleted)
                        .Select(s => new SachViewModel
                        {
                            MaSach = s.MaSach,
                            TuaSach = s.IDTuaSachNavigation.TenTuaSach,
                            DSTacGia = string.Join(", ", s.IDTuaSachNavigation.TUASACH_TACGIA
                                .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                            DSTheLoai = string.Join(", ", s.IDTuaSachNavigation.TUASACH_THELOAI
                                .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai)),
                            NhaXuatBan = s.NhaXuatBan,
                            NamXuatBan = s.NamXuatBan,
                            NgayNhap = s.NgayNhap.ToString("dd/MM/yyyy"),
                            TriGia = s.TriGia,
                            TinhTrang = s.IDTinhTrangNavigation.TenTinhTrang
                        });
                    startIndex = 0;
                }

                // Initialize the result as an empty list to ensure proper materialization before applying any logic
                result = (_searchMode == "AND") ? querySource.ToList() : new List<SachViewModel>();

                // Loop through each search condition
                foreach (var condition in _searchConditions.Skip(startIndex))
                {
                    // Split the condition into property and value
                    var conditionParts = condition.ConditionText.Split(new[] { ": " }, StringSplitOptions.None);
                    if (conditionParts.Length != 2) continue;  // Skip invalid conditions

                    string selectedProperty = conditionParts[0].Trim();
                    string conditionText = conditionParts[1].Trim('\''); // Remove surrounding quotes from the value

                    // Variables for date parsing
                    DateTime? searchDate = null;
                    string comparisonOperator = "=";

                    // Special handling for date searches
                    if (selectedProperty == "Ngày Nhập")
                    {
                        // Check for comparison operators
                        if (conditionText.StartsWith(">=") || conditionText.StartsWith("<=") ||
                            conditionText.StartsWith(">") || conditionText.StartsWith("<") ||
                            conditionText.StartsWith("="))
                        {
                            // Extract comparison operator
                            comparisonOperator = conditionText.Substring(0, 2);
                            if (comparisonOperator != ">=" && comparisonOperator != "<=")
                            {
                                comparisonOperator = conditionText.Substring(0, 1);
                            }

                            // Remove operator from the date string
                            conditionText = conditionText.Substring(comparisonOperator.Length).Trim();
                        }

                        // Thêm nhiều format ngày để parse
                        string[] dateFormats = new[] {
                            "d/M/yyyy",
                            "dd/MM/yyyy",
                            "d/M/yy",
                            "dd/MM/yy",
                            "d-M-yyyy",
                            "dd-MM-yyyy",
                            "d-M-yy",
                            "dd-MM-yy"
                        };

                        if (!DateTime.TryParseExact(conditionText.Trim(), dateFormats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                        {
                            MessageBox.Show("Vui lòng nhập ngày hợp lệ theo các định dạng: dd/MM/yyyy, d/M/yyyy, v.v.",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        searchDate = parsedDate;
                    }

                    // Apply AsEnumerable() before calling NormalizeString for AND conditions
                    var filteredQuery = querySource.AsEnumerable().Where(s =>
                        selectedProperty == "Tựa Sách" ? NormalizeString(s.TuaSach).Contains(NormalizeString(conditionText)) :
                        selectedProperty == "Tác Giả" ? NormalizeString(s.DSTacGia).Contains(NormalizeString(conditionText)) :
                        selectedProperty == "Thể Loại" ? NormalizeString(s.DSTheLoai).Contains(NormalizeString(conditionText)) :
                        selectedProperty == "Nhà Xuất Bản" ? NormalizeString(s.NhaXuatBan).Contains(NormalizeString(conditionText)) :
                        selectedProperty == "Tình Trạng" ? NormalizeString(s.TinhTrang).Contains(NormalizeString(conditionText)) :
                        selectedProperty == "Ngày Nhập" && searchDate.HasValue
                            ? comparisonOperator switch
                            {
                                ">=" => DateTime.ParseExact(s.NgayNhap, "dd/MM/yyyy", null) >= searchDate.Value,
                                "<=" => DateTime.ParseExact(s.NgayNhap, "dd/MM/yyyy", null) <= searchDate.Value,
                                ">" => DateTime.ParseExact(s.NgayNhap, "dd/MM/yyyy", null) > searchDate.Value,
                                "<" => DateTime.ParseExact(s.NgayNhap, "dd/MM/yyyy", null) < searchDate.Value,
                                "=" => DateTime.ParseExact(s.NgayNhap, "dd/MM/yyyy", null) == searchDate.Value,
                                _ => false
                            } :
                        true
                    ).ToList(); // Ensure that the query is evaluated in memory

                    // Combine the results based on search mode (AND/OR)
                    if (_searchMode == "AND")
                    {
                        // Use Intersect only with in-memory query results
                        result = result.Intersect(filteredQuery, new SachViewModelComparer()).ToList();
                    }
                    else
                    {
                        result = result.Concat(filteredQuery).Distinct().ToList();
                    }
                }

                // Convert result back to ObservableCollection to update the UI
                _fullDataSource = new ObservableCollection<SachViewModel>(result);

                // Set the flags for paging and search mode
                _isSearchMode = true;
                _currentPage = 1;
                cbxSelectAll.IsChecked = false;

                // Apply pagination
                ApplyPaging();
            }
        }

        public AUQuanLySach()
        {
            InitializeComponent();
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            _dsSach = new ObservableCollection<SachViewModel>();
            LoadSach(true);
            ictSearchConditions.ItemsSource = _searchConditions;
        }

        private void LoadSach(bool isInitialLoad = false)
        {
            using (var context = new QLTVContext())
            {
                var data = context.SACH
                    .Where(s => !s.IsDeleted && !s.IDTuaSachNavigation.IsDeleted)
                    .Select(s => new SachViewModel
                    {
                        MaSach = s.MaSach,
                        TuaSach = s.IDTuaSachNavigation.TenTuaSach,
                        DSTacGia = string.Join(", ", s.IDTuaSachNavigation.TUASACH_TACGIA
                            .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                        DSTheLoai = string.Join(", ", s.IDTuaSachNavigation.TUASACH_THELOAI
                            .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai)),
                        NhaXuatBan = s.NhaXuatBan,
                        NamXuatBan = s.NamXuatBan,
                        NgayNhap = s.NgayNhap.ToString("dd/MM/yyyy"),
                        TriGia = s.TriGia,
                        TinhTrang = s.IDTinhTrangNavigation.TenTinhTrang
                    })
                    .ToList();

                _fullDataSource = new ObservableCollection<SachViewModel>(data);

                _isSearchMode = false;
                _currentPage = 1;

                cbxSelectAll.IsChecked = false;

                ApplyPaging();
            }
        }

        private void btnAdvanceSearch_Click(object sender, RoutedEventArgs e)
        {
            PerformAdvanceSearch();
        }

        private void PerformSearch()
        {
            string searchTerm = tbxThongTinTim.Text.Trim();
            string selectedProperty = ((ComboBoxItem)cbbThuocTinhTim.SelectedItem)?.Content.ToString();

            if (string.IsNullOrEmpty(selectedProperty))
            {
                MessageBox.Show("Vui lòng chọn thuộc tính tìm kiếm", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime? searchDate = null;
            string comparisonOperator = null;

            // Xử lý tìm kiếm theo Ngày Nhập với điều kiện
            if (selectedProperty == "Ngày Nhập")
            {
                // Kiểm tra cú pháp so sánh (e.g., ">= 1/1/2024", "<= 31/12/2023")
                if (searchTerm.StartsWith(">=") || searchTerm.StartsWith("<=") || searchTerm.StartsWith(">") || searchTerm.StartsWith("<") || searchTerm.StartsWith("="))
                {
                    comparisonOperator = searchTerm.Split(' ')[0]; // Lấy toán tử so sánh
                    searchTerm = searchTerm.Substring(comparisonOperator.Length).Trim(); // Loại bỏ toán tử khỏi chuỗi
                }

                // Parse chuỗi thành DateTime
                if (!DateTime.TryParseExact(searchTerm, "d/M/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    MessageBox.Show("Vui lòng nhập ngày hợp lệ theo định dạng dd/MM/yyyy hoặc sử dụng so sánh như '>= 1/1/2024'", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                searchDate = parsedDate;
            }

            using (var context = new QLTVContext())
            {
                var data = context.SACH
                    .Where(s => !s.IsDeleted && !s.IDTuaSachNavigation.IsDeleted)
                    .Select(s => new SachViewModel
                    {
                        MaSach = s.MaSach,
                        TuaSach = s.IDTuaSachNavigation.TenTuaSach,
                        DSTacGia = string.Join(", ", s.IDTuaSachNavigation.TUASACH_TACGIA
                            .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                        DSTheLoai = string.Join(", ", s.IDTuaSachNavigation.TUASACH_THELOAI
                            .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai)),
                        NhaXuatBan = s.NhaXuatBan,
                        NamXuatBan = s.NamXuatBan,
                        NgayNhap = s.NgayNhap.ToString("dd/MM/yyyy"),
                        TriGia = s.TriGia,
                        TinhTrang = s.IDTinhTrangNavigation.TenTinhTrang
                    })
                    .AsEnumerable()
                    .Where(s =>
                        selectedProperty == "Tựa Sách" ? NormalizeString(s.TuaSach).Contains(NormalizeString(searchTerm)) :
                        selectedProperty == "Tác Giả" ? NormalizeString(s.DSTacGia).Contains(NormalizeString(searchTerm)) :
                        selectedProperty == "Thể Loại" ? NormalizeString(s.DSTheLoai).Contains(NormalizeString(searchTerm)) :
                        selectedProperty == "Nhà Xuất Bản" ? NormalizeString(s.NhaXuatBan).Contains(NormalizeString(searchTerm)) :
                        selectedProperty == "Tình Trạng" ? NormalizeString(s.TinhTrang).Contains(NormalizeString(searchTerm)) :
                        selectedProperty == "Ngày Nhập" && searchDate.HasValue
                            ? comparisonOperator switch
                            {
                                ">=" => DateTime.ParseExact(s.NgayNhap, "dd/MM/yyyy", null) >= searchDate.Value,
                                "<=" => DateTime.ParseExact(s.NgayNhap, "dd/MM/yyyy", null) <= searchDate.Value,
                                ">" => DateTime.ParseExact(s.NgayNhap, "dd/MM/yyyy", null) > searchDate.Value,
                                "<" => DateTime.ParseExact(s.NgayNhap, "dd/MM/yyyy", null) < searchDate.Value,
                                "=" => DateTime.ParseExact(s.NgayNhap, "dd/MM/yyyy", null) == searchDate.Value,
                                _ => false
                            } :
                        true
                    )
                    .ToList();

                _fullDataSource = new ObservableCollection<SachViewModel>(data);

                _isSearchMode = true;
                _currentPage = 1;

                cbxSelectAll.IsChecked = false;

                ApplyPaging();
            }
        }

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void ApplyPaging()
        {
            if (_fullDataSource == null || _fullDataSource.Count == 0)
            {
                dgSach.ItemsSource = new ObservableCollection<SachViewModel>();
                UpdatePageInfo(0);
                return;
            }

            int totalItems = _fullDataSource.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / _itemsPerPage);

            var pageData = _fullDataSource
                .Skip((_currentPage - 1) * _itemsPerPage)
                .Take(_itemsPerPage)
                .ToList();

            _dsSach.Clear();
            foreach (var item in pageData)
            {
                _dsSach.Add(item);
            }

            dgSach.ItemsSource = _dsSach;
            UpdatePageInfo(totalItems);
        }

        private void UpdatePageInfo(int totalItems)
        {
            int totalPages = (int)Math.Ceiling((double)totalItems / _itemsPerPage);
            var tbxPageNumber = (TextBlock)FindName("tbxPageNumber");

            if (tbxPageNumber != null)
            {
                tbxPageNumber.Text = totalItems > 0
                    ? $"Trang {_currentPage}/{totalPages} (Tổng: {totalItems} kết quả)"
                    : "Không có kết quả";
            }

            var btnPrevious = (Button)FindName("btnPrevious");
            var btnNext = (Button)FindName("btnNext");

            if (btnPrevious != null) btnPrevious.IsEnabled = _currentPage > 1;
            if (btnNext != null) btnNext.IsEnabled = _currentPage < totalPages;
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                ApplyPaging();
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_fullDataSource.Count / _itemsPerPage);
            if (_currentPage < totalPages)
            {
                _currentPage++;
                ApplyPaging();
            }
        }

        private DataGridRow GetRow(CheckBox cbx)
        {
            var dgRow = VisualTreeHelper.GetParent(cbx) as FrameworkElement;
            while (dgRow != null && !(dgRow is DataGridRow))
                dgRow = VisualTreeHelper.GetParent(dgRow) as FrameworkElement;
            return dgRow as DataGridRow;
        }

        private CheckBox GetCheckBox(FrameworkElement element)
        {
            if (element == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
                if (child is CheckBox checkBox)
                {
                    return checkBox; // Tìm thấy CheckBox
                }
                else
                {
                    var result = GetCheckBox(child); // Đệ quy tìm sâu hơn
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        private void cbxSelectRow_Checked(object sender, RoutedEventArgs e)
        {
            var cbx = sender as CheckBox;
            var row = GetRow(cbx);
            if (row != null)
            {
                var sach = row.Item as SachViewModel;
                sach.IsSelected = true;
            }
        }

        private void cbxSelectRow_Unchecked(object sender, RoutedEventArgs e)
        {
            var cbx = sender as CheckBox;
            var row = GetRow(cbx);
            if (row != null)
            {
                var sach = row.Item as SachViewModel;
                sach.IsSelected = false;
            }
        }

        private void cbxSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            var lstSach = _fullDataSource;
            MessageBox.Show(lstSach.Count.ToString());
            if (lstSach != null)
            {
                foreach (var sach in lstSach)
                {
                    sach.IsSelected = true;
                }
            }
        }

        private void cbxSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            var lstSach = _fullDataSource;
            if (lstSach != null)
            {
                foreach (var sach in lstSach)
                {
                    sach.IsSelected = false;
                }
            }
        }

        private void dgSach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = dgSach.SelectedItem;

            if (selectedItem != null)
            {
                dynamic selectedBook = selectedItem;
                tbxMaSach.Text = selectedBook.MaSach;
                tbxTuaSach.Text = selectedBook.TuaSach;
                tbxDSTacGia.Text = selectedBook.DSTacGia;
                tbxDSTheLoai.Text = selectedBook.DSTheLoai;
                tbxNhaXuatBan.Text = selectedBook.NhaXuatBan;
                tbxNamXuatBan.Text = selectedBook.NamXuatBan.ToString();

                DateTime parsedDate;
                bool isDateParsed = DateTime.TryParseExact(
                    selectedBook.NgayNhap,
                    "dd/MM/yyyy",  // Định dạng dmy mong muốn
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out parsedDate
                );

                if (isDateParsed)
                {
                    dpNgayNhap.SelectedDate = parsedDate;  // Đồng bộ ngày đã chọn
                    dpNgayNhap.DisplayDate = parsedDate;   // Đặt DisplayDate chính xác
                    dpNgayNhap.Text = parsedDate.ToString("dd/MM/yyyy");  // Đảm bảo Text theo định dạng mong muốn
                }

                tbxTriGia.Text = selectedBook.TriGia.ToString();
                cbbTinhTrang.Text = selectedBook.TinhTrang;

                using (var context = new QLTVContext())
                {
                    // Truyền giá trị TinhTrang vào dưới dạng string
                    string tenTinhTrang = selectedBook.TinhTrang;

                    // Kiểm tra nếu TinhTrang không phải là null hoặc rỗng
                    if (!string.IsNullOrEmpty(tenTinhTrang))
                    {
                        var idTinhTrang = context.TINHTRANG
                            .Where(tt => tt.TenTinhTrang == tenTinhTrang)
                            .Select(tt => tt.ID)
                            .FirstOrDefault();

                        if (idTinhTrang > 0)
                        {
                            // Lưu lại giá trị đã chọn trước khi thay đổi ItemsSource
                            var selectedTinhTrang = cbbTinhTrang.SelectedItem;

                            // Gán ItemsSource cho ComboBox với các tình trạng có ID >= idTinhTrang
                            var lstTinhTrang = context.TINHTRANG
                                .Where(tt => tt.ID >= idTinhTrang)
                                .ToList();

                            cbbTinhTrang.ItemsSource = lstTinhTrang;
                            cbbTinhTrang.DisplayMemberPath = "TenTinhTrang"; // Thiết lập thuộc tính cần hiển thị

                            // Cập nhật lại SelectedItem sau khi thay đổi ItemsSource
                            if (selectedTinhTrang != null)
                            {
                                // Tìm kiếm item đã chọn trong danh sách mới và gán lại
                                var selectedItemInList = lstTinhTrang
                                    .FirstOrDefault(tt => tt.TenTinhTrang == ((TINHTRANG)selectedTinhTrang).TenTinhTrang);

                                cbbTinhTrang.SelectedItem = selectedItemInList;
                            }
                            else
                            {
                                // Nếu không có item đã chọn, chọn mặc định (ví dụ, giá trị đầu tiên)
                                cbbTinhTrang.SelectedIndex = 0;
                            }
                        }
                    }

                    // Xử lý truyền book cover image
                    string tenTuaSach = selectedBook.TuaSach;
                    var tuaSach = context.TUASACH
                        .Where(ts => ts.TenTuaSach == tenTuaSach)
                        .FirstOrDefault();

                    try
                    {
                        Uri uri = new Uri(tuaSach.BiaSach, UriKind.Absolute);

                        if (Uri.IsWellFormedUriString(uri.ToString(), UriKind.Absolute))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = uri;
                            bitmap.EndInit();

                            imgBiaSach.Source = bitmap;
                            bdBiaSach.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            imgBiaSach.Source = null; // Nếu Uri không hợp lệ, không gán hình ảnh
                        }
                    }
                    catch (UriFormatException)
                    {
                        imgBiaSach.Source = null; // Nếu có lỗi trong việc tạo Uri, không gán hình ảnh
                    }
                }
            }
            else
            {
                tbxMaSach.Text = "";
                tbxTuaSach.Text = "";
                tbxDSTacGia.Text = "";
                tbxDSTheLoai.Text = "";
                tbxNhaXuatBan.Text = "";
                tbxNamXuatBan.Text = "";
                dpNgayNhap.Text = "";
                tbxTriGia.Text = "";
                cbbTinhTrang.Text = "";
            }
        }

        private void btnThemSach_Click(object sender, RoutedEventArgs e)
        {
            AWThemSach awThemSach = new AWThemSach();
            if (awThemSach.ShowDialog() == true)
                LoadSach();
        }

        public bool HasError()
        {
            // Tìm tất cả các PackIcon trong UserControl
            foreach (var icon in FindVisualChildren<PackIcon>(this))
            {
                if (icon.Style == FindResource("ErrorIcon") && icon.Visibility == Visibility.Visible)
                {
                    return true;
                }
            }
            return false;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T t)
                    {
                        yield return t;
                    }
                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void btnSuaSach_Click(object sender, RoutedEventArgs e)
        {
            if (dgSach.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn sách cần sửa!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (HasError())
            {
                MessageBox.Show("Tất cả thuộc tính phải hợp lệ trước khi sửa!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new QLTVContext())
            {
                // Lấy MaTuaSach từ item được chọn
                dynamic selectedItem = dgSach.SelectedItem;
                string maSach = selectedItem.MaSach;

                // Tìm tựa sách cần sửa
                var sachToUpdate = context.SACH
                    .FirstOrDefault(s => s.MaSach == maSach);

                if (sachToUpdate != null)
                {
                    // Cập nhật thông tin cơ bản
                    sachToUpdate.NhaXuatBan = tbxNhaXuatBan.Text;
                    sachToUpdate.NamXuatBan = int.Parse(tbxNamXuatBan.Text);
                    sachToUpdate.NgayNhap = DateTime.ParseExact(
                        dpNgayNhap.Text,
                        "dd/MM/yyyy",  // Định dạng ngày dmy
                        System.Globalization.CultureInfo.InvariantCulture
                    ); 
                    sachToUpdate.TriGia = decimal.Parse(tbxTriGia.Text);

                    // Lưu tất cả thay đổi
                    context.SaveChanges();

                    MessageBox.Show("Cập nhật tựa sách thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh lại DataGrid
                    LoadSach();
                }
            }
        }

        private void btnXoaSach_Click(object sender, RoutedEventArgs e)
        {
            if (dgSach.SelectedItem == null)
            {
                // Kiểm tra xem có dòng nào được chọn không
                MessageBox.Show("Vui lòng chọn sách cần xóa!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selectedItem = dgSach.SelectedItem;
            string maSach = selectedItem.MaSach;

            MessageBoxResult mbrXacNhan = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa sách có mã: {maSach}?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (mbrXacNhan == MessageBoxResult.Yes)
            {
                using (var context = new QLTVContext())
                {
                    var sachToDelete = context.SACH
                        .FirstOrDefault(s => s.MaSach == maSach);

                    // Truong hop bat dong bo?
                    if (sachToDelete != null)
                    {
                        sachToDelete.IsDeleted = true;
                        var tuaSach = context.TUASACH
                            .FirstOrDefault(ts => ts.ID == sachToDelete.IDTuaSach);
                        if (tuaSach != null)
                            tuaSach.SoLuong--;
                        context.SaveChanges();
                        MessageBox.Show($"Sách có mã {maSach} đã được xóa.", "Thông báo",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadSach();
                    }
                }
            }
        }

        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            icNhaXuatBanError.Visibility = Visibility.Collapsed;
            icNamXuatBanError.Visibility = Visibility.Collapsed;
            icNgayNhapError.Visibility = Visibility.Collapsed;
            icTriGiaError.Visibility = Visibility.Collapsed;
            LoadSach();
        }

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportDataGridToExcel();
        }

        private void ExportDataGridToExcel()
        {
            // Cấu hình đường dẫn lưu file Excel
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Lưu file Excel",
                FileName = "DanhSachSach.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var filePath = saveFileDialog.FileName;

                // Tạo file Excel mới
                using (ExcelPackage package = new ExcelPackage())
                {
                    // Tạo một sheet mới
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Danh Sách Sách");

                    // Đặt tiêu đề cho các cột trong Excel
                    worksheet.Cells[1, 1].Value = "Mã Sách";
                    worksheet.Cells[1, 2].Value = "Tựa Sách";
                    worksheet.Cells[1, 3].Value = "Tác Giả";
                    worksheet.Cells[1, 4].Value = "Thể Loại";
                    worksheet.Cells[1, 5].Value = "Nhà Xuất Bản";
                    worksheet.Cells[1, 6].Value = "Năm Xuất Bản";
                    worksheet.Cells[1, 7].Value = "Ngày Nhập";
                    worksheet.Cells[1, 8].Value = "Trị Giá";
                    worksheet.Cells[1, 9].Value = "Tình Trạng";

                    // Áp dụng style cho tiêu đề
                    using (var range = worksheet.Cells[1, 1, 1, 9])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Duyệt qua dữ liệu trong DataGrid và ghi vào Excel
                    
                    var items = _fullDataSource as System.Collections.IEnumerable;
                    int rowIndex = 2;

                    foreach (var item in items)
                    {
                        dynamic data = item;
                        worksheet.Cells[rowIndex, 1].Value = data.MaSach;
                        worksheet.Cells[rowIndex, 2].Value = data.TuaSach;
                        worksheet.Cells[rowIndex, 3].Value = data.DSTacGia;
                        worksheet.Cells[rowIndex, 4].Value = data.DSTheLoai;
                        worksheet.Cells[rowIndex, 5].Value = data.NhaXuatBan;
                        worksheet.Cells[rowIndex, 6].Value = data.NamXuatBan;
                        worksheet.Cells[rowIndex, 7].Value = data.NgayNhap;
                        worksheet.Cells[rowIndex, 8].Value = data.TriGia;
                        worksheet.Cells[rowIndex, 9].Value = data.TinhTrang;
                        rowIndex++;
                    }

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Cells.AutoFitColumns();

                    // Lưu file Excel
                    FileInfo excelFile = new FileInfo(filePath);
                    package.SaveAs(excelFile);
                }

                MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private int ImportExcelToDb(string filePath)
        {
            int thanhCong = 0;
            HashSet<int> lstDongBiLoi = new HashSet<int>();
            var context = new QLTVContext();

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null) return -1;

                        // Bước 1: Kiểm tra và đánh dấu các dòng lỗi
                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            string triGia = worksheet.Cells[row, 7].Text;
                            string tenTinhTrang = worksheet.Cells[row, 8].Text;

                            var tinhTrang = context.TINHTRANG
                                .FirstOrDefault(tt => !tt.IsDeleted && tt.TenTinhTrang == tenTinhTrang);

                            if (tinhTrang == null || !decimal.TryParse(triGia, out decimal _))
                                lstDongBiLoi.Add(row);
                        }

                        // Hiển thị hộp thoại xác nhận nếu có dòng bị lỗi
                        MessageBoxResult mbrXacNhan = MessageBox.Show(
                            $"Có {lstDongBiLoi.Count} dòng bị lỗi. Bạn có muốn tiếp tục nhập dữ liệu bỏ qua các dòng đó không?",
                            "Xác nhận nhập",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (mbrXacNhan != MessageBoxResult.Yes)
                            return -1;

                        // Bước 2: Nhập dữ liệu từng dòng
                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            if (lstDongBiLoi.Contains(row)) continue;

                            // Trích xuất thông tin từ Excel
                            string tenTuaSach = worksheet.Cells[row, 1].Text;
                            string dsTacGia = worksheet.Cells[row, 2].Text;
                            string dsTheLoai = worksheet.Cells[row, 3].Text;
                            string nhaXuatBan = worksheet.Cells[row, 4].Text;
                            int namXuatBan = int.Parse(worksheet.Cells[row, 5].Text);
                            DateTime ngayNhap = DateTime.ParseExact(worksheet.Cells[row, 6].Text, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            decimal triGia = decimal.Parse(worksheet.Cells[row, 7].Text);
                            string tenTinhTrang = worksheet.Cells[row, 8].Text;

                            // Tìm tình trạng
                            var tinhTrang = context.TINHTRANG
                                .FirstOrDefault(tt => !tt.IsDeleted && tt.TenTinhTrang == tenTinhTrang);

                            // Xử lý danh sách tác giả và thể loại
                            var lstTenTacGia = dsTacGia.Split(", ").Select(n => n.Trim()).OrderBy(n => n).ToList();
                            var lstTenTheLoai = dsTheLoai.Split(", ").Select(n => n.Trim()).OrderBy(n => n).ToList();

                            // Tìm TUASACH hiện có
                            var existingTuaSach = context.TUASACH
                                .Include(ts => ts.TUASACH_TACGIA)
                                    .ThenInclude(ts_tg => ts_tg.IDTacGiaNavigation)
                                .Include(ts => ts.TUASACH_THELOAI)
                                    .ThenInclude(ts_tl => ts_tl.IDTheLoaiNavigation)
                                .Where(ts => !ts.IsDeleted && ts.TenTuaSach == tenTuaSach)
                                .AsEnumerable()
                                .FirstOrDefault(ts =>
                                    ts.TUASACH_TACGIA
                                        .Select(ts_tg => ts_tg.IDTacGiaNavigation?.TenTacGia)
                                        .Where(name => name != null)
                                        .OrderBy(t => t)
                                        .SequenceEqual(lstTenTacGia) &&
                                    ts.TUASACH_THELOAI
                                        .Select(ts_tl => ts_tl.IDTheLoaiNavigation?.TenTheLoai)
                                        .Where(name => name != null)
                                        .OrderBy(t => t)
                                        .SequenceEqual(lstTenTheLoai)
                                );

                            // Nếu không tìm thấy TUASACH phù hợp, tạo mới
                            TUASACH tuaSach = existingTuaSach ?? new TUASACH { TenTuaSach = tenTuaSach };
                            if (existingTuaSach == null)
                            {
                                context.TUASACH.Add(tuaSach);
                                context.SaveChanges();

                                // Thêm tác giả
                                foreach (var tenTacGia in lstTenTacGia)
                                {
                                    var tacGia = context.TACGIA.FirstOrDefault(tg => !tg.IsDeleted && tg.TenTacGia == tenTacGia) ??
                                        new TACGIA { TenTacGia = tenTacGia, NamSinh = -1, QuocTich = "Chưa Có" };

                                    if (tacGia.ID == 0)
                                    {
                                        context.TACGIA.Add(tacGia);
                                        context.SaveChanges();
                                    }
                                    context.TUASACH_TACGIA.Add(new TUASACH_TACGIA { IDTuaSach = tuaSach.ID, IDTacGia = tacGia.ID });
                                }

                                // Thêm thể loại
                                foreach (var tenTheLoai in lstTenTheLoai)
                                {
                                    var theLoai = context.THELOAI.FirstOrDefault(tl => !tl.IsDeleted && tl.TenTheLoai == tenTheLoai) ??
                                        new THELOAI { TenTheLoai = tenTheLoai, MoTa = "" };

                                    if (theLoai.ID == 0)
                                    {
                                        context.THELOAI.Add(theLoai);
                                        context.SaveChanges();
                                    }
                                    context.TUASACH_THELOAI.Add(new TUASACH_THELOAI { IDTuaSach = tuaSach.ID, IDTheLoai = theLoai.ID });
                                }
                                context.SaveChanges();
                            }

                            // Tạo mới SACH
                            var newSach = new SACH
                            {
                                IDTuaSach = tuaSach.ID,
                                NhaXuatBan = nhaXuatBan,
                                NamXuatBan = namXuatBan,
                                NgayNhap = ngayNhap,
                                TriGia = triGia,
                                IDTinhTrang = tinhTrang.ID
                            };
                            context.SACH.Add(newSach);
                            context.SaveChanges();
                            thanhCong++;
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return -1;
                }
            }

            return thanhCong;
        }

        private void btnImportExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                int thanhCong = ImportExcelToDb(openFileDialog.FileName);
                if (thanhCong != -1)
                {
                    MessageBox.Show($"Nhập thành công {thanhCong} dòng từ Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSach();
                }
            }
        }

        private void dpNgayNhap_Loaded(object sender, RoutedEventArgs e)
        {

            // Tìm TextBox bên trong DatePicker
            var textBox = (dpNgayNhap.Template.FindName("PART_TextBox", dpNgayNhap) as TextBox);
            if (textBox != null)
            {
                textBox.TextChanged += TextBox_TextChanged;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgSach.SelectedItem == null)
                return;

            var textBox = sender as TextBox;

            // Danh sách định dạng hỗ trợ nhiều cách nhập ngày
            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy" };
            if (!DateTime.TryParseExact(textBox.Text, formats, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out DateTime ngayNhap))
            {
                icNgayNhapError.ToolTip = "Ngày Nhập không hợp lệ (định dạng đúng: dd/MM/yyyy)";
                icNgayNhapError.Visibility = Visibility.Visible;
                return;
            }

            // Kiểm tra giới hạn ngày từ 1/1/2000 đến hiện tại
            DateTime minDate = new DateTime(2000, 1, 1);
            if (ngayNhap < minDate)
            {
                icNgayNhapError.ToolTip = "Ngày Nhập không được trước ngày 1/1/2000";
                icNgayNhapError.Visibility = Visibility.Visible;
                return;
            }

            if (ngayNhap > DateTime.Now)
            {
                icNgayNhapError.ToolTip = "Ngày Nhập không được sau ngày hiện tại";
                icNgayNhapError.Visibility = Visibility.Visible;
                return;
            }

            // Nếu hợp lệ, ẩn thông báo lỗi
            icNgayNhapError.Visibility = Visibility.Collapsed;
        }

        private void tbxNhaXuatBan_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgSach.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxNhaXuatBan.Text))
            {
                icNhaXuatBanError.ToolTip = "Nhà Xuất Bản không được để trống";
                icNhaXuatBanError.Visibility = Visibility.Visible;
                return;
            }

            foreach (char c in tbxNhaXuatBan.Text)
            {
                if (!char.IsLetter(c) && !char.IsDigit(c) && !char.IsWhiteSpace(c))
                {
                    icNhaXuatBanError.ToolTip = "Nhà Xuất Bản không được có kí tự đặc biệt";
                    icNhaXuatBanError.Visibility = Visibility.Visible;
                    return;
                }
            }

            icNhaXuatBanError.Visibility = Visibility.Collapsed;
        }

        private void tbxNamXuatBan_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgSach.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxNamXuatBan.Text))
            {
                icNamXuatBanError.ToolTip = "Năm Xuất Bản không được để trống";
                icNamXuatBanError.Visibility = Visibility.Visible;
                return;
            }

            if (!int.TryParse(tbxNamXuatBan.Text, out int nxb))
            {
                icNamXuatBanError.ToolTip = "Năm Xuất Bản phải là số nguyên dương";
                icNamXuatBanError.Visibility = Visibility.Visible;
                return;
            }

            if (nxb <= 1900 || nxb > DateTime.Now.Year)
            {
                icNamXuatBanError.ToolTip = "Năm Xuất Bản phải sau 1900 và không quá năm hiện tại";
                icNamXuatBanError.Visibility = Visibility.Visible;
                return;
            }

            icNamXuatBanError.Visibility = Visibility.Collapsed;
        }

        private void tbxTriGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgSach.SelectedItem == null)
                return;

            if (string.IsNullOrWhiteSpace(tbxTriGia.Text))
            {
                icTriGiaError.ToolTip = "Trị Giá không được để trống";
                icTriGiaError.Visibility = Visibility.Visible;
                return;
            }

            if (!int.TryParse(tbxTriGia.Text, out int tg) || tg < 5000 || tg > 10000000)
            {
                icTriGiaError.ToolTip = "Trị Giá phải là lớn hơn 5 ngàn và nhỏ hơn 10 triệu";
                icTriGiaError.Visibility = Visibility.Visible;
                return;
            }

            icTriGiaError.Visibility = Visibility.Collapsed;
        }

        private string NormalizeString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return new string(
                text.Normalize(NormalizationForm.FormD)
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    .ToArray()
            ).Normalize(NormalizationForm.FormC).ToLower();
        }

        private void btnXoaChon_Click(object sender, RoutedEventArgs e)
        {
            lstSelectedMaSach = _fullDataSource
                .Where(s => s.IsSelected)
                .Select(s => s.MaSach)
                .ToList();

            if (lstSelectedMaSach.IsNullOrEmpty())
            {
                MessageBox.Show("Chưa chọn sách để xóa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult mbrXacNhan = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa {lstSelectedMaSach.Count} sách đã chọn?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (mbrXacNhan == MessageBoxResult.Yes)
            {
                using (var context = new QLTVContext())
                {
                    using (var transaction = context.Database.BeginTransaction()) // Đảm bảo đồng bộ các thao tác
                    {
                        try
                        {
                            foreach (var maSach in lstSelectedMaSach)
                            {
                                var sachToDelete = context.SACH
                                    .FirstOrDefault(s => s.MaSach== maSach);

                                if (sachToDelete != null)
                                {
                                    sachToDelete.IsDeleted = true;

                                    var tuaSach = context.TUASACH
                                        .FirstOrDefault(ts => ts.ID == sachToDelete.IDTuaSach);
                                    if (tuaSach != null)
                                        tuaSach.SoLuong--;
                                }
                            }

                            // Lưu tất cả thay đổi trong một giao dịch
                            context.SaveChanges();
                            transaction.Commit(); // Commit giao dịch

                            MessageBox.Show($"Đã xóa thành công các sách được chọn.", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                            LoadSach();
                            tbxThongTinTim.Text = "";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback(); // Rollback giao dịch nếu có lỗi
                            MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        public class SachViewModelComparer : IEqualityComparer<SachViewModel>
        {
            public bool Equals(SachViewModel x, SachViewModel y)
            {
                if (x == null || y == null)
                    return false;

                return x.MaSach == y.MaSach;  // or any other unique identifier
            }

            public int GetHashCode(SachViewModel obj)
            {
                if (obj == null)
                    return 0;

                return obj.MaSach.GetHashCode();  // or any other unique identifier
            }
        }

        private void btnNhapSua_Click(object sender, RoutedEventArgs e)
        {
            puSuaHangLoat.IsOpen = !puSuaHangLoat.IsOpen;
        }

        private void btnSuaNhieu_Click(object sender, RoutedEventArgs e)
        {
            // Danh sách các trạng thái tình trạng hợp lệ
            string[] tinhTrangHopLe = {
                "Mới", "Hỏng nhẹ", "Hỏng vưa", "Hỏng nặng", "Hỏng hoàn toàn", "Mất"
            };

            List<string> lstMaSachToEdit = _fullDataSource
                .Where(s => s.IsSelected)
                .Select(s => s.MaSach)
                .ToList();

            string selectedProperty = ((ComboBoxItem)cbbThuocTinhSua.SelectedItem)?.Content.ToString();
            string editValue = tbxGiaTriSua.Text.Trim();

            using (var context = new QLTVContext())
            {
                foreach (var maSach in lstMaSachToEdit)
                {
                    var sachToEdit = context.SACH
                        .Where(s => s.MaSach == maSach)
                        .FirstOrDefault();

                    if (sachToEdit == null) continue;

                    switch (selectedProperty)
                    {
                        case "Nhà Xuất Bản":
                            sachToEdit.NhaXuatBan = editValue;
                            break;

                        case "Trị Giá":
                            // Kiểm tra giá trị là số
                            if (decimal.TryParse(editValue, out decimal triGia))
                            {
                                sachToEdit.TriGia = triGia;
                            }
                            else
                            {
                                MessageBox.Show($"Giá trị Trị Giá không hợp lệ",
                                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                            break;

                        case "Ngày Nhập":
                            // Kiểm tra định dạng ngày
                            if (DateTime.TryParseExact(editValue,
                                new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy" },
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out DateTime ngayNhap))
                            {
                                sachToEdit.NgayNhap = ngayNhap;
                            }
                            else
                            {
                                MessageBox.Show($"Ngày Nhập không hợp lệ. Sử dụng định dạng dd/MM/yyyy",
                                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                            break;

                        case "Tình Trạng":
                            // Kiểm tra giá trị Tình Trạng có hợp lệ không
                            if (tinhTrangHopLe.Contains(editValue))
                            {
                                // Tìm ID của trạng thái
                                var tinhTrang = context.TINHTRANG
                                    .FirstOrDefault(tt => tt.TenTinhTrang == editValue);

                                if (tinhTrang != null)
                                {
                                    sachToEdit.IDTinhTrang = tinhTrang.ID;
                                }
                                else
                                {
                                    MessageBox.Show($"Không tìm thấy trạng thái {editValue} trong hệ thống",
                                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }
                            }
                            else
                            {
                                MessageBox.Show($"Trạng thái '{editValue}' không hợp lệ. Chọn một trong các giá trị: " +
                                    string.Join(", ", tinhTrangHopLe),
                                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                            break;
                    }
                }

                // Lưu các thay đổi
                try
                {
                    context.SaveChanges();
                    LoadSach(); // Tải lại dữ liệu
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu thay đổi: {ex.Message}",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnNhapSua_LayoutUpdated(object sender, EventArgs e)
        {
            if (puSuaHangLoat.IsOpen)
            {
                puSuaHangLoat.HorizontalOffset += 1;
                puSuaHangLoat.HorizontalOffset -= 1;
            }
        }

        private void btnOpenQuery_LayoutUpdated(object sender, EventArgs e)
        {
            if (puAdvancedSearch.IsOpen)
            {
                // Force the Popup to reposition
                puAdvancedSearch.HorizontalOffset += 1; // Temporarily change the offset
                puAdvancedSearch.HorizontalOffset -= 1; // Restore the offset
            }
        }
    }
}

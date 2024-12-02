using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QLTV.Models;
using System.IO;
using System.Windows.Media;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System.Windows.Documents;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace QLTV
{
    public partial class UCReaderManagement : Window
    {
        private QLTVContext _context = new QLTVContext();
        public ObservableCollection<DOCGIA> Readers { get; set; }
        private ObservableCollection<PHIEUTHUTIENPHAT> penaltyReceipts;

        public UCReaderManagement()
        {
            InitializeComponent();
            _context = new QLTVContext();
            Readers = new ObservableCollection<DOCGIA>();
            ReadersDataGrid.ItemsSource = Readers;

            penaltyReceipts = new ObservableCollection<PHIEUTHUTIENPHAT>();

            LoadReadersData();
            LoadReaderTypesData();
        }

        private void OpenExportMenu_Click(object sender, RoutedEventArgs e)
        {
            // Tham chiếu đến nút
            Button button = sender as Button;

            // Mở ContextMenu
            if (button.ContextMenu != null)
            {
                button.ContextMenu.IsOpen = true;
            }
        }

        private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
        {
            bool isCollapsed = SidebarOverlay.Visibility == Visibility.Collapsed;

            SidebarOverlay.Visibility = isCollapsed ? Visibility.Visible : Visibility.Collapsed;

            var animation = new DoubleAnimation
            {
                From = isCollapsed ? -250 : 0,
                To = isCollapsed ? 0 : -250,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = isCollapsed ? EasingMode.EaseOut : EasingMode.EaseIn }
            };

            if (!isCollapsed)
            {
                animation.Completed += (s, _) => SidebarOverlay.Visibility = Visibility.Collapsed;
            }

            Sidebar.RenderTransform = new TranslateTransform(-250, 0);
            Sidebar.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void CloseSidebar_Click(object sender, MouseButtonEventArgs e)
        {
            // Ẩn sidebar 
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = -250,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            animation.Completed += (s, _) => SidebarOverlay.Visibility = Visibility.Collapsed;
            Sidebar.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Dashboard");
        }

        private void ReaderManagement_Click(object sender, RoutedEventArgs e)
        {
            UCReaderManagement uC_QLDG = new UCReaderManagement();
            uC_QLDG.Show();
            Close();
        }

        private void BookManagement_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("BookManagement");
        }

        private void LoanManagement_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("LoanManagement");
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("BCTK");
        }

        // Readers 
        private void LoadReadersData()
        {
            Readers.Clear();
            var readers = _context.DOCGIA.Include(d => d.IDTaiKhoanNavigation).ToList();
            foreach (var reader in readers)
            {
                Readers.Add(reader);
            }
        }

        private void AddReader_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DOCGIA newReader = GetReaderFromInputs();
                if (newReader != null)
                {
                    _context.DOCGIA.Add(newReader);
                    _context.SaveChanges();
                    Readers.Add(newReader);
                    ClearInputs();
                }
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Lỗi khi thêm độc giả: {ex.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm độc giả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateReader_Click(object sender, RoutedEventArgs e)
        {
            if (ReadersDataGrid.SelectedItem is DOCGIA selectedReader)
            {
                try
                {
                    selectedReader.IDLoaiDocGia = int.Parse(IDLoaiDocGia.Text);
                    selectedReader.NgayLapThe = NgayLapTheDatePicker.SelectedDate ?? selectedReader.NgayLapThe;
                    selectedReader.NgayHetHan = NgayHetHanDatePicker.SelectedDate ?? selectedReader.NgayHetHan;
                    selectedReader.TongNo = decimal.TryParse(TongNoTextBox.Text, out decimal tongNo) ? tongNo : selectedReader.TongNo;
                    selectedReader.GioiThieu = GioiThieu.Text;
                    _context.SaveChanges();

                    // Cập nhật UI
                    var index = Readers.IndexOf(selectedReader);
                    Readers[index] = selectedReader;
                    ReadersDataGrid.Items.Refresh();
                    MessageBox.Show("Cập nhật thông tin độc giả thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (DbUpdateException ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật độc giả trong cơ sở dữ liệu: {ex.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi không xác định: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một độc giả để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteReader_Click(object sender, RoutedEventArgs e)
        {
            if (ReadersDataGrid.SelectedItem is DOCGIA selectedReader)
            {
                var result = MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa độc giả này?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Xóa độc giả khỏi CSDL
                        var readerToDelete = _context.DOCGIA.Find(selectedReader.ID);
                        if (readerToDelete != null)
                        {
                            _context.DOCGIA.Remove(readerToDelete);
                            _context.SaveChanges();

                            Readers.Remove(selectedReader);
                            ClearInputs();

                            MessageBox.Show("Xóa độc giả thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy độc giả để xóa.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (DbUpdateException ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa độc giả: {ex.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi không xác định: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn một độc giả để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private DOCGIA GetReaderFromInputs()
        {
            try
            {
                DateTime ngayLapThe = NgayLapTheDatePicker.SelectedDate.GetValueOrDefault();
                DateTime? ngayHetHan = NgayHetHanDatePicker.SelectedDate;

                if (ngayLapThe >= ngayHetHan)
                {
                    MessageBox.Show("Ngày lập thẻ phải nhỏ hơn ngày hết hạn.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                if (!decimal.TryParse(TongNoTextBox.Text, out decimal tongNo))
                {
                    MessageBox.Show("Tổng nợ không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
                return new DOCGIA
                {
                    IDTaiKhoan = int.Parse(IDTaiKhoan.Text),
                    IDLoaiDocGia = int.Parse(IDLoaiDocGia.Text),
                    NgayLapThe = NgayLapTheDatePicker.SelectedDate ?? DateTime.Now,
                    NgayHetHan = NgayHetHanDatePicker.SelectedDate ?? DateTime.Now.AddYears(1),
                    TongNo = decimal.Parse(TongNoTextBox.Text),
                    GioiThieu = GioiThieu.Text
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchTextBox.Text.Trim().ToLower();
            string searchCriteria = SearchCriteriaComboBox.SelectedItem.ToString();
            var query = _context.DOCGIA.Include(d => d.IDTaiKhoanNavigation).AsQueryable();

            switch (searchCriteria)
            {
                case "Mã Độc Giả":
                    query = query.Where(r => r.MaDocGia.ToLower().Contains(searchTerm));
                    break;
                case "Họ Tên":
                    query = query.Where(r => r.IDTaiKhoanNavigation.TenTaiKhoan.ToLower().Contains(searchTerm));
                    break;
                case "Email":
                    query = query.Where(r => r.IDTaiKhoanNavigation.Email.ToLower().Contains(searchTerm));
                    break;
                case "Số Điện Thoại":
                    query = query.Where(r => r.IDTaiKhoanNavigation.SDT.Contains(searchTerm));
                    break;
                case "Loại Độc Giả":
                    query = query.Where(r => r.IDLoaiDocGiaNavigation.TenLoaiDocGia.ToLower().Contains(searchTerm));
                    break;
                default:
                    query = query.Where(r =>
                        r.MaDocGia.ToLower().Contains(searchTerm) ||
                        r.IDTaiKhoanNavigation.TenTaiKhoan.ToLower().Contains(searchTerm) ||
                        r.IDTaiKhoanNavigation.Email.ToLower().Contains(searchTerm) ||
                        r.IDTaiKhoanNavigation.SDT.Contains(searchTerm) ||
                        r.IDLoaiDocGiaNavigation.TenLoaiDocGia.ToLower().Contains(searchTerm)
                    );
                    break;
            }

            var filteredReaders = query.ToList();
            Readers.Clear();
            foreach (var reader in filteredReaders)
            {
                Readers.Add(reader);
            }
        }
        private void ClearInputs()
        {
            IDTaiKhoan.Text = string.Empty;
            IDLoaiDocGia.Text = string.Empty;
            NgayLapTheDatePicker.SelectedDate = null;
            NgayHetHanDatePicker.SelectedDate = null;
            TongNoTextBox.Text = string.Empty;
            GioiThieu.Text = string.Empty;
        }

        private void ReadersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReadersDataGrid.SelectedItem is DOCGIA selectedReader)
            {
                IDTaiKhoan.Text = selectedReader.IDTaiKhoan.ToString();
                IDLoaiDocGia.Text = selectedReader.IDLoaiDocGia.ToString();
                NgayLapTheDatePicker.SelectedDate = selectedReader.NgayLapThe;
                NgayHetHanDatePicker.SelectedDate = selectedReader.NgayHetHan;
                TongNoTextBox.Text = selectedReader.TongNo.ToString();
                GioiThieu.Text = selectedReader.GioiThieu;
            }
        }

        // Reader Types 
        private void LoadReaderTypesData()
        {
            var readerTypes = _context.LOAIDOCGIA.ToList();
            ReaderTypesDataGrid.ItemsSource = readerTypes;
        }

        private void AddReaderType_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate 
                if (string.IsNullOrWhiteSpace(TenLoaiDocGiaTextBox.Text) ||
                    !int.TryParse(SoSachMuonToiDaTextBox.Text, out int soSachMuonToiDa))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ và chính xác thông tin.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newReaderType = new LOAIDOCGIA
                {
                    TenLoaiDocGia = TenLoaiDocGiaTextBox.Text,
                    SoSachMuonToiDa = soSachMuonToiDa,
                    IsDeleted = false
                };

                _context.LOAIDOCGIA.Add(newReaderType);
                _context.SaveChanges();

                LoadReaderTypesData();
                ClearReaderTypeInputs();

                MessageBox.Show("Thêm loại độc giả thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm loại độc giả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateReaderType_Click(object sender, RoutedEventArgs e)
        {
            if (ReaderTypesDataGrid.SelectedItem is LOAIDOCGIA selectedReaderType)
            {
                try
                {
                    // Validate 
                    if (string.IsNullOrWhiteSpace(TenLoaiDocGiaTextBox.Text) ||
                        !int.TryParse(SoSachMuonToiDaTextBox.Text, out int soSachMuonToiDa))
                    {
                        MessageBox.Show("Vui lòng điền đầy đủ và chính xác thông tin.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var readerTypeToUpdate = _context.LOAIDOCGIA.Find(selectedReaderType.ID);
                    if (readerTypeToUpdate == null)
                    {
                        MessageBox.Show("Không tìm thấy loại độc giả để cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    readerTypeToUpdate.TenLoaiDocGia = TenLoaiDocGiaTextBox.Text;
                    readerTypeToUpdate.SoSachMuonToiDa = soSachMuonToiDa;

                    _context.SaveChanges();

                    LoadReaderTypesData();
                    ClearReaderTypeInputs();

                    MessageBox.Show("Cập nhật loại độc giả thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật loại độc giả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một loại độc giả để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteReaderType_Click(object sender, RoutedEventArgs e)
        {
            if (ReaderTypesDataGrid.SelectedItem is LOAIDOCGIA selectedReaderType)
            {
                var existingReaders = _context.DOCGIA.Any(d => d.IDLoaiDocGia == selectedReaderType.ID);
                if (existingReaders)
                {
                    MessageBox.Show("Không thể xóa loại độc giả đang được sử dụng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa loại độc giả này?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Soft delete
                        var readerTypeToDelete = _context.LOAIDOCGIA.Find(selectedReaderType.ID);
                        if (readerTypeToDelete != null)
                        {
                            // Option 1: Soft Delete
                            readerTypeToDelete.IsDeleted = true;

                            // Option 2: Hard Delete 
                            // _context.LOAIDOCGIA.Remove(readerTypeToDelete);

                            _context.SaveChanges();

                            LoadReaderTypesData();
                            ClearReaderTypeInputs();

                            MessageBox.Show("Xóa loại độc giả thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa loại độc giả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một loại độc giả để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ReaderTypesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReaderTypesDataGrid.SelectedItem is LOAIDOCGIA selectedReaderType)
            {
                TenLoaiDocGiaTextBox.Text = selectedReaderType.TenLoaiDocGia;
                SoSachMuonToiDaTextBox.Text = selectedReaderType.SoSachMuonToiDa.ToString();
            }
        }
        private void ClearReaderTypeInputs()
        {
            TenLoaiDocGiaTextBox.Text = string.Empty;
            SoSachMuonToiDaTextBox.Text = string.Empty;
        }

        // Penalty Receipts 
        private void LoadPenaltyReceiptsData()
        {
           
        }

        private void CreatePenaltyReceipt_Click(object sender, RoutedEventArgs e)
        {

            LoadPenaltyReceiptsData(); 
        }

        private void DeletePenalty_Click(object sender, RoutedEventArgs e)
        {

            LoadPenaltyReceiptsData(); 
        }

        private void EditPenalty_Click(object sender, RoutedEventArgs e)
        {

            LoadPenaltyReceiptsData(); 
        }

        private void ClearPenalty_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PrintPenaltyReceipt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PenaltyReceiptsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MaDocGiaPhat_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        // Import and Export 
        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
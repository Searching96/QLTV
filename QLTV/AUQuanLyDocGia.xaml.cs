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
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Globalization;
using System.Text;

namespace QLTV
{
    public partial class AUQuanLyDocGia : Window
    {
        private QLTVContext _context = new QLTVContext();
        public ObservableCollection<DOCGIA> Readers { get; set; }
        public ObservableCollection<PHIEUTHUTIENPHAT> PenaltyReceipts { get; set; }
        public ObservableCollection<LOAIDOCGIA> ReaderTypes { get; set; }

        public AUQuanLyDocGia()
        {
            InitializeComponent();
            _context = new QLTVContext();
            Readers = new ObservableCollection<DOCGIA>();
            PenaltyReceipts = new ObservableCollection<PHIEUTHUTIENPHAT>();
            ReaderTypes = new ObservableCollection<LOAIDOCGIA>();

            ReadersDataGrid.ItemsSource = Readers;
            LoadReadersData();

            ReaderTypesDataGrid.ItemsSource = ReaderTypes;
            LoadReaderTypesData();

            PenaltyReceiptsDataGrid.ItemsSource = ReaderTypes;
            LoadPenaltyReceiptsData();
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
            AUQuanLyDocGia uC_QLDG = new AUQuanLyDocGia();
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
            AWThemDocGia aWThemDocGia = new AWThemDocGia();
            bool? result = aWThemDocGia.ShowDialog();

            if (result == true)
            {
                LoadReadersData();
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

        private void TTDGSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = TTDGSearchTextBox.Text.Trim().ToLower();
            string searchCriteria = (TTDGSearchCriteriaComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var query = _context.DOCGIA.AsQueryable();
            switch (searchCriteria)
            {
                case "Mã Độc Giả":
                    query = query.Where(r => r.MaDocGia.ToLower().Contains(searchTerm));
                    break;
                case "ID Tài Khoản":
                    query = query.Where(r => r.IDTaiKhoan.ToString().Contains(searchTerm));
                    break;
                case "ID Loại Độc Giả":
                    query = query.Where(r => r.IDLoaiDocGia.ToString().Contains(searchTerm));
                    break;
                default:
                    // Nếu không có tiêu chí nào được chọn, tìm kiếm trên tất cả các trường
                    query = query.Where(r =>
                        r.MaDocGia.ToLower().Contains(searchTerm) ||
                        r.IDTaiKhoan.ToString().Contains(searchTerm) ||
                        r.IDLoaiDocGia.ToString().Contains(searchTerm)
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

        private void LDGSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = LDGSearchTextBox.Text.Trim().ToLower();
            string searchCriteria = (LDGSearchCriteriaComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var query = _context.LOAIDOCGIA.AsQueryable();
            switch (searchCriteria)
            {
                case "Tên Loại Độc Giả":
                    query = query.Where(r => r.TenLoaiDocGia.ToLower().Contains(searchTerm));
                    break;
                case "Số Sách Mượn Tối Đa":
                    query = query.Where(r => r.SoSachMuonToiDa.ToString().Contains(searchTerm));
                    break;
                default:
                    // Nếu không có tiêu chí nào được chọn, tìm kiếm trên tất cả các trường
                    query = query.Where(r =>
                        r.TenLoaiDocGia.ToLower().Contains(searchTerm) ||
                        r.SoSachMuonToiDa.ToString().Contains(searchTerm)
                    );
                    break;
            }
            var filteredReaderTypes = query.ToList();

            ObservableCollection<LOAIDOCGIA> filteredReaderTypesCollection = new ObservableCollection<LOAIDOCGIA>(filteredReaderTypes);
            ReaderTypesDataGrid.ItemsSource = filteredReaderTypesCollection;
            ReaderTypes.Clear();
            foreach (var readertype in filteredReaderTypes)
            {
                ReaderTypes.Add(readertype);
            }
        }

        // Penalty Receipts 
        private void LoadPenaltyReceiptsData()
        {
            PenaltyReceipts.Clear();
            var receipts = _context.PHIEUTHUTIENPHAT.Include(p => p.IDDocGiaNavigation).ToList();
            foreach (var receipt in receipts)
            {
                PenaltyReceipts.Add(receipt);
            }
            PenaltyReceiptsDataGrid.ItemsSource = PenaltyReceipts;
        }

        private void CreatePenaltyReceipt_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                // Kiểm tra xem MaDocGiaPhat có tồn tại không
                var maDocGia = int.Parse(MaDocGiaPhat.Text);
                var docGia = _context.DOCGIA.Find(maDocGia);
                if (docGia == null)
                {
                    MessageBox.Show("Không tìm thấy độc giả có mã này.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Tạo phiếu thu tiền phạt mới
                var newPenaltyReceipt = new PHIEUTHUTIENPHAT
                {
                    IDDocGia = maDocGia,
                    NgayThu = NgayThuPhat.SelectedDate ?? DateTime.Now,
                    TongNo = docGia.TongNo, // Lấy tổng nợ từ độc giả
                    SoTienThu = decimal.Parse(SoTienThu.Text),
                    ConLai = docGia.TongNo - decimal.Parse(SoTienThu.Text),
                    IsDeleted = false
                };

                _context.PHIEUTHUTIENPHAT.Add(newPenaltyReceipt);

                _context.SaveChanges();

                // Cập nhật tổng nợ của độc giả
                docGia.TongNo = newPenaltyReceipt.ConLai;

                LoadPenaltyReceiptsData();
                ClearPenaltyInputs();

                MessageBox.Show("Tạo phiếu thu tiền phạt thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo phiếu thu tiền phạt: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeletePenalty_Click(object sender, RoutedEventArgs e)
        {
            if (PenaltyReceiptsDataGrid.SelectedItem is PHIEUTHUTIENPHAT selectedPenalty)
            {
                try
                {
                    var result = MessageBox.Show(
                        "Bạn có chắc chắn muốn xóa phiếu thu tiền phạt này?",
                        "Xác nhận xóa",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        // Tìm phiếu thu trong database
                        var penaltyToDelete = _context.PHIEUTHUTIENPHAT.Find(selectedPenalty.ID);
                        if (penaltyToDelete != null)
                        {
                            // Tìm độc giả
                            var docGia = _context.DOCGIA.Find(penaltyToDelete.IDDocGia);
                            if (docGia != null)
                            {
                                // Hoàn lại số tiền đã thu vào tổng nợ
                                docGia.TongNo += penaltyToDelete.SoTienThu;
                            }

                            // Xóa thực sự phiếu thu khỏi database
                            _context.PHIEUTHUTIENPHAT.Remove(penaltyToDelete);
                            _context.SaveChanges();

                            // Load lại dữ liệu
                            LoadPenaltyReceiptsData();
                            ClearPenaltyInputs();

                            MessageBox.Show("Xóa phiếu thu tiền phạt thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa phiếu thu tiền phạt: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phiếu thu tiền phạt để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditPenalty_Click(object sender, RoutedEventArgs e)
        {
            if (PenaltyReceiptsDataGrid.SelectedItem is PHIEUTHUTIENPHAT selectedPenalty)
            {
                try
                {
                    // Kiểm tra dữ liệu đầu vào
                    if (NgayThuPhat.SelectedDate == null || string.IsNullOrEmpty(SoTienThu.Text))
                    {
                        MessageBox.Show("Vui lòng nhập đầy đủ thông tin phiếu thu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Tìm phiếu thu trong database
                    var penaltyToUpdate = _context.PHIEUTHUTIENPHAT.Find(selectedPenalty.ID);
                    if (penaltyToUpdate != null)
                    {
                        // Tìm độc giả
                        var docGia = _context.DOCGIA.Find(penaltyToUpdate.IDDocGia);
                        if (docGia == null)
                        {
                            MessageBox.Show("Không tìm thấy độc giả!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Lưu giá trị cũ để khôi phục nếu có lỗi
                        decimal oldSoTienThu = penaltyToUpdate.SoTienThu;

                        // Tính số tiền mới
                        decimal newSoTienThu = decimal.Parse(SoTienThu.Text);

                        // Kiểm tra số tiền thu không được lớn hơn tổng nợ
                        if (newSoTienThu > docGia.TongNo + oldSoTienThu)
                        {
                            MessageBox.Show("Số tiền thu không được lớn hơn tổng nợ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Cập nhật thông tin phiếu thu
                        penaltyToUpdate.NgayThu = NgayThuPhat.SelectedDate.Value;
                        penaltyToUpdate.SoTienThu = newSoTienThu;

                        // Điều chỉnh tổng nợ
                        docGia.TongNo = docGia.TongNo + oldSoTienThu - newSoTienThu;
                        penaltyToUpdate.ConLai = docGia.TongNo;

                        try
                        {
                            _context.SaveChanges();

                            // Load lại dữ liệu
                            LoadPenaltyReceiptsData();
                            ClearPenaltyInputs();

                            MessageBox.Show("Cập nhật phiếu thu tiền phạt thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            // Khôi phục lại giá trị cũ nếu có lỗi
                            docGia.TongNo = docGia.TongNo + newSoTienThu - oldSoTienThu;
                            throw new Exception($"Lỗi khi lưu thay đổi: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật phiếu thu tiền phạt: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phiếu thu tiền phạt để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearPenalty_Click(object sender, RoutedEventArgs e)
        {
            ClearPenaltyInputs();
        }

        private void PrintPenaltyReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (PenaltyReceiptsDataGrid.SelectedItem is PHIEUTHUTIENPHAT selectedPenalty)
            {
                // Mở cửa sổ AWPhieuThuTienPhat để hiển thị phiếu thu
                AWPhieuThuTienPhat printWindow = new AWPhieuThuTienPhat(selectedPenalty);
                printWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phiếu thu tiền phạt để in.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PenaltyReceiptsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PenaltyReceiptsDataGrid.SelectedItem is PHIEUTHUTIENPHAT selectedPenalty)
            {
                MaDocGiaPhat.Text = selectedPenalty.IDDocGia.ToString();
                NgayThuPhat.SelectedDate = selectedPenalty.NgayThu;
                SoTienThu.Text = selectedPenalty.SoTienThu.ToString();
                TongNoPhat.Text = selectedPenalty.TongNo.ToString();
                ConLai.Text = selectedPenalty.ConLai.ToString();
            }
        }

        private void MaDocGiaPhat_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(MaDocGiaPhat.Text, out int maDocGia))
            {
                var docGia = _context.DOCGIA.Find(maDocGia);
                if (docGia != null)
                {
                    TongNoPhat.Text = docGia.TongNo.ToString();
                }
                else
                {
                    TongNoPhat.Text = "0";
                }
            }
        }

        private void TTPSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = TTPSearchTextBox.Text.Trim().ToLower();
            string searchCriteria = (TTPSearchCriteriaComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var query = _context.PHIEUTHUTIENPHAT.Include(p => p.IDDocGiaNavigation).AsQueryable();
            switch (searchCriteria)
            {
                case "ID Độc Giả":
                    query = query.Where(p => p.IDDocGia.ToString().Contains(searchTerm));
                    break;
                case "Số Tiền Thu":
                    query = query.Where(p => p.SoTienThu.ToString().Contains(searchTerm));
                    break;
                default:
                    // Nếu không có tiêu chí nào được chọn, tìm kiếm trên tất cả các trường
                    query = query.Where(p =>
                        p.IDDocGia.ToString().Contains(searchTerm) ||
                        p.SoTienThu.ToString().Contains(searchTerm)
                    );
                    break;
            }
            var filteredPenalties = query.ToList();
            PenaltyReceipts.Clear();
            foreach (var penalty in filteredPenalties)
            {
                PenaltyReceipts.Add(penalty);
            }
        }

        private void ClearPenaltyInputs()
        {
            MaDocGiaPhat.Text = string.Empty;
            NgayThuPhat.SelectedDate = null;
            SoTienThu.Text = string.Empty;
            TongNoPhat.Text = string.Empty;
            ConLai.Text = string.Empty;
        }


        // Import and Export 
        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";

                if (openFileDialog.ShowDialog() == true)
                {
                    var filePath = openFileDialog.FileName;

                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        var worksheet = package.Workbook.Worksheets[0]; // Lấy sheet đầu tiên

                        // Đọc dữ liệu từ dòng 2 (bỏ qua dòng tiêu đề)
                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            try
                            {
                                var newReader = new DOCGIA
                                {
                                    IDTaiKhoan = int.Parse(worksheet.Cells[row, 2].Value.ToString()), // Đọc từ cột B
                                    IDLoaiDocGia = int.Parse(worksheet.Cells[row, 3].Value.ToString()), // Đọc từ cột C
                                    NgayLapThe = DateTime.ParseExact(worksheet.Cells[row, 4].Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture), // Đọc từ cột D
                                    NgayHetHan = DateTime.ParseExact(worksheet.Cells[row, 5].Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture), // Đọc từ cột E
                                    TongNo = decimal.Parse(worksheet.Cells[row, 6].Value.ToString()), // Đọc từ cột F
                                    GioiThieu = worksheet.Cells[row, 7].Value.ToString() // Đọc từ cột G
                                };

                                _context.DOCGIA.Add(newReader);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Lỗi khi đọc dữ liệu từ dòng {row}: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }

                        _context.SaveChanges();
                        LoadReadersData(); // Load lại dữ liệu
                        MessageBox.Show("Import dữ liệu từ Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi import dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel Files|*.xlsx";

                if (saveFileDialog.ShowDialog()
 == true)
                {
                    var filePath = saveFileDialog.FileName;

                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Danh sách độc giả");

                        // Tạo tiêu đề cột 
                        worksheet.Cells[1, 1].Value = "Mã độc giả";
                        worksheet.Cells[1, 2].Value = "Tên tài khoản"; 
                        worksheet.Cells[1, 3].Value = "Tên loại độc giả"; 
                        worksheet.Cells[1, 4].Value = "Ngày lập thẻ";
                        worksheet.Cells[1, 5].Value = "Ngày hết hạn";
                        worksheet.Cells[1, 6].Value = "Tổng nợ";
                        worksheet.Cells[1, 7].Value = "Giới thiệu";

                        // Điền dữ liệu 
                        int row = 2;
                        foreach (var reader in Readers)
                        {
                            worksheet.Cells[row, 1].Value = reader.MaDocGia;
                            worksheet.Cells[row, 2].Value = reader.IDTaiKhoanNavigation?.TenTaiKhoan; 
                            worksheet.Cells[row, 3].Value = reader.IDLoaiDocGiaNavigation?.TenLoaiDocGia; 
                            worksheet.Cells[row, 4].Value = reader.NgayLapThe.ToString("dd/MM/yyyy");
                            worksheet.Cells[row, 5].Value = reader.NgayHetHan.ToString("dd/MM/yyyy");
                            worksheet.Cells[row, 6].Value = reader.TongNo;
                            worksheet.Cells[row, 7].Value = reader.GioiThieu;
                            row++;
                        }

                        // Lưu 
                        var fileInfo = new FileInfo(filePath);
                        package.SaveAs(fileInfo);
                        MessageBox.Show("Xuất dữ liệu ra Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF Files|*.pdf",
                    FileName = "DanhSachDocGia.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        Document document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
                        PdfWriter writer = PdfWriter.GetInstance(document, fs);
                        document.Open();

                        // Font 
                        BaseFont baseFont = BaseFont.CreateFont("c:/windows/fonts/arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        Font titleFont = new Font(baseFont, 16, Font.BOLD);
                        Font headerFont = new Font(baseFont, 10, Font.BOLD);
                        Font dataFont = new Font(baseFont, 10, Font.NORMAL);

                        // Tiêu đề
                        Paragraph title = new Paragraph("DANH SÁCH ĐỘC GIẢ", titleFont)
                        {
                            Alignment = Element.ALIGN_CENTER
                        };
                        document.Add(title);
                        document.Add(new Paragraph(" ")); // Khoảng trắng

                        // Tạo bảng với 7 cột
                        PdfPTable table = new PdfPTable(7)
                        {
                            WidthPercentage = 100,
                            SpacingBefore = 10f,
                            SpacingAfter = 10f
                        };
                        table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 3f });

                        // Tiêu đề cột
                        string[] headers = {
                    "Mã độc giả",
                    "Tên tài khoản",
                    "Tên loại độc giả",
                    "Ngày lập thẻ",
                    "Ngày hết hạn",
                    "Tổng nợ",
                    "Giới thiệu"
                };

                        foreach (string header in headers)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                            {
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                VerticalAlignment = Element.ALIGN_MIDDLE,
                                Padding = 5f
                            };
                            table.AddCell(cell);
                        }

                        // Dữ liệu
                        var readers = _context.DOCGIA
                            .Include(r => r.IDTaiKhoanNavigation)
                            .Include(r => r.IDLoaiDocGiaNavigation)
                            .ToList();

                        foreach (var reader in readers)
                        {
                            table.AddCell(new Phrase(reader.MaDocGia.ToString(), dataFont));
                            table.AddCell(new Phrase(reader.IDTaiKhoanNavigation?.TenTaiKhoan ?? "", dataFont));
                            table.AddCell(new Phrase(reader.IDLoaiDocGiaNavigation?.TenLoaiDocGia ?? "", dataFont));
                            table.AddCell(new Phrase(reader.NgayLapThe.ToString("dd/MM/yyyy"), dataFont));
                            table.AddCell(new Phrase(reader.NgayHetHan.ToString("dd/MM/yyyy") ?? "N/A", dataFont));
                            table.AddCell(new Phrase(reader.TongNo.ToString("N2"), dataFont));
                            table.AddCell(new Phrase(reader.GioiThieu?.ToString() ?? "", dataFont));
                        }

                        document.Add(table);
                        document.Close();

                        MessageBox.Show("Xuất PDF thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
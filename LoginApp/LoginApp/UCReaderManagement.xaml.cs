using LoginApp.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using OfficeOpenXml;
using Microsoft.Win32;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoginApp
{
    public partial class UCReaderManagement : Window
    {
        private readonly AppDbContext _context;
        public ObservableCollection<Reader> Readers { get; set; }
        private ObservableCollection<PenaltyReceipt> penaltyReceipts;
        private int currentReceiptNumber = 0;

        public UCReaderManagement()
        {
            InitializeComponent();
            _context = new AppDbContext();
            Readers = new ObservableCollection<Reader>();
            ReadersDataGrid.ItemsSource = Readers;
            LoadReaders();

            //Đánh dấu mã phiếu
            var lastReceipt = _context.PenaltyReceipts.OrderByDescending(r => r.MaPhieuThu).FirstOrDefault();
            if (lastReceipt != null)
            {
                if (int.TryParse(lastReceipt.MaPhieuThu.Substring(2), out int lastNumber))
                {
                    currentReceiptNumber = lastNumber;
                }
            }

            // Khởi tạo collection
            penaltyReceipts = new ObservableCollection<PenaltyReceipt>();

            // Lấy dữ liệu phiếu thu từ database
            var receipts = _context.PenaltyReceipts.ToList();
            foreach (var receipt in receipts)
            {
                penaltyReceipts.Add(receipt);
            }

            // Gán DataSource cho DataGrid
            PenaltyReceiptsDataGrid.ItemsSource = penaltyReceipts;
                LoadPenaltyReceipts();
        }


        private void LoadReaders()
        {
            Readers.Clear();
            var readers = _context.Readers.ToList();
            foreach (var reader in readers)
            {
                Readers.Add(reader);
            }
        }

        private void AddReader_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Reader newReader = GetReaderFromInputs();
                if (newReader != null)
                {
                    _context.Readers.Add(newReader);
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
            if (ReadersDataGrid.SelectedItem is Reader selectedReader)
            {
                try
                {
                    // Tạo một bản sao của đối tượng Reader để cập nhật
                    var updatedReader = new Reader
                    {
                        MaDocGia = selectedReader.MaDocGia,
                        HoTen = HoTen.Text,
                        NgaySinh = NgaySinh.SelectedDate ?? selectedReader.NgaySinh,
                        DiaChi = DiaChi.Text,
                        Email = Email.Text,
                        SoDienThoai = SoDienThoai.Text,
                        LoaiDocGia = LoaiDocGiaComboBox.Text,
                        NgayLapThe = NgayLapTheDatePicker.SelectedDate ?? selectedReader.NgayLapThe,
                        NgayHetHan = NgayHetHanDatePicker.SelectedDate,
                        TongNo = decimal.TryParse(TongNoTextBox.Text, out decimal tongNo) ? tongNo : selectedReader.TongNo
                    };

                    // Thực hiện kiểm tra hợp lệ
                    if (updatedReader.NgaySinh >= updatedReader.NgayLapThe)
                    {
                        MessageBox.Show("Ngày sinh phải nhỏ hơn ngày lập thẻ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (updatedReader.NgayLapThe >= updatedReader.NgayHetHan)
                    {
                        MessageBox.Show("Ngày lập thẻ phải nhỏ hơn ngày hết hạn.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Cập nhật cơ sở dữ liệu
                    var readerToUpdate = _context.Readers.Find(updatedReader.MaDocGia);
                    if (readerToUpdate != null)
                    {
                        _context.Entry(readerToUpdate).CurrentValues.SetValues(updatedReader);
                        _context.SaveChanges();

                        // Cập nhật UI
                        var index = Readers.IndexOf(selectedReader);
                        Readers[index] = updatedReader;
                        ReadersDataGrid.Items.Refresh();

                        MessageBox.Show("Cập nhật thông tin độc giả thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy độc giả để cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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
            if (ReadersDataGrid.SelectedItem is Reader selectedReader)
            {
                Readers.Remove(selectedReader);
                ClearInputs();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một độc giả để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void HoTen_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z\s]+$");
        }

        private void SoDienThoai_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void TongNoTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void TongNoTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TongNoTextBox.Text, out decimal result))
            {
                TongNoTextBox.Text = result.ToString("N2");
            }
            else
            {
                TongNoTextBox.Text = "0.00";
            }
        }
        private Reader GetReaderFromInputs()
        {
            try
            {
                string loaiDocGia = LoaiDocGiaComboBox.Text;
                if (!new[] { "Học sinh/Sinh viên", "Giáo viên", "Khách" }.Contains(loaiDocGia))
                {
                    MessageBox.Show("Loại độc giả không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                DateTime ngaySinh = NgaySinh.SelectedDate.GetValueOrDefault();
                DateTime ngayLapThe = NgayLapTheDatePicker.SelectedDate.GetValueOrDefault();
                DateTime? ngayHetHan = NgayHetHanDatePicker.SelectedDate;

                if (ngaySinh >= ngayLapThe)
                {
                    MessageBox.Show("Ngày sinh phải nhỏ hơn ngày lập thẻ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                if (ngayLapThe >= ngayHetHan)
                {
                    MessageBox.Show("Ngày lập thẻ phải nhỏ hơn ngày hết hạn.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
                if (!Regex.IsMatch(HoTen.Text, @"^[a-zA-Z\s]+$"))
                {
                    MessageBox.Show("Họ tên chỉ được chứa chữ cái và khoảng trắng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                if (!Regex.IsMatch(SoDienThoai.Text, @"^\d{10}$"))
                {
                    MessageBox.Show("Số điện thoại phải chứa đúng 10 số.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                if (!decimal.TryParse(TongNoTextBox.Text, out decimal tongNo))
                {
                    MessageBox.Show("Tổng nợ không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
                return new Reader
                {
                    MaDocGia = MaDocGia.Text,
                    HoTen = HoTen.Text,
                    NgaySinh = NgaySinh.SelectedDate ?? DateTime.Now,
                    DiaChi = DiaChi.Text,
                    Email = Email.Text,
                    SoDienThoai = SoDienThoai.Text,
                    LoaiDocGia = LoaiDocGiaComboBox.Text,
                    NgayLapThe = NgayLapTheDatePicker.SelectedDate ?? DateTime.Now,
                    NgayHetHan = NgayHetHanDatePicker.SelectedDate,
                    TongNo = decimal.Parse(TongNoTextBox.Text)
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
            string searchCriteria = (SearchCriteriaComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            var query = _context.Readers.AsQueryable();

            switch (searchCriteria)
            {
                case "Mã Độc Giả":
                    query = query.Where(r => r.MaDocGia.ToLower().Contains(searchTerm));
                    break;
                case "Họ Tên":
                    query = query.Where(r => r.HoTen.ToLower().Contains(searchTerm));
                    break;
                case "Email":
                    query = query.Where(r => r.Email.ToLower().Contains(searchTerm));
                    break;
                case "Số Điện Thoại":
                    query = query.Where(r => r.SoDienThoai.Contains(searchTerm));
                    break;
                case "Loại Độc Giả":
                    query = query.Where(r => r.LoaiDocGia.ToLower().Contains(searchTerm));
                    break;
                default:
                    // Nếu không có tiêu chí nào được chọn, tìm kiếm trên tất cả các trường
                    query = query.Where(r =>
                        r.MaDocGia.ToLower().Contains(searchTerm) ||
                        r.HoTen.ToLower().Contains(searchTerm) ||
                        r.Email.ToLower().Contains(searchTerm) ||
                        r.SoDienThoai.Contains(searchTerm) ||
                        r.LoaiDocGia.ToLower().Contains(searchTerm)
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
            MaDocGia.Text = string.Empty;
            HoTen.Text = string.Empty;
            NgaySinh.SelectedDate = null;
            DiaChi.Text = string.Empty;
            Email.Text = string.Empty;
            SoDienThoai.Text = string.Empty;
            LoaiDocGiaComboBox.SelectedIndex = -1; 
            NgayLapTheDatePicker.SelectedDate = null;
            NgayHetHanDatePicker.SelectedDate = null;
            TongNoTextBox.Text = string.Empty;
        }

        private void ReadersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReadersDataGrid.SelectedItem is Reader selectedReader)
            {
                MaDocGia.Text = selectedReader.MaDocGia;
                HoTen.Text = selectedReader.HoTen;
                NgaySinh.SelectedDate = selectedReader.NgaySinh;
                DiaChi.Text = selectedReader.DiaChi;
                Email.Text = selectedReader.Email;
                SoDienThoai.Text = selectedReader.SoDienThoai;
                LoaiDocGiaComboBox.Text = selectedReader.LoaiDocGia;
                NgayLapTheDatePicker.SelectedDate = selectedReader.NgayLapThe;
                NgayHetHanDatePicker.SelectedDate = selectedReader.NgayHetHan;
                TongNoTextBox.Text = selectedReader.TongNo.ToString();
            }
        }

        // Import từ Excel
        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ImportReadersFromExcel(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi import: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportReadersFromExcel(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // Bỏ qua dòng tiêu đề
                {
                    var reader = new Reader
                    {
                        MaDocGia = worksheet.Cells[row, 1].Value?.ToString(),
                        HoTen = worksheet.Cells[row, 2].Value?.ToString(),
                        NgaySinh = DateTime.ParseExact(worksheet.Cells[row, 3].Value?.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture), // Chuyển đổi NgaySinh
                        DiaChi = worksheet.Cells[row, 4].Value?.ToString(),
                        Email = worksheet.Cells[row, 5].Value?.ToString(),
                        SoDienThoai = worksheet.Cells[row, 6].Value?.ToString(),
                        LoaiDocGia = worksheet.Cells[row, 7].Value?.ToString(),
                        NgayLapThe = DateTime.ParseExact(worksheet.Cells[row, 8].Value?.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture), // Chuyển đổi NgayLapThe
                        NgayHetHan = worksheet.Cells[row, 9].Value != null
                    ? DateTime.ParseExact(worksheet.Cells[row, 9].Value?.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture) // Chuyển đổi NgayHetHan
                    : (DateTime?)null,
                        TongNo = decimal.Parse(worksheet.Cells[row, 10].Value?.ToString() ?? "0")
                    };

                    // Kiểm tra trùng lặp trước khi thêm
                    var existingReader = _context.Readers.Find(reader.MaDocGia);
                    if (existingReader == null)
                    {
                        _context.Readers.Add(reader);
                    }
                    else
                    {
                        // Cập nhật thông tin nếu độc giả đã tồn tại
                        _context.Entry(existingReader).CurrentValues.SetValues(reader);
                    }
                }

                _context.SaveChanges();
                LoadReaders(); 
                MessageBox.Show("Import dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Export sang Excel
        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = "DanhSachDocGia.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                ExportReadersToExcel(saveFileDialog.FileName);
            }
        }

        private void ExportReadersToExcel(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh Sách Độc Giả");

                // Tiêu đề cột
                string[] headers = {
            "Mã Độc Giả", "Họ Tên", "Ngày Sinh", "Địa Chỉ",
            "Email", "Số Điện Thoại", "Loại Độc Giả",
            "Ngày Lập Thẻ", "Ngày Hết Hạn", "Tổng Nợ"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }

                // Điền dữ liệu
                var readers = _context.Readers.ToList();
                for (int row = 0; row < readers.Count; row++)
                {
                    var reader = readers[row];
                    worksheet.Cells[row + 2, 1].Value = reader.MaDocGia;
                    worksheet.Cells[row + 2, 2].Value = reader.HoTen;
                    worksheet.Cells[row + 2, 3].Value = reader.NgaySinh.ToString("dd/MM/yyyy");
                    worksheet.Cells[row + 2, 4].Value = reader.DiaChi;
                    worksheet.Cells[row + 2, 5].Value = reader.Email;
                    worksheet.Cells[row + 2, 6].Value = reader.SoDienThoai;
                    worksheet.Cells[row + 2, 7].Value = reader.LoaiDocGia;
                    worksheet.Cells[row + 2, 8].Value = reader.NgayLapThe.ToString("dd/MM/yyyy");
                    worksheet.Cells[row + 2, 9].Value = reader.NgayHetHan?.ToString("dd/MM/yyyy");
                    worksheet.Cells[row + 2, 10].Value = reader.TongNo;
                }

                // Tự động chỉnh độ rộng cột
                worksheet.Cells.AutoFitColumns();

                // Lưu file
                File.WriteAllBytes(filePath, package.GetAsByteArray());
                MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Export sang PDF
        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                FileName = "DanhSachDocGia.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                ExportReadersToPDF(saveFileDialog.FileName);
            }
        }

        private void ExportReadersToPDF(string filePath)
        {
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

                // Tạo bảng
                PdfPTable table = new PdfPTable(10)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10f,
                    SpacingAfter = 10f
                };
                table.SetWidths(new float[] { 1.5f, 2f, 1.5f, 3f, 3f, 2f, 2f, 2f, 2f, 1f });

                // Tiêu đề cột
                string[] headers = { "Mã ĐG", "Họ Tên", "Ngày Sinh", "Địa Chỉ", "Email", "SĐT", "Loại ĐG", "Ngày Lập Thẻ", "Ngày Hết Hạn", "Tổng Nợ" };
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
                var readers = _context.Readers.ToList();
                foreach (var reader in readers)
                {
                    table.AddCell(new Phrase(reader.MaDocGia ?? "N/A", dataFont));
                    table.AddCell(new Phrase(reader.HoTen ?? "N/A", dataFont));
                    table.AddCell(new Phrase(reader.NgaySinh.ToString("dd/MM/yyyy"), dataFont));
                    table.AddCell(new Phrase(reader.DiaChi ?? "N/A", dataFont));
                    table.AddCell(new Phrase(reader.Email ?? "N/A", dataFont));
                    table.AddCell(new Phrase(reader.SoDienThoai ?? "N/A", dataFont));
                    table.AddCell(new Phrase(reader.LoaiDocGia ?? "N/A", dataFont));
                    table.AddCell(new Phrase(reader.NgayLapThe.ToString("dd/MM/yyyy"), dataFont));
                    table.AddCell(new Phrase(reader.NgayHetHan?.ToString("dd/MM/yyyy") ?? "N/A", dataFont));
                    table.AddCell(new Phrase(reader.TongNo.ToString("N2"), dataFont));
                }

                document.Add(table);
                document.Close();
                MessageBox.Show("Xuất PDF thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadPenaltyReceipts()
        {
            penaltyReceipts.Clear();
            var receipts = _context.PenaltyReceipts.ToList();
            foreach (var receipt in receipts)
            {
                penaltyReceipts.Add(receipt);
            }
        }

        private string GenerateReceiptNumber()
        {
            currentReceiptNumber++;
            return $"PT{currentReceiptNumber:000000}";
        }

        private void CreatePenaltyReceipt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrEmpty(MaDocGiaPhat.Text) || NgayThuPhat.SelectedDate == null || string.IsNullOrEmpty(SoTienThu.Text))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin phiếu thu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal soTienThu = decimal.Parse(SoTienThu.Text);
                decimal tongNo = decimal.Parse(TongNoPhat.Text);

                if (soTienThu > tongNo)
                {
                    MessageBox.Show("Số tiền thu không được lớn hơn tổng nợ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Tạo phiếu thu mới
                var newReceipt = new PenaltyReceipt
                {
                    MaPhieuThu = GenerateReceiptNumber(),
                    NgayThu = NgayThuPhat.SelectedDate.Value,
                    MaDocGia = MaDocGiaPhat.Text,
                    TenDocGia = TenDocGiaPhat.Text,
                    SoTienThu = soTienThu,
                    ConNo = tongNo - soTienThu,
                    NguoiThu = "Admin", 
                    GhiChu = ""
                };
                // Thêm phiếu thu 
                _context.PenaltyReceipts.Add(newReceipt);

                // Lấy database
                var reader = _context.Readers.FirstOrDefault(r => r.MaDocGia == newReceipt.MaDocGia);
                if (reader != null)
                {
                    reader.TongNo -= newReceipt.SoTienThu;
                    _context.SaveChanges();
                    LoadReaders();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy độc giả!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Lưu database
                _context.SaveChanges();
                penaltyReceipts.Add(newReceipt);

                MaPhieuThu.Text = "";
                SoTienThu.Text = "";
                MessageBox.Show("Tạo phiếu thu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo phiếu thu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MaDocGiaPhat_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Lấy thông tin độc giả 
            var reader = ReadersDataGrid.Items.Cast<dynamic>()
                .FirstOrDefault(r => r.MaDocGia == MaDocGiaPhat.Text);

            if (reader != null)
            {
                TenDocGiaPhat.Text = reader.HoTen;
                TongNoPhat.Text = reader.TongNo.ToString();
            }
            else
            {
                TenDocGiaPhat.Text = "";
                TongNoPhat.Text = "0";
            }
        }

        private void EditPenalty_Click(object sender, RoutedEventArgs e)
        {
            if (PenaltyReceiptsDataGrid.SelectedItem is PenaltyReceipt selectedReceipt)
            {
                try
                {
                    if (NgayThuPhat.SelectedDate == null || string.IsNullOrEmpty(SoTienThu.Text))
                    {
                        MessageBox.Show("Vui lòng nhập đầy đủ thông tin phiếu thu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    decimal newAmount = decimal.Parse(SoTienThu.Text);
                    decimal oldAmount = selectedReceipt.SoTienThu;
                    decimal tongNo = decimal.Parse(TongNoPhat.Text);

                    decimal difference = newAmount - oldAmount;

                    if (tongNo - newAmount < 0)
                    {
                        MessageBox.Show("Số tiền thu không được lớn hơn tổng nợ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var result = MessageBox.Show(
                        "Bạn có chắc chắn muốn cập nhật phiếu thu này?",
                        "Xác nhận cập nhật",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        var receiptToUpdate = _context.PenaltyReceipts.Find(selectedReceipt.MaPhieuThu);
                        if (receiptToUpdate != null)
                        {
                            // Lưu thông tin cũ để có thể khôi phục 
                            var oldReceipt = new PenaltyReceipt
                            {
                                MaPhieuThu = receiptToUpdate.MaPhieuThu,
                                NgayThu = receiptToUpdate.NgayThu,
                                MaDocGia = receiptToUpdate.MaDocGia,
                                TenDocGia = receiptToUpdate.TenDocGia,
                                SoTienThu = receiptToUpdate.SoTienThu,
                                ConNo = receiptToUpdate.ConNo,
                                NguoiThu = receiptToUpdate.NguoiThu,
                                GhiChu = receiptToUpdate.GhiChu
                            };

                            // Cập nhật thông tin 
                            receiptToUpdate.NgayThu = NgayThuPhat.SelectedDate.Value;
                            receiptToUpdate.SoTienThu = newAmount;
                            receiptToUpdate.ConNo = tongNo - newAmount;

                            // Cập nhật tổng nợ 
                            var reader = _context.Readers.FirstOrDefault(r => r.MaDocGia == receiptToUpdate.MaDocGia);
                            if (reader != null)
                            {
                                // Điều chỉnh tổng nợ dựa trên sự thay đổi của số tiền thu
                                if (difference > 0)
                                {
                                    reader.TongNo -= difference; // Giảm nợ nếu thu thêm
                                }
                                else
                                {
                                    reader.TongNo += Math.Abs(difference); // Tăng nợ nếu giảm số tiền thu
                                }
                            }

                            try
                            {
                                _context.SaveChanges();
                                LoadPenaltyReceipts();
                                LoadReaders();

                                MessageBox.Show("Cập nhật phiếu thu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                // Khôi phục lại thông tin cũ nếu lỗi
                                _context.Entry(receiptToUpdate).CurrentValues.SetValues(oldReceipt);
                                if (reader != null)
                                {
                                    reader.TongNo += difference;
                                }
                                throw new Exception($"Lỗi khi lưu thay đổi: {ex.Message}");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy phiếu thu để cập nhật!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật phiếu thu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phiếu thu để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeletePenalty_Click(object sender, RoutedEventArgs e)
        {
            if (PenaltyReceiptsDataGrid.SelectedItem is PenaltyReceipt selectedPenaltyReceipt)
            {
                try
                {
                    var result = MessageBox.Show(
                        "Bạn có chắc chắn muốn xóa phiếu thu này?",
                        "Xác nhận xóa",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        // Tìm độc giả
                        var reader = _context.Readers.FirstOrDefault(r => r.MaDocGia == selectedPenaltyReceipt.MaDocGia);
                        if (reader != null)
                        {
                            // Hoàn lại số tiền đã thu vào tổng nợ
                            reader.TongNo += selectedPenaltyReceipt.SoTienThu;
                        }

                        _context.PenaltyReceipts.Remove(selectedPenaltyReceipt);
                        _context.SaveChanges();
                        penaltyReceipts.Remove(selectedPenaltyReceipt);

                        MessageBox.Show("Xóa phiếu thu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa phiếu thu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phiếu thu để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearPenalty_Click(object sender, RoutedEventArgs e)
        {
            MaPhieuThu.Text = string.Empty;
            MaDocGiaPhat.Text = string.Empty;
            TongNoPhat.Text = string.Empty;
            SoTienThu.Text = string.Empty;
            NgayThuPhat.SelectedDate = null;
        }

        private void PrintPenaltyReceipt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PenaltyReceiptsDataGrid.SelectedItem is PenaltyReceipt selectedReceipt)
                {
                    // Tạo một instance của PenaltyReceiptWindow và truyền dữ liệu phiếu thu
                    UCRM_PenaltyReceiptWindow UCRMreceiptWindow = new UCRM_PenaltyReceiptWindow(selectedReceipt);
                    UCRMreceiptWindow.ShowDialog(); // Hiển thị window
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn một phiếu thu để in.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in phiếu thu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Load data lên Input
        private void PenaltyReceiptsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (PenaltyReceiptsDataGrid.SelectedItem is PenaltyReceipt selectedReceipt)
                {
                    // Điền dữ liệu 
                    MaPhieuThu.Text = selectedReceipt.MaPhieuThu;
                    NgayThuPhat.SelectedDate = selectedReceipt.NgayThu;
                    MaDocGiaPhat.Text = selectedReceipt.MaDocGia;
                    TenDocGiaPhat.Text = selectedReceipt.TenDocGia;
                    SoTienThu.Text = selectedReceipt.SoTienThu.ToString();
                    // Hiển thị tiền nợ
                    TongNoPhat.Text = selectedReceipt.ConNo.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi load thông tin phiếu thu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class Reader
    {
        public string MaDocGia { get; set; }
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string DiaChi { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string LoaiDocGia { get; set; }
        public DateTime NgayLapThe { get; set; }
        public DateTime? NgayHetHan { get; set; }
        public decimal TongNo { get; set; }
    }
    public class PenaltyReceipt
    {
        public string MaPhieuThu { get; set; }
        public DateTime NgayThu { get; set; }
        public string MaDocGia { get; set; }
        public string TenDocGia { get; set; }
        public decimal SoTienThu { get; set; }
        public decimal ConNo { get; set; }
        public string NguoiThu { get; set; }
        public string GhiChu { get; set; }
    }
}
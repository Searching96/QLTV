using QLTV.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace QLTV.Admin
{
    public class BooleanToContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string content)
            {
                var contents = content.Split(':');
                return boolValue ? contents[1] : contents[0];
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class QuyDinhModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _TuoiToiThieu;
        public string TuoiToiThieu
        {
            get => _TuoiToiThieu;
            set
            {
                if (_TuoiToiThieu != value)
                {
                    _TuoiToiThieu = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _TienPhatTraTreMotNgay;
        public string TienPhatTraTreMotNgay
        {
            get => _TienPhatTraTreMotNgay;
            set
            {
                if (_TienPhatTraTreMotNgay != value)
                {
                    _TienPhatTraTreMotNgay = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _TiLeDenBu;
        public string TiLeDenBu
        {
            get => _TiLeDenBu;
            set
            {
                if (_TiLeDenBu != value)
                {
                    _TiLeDenBu = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool isValid = true;

        private bool _isEditable = false;
        public bool isEditable
        {
            get => _isEditable;
            set
            {
                if (_isEditable != value)
                {
                    _isEditable = value;
                    OnPropertyChanged();
                }
            }
        }

        string IDataErrorInfo.Error => throw new NotImplementedException();

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (columnName == nameof(TuoiToiThieu))
                {
                    if (string.IsNullOrWhiteSpace(TuoiToiThieu))
                    {
                        isValid = false;
                        return "Thầy Dũng đẹp trai";
                    }

                    if (!int.TryParse(TuoiToiThieu, out int a))
                    {
                        isValid = false;
                        return "Nhập số nguyên hợp lệ";
                    }
                    if (a < 0)
                    {
                        isValid = false;
                        return "Tuổi tối thiểu phải là số nguyên dương.";
                    }

                    if (a > 122)
                    {
                        isValid = false;
                        return "Tuổi tối đa không được lớn quá 122";
                    }
                }

                if (columnName == nameof(TienPhatTraTreMotNgay))
                {
                    if (string.IsNullOrWhiteSpace(TienPhatTraTreMotNgay))
                    {
                        isValid = false;
                        return "Thầy Dũng đẹp trai";
                    }
                    if (!decimal.TryParse(TienPhatTraTreMotNgay, out decimal a))
                    {
                        isValid = false;
                        return "Nhập số hợp lệ";
                    }
                    if (a < 0)
                    {
                        isValid = false;
                        return "Số tiền phạt phải là số dương.";
                    }
                }

                if (columnName == nameof(TiLeDenBu))
                {
                    if (string.IsNullOrWhiteSpace(TiLeDenBu))
                    {
                        isValid = false;
                        return "Thầy Dũng đẹp trai";
                    }

                    if (!decimal.TryParse(TiLeDenBu, out decimal a))
                    {
                        isValid = false;
                        return "Nhập số hợp lệ";
                    }
                    if (a < 0)
                    {
                        isValid = false;
                        return "Tỉ lệ đền bù phải là số dương.";
                    }
                }
                isValid = true;
                return null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Interaction logic for UcQuyDinh.xaml
    /// </summary>
    public partial class UcQuyDinh : UserControl
    {
        private QuyDinhModel quyDinhModel;
        public UcQuyDinh()
        {
            InitializeComponent();
            LoadData();
        }

        public void LoadData()
        {
            using (var context = new QLTVContext())
            {
                THAMSO latest = context.THAMSO.OrderByDescending(x => x.ID).FirstOrDefault();
                quyDinhModel = new QuyDinhModel
                {
                    TuoiToiThieu = latest.TuoiToiThieu.ToString(),
                    TienPhatTraTreMotNgay = latest.TienPhatTraTreMotNgay.ToString(),
                    TiLeDenBu = (latest.TiLeDenBu * 100).ToString(),
                    isEditable = false
                };
                DataContext = quyDinhModel;
            }
        }

        private void btnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!quyDinhModel.isEditable)
                {
                    quyDinhModel.isEditable = true;
                }
                else
                {
                    if (quyDinhModel.isValid)
                    {
                        var QuyDinh = new THAMSO
                        {
                            TuoiToiThieu = int.Parse(quyDinhModel.TuoiToiThieu),
                            TienPhatTraTreMotNgay = decimal.Parse(quyDinhModel.TienPhatTraTreMotNgay),
                            TiLeDenBu = decimal.Parse(quyDinhModel.TiLeDenBu) / 100
                        };
                        using (var context = new QLTVContext())
                        {
                            var latestQuyDinh = context.THAMSO.OrderByDescending(ts => ts.ID).FirstOrDefault();
                            if (QuyDinh.TuoiToiThieu == latestQuyDinh.TuoiToiThieu &&
                                QuyDinh.TienPhatTraTreMotNgay == latestQuyDinh.TienPhatTraTreMotNgay &&
                                QuyDinh.TiLeDenBu == latestQuyDinh.TiLeDenBu)
                            {
                                MessageBox.Show("Quy định đã tồn tại trong hệ thống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                                quyDinhModel.isEditable = false;
                                return;
                            }
                            context.THAMSO.Add(QuyDinh);
                            context.SaveChanges();
                            LoadData();
                        }
                        quyDinhModel.isEditable = false;
                        if (DateTime.Now.Day != 1)
                        {
                            MessageBox.Show("Chỉ được chỉnh sửa quy định vào ngày đầu tiên của tháng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else if (!quyDinhModel.isValid)
                    {
                        MessageBox.Show("Dữ liệu không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            quyDinhModel.isEditable = false;
            LoadData();
        }
    }
}

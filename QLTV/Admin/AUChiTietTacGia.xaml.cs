using Microsoft.EntityFrameworkCore;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// <summary>
    /// Interaction logic for AUChiTietTacGia.xaml
    /// </summary>
    public partial class AUChiTietTacGia : UserControl
    {
        public string MaTacGia;

        public class TuaSachViewModel 
        {
            public string MaTuaSach { get; set; }
            public string TenTuaSach { get; set; }
            public int SoLuong { get; set; }
            public int HanMuonToiDa { get; set; }
            public string MoTa { get; set; }
            public string DSTacGia { get; set; }
            public string DSTheLoai { get; set; }
            public int SoLuotMuon {  get; set; }
        }
        public AUChiTietTacGia(string maTacGia)
        {
            InitializeComponent();
            MaTacGia = maTacGia;
            LoadTuaSach();
        }

        private void LoadTuaSach(bool isInitialLoad = false)
        {
            using (var context = new QLTVContext())
            {
                // Truy vấn cơ sở dữ liệu gốc
                var data = context.TUASACH
                    .Include(ts => ts.TUASACH_TACGIA)
                        .ThenInclude(ts_tg => ts_tg.IDTacGiaNavigation)
                    .Include(ts => ts.TUASACH_THELOAI)
                        .ThenInclude(ts_tl => ts_tl.IDTheLoaiNavigation)
                    .Include(ts => ts.SACH)
                        .ThenInclude(s => s.CTPHIEUMUON)
                    .Where(ts => !ts.IsDeleted &&
                                 ts.TUASACH_TACGIA.Any(ts_tg => ts_tg.IDTacGiaNavigation.MaTacGia == MaTacGia))
                    .Select(ts => new
                    {
                        MaTuaSach = ts.MaTuaSach,
                        TenTuaSach = ts.TenTuaSach,
                        SoLuong = ts.SoLuong,
                        HanMuonToiDa = ts.HanMuonToiDa,
                        MoTa = ts.MoTa,
                        DSTacGia = string.Join(", ", ts.TUASACH_TACGIA
                            .Select(ts_tg => ts_tg.IDTacGiaNavigation.TenTacGia)),
                        DSTheLoai = string.Join(", ", ts.TUASACH_THELOAI
                            .Select(ts_tl => ts_tl.IDTheLoaiNavigation.TenTheLoai)),
                        Sach = ts.SACH // Lấy danh sách sách để xử lý SoLuotMuon sau
                    })
                    .AsEnumerable() // Chuyển sang xử lý trên bộ nhớ
                    .Select(ts => new TuaSachViewModel
                    {
                        MaTuaSach = ts.MaTuaSach,
                        TenTuaSach = ts.TenTuaSach,
                        SoLuong = ts.SoLuong,
                        HanMuonToiDa = ts.HanMuonToiDa,
                        MoTa = ts.MoTa,
                        DSTacGia = ts.DSTacGia,
                        DSTheLoai = ts.DSTheLoai,
                        SoLuotMuon = ts.Sach.Sum(s => s.CTPHIEUMUON.Count) // Tính SoLuotMuon sau khi tải dữ liệu
                    })
                    .ToList();

                MessageBox.Show($"So sach: {data.Count}");

                dgTuaSach.ItemsSource = data;
            }
        }
    }
}

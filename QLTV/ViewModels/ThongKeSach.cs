using Microsoft.EntityFrameworkCore;
using QLTV.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QLTV.ViewModels
{
    public class ThongKeSach
    {
        public static ObservableCollection<TUASACH> GetSachMuonNhieuTheoThoiGian(
            DbSet<PHIEUMUON> phieuMuons,
            string loaiThoiGian)
        {
            // Xác định khoảng thời gian bắt đầu
            DateTime ngayBatDau;
            DateTime ngayKetThuc = DateTime.Now;
            

            switch (loaiThoiGian.ToLower())
            {
                case "tuan":
                    ngayBatDau = ngayKetThuc.AddDays(-7);
                    break;
                case "thang":
                    ngayBatDau = ngayKetThuc.AddMonths(-1);
                    break;
                case "nam":
                    ngayBatDau = ngayKetThuc.AddYears(-1);
                    break;
                default:
                    throw new ArgumentException("Loại thời gian không hợp lệ. Chọn 'tuan', 'thang', hoặc 'nam'.");
            }

            // Truy vấn dữ liệu
            var danhSachTuaSach = phieuMuons
                .Where(pm => pm.NgayMuon >= ngayBatDau && pm.NgayMuon <= ngayKetThuc && !pm.IsDeleted) // Lọc phiếu mượn hợp lệ
                .Include(pm => pm.CTPHIEUMUON) // Bao gồm chi tiết phiếu mượn
                .ThenInclude(ctpm => ctpm.IDSachNavigation) // Bao gồm sách liên quan
                .Where(pm => pm.CTPHIEUMUON.Any(ctpm => !ctpm.IDSachNavigation.IsDeleted)) // Lọc chi tiết phiếu mượn có sách hợp lệ
                .SelectMany(pm => pm.CTPHIEUMUON) // Lấy các chi tiết phiếu mượn
                .Where(ctpm => !ctpm.IDSachNavigation.IsDeleted) // Lọc sách hợp lệ
                .GroupBy(ctpm => ctpm.IDSachNavigation.IDTuaSachNavigation) // Nhóm theo tựa sách
                .OrderByDescending(group => group.Count()) // Sắp xếp giảm dần theo số lượng mượn
                .Select(group => group.Key) // Chọn tựa sách
                .ToList();
            
            var pm = phieuMuons.Where(pm => pm.NgayMuon >= ngayBatDau && pm.NgayMuon <= ngayKetThuc && !pm.IsDeleted).SelectMany(pm => pm.CTPHIEUMUON).ToList();
            
            // Chuyển sang ObservableCollection
            return new ObservableCollection<TUASACH>(danhSachTuaSach);
        }
    }
}

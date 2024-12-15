using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using QLTV.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;
namespace QLTV.ViewModels
{
    public class ThongBaoList
    {
        public ObservableCollection<ThongBao> ThongBao { get; set; }

        public ThongBaoList()
        {
            ThongBao = new ObservableCollection<ThongBao>();
        }

        public void LoadThongBao(int userId)
        {
            using (var dbContext = new QLTVContext())
            {
                var taiKhoan = dbContext.TAIKHOAN.FirstOrDefault(tk => tk.ID == userId);
                var docGia = dbContext.DOCGIA.FirstOrDefault(dg => dg.IDTaiKhoan == userId);

                if (taiKhoan != null)
                {
                    
                    var ngayHienTai = DateTime.Now;
                    var khoangCach = taiKhoan.NgayDong - ngayHienTai;

                    // Xóa các thông báo cũ
                    ThongBao.Clear();

                    // Thêm thông báo về hạn tài khoản
                    if (khoangCach.TotalDays <= 7)
                    {
                        ThongBao.Add(new ThongBao(
                            "Cảnh báo hết hạn tài khoản",
                            $"Tài khoản sẽ hết hạn trong {khoangCach.Days} ngày. \nVui lòng gia hạn tài khoản để tiếp tục sử dụng.",
                            "warning_icon"
                        ));
                        
                    }
                    else
                    {
                        ThongBao.Add(new ThongBao(
                            "Trạng thái tài khoản",
                            "Tài khoản của bạn vẫn còn hạn sử dụng.",
                            "AccountAlert"
                        ));
                    }

                    // Thêm thông báo về khoản nợ
                    if (docGia != null && docGia.TongNo > 0)
                    {
                        var thongBaoNo = $"Tổng nợ hiện tại của bạn là {docGia.TongNo:C}.\nVui lòng đến thư viện và thanh toán.";
                        if (!ThongBao.Any(tb => tb.Message == thongBaoNo))
                        {
                            ThongBao.Add(new ThongBao(
                                "Cảnh báo nợ",
                                thongBaoNo,
                                "Cash"
                            ));
                        }
                    }
                    var phieuMuons = dbContext.PHIEUMUON
                .Include(pm => pm.CTPHIEUMUON) // Include để tải chi tiết phiếu mượn
                .Where(pm => pm.IDDocGia == docGia.ID && !pm.IsDeleted)
                .ToList();

                    foreach (var phieuMuon in phieuMuons)
                    {
                        foreach (var ctPhieuMuon in phieuMuon.CTPHIEUMUON)
                        {
                            khoangCach = ctPhieuMuon.HanTra - ngayHienTai;

                            if (khoangCach.TotalDays <= 7 && khoangCach.TotalDays >= 0)
                            {
                                ThongBao.Add(new ThongBao(
                                    "Đã đến hạn trả sách",
                                    $"Sách mượn từ phiếu {phieuMuon.MaPhieuMuon} sắp đến hạn trả trong {khoangCach.Days} ngày. Vui lòng trả sách đúng hạn.",
                                    "book_warning_icon"
                                ));
                            }
                            else if (khoangCach.TotalDays < 0)
                            {
                                ThongBao.Add(new ThongBao(
                                    "Quá hạn trả sách",
                                    $"Sách mượn từ phiếu {phieuMuon.MaPhieuMuon} đã quá hạn trả {Math.Abs(khoangCach.Days)} ngày. Vui lòng trả ngay.",
                                    "book_overdue_icon"
                                ));
                            }
                        }
                    }
                }
            }
        }
    }
}

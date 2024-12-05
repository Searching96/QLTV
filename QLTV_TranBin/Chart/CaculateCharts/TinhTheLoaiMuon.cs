using QLTV_TranBin.ModelCharts;
using QLTV_TranBin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLTV_TranBin.Chart.CaculateCharts
{
    public class TinhTheLoaiMuon
    {
        private readonly QLTV2Context _context;

        public TinhTheLoaiMuon(QLTV2Context context)
        {
            _context = context;
        }

        public List<TheLoaiMuon> GetTheLoaiMuon()
        {
            var data = _context.CTPHIEUMUON
                .Where(ct => !ct.IDSachNavigation.IsDeleted) // Lọc sách không bị xóa
                .Select(ct => new
                {
                    TheLoai = ct.IDSachNavigation.IDTuaSachNavigation.IDTheLoai // Lấy thể loại của tựa sách
                })
                .ToList(); // Lấy dữ liệu từ DB về memory (client-side)

            var result = data
                .SelectMany(d => d.TheLoai) // Flatten danh sách thể loại
                .GroupBy(tl => tl.TenTheLoai) // Nhóm theo tên thể loại
                .Select(group => new TheLoaiMuon
                {
                    TenTheLoai = group.Key,
                    SoLuongMuon = group.Count()
                })
                .ToList();

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginApp.Model
{
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

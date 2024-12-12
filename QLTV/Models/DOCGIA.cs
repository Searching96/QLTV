using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class DOCGIA
{
    public int ID { get; set; }

    public string? MaDocGia { get; set; }

    public int IDTaiKhoan { get; set; }

    public int IDLoaiDocGia { get; set; }

    public string GioiThieu { get; set; } = null!;

    public decimal TongNo { get; set; }

    public virtual LOAIDOCGIA IDLoaiDocGiaNavigation { get; set; } = null!;

    public virtual TAIKHOAN IDTaiKhoanNavigation { get; set; } = null!;

    public virtual ICollection<PHIEUMUON> PHIEUMUON { get; set; } = new List<PHIEUMUON>();

    public virtual ICollection<PHIEUTHUTIENPHAT> PHIEUTHUTIENPHAT { get; set; } = new List<PHIEUTHUTIENPHAT>();
}

using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class SACH
{
    public int ID { get; set; }

    public string? MaSach { get; set; }

    public int IDTuaSach { get; set; }

    public int NamXuatBan { get; set; }

    public string NhaXuatBan { get; set; } = null!;

    public DateTime NgayNhap { get; set; }

    public decimal TriGia { get; set; }

    public int IDTinhTrang { get; set; }

    public bool? IsAvailable { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<CTPHIEUMUON> CTPHIEUMUON { get; set; } = new List<CTPHIEUMUON>();

    public virtual ICollection<CTPHIEUTRA> CTPHIEUTRA { get; set; } = new List<CTPHIEUTRA>();

    public virtual TINHTRANG IDTinhTrangNavigation { get; set; } = null!;

    public virtual TUASACH IDTuaSachNavigation { get; set; } = null!;
}

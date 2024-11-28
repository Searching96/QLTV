using System;
using System.Collections.Generic;

namespace QLTV_TranBin.Models;

public partial class PHIEUMUON
{
    public int ID { get; set; }

    public string? MaPhieuMuon { get; set; }

    public int IDDocGia { get; set; }

    public DateTime NgayMuon { get; set; }

    public bool IsPending { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<CTPHIEUMUON> CTPHIEUMUON { get; } = new List<CTPHIEUMUON>();

    public virtual ICollection<CTPHIEUTRA> CTPHIEUTRA { get; } = new List<CTPHIEUTRA>();

    public virtual DOCGIA IDDocGiaNavigation { get; set; } = null!;
}

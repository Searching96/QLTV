using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class CTPHIEUMUON
{
    public int IDPhieuMuon { get; set; }

    public int IDSach { get; set; }

    public DateTime HanTra { get; set; }

    public string TinhTrangMuon { get; set; } = null!;

    public virtual PHIEUMUON IDPhieuMuonNavigation { get; set; } = null!;

    public virtual SACH IDSachNavigation { get; set; } = null!;
}

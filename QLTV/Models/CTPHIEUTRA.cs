using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class CTPHIEUTRA
{
    public int IDPhieuTra { get; set; }

    public int IDPhieuMuon { get; set; }

    public int IDSach { get; set; }

    public int SoNgayMuon { get; set; }

    public decimal TienPhat { get; set; }

    public string TinhTrangTra { get; set; } = null!;

    public string GhiChu { get; set; } = null!;

    public virtual PHIEUMUON IDPhieuMuonNavigation { get; set; } = null!;

    public virtual PHIEUTRA IDPhieuTraNavigation { get; set; } = null!;

    public virtual SACH IDSachNavigation { get; set; } = null!;
}

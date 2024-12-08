using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class DANHGIA
{
    public int ID { get; set; }

    public int IDSach { get; set; }

    public int IDPhieuMuon { get; set; }

    public decimal DanhGia1 { get; set; }

    public virtual PHIEUMUON IDPhieuMuonNavigation { get; set; } = null!;

    public virtual SACH IDSachNavigation { get; set; } = null!;
}

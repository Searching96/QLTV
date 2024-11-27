using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class CTBCMUONSACH
{
    public int IDBCMuonSach { get; set; }

    public int IDTheLoai { get; set; }

    public int SoLuotMuon { get; set; }

    public double TiLe { get; set; }

    public virtual BCMUONSACH IDBCMuonSachNavigation { get; set; } = null!;

    public virtual THELOAI IDTheLoaiNavigation { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class CTBCTRATRE
{
    public int IDBCTraTre { get; set; }

    public int IDPhieuTra { get; set; }

    public int SoNgayTraTre { get; set; }

    public virtual BCTRATRE IDBCTraTreNavigation { get; set; } = null!;

    public virtual PHIEUTRA IDPhieuTraNavigation { get; set; } = null!;
}

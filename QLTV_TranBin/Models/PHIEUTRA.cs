using System;
using System.Collections.Generic;

namespace QLTV_TranBin.Models;

public partial class PHIEUTRA
{
    public int ID { get; set; }

    public string? MaPhieuTra { get; set; }

    public DateTime NgayTra { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<CTBCTRATRE> CTBCTRATRE { get; } = new List<CTBCTRATRE>();

    public virtual ICollection<CTPHIEUTRA> CTPHIEUTRA { get; } = new List<CTPHIEUTRA>();
}

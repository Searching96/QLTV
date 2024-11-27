using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class PHIEUTRA
{
    public int ID { get; set; }

    public string? MaPhieuTra { get; set; }

    public DateTime NgayTra { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<CTBCTRATRE> CTBCTRATRE { get; set; } = new List<CTBCTRATRE>();

    public virtual ICollection<CTPHIEUTRA> CTPHIEUTRA { get; set; } = new List<CTPHIEUTRA>();
}

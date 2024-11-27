using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class BCMUONSACH
{
    public int ID { get; set; }

    public string? MaBCMuonSach { get; set; }

    public DateTime Thang { get; set; }

    public int TongSoLuotMuon { get; set; }

    public virtual ICollection<CTBCMUONSACH> CTBCMUONSACH { get; set; } = new List<CTBCMUONSACH>();
}

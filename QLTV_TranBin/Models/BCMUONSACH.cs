using System;
using System.Collections.Generic;

namespace QLTV_TranBin.Models;

public partial class BCMUONSACH
{
    public int ID { get; set; }

    public string? MaBCMuonSach { get; set; }

    public DateTime Thang { get; set; }

    public int TongSoLuotMuon { get; set; }

    public virtual ICollection<CTBCMUONSACH> CTBCMUONSACH { get; } = new List<CTBCMUONSACH>();
}

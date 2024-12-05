using System;
using System.Collections.Generic;

namespace QLTV_TranBin.Models;

public partial class BCTRATRE
{
    public int ID { get; set; }

    public string? MaBCTraTre { get; set; }

    public DateTime Ngay { get; set; }

    public virtual ICollection<CTBCTRATRE> CTBCTRATRE { get; } = new List<CTBCTRATRE>();
}

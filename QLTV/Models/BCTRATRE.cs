using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class BCTRATRE
{
    public int ID { get; set; }

    public string? MaBCTraTre { get; set; }

    public DateTime Ngay { get; set; }

    public virtual ICollection<CTBCTRATRE> CTBCTRATRE { get; set; } = new List<CTBCTRATRE>();
}

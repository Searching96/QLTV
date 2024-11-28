using System;
using System.Collections.Generic;

namespace QLTV_TranBin.Models;

public partial class THELOAI
{
    public int ID { get; set; }

    public string? MaTheLoai { get; set; }

    public string TenTheLoai { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<CTBCMUONSACH> CTBCMUONSACH { get; } = new List<CTBCMUONSACH>();

    public virtual ICollection<TUASACH> IDTuaSach { get; } = new List<TUASACH>();
}

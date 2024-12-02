using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class THELOAI
{
    public int ID { get; set; }

    public string? MaTheLoai { get; set; }

    public string TenTheLoai { get; set; } = null!;

    public string MoTa { get; set; } = null;

    public bool IsDeleted { get; set; }

    public virtual ICollection<CTBCMUONSACH> CTBCMUONSACH { get; set; } = new List<CTBCMUONSACH>();

    public virtual ICollection<TUASACH_THELOAI> TUASACH_THELOAI { get; set; } = new List<TUASACH_THELOAI>();
}

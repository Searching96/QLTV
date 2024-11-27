using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class TUASACH_THELOAI
{
    public int IDTuaSach { get; set; }

    public int IDTheLoai { get; set; }

    public int? Dummy { get; set; }

    public virtual THELOAI IDTheLoaiNavigation { get; set; } = null!;

    public virtual TUASACH IDTuaSachNavigation { get; set; } = null!;
}

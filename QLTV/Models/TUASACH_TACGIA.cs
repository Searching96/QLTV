using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class TUASACH_TACGIA
{
    public int IDTuaSach { get; set; }

    public int IDTacGia { get; set; }

    public int? Dummy { get; set; }

    public virtual TACGIA IDTacGiaNavigation { get; set; } = null!;

    public virtual TUASACH IDTuaSachNavigation { get; set; } = null!;
}

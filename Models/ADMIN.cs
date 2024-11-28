using System;
using System.Collections.Generic;

namespace QLTV_TranBin.Models;

public partial class ADMIN
{
    public int ID { get; set; }

    public string? MaAdmin { get; set; }

    public int IDTaiKhoan { get; set; }

    public string? TenAdmin { get; set; }

    public string? GioiTinh { get; set; }

    public virtual TAIKHOAN IDTaiKhoanNavigation { get; set; } = null!;
}

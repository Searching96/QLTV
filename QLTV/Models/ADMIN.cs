using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class ADMIN
{
    public int ID { get; set; }

    public string? MaAdmin { get; set; }

    public int IDTaiKhoan { get; set; }

    public DateTime NgayVaoLam { get; set; }

    public DateTime NgayKetThuc { get; set; }

    public virtual TAIKHOAN IDTaiKhoanNavigation { get; set; } = null!;
}

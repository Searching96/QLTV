using System;
using System.Collections.Generic;

namespace QLTV_TranBin.Models;

public partial class TINHTRANG
{
    public int ID { get; set; }

    public string? MaTinhTrang { get; set; }

    public string TenTinhTrang { get; set; } = null!;

    public int MucHuHong { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<SACH> SACH { get; } = new List<SACH>();
}

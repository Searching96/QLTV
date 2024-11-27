using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class PHIEUTHUTIENPHAT
{
    public int ID { get; set; }

    public string? MaPTTP { get; set; }

    public int IDDocGia { get; set; }

    public DateTime NgayThu { get; set; }

    public decimal TongNo { get; set; }

    public decimal SoTienThu { get; set; }

    public decimal ConLai { get; set; }

    public bool IsDeleted { get; set; }

    public virtual DOCGIA IDDocGiaNavigation { get; set; } = null!;
}

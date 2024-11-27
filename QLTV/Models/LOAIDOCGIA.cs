using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class LOAIDOCGIA
{
    public int ID { get; set; }

    public string? MaLoaiDocGia { get; set; }

    public string? TenLoaiDocGia { get; set; }

    public int SoSachMuonToiDa { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<DOCGIA> DOCGIA { get; set; } = new List<DOCGIA>();
}

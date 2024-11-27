using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class TACGIA
{
    public int ID { get; set; }

    public string? MaTacGia { get; set; }

    public string TenTacGia { get; set; } = null!;

    public int NamSinh { get; set; }

    public string QuocTich { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<TUASACH_TACGIA> TUASACH_TACGIA { get; set; } = new List<TUASACH_TACGIA>();
}

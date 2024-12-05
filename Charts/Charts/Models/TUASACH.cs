using System;
using System.Collections.Generic;

namespace QLTV_TranBin.Models;

public partial class TUASACH
{
    public int ID { get; set; }

    public string? MaTuaSach { get; set; }

    public string TenTuaSach { get; set; } = null!;

    public string BiaSach { get; set; } = null!;

    public int SoLuong { get; set; }

    public int HanMuonToiDa { get; set; }

    public bool IsDeleted { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<SACH> SACH { get; } = new List<SACH>();

    public virtual ICollection<TACGIA> IDTacGia { get; } = new List<TACGIA>();

    public virtual ICollection<THELOAI> IDTheLoai { get; } = new List<THELOAI>();
}

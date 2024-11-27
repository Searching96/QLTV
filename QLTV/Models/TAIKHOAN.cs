using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class TAIKHOAN
{
    public int ID { get; set; }

    public string? MaTaiKhoan { get; set; }

    public string MatKhau { get; set; } = null!;

    public string TenTaiKhoan { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime SinhNhat { get; set; }

    public string DiaChi { get; set; } = null!;

    public string SDT { get; set; } = null!;

    public string Avatar { get; set; } = null!;

    public bool TrangThai { get; set; }

    public int IDPhanQuyen { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<ADMIN> ADMIN { get; set; } = new List<ADMIN>();

    public virtual ICollection<DOCGIA> DOCGIA { get; set; } = new List<DOCGIA>();

    public virtual PHANQUYEN IDPhanQuyenNavigation { get; set; } = null!;
}

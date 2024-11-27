using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLTV.Models;

public partial class QLTVContext : DbContext
{
    public QLTVContext()
    {
    }

    public QLTVContext(DbContextOptions<QLTVContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ADMIN> ADMIN { get; set; }

    public virtual DbSet<BCMUONSACH> BCMUONSACH { get; set; }

    public virtual DbSet<BCTRATRE> BCTRATRE { get; set; }

    public virtual DbSet<CTBCMUONSACH> CTBCMUONSACH { get; set; }

    public virtual DbSet<CTBCTRATRE> CTBCTRATRE { get; set; }

    public virtual DbSet<CTPHIEUMUON> CTPHIEUMUON { get; set; }

    public virtual DbSet<CTPHIEUTRA> CTPHIEUTRA { get; set; }

    public virtual DbSet<DOCGIA> DOCGIA { get; set; }

    public virtual DbSet<LOAIDOCGIA> LOAIDOCGIA { get; set; }

    public virtual DbSet<PHANQUYEN> PHANQUYEN { get; set; }

    public virtual DbSet<PHIEUMUON> PHIEUMUON { get; set; }

    public virtual DbSet<PHIEUTHUTIENPHAT> PHIEUTHUTIENPHAT { get; set; }

    public virtual DbSet<PHIEUTRA> PHIEUTRA { get; set; }

    public virtual DbSet<SACH> SACH { get; set; }

    public virtual DbSet<TACGIA> TACGIA { get; set; }

    public virtual DbSet<TAIKHOAN> TAIKHOAN { get; set; }

    public virtual DbSet<THAMSO> THAMSO { get; set; }

    public virtual DbSet<THELOAI> THELOAI { get; set; }

    public virtual DbSet<TINHTRANG> TINHTRANG { get; set; }

    public virtual DbSet<TUASACH> TUASACH { get; set; }

    public virtual DbSet<TUASACH_TACGIA> TUASACH_TACGIA { get; set; }

    public virtual DbSet<TUASACH_THELOAI> TUASACH_THELOAI { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-S9VDQ6Q1;Database=QLTV;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ADMIN>(entity =>
        {
            entity.Property(e => e.MaAdmin)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('AD'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.NgayKetThuc).HasColumnType("date");
            entity.Property(e => e.NgayVaoLam).HasColumnType("date");

            entity.HasOne(d => d.IDTaiKhoanNavigation).WithMany(p => p.ADMIN)
                .HasForeignKey(d => d.IDTaiKhoan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ADMIN_IDTaiKhoan");
        });

        modelBuilder.Entity<BCMUONSACH>(entity =>
        {
            entity.Property(e => e.MaBCMuonSach)
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasComputedColumnSql("('BCMS'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.Thang).HasColumnType("date");
        });

        modelBuilder.Entity<BCTRATRE>(entity =>
        {
            entity.Property(e => e.MaBCTraTre)
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasComputedColumnSql("('BCTT'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.Ngay).HasColumnType("date");
        });

        modelBuilder.Entity<CTBCMUONSACH>(entity =>
        {
            entity.HasKey(e => new { e.IDBCMuonSach, e.IDTheLoai });

            entity.HasOne(d => d.IDBCMuonSachNavigation).WithMany(p => p.CTBCMUONSACH)
                .HasForeignKey(d => d.IDBCMuonSach)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTBCMUONSACH_IDBCMuonSach");

            entity.HasOne(d => d.IDTheLoaiNavigation).WithMany(p => p.CTBCMUONSACH)
                .HasForeignKey(d => d.IDTheLoai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTBCMUONSACH_IDTheLoai");
        });

        modelBuilder.Entity<CTBCTRATRE>(entity =>
        {
            entity.HasKey(e => new { e.IDBCTraTre, e.IDPhieuTra });

            entity.HasOne(d => d.IDBCTraTreNavigation).WithMany(p => p.CTBCTRATRE)
                .HasForeignKey(d => d.IDBCTraTre)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTBCTRATRE_IDBCTraTre");

            entity.HasOne(d => d.IDPhieuTraNavigation).WithMany(p => p.CTBCTRATRE)
                .HasForeignKey(d => d.IDPhieuTra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTBCTRATRE_IDPhieuTra");
        });

        modelBuilder.Entity<CTPHIEUMUON>(entity =>
        {
            entity.HasKey(e => new { e.IDPhieuMuon, e.IDSach });

            entity.Property(e => e.HanTra).HasColumnType("datetime");
            entity.Property(e => e.TinhTrangMuon).HasMaxLength(100);

            entity.HasOne(d => d.IDPhieuMuonNavigation).WithMany(p => p.CTPHIEUMUON)
                .HasForeignKey(d => d.IDPhieuMuon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTPHIEUMUON_IDPhieuMuon");

            entity.HasOne(d => d.IDSachNavigation).WithMany(p => p.CTPHIEUMUON)
                .HasForeignKey(d => d.IDSach)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTPHIEUMUON_IDSach");
        });

        modelBuilder.Entity<CTPHIEUTRA>(entity =>
        {
            entity.HasKey(e => new { e.IDPhieuTra, e.IDPhieuMuon, e.IDSach });

            entity.Property(e => e.GhiChu).HasMaxLength(200);
            entity.Property(e => e.TienPhat).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TinhTrangTra).HasMaxLength(100);

            entity.HasOne(d => d.IDPhieuMuonNavigation).WithMany(p => p.CTPHIEUTRA)
                .HasForeignKey(d => d.IDPhieuMuon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTPHIEUTRA_IDPhieuMuon");

            entity.HasOne(d => d.IDPhieuTraNavigation).WithMany(p => p.CTPHIEUTRA)
                .HasForeignKey(d => d.IDPhieuTra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTPHIEUTRA_IDPhieuTra");

            entity.HasOne(d => d.IDSachNavigation).WithMany(p => p.CTPHIEUTRA)
                .HasForeignKey(d => d.IDSach)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTPHIEUTRA_IDSach");
        });

        modelBuilder.Entity<DOCGIA>(entity =>
        {
            entity.Property(e => e.GioiThieu).HasMaxLength(500);
            entity.Property(e => e.MaDocGia)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('DG'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.NgayHetHan).HasColumnType("date");
            entity.Property(e => e.NgayLapThe).HasColumnType("date");
            entity.Property(e => e.TongNo).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.IDLoaiDocGiaNavigation).WithMany(p => p.DOCGIA)
                .HasForeignKey(d => d.IDLoaiDocGia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DOCGIA_IDLoaiDocGia");

            entity.HasOne(d => d.IDTaiKhoanNavigation).WithMany(p => p.DOCGIA)
                .HasForeignKey(d => d.IDTaiKhoan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DOCGIA_IDTaiKhoan");
        });

        modelBuilder.Entity<LOAIDOCGIA>(entity =>
        {
            entity.Property(e => e.MaLoaiDocGia)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasComputedColumnSql("('LDG'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", false);
            entity.Property(e => e.TenLoaiDocGia).HasMaxLength(100);
        });

        modelBuilder.Entity<PHANQUYEN>(entity =>
        {
            entity.Property(e => e.MaHanhDong)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MaPhanQuyen)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('PQ'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.MoTa).HasMaxLength(100);
        });

        modelBuilder.Entity<PHIEUMUON>(entity =>
        {
            entity.Property(e => e.MaPhieuMuon)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('PM'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.NgayMuon).HasColumnType("datetime");

            entity.HasOne(d => d.IDDocGiaNavigation).WithMany(p => p.PHIEUMUON)
                .HasForeignKey(d => d.IDDocGia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PHIEUMUON_IDDocGia");
        });

        modelBuilder.Entity<PHIEUTHUTIENPHAT>(entity =>
        {
            entity.Property(e => e.ConLai).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.MaPTTP)
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasComputedColumnSql("('PTTP'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.NgayThu).HasColumnType("datetime");
            entity.Property(e => e.SoTienThu).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TongNo).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.IDDocGiaNavigation).WithMany(p => p.PHIEUTHUTIENPHAT)
                .HasForeignKey(d => d.IDDocGia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PHIEUTHUTIENPHAT_IDDocGia");
        });

        modelBuilder.Entity<PHIEUTRA>(entity =>
        {
            entity.Property(e => e.MaPhieuTra)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('PT'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.NgayTra).HasColumnType("datetime");
        });

        modelBuilder.Entity<SACH>(entity =>
        {
            entity.Property(e => e.IsAvailable)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.MaSach)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasComputedColumnSql("('S'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.NgayNhap).HasColumnType("date");
            entity.Property(e => e.NhaXuatBan).HasMaxLength(100);
            entity.Property(e => e.TriGia).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.IDTinhTrangNavigation).WithMany(p => p.SACH)
                .HasForeignKey(d => d.IDTinhTrang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SACH_IDTinhTrang");

            entity.HasOne(d => d.IDTuaSachNavigation).WithMany(p => p.SACH)
                .HasForeignKey(d => d.IDTuaSach)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SACH_IDTuaSach");
        });

        modelBuilder.Entity<TACGIA>(entity =>
        {
            entity.Property(e => e.MaTacGia)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('TG'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.QuocTich).HasMaxLength(100);
            entity.Property(e => e.TenTacGia).HasMaxLength(100);
        });

        modelBuilder.Entity<TAIKHOAN>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Email").IsUnique();

            entity.HasIndex(e => e.TenTaiKhoan, "UQ_TenTaiKhoan").IsUnique();

            entity.Property(e => e.Avatar)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MaTaiKhoan)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('TK'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.MatKhau)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SDT)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SinhNhat).HasColumnType("datetime");
            entity.Property(e => e.TenTaiKhoan).HasMaxLength(100);

            entity.HasOne(d => d.IDPhanQuyenNavigation).WithMany(p => p.TAIKHOAN)
                .HasForeignKey(d => d.IDPhanQuyen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TAIKHOAN_IDPhanQuyen");
        });

        modelBuilder.Entity<THAMSO>(entity =>
        {
            entity.Property(e => e.TienPhatTraTreMotNgay).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<THELOAI>(entity =>
        {
            entity.Property(e => e.MaTheLoai)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('TL'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.TenTheLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<TINHTRANG>(entity =>
        {
            entity.Property(e => e.MaTinhTrang)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('TT'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.TenTinhTrang).HasMaxLength(100);
        });

        modelBuilder.Entity<TUASACH>(entity =>
        {
            entity.Property(e => e.BiaSach)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasDefaultValueSql("('https://upload.wikimedia.org/wikipedia/commons/thumb/a/ac/No_image_available.svg/800px-No_image_available.svg.png')");
            entity.Property(e => e.MaTuaSach)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('TS'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.TenTuaSach).HasMaxLength(100);
        });

        modelBuilder.Entity<TUASACH_TACGIA>(entity =>
        {
            entity.HasKey(e => new { e.IDTuaSach, e.IDTacGia });

            entity.Property(e => e.Dummy).HasDefaultValueSql("((0))");

            entity.HasOne(d => d.IDTacGiaNavigation).WithMany(p => p.TUASACH_TACGIA)
                .HasForeignKey(d => d.IDTacGia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TUASACH_TACGIA_IDTacGia");

            entity.HasOne(d => d.IDTuaSachNavigation).WithMany(p => p.TUASACH_TACGIA)
                .HasForeignKey(d => d.IDTuaSach)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TUASACH_TACGIA_IDTuaSach");
        });

        modelBuilder.Entity<TUASACH_THELOAI>(entity =>
        {
            entity.HasKey(e => new { e.IDTuaSach, e.IDTheLoai });

            entity.Property(e => e.Dummy).HasDefaultValueSql("((0))");

            entity.HasOne(d => d.IDTheLoaiNavigation).WithMany(p => p.TUASACH_THELOAI)
                .HasForeignKey(d => d.IDTheLoai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TUASACH_THELOAI_IDTheLoai");

            entity.HasOne(d => d.IDTuaSachNavigation).WithMany(p => p.TUASACH_THELOAI)
                .HasForeignKey(d => d.IDTuaSach)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TUASACH_THELOAI_IDTuaSach");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

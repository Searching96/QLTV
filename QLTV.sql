USE master;
GO
ALTER DATABASE QLTV SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
DROP DATABASE QLTV;
GO

create database [QLTV]
go
use [QLTV]
go

set dateformat dmy
go

create table [ACTIVE_SESSION]
(
	[ID] int identity(1, 1) constraint [PK_ACTIVESESSION] primary key,
	[IDTaiKhoan] int not null,
	[SessionToken] nvarchar(255) not null,
	[ExpiryTime] datetime not null,
)

create table [PHANQUYEN]
(
	[ID] int identity(1, 1) constraint [PK_PHANQUYEN] primary key,
	[MaPhanQuyen] as ('PQ' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
	[MaHanhDong] varchar(100) not null,
	[MoTa] nvarchar(100) not null,
	[IsDeleted] bit not null,
)

create table [LOAIDOCGIA]
(
    [ID] int identity(1, 1) constraint [PK_LOAIDOCGIA] primary key,
    [MaLoaiDocGia] as ('LDG' + right('00000' + cast([ID] as varchar(5)), 5)),
    [TenLoaiDocGia] nvarchar(100),
    [SoSachMuonToiDa] int not null,
    [IsDeleted] bit not null
)

create table [TAIKHOAN]
(
	[ID] int identity(1, 1) constraint [PK_TAIKHOAN] primary key,
	[MaTaiKhoan] as ('TK' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
	[TenTaiKhoan] nvarchar(100) constraint [UQ_TenTaiKhoan] unique not null,
	[MatKhau] varchar(50) not null,
	[HoTen] nvarchar(100) not null,
	[Email] varchar(100) constraint [UQ_Email] unique not null,
	[GioiTinh] nvarchar(5) not null,
	[SinhNhat] date not null,
	[DiaChi] nvarchar(200) not null,
	[SDT] varchar(20) not null,
	[Avatar] nvarchar(500) not null DEFAULT 'D:\WPF\QLTV\QLTV\Image\DefaultAvatar.png',
	[TrangThai] bit not null,
	[IDPhanQuyen] int not null, -- FK
	[IsDeleted] bit not null,
	[NgayMo] date not null,
	[NgayDong] date not null
)

create table [ADMIN] 
(
	[ID] int identity(1, 1) constraint [PK_ADMIN] primary key,
	[MaAdmin] as ('AD' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
	[IDTaiKhoan] int not null, -- FK 
)

create table [DOCGIA]
(
    [ID] int identity(1, 1) constraint [PK_DOCGIA] primary key,
    [MaDocGia] as ('DG' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,	
    [IDTaiKhoan] int not null, -- FK
    [IDLoaiDocGia] int not null, -- FK
    [GioiThieu] nvarchar(500) not null,
    [TongNo] decimal(18, 0) not null,
)

create table [TACGIA]
(
    [ID] int identity(1, 1) constraint [PK_TACGIA] primary key,
    [MaTacGia] as ('TG' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
	[TenTacGia] nvarchar(100) not null,
    [NamSinh] int not null,
    [QuocTich] nvarchar(100) not null,
    [IsDeleted] bit not null
)

create table [THELOAI]
(
    [ID] int identity(1, 1) constraint [PK_THELOAI] primary key,
    [MaTheLoai] as ('TL' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
    [TenTheLoai] nvarchar(100) not null,
	[MoTa] nvarchar(500) not null,
    [IsDeleted] bit not null
)

create table [TUASACH]
(
    [ID] int identity(1, 1) constraint [PK_TUASACH] primary key,
    [MaTuaSach] as ('TS' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
    [TenTuaSach] nvarchar(100) not null,
    [BiaSach] varchar(500) not null,
    [SoLuong] int not null, -- SoLuong phai bang so luong sach co ma tua sach nay
    [HanMuonToiDa] int not null, -- don vi: tuan
    [MoTa] nvarchar(1500) not null,
    [IsDeleted] bit not null
)

create table [TUASACH_TACGIA]
(
    [IDTuaSach] int not null,
    [IDTacGia] int not null,
    constraint [PK_TUASACH_TACGIA] primary key([IDTuaSach], [IDTacGia])
)

create table [TUASACH_THELOAI]
(
    [IDTuaSach] int not null,
    [IDTheLoai] int not null,
    constraint [PK_TUASACH_THELOAI] primary key([IDTuaSach], [IDTheLoai])
)

create table [TINHTRANG]
(
    [ID] int identity(1, 1) constraint [PK_TINHTRANG] primary key,
    [MaTinhTrang] as ('TT' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
    [TenTinhTrang] nvarchar(100) not null,
    [MucHuHong] int not null,
    [IsDeleted] bit not null
)

create table [SACH]
(
    [ID] int identity(1, 1) constraint [PK_SACH] primary key,
    [MaSach] as ('S' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
    [IDTuaSach] int not null, -- FK
    [NamXuatBan] int not null,
    [NhaXuatBan] nvarchar(100) not null,
    [NgayNhap] date not null,
    [TriGia] decimal(18, 0) not null,
	[ViTri] nvarchar(20) not null,
    [IDTinhTrang] int not null, -- FK
    [IsAvailable] bit not null,
    [IsDeleted] bit not null
)

create table [PHIEUMUON]
(
    [ID] int identity(1, 1) constraint [PK_PHIEUMUON] primary key,
    [MaPhieuMuon] as ('PM' + right('00000' + cast ([ID] as varchar(5)), 5)) persisted,
    [IDDocGia] int not null, -- FK
    [NgayMuon] datetime not null,
    [IsPending] bit not null,
    [IsDeleted] bit not null
)

create table [CTPHIEUMUON]
(
    [IDPhieuMuon] int not null, -- FK
    [IDSach] int not null, -- FK
    constraint [PK_CTPHIEUMUON] primary key([IDPhieuMuon], [IDSach]),
    [HanTra] datetime not null,
    [IDTinhTrangMuon] int not null 
)

create table [PHIEUTRA]
(
    [ID] int identity(1, 1) constraint [PK_PHIEUTRA] primary key,
    [MaPhieuTra] as ('PT' + right('00000' + cast ([ID] as varchar(5)), 5)) persisted,
    [NgayTra] datetime not null,
    [IsDeleted] bit not null
)

create table [CTPHIEUTRA]
(
    [IDPhieuTra] int not null,
    [IDPhieuMuon] int not null,
    [IDSach] int not null,
    constraint [PK_CTPHIEUTRA] primary key([IDPhieuTra], [IDPhieuMuon], [IDSach]),
    [SoNgayMuon] int not null,
    [TienPhat] decimal(18, 0) not null,
    [IDTinhTrangTra] int not null,
    [GhiChu] nvarchar(200) not null
)

create table [BCTRATRE]
(
    [ID] int identity(1, 1) constraint [PK_BCTRATRE] primary key,
    [MaBCTraTre] as ('BCTT' + right('00000' + cast ([ID] as varchar(5)), 5)) persisted,
    [Ngay] date not null
)

create table [CTBCTRATRE]
(
    [IDBCTraTre] int not null,
    [IDPhieuTra] int not null,
    constraint [PK_CTBCTRATRE] primary key([IDBCTraTre], [IDPhieuTra]),
    [SoNgayTraTre] int not null
)

create table [BCMUONSACH]
(
    [ID] int identity(1, 1) constraint [PK_BCMUONSACH] primary key,
    [MaBCMuonSach] as ('BCMS' + right('00000' + cast ([ID] as varchar(5)), 5)) persisted,
    [Thang] date not null, -- vd: 11/2024
    [TongSoLuotMuon] int not null
)

create table [CTBCMUONSACH]
(
    [IDBCMuonSach] int not null,
    [IDTheLoai] int not null,
    constraint [PK_CTBCMUONSACH] primary key([IDBCMuonSach], [IDTheLoai]),
    [SoLuotMuon] int not null,
    [TiLe] float not null
)

create table [PHIEUTHUTIENPHAT]
(
    [ID] int identity(1, 1) constraint [PK_PHIEUTHUTIENPHAT] primary key,
    [MaPTTP] as ('PTTP' + right('00000' + cast ([ID] as varchar(5)), 5)) persisted,
    [IDDocGia] int not null,
    [NgayThu] datetime not null,
    [TongNo] decimal(18, 0) not null,
    [SoTienThu] decimal(18, 0) not null,
    [ConLai] decimal(18, 0) not null,
	[IsDeleted] bit not null
)

create table [THAMSO]
(
    [ID] int identity(1, 1) constraint [PK_THAMSO] primary key,
	[ThoiGian] datetime not null,
    [TuoiToiThieu] int not null,
    [TienPhatTraTreMotNgay] decimal(18, 0) not null,
	[TiLeDenBu] decimal(4, 2) not null
)

create table [DANHGIA]
(
	[ID] int identity(1,1) constraint [PK_DANHGIA] primary key,
	[IDSach] int not null,
	[IDPhieuMuon] int not null,
	[DanhGia] decimal(4, 2) not null
)

-- Dummy: delete when done
alter table TUASACH_TACGIA add Dummy int;
alter table TUASACH_TACGIA add constraint DF_TSTG_Dummy default 0 for Dummy;
alter table TUASACH_THELOAI add Dummy int;
alter table TUASACH_THELOAI add constraint DF_TSTL_Dummy default 0 for Dummy;

-- default BiaSach
alter table [TUASACH]
ADD CONSTRAINT [DF_BiaSach] 
DEFAULT 'https://upload.wikimedia.org/wikipedia/commons/thumb/a/ac/No_image_available.svg/800px-No_image_available.svg.png' 
FOR [BiaSach];

-- default TongNo
alter table [DOCGIA]
add constraint [DF_DOCGIA_TongNo] default 0 for [TongNo];

-- default HanMuoiToiDa
alter table [TUASACH]
add constraint [DF_TUASACH_HanMuonToiDa] default 2 for [HanMuonToiDa];

-- default SoLuong 
alter table [TUASACH]
add constraint [DF_TUASACH_SoLuong] default 0 for [SoLuong];

-- default IsAvailable
alter table [SACH]
add constraint [DF_SACH_IsAvailable] default 1 for [IsAvailable];

-- default IsDeleted
alter table [PHANQUYEN]
add constraint [DF_PHANQUYEN_IsDeleted] default 0 for [IsDeleted];

alter table [LOAIDOCGIA]
add constraint [DF_LOAIDOCGIA_IsDeleted] default 0 for [IsDeleted];

alter table [TAIKHOAN]
add constraint [DF_TAIKHOAN_IsDeleted] default 0 for [IsDeleted];

alter table [TACGIA]
add constraint [DF_TACGIA_IsDeleted] default 0 for [IsDeleted];

alter table [THELOAI]
add constraint [DF_THELOAI_IsDeleted] default 0 for [IsDeleted];

alter table [TUASACH]
add constraint [DF_TUASACH_IsDeleted] default 0 for [IsDeleted];

alter table [TINHTRANG]
add constraint [DF_TINHTRANG_IsDeleted] default 0 for [IsDeleted];

alter table [SACH]
add constraint [DF_SACH_IsDeleted] default 0 for [IsDeleted];

alter table [PHIEUMUON]
add constraint [DF_PHIEUMUON_IsDeleted] default 0 for [IsDeleted];

alter table [PHIEUTRA]
add constraint [DF_PHIEUTRA_IsDeleted] default 0 for [IsDeleted];

alter table [PHIEUTHUTIENPHAT]
add constraint [DF_PHIEUTHUTIENPHAT_IsDeleted] default 0 for [IsDeleted];


-- FK
alter table [ACTIVE_SESSION] add constraint [FK_ACTIVESESSION_TAIKHOAN]
foreign key ([IDTaiKhoan]) references [TAIKHOAN]([ID]);

alter table [CTPHIEUMUON] add constraint [FK_CTPHIEUMUON_IDTinhTrangMuon]
foreign key ([IDTinhTrangMuon]) references [TINHTRANG]([ID]);

alter table [CTPHIEUTRA] add constraint [FK_CTPHIEUTRA_IDTinhTrangTra]
foreign key ([IDTinhTrangTra]) references [TINHTRANG]([ID]);

alter table [DANHGIA] add constraint [FK_DANHGIA_IDSach]
foreign key ([IDSach]) references [SACH]([ID]);

alter table [DANHGIA] add constraint [FK_DANHGIA_IDPhieuMuon]
foreign key ([IDPhieuMuon]) references [PHIEUMUON]([ID]);

alter table [TAIKHOAN] add constraint [FK_TAIKHOAN_IDPhanQuyen]
foreign key ([IDPhanQuyen]) references [PHANQUYEN]([ID]);

alter table [ADMIN] add constraint [FK_ADMIN_IDTaiKhoan]
foreign key ([IDTaiKhoan]) references [TAIKHOAN]([ID]);

alter table [DOCGIA] add constraint [FK_DOCGIA_IDTaiKhoan]
foreign key ([IDTaiKhoan]) references [TAIKHOAN]([ID]);

alter table [DOCGIA] add constraint [FK_DOCGIA_IDLoaiDocGia]
foreign key ([IDLoaiDocGia]) references [LOAIDOCGIA]([ID]);

alter table [TUASACH_TACGIA] add constraint [FK_TUASACH_TACGIA_IDTuaSach]
foreign key ([IDTuaSach]) references [TUASACH]([ID]);

alter table [TUASACH_TACGIA] add constraint [FK_TUASACH_TACGIA_IDTacGia]
foreign key ([IDTacGia]) references [TACGIA]([ID]);

alter table [TUASACH_THELOAI] add constraint [FK_TUASACH_THELOAI_IDTuaSach]
foreign key ([IDTuaSach]) references [TUASACH]([ID]);

alter table [TUASACH_THELOAI] add constraint [FK_TUASACH_THELOAI_IDTheLoai]
foreign key ([IDTheLoai]) references [THELOAI]([ID]);

alter table [SACH] add constraint [FK_SACH_IDTuaSach]
foreign key ([IDTuaSach]) references [TUASACH]([ID]);

alter table [SACH] add constraint [FK_SACH_IDTinhTrang]
foreign key ([IDTinhTrang]) references [TINHTRANG]([ID]);

alter table [PHIEUMUON] add constraint [FK_PHIEUMUON_IDDocGia]
foreign key ([IDDocGia]) references [DOCGIA]([ID]);

alter table [CTPHIEUMUON] add constraint [FK_CTPHIEUMUON_IDPhieuMuon]
foreign key ([IDPhieuMuon]) references [PHIEUMUON]([ID]);

alter table [CTPHIEUMUON] add constraint [FK_CTPHIEUMUON_IDSach]
foreign key ([IDSach]) references [SACH]([ID]);

alter table [CTPHIEUTRA] add constraint [FK_CTPHIEUTRA_IDPhieuTra]
foreign key ([IDPhieuTra]) references [PHIEUTRA]([ID]);

alter table [CTPHIEUTRA] add constraint [FK_CTPHIEUTRA_IDPhieuMuon]
foreign key ([IDPhieuMuon]) references [PHIEUMUON]([ID]);

alter table [CTPHIEUTRA] add constraint [FK_CTPHIEUTRA_IDSach]
foreign key ([IDSach]) references [SACH]([ID]);

alter table [CTBCTRATRE] add constraint [FK_CTBCTRATRE_IDBCTraTre]
foreign key ([IDBCTraTre]) references [BCTRATRE]([ID]);

alter table [CTBCTRATRE] add constraint [FK_CTBCTRATRE_IDPhieuTra]
foreign key ([IDPhieuTra]) references [PHIEUTRA]([ID]);

alter table [CTBCMUONSACH] add constraint [FK_CTBCMUONSACH_IDBCMuonSach]
foreign key ([IDBCMuonSach]) references [BCMUONSACH]([ID]);

alter table [CTBCMUONSACH] add constraint [FK_CTBCMUONSACH_IDTheLoai]
foreign key ([IDTheLoai]) references [THELOAI]([ID]);

alter table [PHIEUTHUTIENPHAT] add constraint [FK_PHIEUTHUTIENPHAT_IDDocGia]
foreign key ([IDDocGia]) references [DOCGIA]([ID]);

INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES (N'William Shakespeare', 1564, 'England');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES (N'Jane Austen', 1775, 'England');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES (N'Mark Twain', 1835, 'United States');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES (N'Leo Tolstoy', 1828, 'Russia');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES (N'Victor Hugo', 1802, 'France');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES (N'Gabriel Garcia Marquez', 1927, 'Colombia');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES (N'Haruki Murakami', 1949, 'Japan');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES (N'Nguyễn Du', 1766, 'Vietnam');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES (N'Chinua Achebe', 1930, 'Nigeria');

INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Văn học cổ điển', N'Những tác phẩm văn học thời xưa');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Khoa học viễn tưởng', N'Chủ đề về khoa học công nghệ chưa có thật');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Kinh dị', N'Giật gân ám ảnh');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Hồi ký', N'Kể về những chuyển đã trải qua');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Trinh thám', N'Phiêu lưu phá án');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Lãng mạn', N'Nói về tình yêu');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Tâm lý học', N'Bàn về các chủ đề tâm lý');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Giáo dục', N'Nội dung về việc giáo dục');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Thần thoại', N'Kể các chuyện thần thoại');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Thiếu nhi', N'Nội dung dành cho trẻ em');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Kinh doanh', N'Các kiến thức kinh doanh');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Kỹ năng số', N'Cung cấp kỹ năng trong thời đại mới');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Lịch sử', N'Kể về lịch sử');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Chính trị', N'Các chủ đề chính trị');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Khoa học tự nhiên', N'Các kiến thức về khoa học');

insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Mới', 0);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Hỏng nhẹ', 25);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Hỏng vừa', 50);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Hỏng nặng', 75);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Hỏng hoàn toàn', 100);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Mất', 100);

insert into [PHANQUYEN](MaHanhDong, MoTa) values('SuperAdmin', N'Siêu Quản Trị');
insert into [PHANQUYEN](MaHanhDong, MoTa) values('QLTK', N'Quản Lý Tài Khoản');
insert into [PHANQUYEN](MaHanhDong, MoTa) values('ThuThu', N'Thủ Thư');
insert into [PHANQUYEN](MaHanhDong, MoTa) values('DG', N'Độc Giả');

insert into [LOAIDOCGIA](TenLoaiDocGia, SoSachMuonToiDa) values(N'Sinh Viên', 10);
insert into [LOAIDOCGIA](TenLoaiDocGia, SoSachMuonToiDa) values(N'Giảng Viên', 20);
insert into [LOAIDOCGIA](TenLoaiDocGia, SoSachMuonToiDa) values(N'Cán Bộ', 15);
insert into [LOAIDOCGIA](TenLoaiDocGia, SoSachMuonToiDa) values(N'Mặc Định', 0);

insert into [TAIKHOAN](TenTaiKhoan, Hoten, GioiTinh, MatKhau, Email, SinhNhat, DiaChi, SDT, TrangThai, IDPhanQuyen, NgayMo, NgayDong)
values 
(N'sa', N'Super Admin', N'Nam', 'sa123', 'sa@gm.com', '1/2/1995', N'Right here', '0999987789', 0, 1, '1/1/2024', '1/1/2026'),
(N'qltk', N'Quản Lý Hoàng', N'Nam', 'qltk123', 'qltk@gm.com', '1/2/1995', N'Right here', '0999987789', 0, 2, '1/1/2024', '1/1/2026'),
(N'thuthu', N'Thủ Thư Hải', N'Nam', 'tt123', 'tt@gm.com', '1/2/1995', N'Right here', '0999987789', 0, 3, '1/1/2024', '1/1/2026'),
(N'docgia', N'Phúc Thành', N'Nam', 'dg123', 'dg@gm.com', '31/1/2000', N'Right here', '0999987789', 0, 4, '1/1/2024', '1/1/2026'),
(N'docgia1', N'Hải Dương', N'Nam', 'dg123', 'dg1@gm.com', '31/1/2000', N'Right here', '0999987789', 0, 4, '1/1/2024', '1/1/2026'),
(N'docgia2', N'Minh Anh', N'Nam', 'dg123', 'dg2@gm.com', '31/1/2000', N'Right here', '0999987789', 0, 4, '1/1/2024', '1/1/2026'),
(N'docgia3', N'Thế Việt', N'Nam', 'dg123', 'dg3@gm.com', '31/1/2000', N'Right here', '0999987789', 0, 4, '1/1/2024', '1/1/2026'),
(N'docgia4', N'Đắc Minh', N'Nam', 'dg123', 'dg4@gm.com', '31/1/2000', N'Right here', '0999987789', 0, 4, '1/1/2024', '1/1/2026'),
(N'docgia5', N'Việt Hoàng', N'Nam', 'dg123', 'dg5@gm.com', '31/1/2000', N'Right here', '0999987789', 0, 4, '1/1/2024', '1/1/2026'),
(N'docgia6', N'Nhân Thiện', N'Nam', 'dg123', 'dg6@gm.com', '31/1/2000', N'Right here', '0999987789', 0, 4, '1/1/2024', '1/1/2026'),
(N'docgia7', N'Trường Thọ', N'Nam', 'dg123', 'dg7@gm.com', '31/1/2000', N'Right here', '0999987789', 0, 4, '1/1/2024', '1/1/2026'),
(N'docgia8', N'Chính Vũ', N'Nam', 'dg123', 'dg8@gm.com', '31/1/2000', N'Right here', '0999987789', 0, 4, '1/1/2024', '1/1/2026'),
(N'docgia9', N'Thái Sơn', N'Nam', 'dg123', 'dg9@gm.com', '31/1/2000', N'Right here', '0999987789', 0, 4, '1/1/2024', '1/1/2026'),
(N'docgia10', N'Ngọc Anh', N'Nam', 'dg123', 'dg10@gm.com', '31/1/2000', N'Right here', '0999987789', 0, 4, '1/1/2024', '1/1/2026');


INSERT INTO [ADMIN] ([IDTaiKhoan])
SELECT [ID]
FROM [TAIKHOAN]
WHERE [IDPhanQuyen] BETWEEN 1 AND 3;

INSERT INTO [DOCGIA] ([IDTaiKhoan], [IDLoaiDocGia], [GioiThieu])
SELECT 
    [ID] AS IDTaiKhoan,
    ABS(CHECKSUM(NEWID())) % 3 + 1 AS IDLoaiDocGia,
	N'Chưa Có'
FROM [TAIKHOAN]
WHERE [IDPhanQuyen] = 4;

insert into THAMSO(ThoiGian, TuoiToiThieu, TienPhatTraTreMotNgay, TiLeDenBu) values('01/12/2023', 10, 4000, 1.5)
insert into THAMSO(ThoiGian, TuoiToiThieu, TienPhatTraTreMotNgay, TiLeDenBu) values('01/04/2024', 10, 5000, 2)
insert into THAMSO(ThoiGian, TuoiToiThieu, TienPhatTraTreMotNgay, TiLeDenBu) values('01/06/2024', 10, 4500, 2.5)
insert into THAMSO(ThoiGian, TuoiToiThieu, TienPhatTraTreMotNgay, TiLeDenBu) values('01/09/2024', 10, 5500, 2)
insert into THAMSO(ThoiGian, TuoiToiThieu, TienPhatTraTreMotNgay, TiLeDenBu) values('01/12/2024', 10, 7000, 1.75)

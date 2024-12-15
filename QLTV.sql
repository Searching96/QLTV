use [master]
go
alter database [QLTV_Merged] set single_user with rollback immediate
drop database [QLTV_Merged]

create database [QLTV_Merged]
go
use [QLTV_Merged]
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
	-- Xóa đi thì trước khi xóa thay bằng 1 phân quyền khác có tồn tại.
	-- Thêm phân quyền Banned hoặc xét IsDeleted = 1 bằng Banned cũng được
	-- Khi xóa 1 PQ thì hiện thông báo: Hãy gán quyền khác cho các tài khoản được gán quyền này
	-- Quyền Ko Xac Dinh: phân quyền của bạn không xác định, vấn đề này do bên admin, 
	-- liên hệ họ để được hỗ trợ (trường hợp cần sửa chữa hệ thống dành cho 1 phân quyền nào đó)
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
	[Avatar] nvarchar(500) not null DEFAULT 'D:\WPF\QLTVReal - User\QLTV\Image\DefaultAvatar.png',
	[TrangThai] bit not null, -- co dang dang nhap hay ko
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
    [IsDeleted] bit not null -- thu xem them cai global query excluder thi khi query sach tac gia, voi mot sach co nhieu tac gia thi co tu dong ko lay cac tac gia da delete ko
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
    [SoLuong] int not null, -- them rang buoc so luong nay phai bang so luong sach co ma tua sach nay
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
    -- vd: TT1: Nguyen ven, TT2: Hu muc 1 (25), TT3: muc 2 (50), TT4: 75, TT5: (Hu nang ne/ hoan toan) 100
    -- -> tien phat bang (MHH(moi) - MHH(cu)) x 2 x GiaTri
    [ID] int identity(1, 1) constraint [PK_TINHTRANG] primary key,
    [MaTinhTrang] as ('TT' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
    [TenTinhTrang] nvarchar(100) not null,
    [MucHuHong] int not null,
    [IsDeleted] bit not null
)

create table [SACH]
(
    -- Một đợt nhập nhiều quyển giống nhau thì vẫn cho là 2 sách khác nhau vì sau này có thể khác tình trạng
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
    [IsPending] bit not null, -- kiem tra sau 3 ngay ma IsPending van bang 1 thi IsDeleted = 1
    [IsDeleted] bit not null
    -- IsPending = 0/1 xu ly tren code chu ko set default 
)

create table [CTPHIEUMUON]
(
    [IDPhieuMuon] int not null, -- FK
    [IDSach] int not null, -- FK
    constraint [PK_CTPHIEUMUON] primary key([IDPhieuMuon], [IDSach]),
    [HanTra] datetime not null,
    [IDTinhTrangMuon] int not null -- tinh trang moi/cu cua sach luc muon?
)

create table [PHIEUTRA]
(
    [ID] int identity(1, 1) constraint [PK_PHIEUTRA] primary key,
    [MaPhieuTra] as ('PT' + right('00000' + cast ([ID] as varchar(5)), 5)) persisted,
    [NgayTra] datetime not null,
    [IsDeleted] bit not null -- cho xoa vi lo nhap sai
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
    [TongNo] decimal(18, 0) not null, -- = TongNo doc gia, lay ben DOCGIA khoi luu cung duoc, ma luu cho tien?
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

INSERT INTO THAMSO (ThoiGian, TuoiToiThieu, TienPhatTraTreMotNgay, TiLeDenBu) VALUES
('2023-09-01', 10, 5000, 2.0),
('2023-10-01', 11, 6000, 2.1),
('2023-12-01', 10, 5000, 1.9),
('2024-01-01', 13, 7000, 2.3),
('2024-04-01', 11, 6500, 2.4),
('2024-05-01', 13, 7000, 2.2),
('2024-06-01', 12, 5000, 2.0),
('2024-09-01', 14, 7500, 2.5),
('2024-10-01', 13, 7000, 2.3);


insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Mới', 0);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Hỏng nhẹ', 25);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Hỏng vừa', 50);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Hỏng nặng', 75);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Hỏng hoàn toàn', 100);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Mất', 100);

--insert into SACH(IDTuaSach, NamXuatBan, NhaXuatBan, NgayNhap, TriGia, IDTinhTrang, ViTri) values(1 , 2077, 'Su That', '1/1/2049', 100000, 1, 'L02P01K12');
--insert into SACH(IDTuaSach, NamXuatBan, NhaXuatBan, NgayNhap, TriGia, IDTinhTrang, ViTri) values(1 , 2011, 'Su That', '1/1/2009', 90000, 1, 'L02P01K11');

set dateformat ymd
go

﻿-- Thêm dữ liệu cho bảng PHANQUYEN
INSERT INTO PHANQUYEN (MaHanhDong, MoTa, IsDeleted) VALUES
('SieuQuanTri', N'Siêu quản trị viên', 0),
('QuanLyTaiKhoan', N'Quản lý tài khoản', 0),
('ThuThu', N'Thủ thư', 0),
('DocGia', N'Độc giả', 0);

-- Thêm dữ liệu cho bảng LOAIDOCGIA
INSERT INTO LOAIDOCGIA (TenLoaiDocGia, SoSachMuonToiDa, IsDeleted) VALUES
(N'Sinh viên', 5, 0),
(N'Giáo viên', 10, 0),
(N'Cán bộ', 7, 0),
(N'Khách', 3, 0),
(N'VIP', 15, 0),
(N'Thân thiết', 12, 0),
(N'Mới', 3, 0),
(N'Thử nghiệm', 1, 0),
(N'Học sinh', 4, 0),
(N'Người cao tuổi', 5, 0),
(N'Trẻ em', 3, 0),
(N'Người khuyết tật', 7, 0),
(N'Doanh nghiệp', 10, 0),
(N'Tổ chức', 20, 0),
(N'Gia đình', 8, 0),
(N'Nhóm bạn', 5, 0),
(N'Cộng đồng', 10, 0),
(N'Hội viên', 7, 0),
(N'Thành viên', 5, 0),
(N'Đặc biệt', 15, 0);

INSERT INTO TAIKHOAN 
    (TenTaiKhoan, MatKhau, HoTen, Email, GioiTinh, SinhNhat, DiaChi, SDT, TrangThai, IDPhanQuyen, IsDeleted, NgayMo, NgayDong)
VALUES
-- Admin Users
(N'Admin1', 'adminpass1', N'Nguyễn Văn Admin1', 'admin1@gmail.com', N'Nam', '1990-01-01', N'Hà Nội', '0911111111', 1, 1, 0, '2024-01-01', '2025-01-01'),
(N'Admin2', 'adminpass2', N'Nguyễn Văn Admin2', 'admin2@gmail.com', N'Nam', '1991-02-02', N'Hồ Chí Minh', '0922222222', 1, 1, 0, '2024-01-01', '2025-01-01'),

-- Manager Users
(N'Manager1', 'managerpass1', N'Trần Thị Manager1', 'manager1@gmail.com', N'Nữ', '1992-03-03', N'Đà Nẵng', '0933333333', 1, 2, 0, '2024-01-01', '2025-01-01'),
(N'Manager2', 'managerpass2', N'Trần Thị Manager2', 'manager2@gmail.com', N'Nữ', '1993-04-04', N'Cần Thơ', '0944444444', 1, 2, 0, '2024-01-01', '2025-01-01'),

-- Regular Users
(N'User1', 'userpass1', N'Nguyễn Văn Bình', 'user1@gmail.com', N'Nam', '1998-09-09', N'Hà Nội', '0999999999', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'User2', 'userpass2', N'Trần Minh Khoa', 'user2@gmail.com', N'Nam', '1999-10-10', N'Đà Nẵng', '0909090909', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'User3', 'userpass3', N'Nguyễn Thị Lan', 'user3@gmail.com', N'Nữ', '2000-11-11', N'Hải Phòng', '0910101010', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'User4', 'userpass4', N'Lê Thị Mai', 'user4@gmail.com', N'Nữ', '2001-12-12', N'Hồ Chí Minh', '0920202020', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'User5', 'userpass5', N'Phạm Văn Dũng', 'user5@gmail.com', N'Nam', '2002-01-01', N'Cần Thơ', '0930303030', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'User6', 'userpass6', N'Vũ Minh Anh', 'user6@gmail.com', N'Nam', '2003-02-02', N'Nha Trang', '0940404040', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'User7', 'userpass7', N'Hoàng Thị Hương', 'user7@gmail.com', N'Nữ', '2004-03-03', N'Đà Lạt', '0950505050', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'User8', 'userpass8', N'Nguyễn Thị Bích', 'user8@gmail.com', N'Nữ', '2005-04-04', N'Vũng Tàu', '0960606060', 1, 4, 0, '2024-01-01', '2025-01-01'),

-- Additional Users
(N'Guest1', 'guestpass1', N'Nguyễn Văn Hải', 'guest1@gmail.com', N'Nam', '2006-05-05', N'Hà Nội', '0970707070', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'Guest2', 'guestpass2', N'Trần Thị Thu', 'guest2@gmail.com', N'Nữ', '2007-06-06', N'Hồ Chí Minh', '0980808080', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'Guest3', 'guestpass3', N'Lê Văn Nam', 'guest3@gmail.com', N'Nam', '2008-07-07', N'Cần Thơ', '0990909090', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'Guest4', 'guestpass4', N'Phan Thị Hiền', 'guest4@gmail.com', N'Nữ', '2009-08-08', N'Nha Trang', '0911111112', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'Guest5', 'guestpass5', N'Nguyễn Văn Đức', 'guest5@gmail.com', N'Nam', '2010-09-09', N'Đà Lạt', '0922222223', 1, 4, 0, '2024-01-01', '2025-01-01'),

-- Test Users
(N'TestUser1', 'testpass1', N'Trần Thị Hà', 'test1@gmail.com', N'Nữ', '2011-10-10', N'Hà Nội', '0933333334', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'TestUser2', 'testpass2', N'Nguyễn Văn Minh', 'test2@gmail.com', N'Nam', '2012-11-11', N'Đà Nẵng', '0944444445', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'TestUser3', 'testpass3', N'Lê Thị Thanh', 'test3@gmail.com', N'Nữ', '2013-12-12', N'Hồ Chí Minh', '0955555556', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'TestUser4', 'testpass4', N'Phạm Văn An', 'test4@gmail.com', N'Nam', '2014-01-01', N'Cần Thơ', '0966666667', 1, 4, 0, '2024-01-01', '2025-01-01'),
(N'TestUser5', 'testpass5', N'Nguyễn Thị Ngọc', 'test5@gmail.com', N'Nữ', '2015-02-02', N'Nha Trang', '0977777778', 1, 4, 0, '2024-01-01', '2025-01-01');


-- Thêm dữ liệu cho bảng ADMIN
INSERT INTO ADMIN (IDTaiKhoan) VALUES
(1);


INSERT INTO DOCGIA (IDTaiKhoan, IDLoaiDocGia, GioiThieu, TongNo) VALUES
-- Regular Users
(4, 1, N'Độc giả Sinh viên - Nguyễn Văn Bình', 0),
(5, 2, N'Độc giả Giáo viên - Trần Minh Khoa', 0),
(6, 3, N'Độc giả Cán bộ - Nguyễn Thị Lan', 0),
(7, 4, N'Độc giả Khách - Lê Thị Mai', 0),
(8, 5, N'Độc giả VIP - Phạm Văn Dũng', 0),
(9, 6, N'Độc giả Thân thiết - Vũ Minh Anh', 0),
(10, 7, N'Độc giả Mới - Hoàng Thị Hương', 0),
(11, 8, N'Độc giả Thử nghiệm - Nguyễn Thị Bích', 0),

-- Additional Users
(12, 9, N'Độc giả Học sinh - Nguyễn Văn Hải', 0),
(13, 10, N'Độc giả Người cao tuổi - Trần Thị Thu', 0),
(14, 11, N'Độc giả Trẻ em - Lê Văn Nam', 0),
(15, 12, N'Độc giả Người khuyết tật - Phan Thị Hiền', 0),
(16, 13, N'Độc giả Doanh nghiệp - Nguyễn Văn Đức', 0),

-- Test Users
(17, 14, N'Độc giả Tổ chức - Trần Thị Hà', 0),
(18, 15, N'Độc giả Gia đình - Nguyễn Văn Minh', 0),
(19, 16, N'Độc giả Nhóm bạn - Lê Thị Thanh', 0),
(20, 17, N'Độc giả Cộng đồng - Phạm Văn An', 0),
(21, 18, N'Độc giả Hội viên - Nguyễn Thị Ngọc', 0);

﻿USE [master]
GO
alter database [QLTVten] set single_user with rollback immediate;
drop database [QLTVten];

create database [QLTVten]
go
use [QLTVten]
go

set dateformat dmy
go

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
	[MatKhau] varchar(50) not null,
	[TenTaiKhoan] nvarchar(100) constraint [UQ_TenTaiKhoan] unique not null,
	[Email] varchar(100) constraint [UQ_Email] unique not null,
	[SinhNhat] datetime not null,
	[DiaChi] nvarchar(200) not null,
	[SDT] varchar(20) not null,
	[Avatar] varchar(500) not null,
	[TrangThai] bit not null, -- co dang dang nhap hay ko
	[IDPhanQuyen] int not null, -- FK
	[IsDeleted] bit not null,
)

create table [ADMIN] 
(
	[ID] int identity(1, 1) constraint [PK_ADMIN] primary key,
	[MaAdmin] as ('AD' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
	[IDTaiKhoan] int not null, -- FK 
	[NgayVaoLam] date not null,
	[NgayKetThuc] date not null, -- Tai khoan admin het han giong nhu The doc gia het han
)

create table [DOCGIA]
(
    [ID] int identity(1, 1) constraint [PK_DOCGIA] primary key,
    [MaDocGia] as ('DG' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
    [IDTaiKhoan] int not null, -- FK
    [IDLoaiDocGia] int not null, -- FK
    [NgayLapThe] date not null,
    [NgayHetHan] date not null,
    [TongNo] decimal(18, 0) not null,
    [GioiThieu] nvarchar(500) not null -- bo cung duoc
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
    [HanMuonToiDa] int not null, -- don vi: thang
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
    [TuoiToiThieu] int not null,
    [TienPhatTraTreMotNgay] decimal(18, 0) not null,
	[TiLeDenBu] decimal(4,2) not null
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





--create table [TINHTRANG]
--(
--    -- vd: TT1: Nguyen ven, TT2: Hu muc 1 (25), TT3: muc 2 (50), TT4: 75, TT5: (Hu nang ne/ hoan toan) 100
--    -- -> tien phat bang (MHH(moi) - MHH(cu)) x 2 x GiaTri
--    [ID] int identity(1, 1) constraint [PK_TINHTRANG] primary key,
--    [MaTinhTrang] as ('TT' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
--    [TenTinhTrang] nvarchar(100) not null,
--    [MucHuHong] int not null,
--    [IsDeleted] bit not null
--)



--create table [SACH]
--(
--    -- Một đợt nhập nhiều quyển giống nhau thì vẫn cho là 2 sách khác nhau vì sau này có thể khác tình trạng
--    [ID] int identity(1, 1) constraint [PK_SACH] primary key,
--    [MaSach] as ('S' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
--    [IDTuaSach] int not null, -- FK
--    [NamXuatBan] int not null,
--    [NhaXuatBan] nvarchar(100) not null,
--    [NgayNhap] date not null,
--    [TriGia] decimal(18, 0) not null,
--    [IDTinhTrang] int not null, -- FK
--    [IsAvailable] bit not null,
--    [IsDeleted] bit not null
--)

-- Trigger
--CREATE TRIGGER trg_AfterInsertSACH_UpdateTUASACHSoLuong 
--ON SACH
--AFTER INSERT
--AS
--BEGIN
--    -- Updating the TUASACH table after an insert into SACH
--    UPDATE TUASACH
--    SET SoLuong = SoLuong + 1
--    WHERE ID = (SELECT IDTuaSach FROM inserted)
--END

--drop trigger trg_AfterInsertSACH_UpdateTUASACHSoLuong 
--select * from sys.triggers

INSERT INTO PHANQUYEN (MaHanhDong, MoTa, IsDeleted)
VALUES 
('Admin', N'Quản trị hệ thống', 0),
('DocGia', N'Độc giả thư viện', 0),
('Banned', N'Tài khoản bị cấm', 0),
('KoXacDinh', N'Phân quyền không xác định', 0);

-- Bảng LOAIDOCGIA
INSERT INTO LOAIDOCGIA (TenLoaiDocGia, SoSachMuonToiDa, IsDeleted)
VALUES 
('Sinh viên', 5, 0),
('Giảng viên', 10, 0),
('Nhân viên thư viện', 15, 0);

-- Bảng TAIKHOAN
INSERT INTO TAIKHOAN (MatKhau, TenTaiKhoan, Email, SinhNhat, DiaChi, SDT, Avatar, TrangThai, IDPhanQuyen, IsDeleted)
VALUES 
('admin123', 'admin1', 'admin1@example.com', '1/1/1982', 'Trụ sở chính', '0901111222', 'admin_avatar.png', 1, 1, 0),
('pass123', 'sv1', 'sv1@example.com', '15/5/2005', '123 Đường ABC', '0901234567', 'avatar1.png', 0, 2, 0),
('pass234', 'gv1', 'gv1@example.com', '10/3/1985', '456 Đường XYZ', '0912345678', 'avatar2.png', 0, 2, 0),
('pass345', 'nvthuvien', 'nvthuvien@example.com', '20/8/1980', '789 Đường DEF', '0923456789', 'avatar3.png', 0, 2, 0),
('banned123', 'banned1', 'banned1@example.com', '30/7/1995', 'Nơi nào đó', '0939876543', 'banned_avatar.png', 0, 3, 0);

-- Bảng DOCGIA
INSERT INTO DOCGIA (IDTaiKhoan, IDLoaiDocGia, NgayLapThe, NgayHetHan, TongNo, GioiThieu)
VALUES 
(2, 1, '2024-01-10', '2025-01-10', 0, N'Sinh viên năm nhất'),
(3, 2, '2023-09-01', '2025-09-01', 0, N'Giảng viên khoa CNTT'),
(4, 3, '2022-05-20', '2024-05-20', 0, N'Nhân viên thư viện chính');

select * from TUASACH;
select * from SACH;
select * from TINHTRANG;

-- Insert data into TACGIA
INSERT INTO TACGIA (TenTacGia, NamSinh, QuocTich, IsDeleted)
VALUES
('Aurora', 2077, 'Somewhere', 0),
('William Shakespeare', 1564, 'England', 0),
('Jane Austen', 1775, 'England', 0),
('Mark Twain', 1835, 'United States', 0),
('Leo Tolstoy', 1828, 'Russia', 0),
('Victor Hugo', 1802, 'France', 0),
('Gabriel Garcia Marquez', 1927, 'Colombia', 0),
('Haruki Murakami', 1949, 'Japan', 0),
('Nguyen Du', 1766, 'Vietnam', 0),
('Chinua Achebe', 1930, 'Nigeria', 0);


-- Insert data into THELOAI
INSERT INTO THELOAI (TenTheLoai, MoTa, IsDeleted)
VALUES
    (N'Khong xac dinh', 'mo ta', 0),
    (N'Văn học cổ điển', 'mo ta', 0),
    (N'Khoa học viễn tưởng', 'mo ta', 0),
    (N'Kinh dị', 'mo ta', 0),
    (N'Hồi ký', 'mo ta', 0),
    (N'Trinh thám', 'mo ta', 0),
    (N'Lãng mạn', 'mo ta', 0),
    (N'Tâm lý học', 'mo ta', 0),
    (N'Giáo dục', 'mo ta', 0),
    (N'Thần thoại', 'mo ta', 0),
    (N'Thiếu nhi', 'mo ta', 0),
    (N'Kinh doanh', 'mo ta', 0),
    (N'Kỹ năng số', 'mo ta', 0),
    (N'Lịch sử', 'mo ta', 0),
    (N'Chính trị', 'mo ta', 0),
    (N'Khoa học tự nhiên', 'mo ta', 0);


-- Insert data into TUASACH
INSERT INTO TUASACH (TenTuaSach, HanMuonToiDa, BiaSach, SoLuong, IsDeleted)
VALUES
('Two Dimensions', 2, 'https://upload.wikimedia.org/wikipedia/commons/thumb/a/ac/No_image_available.svg/800px-No_image_available.svg.png', 0, 0);

insert into TUASACH_TACGIA(IDTuaSach, IDTacGia) values(1, 1);

insert into TUASACH_THELOAI(IDTuaSach, IDTheLoai) values(1, 1);

-- Insert data into TINHTRANG
INSERT INTO TINHTRANG (TenTinhTrang, MucHuHong, IsDeleted)
VALUES
(N'Mới', 0, 0),
(N'Hỏng nhẹ', 25, 0),
(N'Hỏng vừa', 50, 0),
(N'Hỏng nặng', 75, 0),
(N'Hỏng hoàn toàn', 100, 0),
(N'Mất', 100, 0);

-- Insert data into SACH
INSERT INTO SACH (IDTuaSach, NamXuatBan, NhaXuatBan, NgayNhap, TriGia, IDTinhTrang, IsAvailable, IsDeleted)
VALUES
(1, 2077, 'Su That', '01/01/2049', 100000, 1, 1, 0),
(1, 2011, 'Su That', '01/01/2009', 90000, 1, 1, 0);

-- Insert data into THAMSO
INSERT INTO THAMSO (TuoiToiThieu, TienPhatTraTreMotNgay, TiLeDenBu)
VALUES
(10, 5000, 1.2);

-- Insert data into PHIEUMUON (Borrow Tickets)
INSERT INTO [PHIEUMUON] (IDDocGia, NgayMuon, IsPending, IsDeleted)
VALUES
(1, '01/03/2022', 0, 0);

-- Insert data into CTPHIEUMUON (Borrow Ticket Details)
INSERT INTO [CTPHIEUMUON] (IDPhieuMuon, IDSach, HanTra, IDTinhTrangMuon)
VALUES
(1, 1, '01/05/2022', 1),
(1, 3, '01/05/2022', 1);

-- Insert data into PHIEUTRA (Return Tickets)
INSERT INTO [PHIEUTRA] (NgayTra, IsDeleted)
VALUES
('01/04/2022', 0);

-- Insert data into CTPHIEUTRA (Return Ticket Details)
INSERT INTO [CTPHIEUTRA] (IDPhieuTra, IDPhieuMuon, IDSach, SoNgayMuon, TienPhat, IDTinhTrangTra, GhiChu)
VALUES
(1, 1, 1, 31, 0, 1, N'Returned on time'),
(1, 1, 3, 31, 0, 1, N'Returned on time');

-- Insert data into BCTRATRE (Late Return Reports)
INSERT INTO [BCTRATRE] (Ngay)
VALUES
('30/04/2022');

-- Insert data into CTBCTRATRE (Late Return Report Details)
INSERT INTO [CTBCTRATRE] (IDBCTraTre, IDPhieuTra, SoNgayTraTre)
VALUES
(1, 1, 0);

-- Insert data into PHIEUTHUTIENPHAT (Penalty Receipts)
INSERT INTO [PHIEUTHUTIENPHAT] (IDDocGia, NgayThu, TongNo, SoTienThu, ConLai, IsDeleted)
VALUES
(1, '05/04/2022', 50000, 50000, 0, 0);

-- Insert data into DANHGIA (Ratings)
INSERT INTO [DANHGIA] (IDSach, IDPhieuMuon, DanhGia)
VALUES
(1, 1, 4.5),
(3, 1, 4.0);

DECLARE @StartDate DATE = '2022-01-01';
DECLARE @EndDate DATE = '2024-12-31';
DECLARE @TongSoLuotMuon INT;

-- Loop through months from 2022 to 2024
WHILE @StartDate <= @EndDate
BEGIN
    DECLARE @RandomDay DATE;

    -- Generate a random day within the month
    SET @RandomDay = DATEADD(DAY, FLOOR(RAND() * (DAY(EOMONTH(@StartDate)) - 1)) + 1, EOMONTH(@StartDate, -1));
    SET @TongSoLuotMuon = CAST(RAND() * 500 + 100 AS INT); -- Random borrow count

    -- Insert data into BCMUONSACH
    INSERT INTO BCMUONSACH (Thang, TongSoLuotMuon)
    VALUES (@RandomDay, @TongSoLuotMuon);

    -- Increment month
    SET @StartDate = DATEADD(MONTH, 1, @StartDate);
END;

-- Declare table for genres
DECLARE @Genres TABLE (ID INT);

-- Get list of active genres
INSERT INTO @Genres (ID)
SELECT ID FROM THELOAI WHERE IsDeleted = 0;

DECLARE @BCMuonSachID INT;

-- Declare cursor to loop through BCMUONSACH data
DECLARE BCMuonSach_Cursor CURSOR FOR
SELECT ID, TongSoLuotMuon FROM BCMUONSACH ORDER BY Thang;

OPEN BCMuonSach_Cursor;
FETCH NEXT FROM BCMuonSach_Cursor INTO @BCMuonSachID, @TongSoLuotMuon;

-- Loop through BCMUONSACH records and populate CTBCMUONSACH
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @GenreWeights TABLE (ID INT, Weight FLOAT);

    -- Generate random weight for each genre
    INSERT INTO @GenreWeights (ID, Weight)
    SELECT ID, RAND(CHECKSUM(NEWID())) AS Weight FROM @Genres;

    -- Calculate total weight
    DECLARE @TotalWeight FLOAT = (SELECT SUM(Weight) FROM @GenreWeights);

    -- Prepare genre data
    DECLARE @GenreData TABLE (ID INT, SoLuotMuon INT, TiLe FLOAT);

    INSERT INTO @GenreData (ID, SoLuotMuon, TiLe)
    SELECT
        G.ID,
        FLOOR((G.Weight / @TotalWeight * @TongSoLuotMuon)) AS SoLuotMuon,
        G.Weight / @TotalWeight AS TiLe
    FROM @GenreWeights G;

    -- Calculate the difference between total calculated SoLuotMuon and the desired TongSoLuotMuon
    DECLARE @TotalSoLuotMuon INT = (SELECT SUM(SoLuotMuon) FROM @GenreData);
    DECLARE @Difference INT = @TongSoLuotMuon - @TotalSoLuotMuon;

    -- Adjust SoLuotMuon to match TongSoLuotMuon
    IF @Difference != 0
    BEGIN
        IF @Difference > 0
        BEGIN
            -- Increase SoLuotMuon for random rows
            UPDATE @GenreData
            SET SoLuotMuon = SoLuotMuon + 1
            WHERE ID IN (SELECT TOP (@Difference) ID FROM @GenreData ORDER BY NEWID());
        END
        ELSE
        BEGIN
            -- Decrease SoLuotMuon for random rows
            UPDATE @GenreData
            SET SoLuotMuon = SoLuotMuon - 1
            WHERE ID IN (SELECT TOP (ABS(@Difference)) ID FROM @GenreData ORDER BY NEWID());
        END
    END

    -- Insert data into CTBCMUONSACH, ensuring no duplicates
    INSERT INTO CTBCMUONSACH (IDBCMuonSach, IDTheLoai, SoLuotMuon, TiLe)
    SELECT
        @BCMuonSachID,
        GD.ID,
        GD.SoLuotMuon,
        CAST(GD.SoLuotMuon AS FLOAT) / @TongSoLuotMuon
    FROM @GenreData GD
    WHERE NOT EXISTS (
        SELECT 1
        FROM CTBCMUONSACH
        WHERE IDBCMuonSach = @BCMuonSachID AND IDTheLoai = GD.ID
    );

    -- Fetch next BCMuonSach record
    FETCH NEXT FROM BCMuonSach_Cursor INTO @BCMuonSachID, @TongSoLuotMuon;
END

-- Close and deallocate the cursor
CLOSE BCMuonSach_Cursor;
DEALLOCATE BCMuonSach_Cursor;

-- Verify data population
SELECT * FROM BCMUONSACH;
SELECT * FROM CTBCMUONSACH;

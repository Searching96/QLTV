﻿create database [QLTV_Merged]
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

insert into THAMSO(ThoiGian, TuoiToiThieu, TienPhatTraTreMotNgay, TiLeDenBu)
values('2024/11/10', 10, 5000, 2)

insert into TUASACH(TenTuaSach, HanMuonToiDa, MoTa) values('Two Dimensions', 2, N'Giữa hai the gioi ta thay duoc dieu gi');
insert into THELOAI(TenTheLoai, MoTa) values('Khong xac dinh', 'mo ta')
insert into TACGIA(TenTacGia, NamSinh, QuocTich) values('Aurora', 2077, 'Somewhere')

insert into TUASACH_TACGIA(IDTuaSach, IDTacGia) values(1, 1)
insert into TUASACH_THELOAI(IDTuaSach, IDTheLoai) values(1, 1)

INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES ('William Shakespeare', 1564, 'England');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES ('Jane Austen', 1775, 'England');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES ('Mark Twain', 1835, 'United States');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES ('Leo Tolstoy', 1828, 'Russia');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES ('Victor Hugo', 1802, 'France');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES ('Gabriel Garcia Marquez', 1927, 'Colombia');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES ('Haruki Murakami', 1949, 'Japan');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES ('Nguyen Du', 1766, 'Vietnam');
INSERT INTO TACGIA(TenTacGia, NamSinh, QuocTich) VALUES ('Chinua Achebe', 1930, 'Nigeria');

INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Văn học cổ điển', 'mo ta');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Khoa học viễn tưởng', 'mo ta');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Kinh dị', 'mo ta');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Hồi ký', 'mo ta');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Trinh thám', 'mo ta');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Lãng mạn', 'mo ta');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Tâm lý học', 'mo ta');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Giáo dục', 'mo ta');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Thần thoại', 'mo ta');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Thiếu nhi', 'mo ta');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Kinh doanh', 'mo ta');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Kỹ năng số', 'mo ta');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Lịch sử', 'mo ta');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Chính trị', 'mo ta');
INSERT INTO THELOAI(TenTheLoai, MoTa) VALUES (N'Khoa học tự nhiên', 'mo ta');

insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Mới', 0);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Hỏng nhẹ', 25);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Hỏng vừa', 50);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Hỏng nặng', 75);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Hỏng hoàn toàn', 100);
insert into TINHTRANG(TenTinhTrang, MucHuHong) values(N'Mất', 100);

insert into SACH(IDTuaSach, NamXuatBan, NhaXuatBan, NgayNhap, TriGia, IDTinhTrang, ViTri) values(1 , 2077, 'Su That', '1/1/2049', 100000, 1, 'L02P01K12');
insert into SACH(IDTuaSach, NamXuatBan, NhaXuatBan, NgayNhap, TriGia, IDTinhTrang, ViTri) values(1 , 2011, 'Su That', '1/1/2009', 90000, 1, 'L02P01K11');

set dateformat ymd
go

insert into [PHANQUYEN](MaHanhDong, MoTa) values('SuperAdmin', N'Siêu Quản Trị');
insert into [PHANQUYEN](MaHanhDong, MoTa) values('QLTK', N'Quản Lý Tài Khoản');
insert into [PHANQUYEN](MaHanhDong, MoTa) values('ThuThu', N'Thủ Thư');
insert into [PHANQUYEN](MaHanhDong, MoTa) values('DG', N'Độc Giả');

insert into [TAIKHOAN](TenTaiKhoan, Hoten, GioiTinh, MatKhau, Email, SinhNhat, DiaChi, SDT, TrangThai, IDPhanQuyen, NgayMo, NgayDong)
values (N'sa', N'super admin', N'Nam', 'sa123', 'sa@gm.com', '1999-01-02', N'Right here', '0999987789', 0, 1, '2024-01-01', '2025-01-01');

INSERT INTO [ADMIN] ([IDTaiKhoan])
SELECT [ID]
FROM [TAIKHOAN]
WHERE [IDPhanQuyen] BETWEEN 1 AND 3;



INSERT INTO LOAIDOCGIA (TenLoaiDocGia, SoSachMuonToiDa) VALUES
(N'Sinh viên', 5),
(N'Giáo viên', 10),
(N'Cán bộ', 7),
(N'Học sinh', 3),
(N'Khách', 2),
(N'Thạc sĩ', 8),
(N'Tiến sĩ', 12),
(N'Nhà nghiên cứu', 15),
(N'Người cao tuổi', 4),
(N'Trẻ em', 2),
(N'Người khuyết tật', 6),
(N'Doanh nhân', 7),
(N'Mặc định', 9),
(N'Nhà báo', 8),
(N'Người nước ngoài', 5);

INSERT INTO TAIKHOAN (TenTaiKhoan, MatKhau, HoTen, Email, GioiTinh, SinhNhat, DiaChi, SDT, TrangThai, IDPhanQuyen, NgayMo, NgayDong) VALUES
('user1', 'pass1', N'Nguyễn Văn An', 'annguyen@example.com', N'Nam', '2000-01-01', N'123 Đường A', '0123456789', 0, 4, '2024-01-01', '2025-01-01'),
('user2', 'pass2', N'Trần Thị Bình', 'binhtran@example.com', N'Nữ', '1999-02-02', N'456 Đường B', '0987654321', 0, 4, '2024-02-01', '2025-02-01'),
('user3', 'pass3', N'Lê Văn Cường', 'cuongle@example.com', N'Nam', '1998-03-03', N'789 Đường C', '0123456788', 0, 4, '2024-03-01', '2025-03-01'),
('user4', 'pass4', N'Phạm Thị Dung', 'dungpham@example.com', N'Nữ', '1997-04-04', N'101 Đường D', '0987654320', 0, 4, '2024-04-01', '2025-04-01'),
('user5', 'pass5', N'Hoàng Văn Em', 'emhoang@example.com', N'Nam', '1996-05-05', N'112 Đường E', '0123456787', 0, 4, '2024-05-01', '2025-05-01'),
('user6', 'pass6', N'Vũ Thị Hoa', 'hoavu@example.com', N'Nữ', '1995-06-06', N'134 Đường F', '0987654319', 0, 4, '2024-06-01', '2025-06-01'),
('user7', 'pass7', N'Nguyễn Văn Hùng', 'hungnguyen@example.com', N'Nam', '1994-07-07', N'156 Đường G', '0123456786', 0, 4, '2024-07-01', '2025-07-01'),
('user8', 'pass8', N'Trần Thị Lan', 'lantran@example.com', N'Nữ', '1993-08-08', N'178 Đường H', '0987654318', 0, 4, '2024-08-01', '2025-08-01'),
('user9', 'pass9', N'Lê Văn Minh', 'minhle@example.com', N'Nam', '1992-09-09', N'190 Đường I', '0123456785', 0, 4, '2024-09-01', '2025-09-01'),
('user10', 'pass10', N'Phạm Thị Oanh', 'oanhpham@example.com', N'Nữ', '1991-10-10', N'212 Đường K', '0987654317', 0, 4, '2024-10-01', '2025-10-01'),
('user11', 'pass11', N'Hoàng Văn Phong', 'phonghoang@example.com', N'Nam', '1990-11-11', N'234 Đường L', '0123456784', 0, 4, '2024-11-01', '2025-11-01'),
('user12', 'pass12', N'Vũ Thị Quỳnh', 'quynhvu@example.com', N'Nữ', '1989-12-12', N'256 Đường M', '0987654316', 0, 4, '2024-12-01', '2025-12-01'),
('user13', 'pass13', N'Nguyễn Văn Sơn', 'sonnguyen@example.com', N'Nam', '1988-01-13', N'278 Đường N', '0123456783', 0, 4, '2023-01-01', '2024-01-01'),
('user14', 'pass14', N'Trần Thị Thảo', 'thaotran@example.com', N'Nữ', '1987-02-14', N'290 Đường O', '0987654315', 0, 4, '2023-02-01', '2024-02-01'),
('user15', 'pass15', N'Lê Văn Tùng', 'tungle@example.com', N'Nam', '1986-03-15', N'312 Đường P', '0123456782', 0, 4, '2023-03-01', '2024-03-01');

INSERT INTO TAIKHOAN (TenTaiKhoan, MatKhau, HoTen, Email, GioiTinh, SinhNhat, DiaChi, SDT, TrangThai, IDPhanQuyen, NgayMo, NgayDong) VALUES
('user16', 'pass16', N'Nguyễn Thị Ánh', 'anhnguyen@example.com', N'Nữ', '1985-04-16', N'334 Đường Q', '0123456781', 0, 4, '2023-04-01', '2024-04-01'),
('user17', 'pass17', N'Trần Văn Bảo', 'baotran@example.com', N'Nam', '1984-05-17', N'356 Đường R', '0987654314', 0, 4, '2023-05-01', '2024-05-01'),
('user18', 'pass18', N'Lê Thị Chi', 'chile@example.com', N'Nữ', '1983-06-18', N'378 Đường S', '0123456780', 0, 4, '2023-06-01', '2024-06-01'),
('user19', 'pass19', N'Phạm Văn Đức', 'ducpham@example.com', N'Nam', '1982-07-19', N'390 Đường T', '0987654313', 0, 4, '2023-07-01', '2024-07-01'),
('user20', 'pass20', N'Hoàng Thị  Giang', 'gianghoang@example.com', N'Nữ', '1981-08-20', N'412 Đường U', '0123456779', 0, 4, '2023-08-01', '2024-08-01');

INSERT INTO DOCGIA (IDTaiKhoan, IDLoaiDocGia, GioiThieu) VALUES
(2, 1, N'Sinh viên năm 3'),
(3, 2, N'Giáo viên Toán'),
(4, 3, N'Cán bộ phòng Tài chính'),
(5, 4, N'Học sinh lớp 10'),
(6, 5, N'Khách vãng lai'),
(7, 6, N'Thạc sĩ Văn học'),
(8, 7, N'Tiến sĩ Vật lý'),
(9, 8, N'Nhà nghiên cứu lịch sử'),
(10, 9, N'Người cao tuổi yêu sách'),
(11, 10, N'Trẻ em ham đọc'),
(12, 11, N'Người khuyết tật'),
(13, 12, N'Doanh nhân thành đạt'),
(14, 14, N'Nhà báo cùi'),
(15, 14, N'Nhà báo nổi tiếng'),
(16, 15, N'Người nước ngoài');

INSERT INTO PHIEUTHUTIENPHAT (IDDocGia, NgayThu, TongNo, SoTienThu, ConLai) VALUES
(1, '2024-11-10', 10000, 5000, 5000),
(2, '2024-11-11', 20000, 10000, 10000),
(3, '2024-11-12', 30000, 15000, 15000),
(4, '2024-11-13', 40000, 20000, 20000),
(5, '2024-11-14', 50000, 25000, 25000),
(6, '2024-11-15', 60000, 30000, 30000),
(7, '2024-11-16', 70000, 35000, 35000),
(8, '2024-11-17', 80000, 40000, 40000),
(9, '2024-11-18', 90000, 45000, 45000),
(10, '2024-11-19', 100000, 50000, 50000),
(11, '2024-11-20', 110000, 55000, 55000),
(12, '2024-11-21', 120000, 60000, 60000),
(13, '2024-11-22', 130000, 65000, 65000),
(14, '2024-11-23', 140000, 70000, 70000),
(15, '2024-11-24', 150000, 75000, 75000);
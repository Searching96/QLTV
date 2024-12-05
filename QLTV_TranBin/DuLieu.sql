INSERT INTO [PHANQUYEN]
(
    [MaHanhDong], [MoTa], [IsDeleted]
)
VALUES
-- Dòng 1: SuperAdmin
('SuperAdmin', N'Siêu Quản Trị', 0),

-- Dòng 2: QLNS
('QLNS', N'Quản Lý Nhân Sự', 0),

-- Dòng 3: ThuThư
('ThuThu', N'Thủ Thư', 0),

-- Dòng 4: Độc Giả
('DG', N'Độc Giả', 0);
INSERT INTO [TAIKHOAN] 
(
    [TenTaiKhoan], [Hoten], [GioiTinh], [MatKhau], 
    [Email], [SinhNhat], [DiaChi], [SDT], [TrangThai], 
    [IDPhanQuyen], [IsDeleted], [NgayMo], [NgayDong]
)
VALUES
-- Dòng 1
(N'NguyenVanA', N'Nguyễn Văn A', N'Nam', 'MatKhau123', 
 'nguyenvana@example.com', '1990-01-01', N'Hà Nội', '0987654321', 
 0, 1, 0, '2024-01-01', '2025-01-01'),

-- Dòng 2
(N'LeThiB', N'Lê Thị B', N'Nữ', '123MatKhau', 
 'lethib@example.com', '1995-05-15', N'Hồ Chí Minh', '0988123456', 
 0, 2, 0, '2023-01-01', '2024-01-01'),

-- Dòng 3
(N'TranVanC', N'Trần Văn C', N'Nam', 'MkC12345', 
 'tranvanc@example.com', '1992-12-20', N'Đà Nẵng', '0977345678', 
 0, 3, 0, '2022-06-01', '2023-06-01'),

-- Dòng 4
(N'PhamThiD', N'Phạm Thị D', N'Nữ', 'MkD67890', 
 'phamthid@example.com', '1988-11-11', N'Hải Phòng', '0932456789', 
 0, 4, 0, '2023-03-15', '2024-03-15'),

-- Dòng 5
(N'HoangVanE', N'Hoàng Văn E', N'Nam', 'MatKhauHoangE', 
 'hoangvane@example.com', '2000-07-07', N'Cần Thơ', '0912345678', 
 0, 2, 0, '2021-09-01', '2022-09-01');

 INSERT INTO [ADMIN] ([IDTaiKhoan])
SELECT [ID]
FROM [TAIKHOAN]
WHERE [IDPhanQuyen] BETWEEN 1 AND 3;

 INSERT INTO [LOAIDOCGIA] 
(
    [TenLoaiDocGia], [SoSachMuonToiDa], [IsDeleted]
)
VALUES
-- Dòng 1: Học Sinh
(N'Học Sinh', 20, 0),

-- Dòng 2: Sinh Viên
(N'Sinh Viên', 30, 0),

-- Dòng 3: Giảng Viên
(N'Giảng Viên', 40, 0);
-- Chèn dữ liệu vào bảng DOCGIA cho các tài khoản có IDPhanQuyen = 4
INSERT INTO [DOCGIA] 
(
    [IDTaiKhoan], [IDLoaiDocGia], [GioiThieu], [TongNo]
)
SELECT 
    [ID],         -- ID từ bảng TAIKHOAN
    (SELECT TOP 1 [ID] FROM [LOAIDOCGIA] WHERE [IsDeleted] = 0 ORDER BY [ID] ASC), -- Chọn ID đầu tiên trong LOAIDOCGIA
    N'Tôi là một độc giả.', -- Giới thiệu mặc định
    0             -- Tổng nợ mặc định
FROM [TAIKHOAN]
WHERE [IDPhanQuyen] = 4; -- Điều kiện lấy các tài khoản độc giả
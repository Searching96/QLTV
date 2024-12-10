-- Thêm dữ liệu cho bảng PHANQUYEN
INSERT INTO PHANQUYEN (MaHanhDong, MoTa, IsDeleted) VALUES
('SieuQuanTri', N'Siêu quản trị viên', 0),
('QuanLyNhanSu', N'Quản lý nhân sự', 0),
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

-- Thêm dữ liệu cho bảng TAIKHOAN
INSERT INTO TAIKHOAN (MatKhau, TenTaiKhoan, Email, SinhNhat, DiaChi, SDT, Avatar, TrangThai, IDPhanQuyen, IsDeleted) VALUES
('123456', N'Nguyễn Văn A', 'nguyenvana@gmail.com', '2000-01-01', N'Hà Nội', '0987654321', 'https://example.com/avatar1.jpg', 1, 4, 0),
('password', N'Trần Thị B', 'tranthib@gmail.com', '2001-02-02', N'Hồ Chí Minh', '0912345678', 'https://example.com/avatar2.jpg', 0, 4, 0),
('secure', N'Lê Văn C', 'levanc@gmail.com', '2002-03-03', N'Đà Nẵng', '0987654321', 'https://example.com/avatar3.jpg', 1, 3, 0),
('admin123', N'Admin', 'admin@gmail.com', '1990-04-04', N'Hà Nội', '0987654321', 'https://example.com/avatar4.jpg', 1, 1, 0),
('user123', N'User', 'user@gmail.com', '1991-05-05', N'Hồ Chí Minh', '0912345678', 'https://example.com/avatar5.jpg', 0, 2, 0),
('pass123', N'Phan Thị D', 'phanthid@gmail.com', '2003-06-06', N'Huế', '0987654321', 'https://example.com/avatar6.jpg', 1, 4, 0),
('qwerty', N'Võ Văn E', 'vovane@gmail.com', '2004-07-07', N'Cần Thơ', '0912345678', 'https://example.com/avatar7.jpg', 0, 4, 0),
('securepass', N'Nguyễn Thị F', 'nguyenthif@gmail.com', '1992-08-08', N'Hải Phòng', '0987654321', 'https://example.com/avatar8.jpg', 1, 3, 0),
('manager', N'Manager', 'manager@gmail.com', '1993-09-09', N'Hà Nội', '0987654321', 'https://example.com/avatar9.jpg', 1, 2, 0),
('staff', N'Staff', 'staff@gmail.com', '1994-10-10', N'Hồ Chí Minh', '0912345678', 'https://example.com/avatar10.jpg', 0, 3, 0),
('12345678', N'Hồ Thị G', 'hothig@gmail.com', '2005-11-11', N'Nha Trang', '0987654321', 'https://example.com/avatar11.jpg', 1, 4, 0),
('password123', N'Đặng Văn H', 'dangvanh@gmail.com', '2006-12-12', N'Vũng Tàu', '0912345678', 'https://example.com/avatar12.jpg', 0, 4, 0),
('securepassword', N'Trần Thị I', 'tranthii@gmail.com', '1995-01-13', N'Đà Lạt', '0987654321', 'https://example.com/avatar13.jpg', 1, 3, 0),
('admin456', N'Admin2', 'admin2@gmail.com', '1996-02-14', N'Hà Nội', '0987654321', 'https://example.com/avatar14.jpg', 1, 1, 0),
('user456', N'User2', 'user2@gmail.com', '1997-03-15', N'Hồ Chí Minh', '0912345678', 'https://example.com/avatar15.jpg', 0, 2, 0),
('pass456', N'Lê Thị K', 'lethik@gmail.com', '2007-04-16', N'Hạ Long', '0987654321', 'https://example.com/avatar16.jpg', 1, 4, 0),
('qwerty456', N'Phạm Văn L', 'phamvanl@gmail.com', '2008-05-17', N'Phan Thiết', '0912345678', 'https://example.com/avatar17.jpg', 0, 4, 0),
('securepass456', N'Vũ Thị M', 'vuthim@gmail.com', '1998-06-18', N'Quy Nhơn', '0987654321', 'https://example.com/avatar18.jpg', 1, 3, 0),
('manager456', N'Manager2', 'manager2@gmail.com', '1999-07-19', N'Hà Nội', '0987654321', 'https://example.com/avatar19.jpg', 1, 2, 0),
('staff456', N'Staff2', 'staff2@gmail.com', '2000-08-20', N'Hồ Chí Minh', '0912345678', 'https://example.com/avatar20.jpg', 0, 3, 0);

-- Thêm dữ liệu cho bảng ADMIN
INSERT INTO ADMIN (IDTaiKhoan, NgayVaoLam, NgayKetThuc) VALUES
(4, '2022-01-01', '2024-12-31'),
(9, '2023-02-01', '2025-12-31'),
(14, '2024-03-01', '2026-12-31'),
(19, '2022-04-01', '2024-12-31');

-- Thêm dữ liệu cho bảng DOCGIA
INSERT INTO DOCGIA (IDTaiKhoan, IDLoaiDocGia, NgayLapThe, NgayHetHan, TongNo, GioiThieu) VALUES
(1, 1, '2024-01-01', '2025-01-01', 0, N'Sinh viên năm 3'),
(2, 2, '2024-02-01', '2025-02-01', 100000, N'Giáo viên dạy Toán'),
(3, 3, '2024-03-01', '2025-03-01', 50000, N'Cán bộ phòng Tài chính'),
(5, 5, '2024-05-01', '2025-05-01', 200000, N'Khách VIP'),
(6, 6, '2024-06-01', '2025-06-01', 0, N'Khách thân thiết'),
(7, 7, '2024-07-01', '2025-07-01', 50000, N'Khách mới'),
(8, 8, '2024-08-01', '2025-08-01', 100000, N'Khách thử nghiệm'),
(10, 2, '2024-10-01', '2025-10-01', 0, N'Giáo viên dạy Văn'),
(11, 3, '2024-11-01', '2025-11-01', 50000, N'Cán bộ phòng Nhân sự'),
(12, 4, '2024-12-01', '2025-12-01', 100000, N'Khách vãng lai'),
(13, 5, '2023-01-01', '2024-01-01', 0, N'Khách VIP'),
(15, 7, '2023-03-01', '2024-03-01', 50000, N'Khách mới'),
(16, 8, '2023-04-01', '2024-04-01', 0, N'Khách thử nghiệm'),
(17, 1, '2023-05-01', '2024-05-01', 100000, N'Sinh viên năm 2'),
(18, 2, '2023-06-01', '2024-06-01', 0, N'Giáo viên dạy Anh'),
(20, 4, '2023-08-01', '2024-08-01', 100000, N'Khách vãng lai');


-- Thêm dữ liệu cho bảng PHIEUTHUTIENPHAT
INSERT INTO PHIEUTHUTIENPHAT (IDDocGia, NgayThu, TongNo, SoTienThu, ConLai, IsDeleted) VALUES
(1, '2024-01-01', 100000, 50000, 50000, 0),
(2, '2024-02-01', 50000, 25000, 25000, 0),
(3, '2024-03-01', 200000, 100000, 100000, 0),
(5, '2024-05-01', 50000, 50000, 0, 0),
(6, '2024-06-01', 200000, 200000, 0, 0),
(7, '2024-07-01', 100000, 50000, 50000, 0),
(8, '2024-08-01', 50000, 25000, 25000, 0),
(10, '2024-10-01', 100000, 100000, 0, 0),
(11, '2024-11-01', 50000, 50000, 0, 0),
(12, '2024-12-01', 200000, 200000, 0, 0),
(13, '2023-01-01', 100000, 50000, 50000, 0),
(15, '2023-03-01', 200000, 100000, 100000, 0),
(16, '2023-04-01', 100000, 100000, 0, 0);
-- =========================================================================
-- ĐỒ ÁN GAME PIKACHU - SCRIPT TẠO DATABASE TỔNG HỢP
-- =========================================================================

-- 1. TẠO CƠ SỞ DỮ LIỆU
CREATE DATABASE PikachuDB;
GO

-- 2. CHUYỂN VÀO DB VỪA TẠO ĐỂ THAO TÁC
USE PikachuDB;
GO

-- =========================================================================
-- PHẦN I: TẠO CÁC BẢNG (TABLES)
-- =========================================================================

-- Bảng 1: Thông tin người chơi & Tiến trình mở khóa
CREATE TABLE Users (
    ID VARCHAR(50) PRIMARY KEY,      -- Căn cước người chơi (Khóa chính)
    Username NVARCHAR(100) NOT NULL, -- Tên hiển thị
    MaxLevel INT DEFAULT 1           -- Cấp độ cao nhất mở khóa được (Mặc định: Màn 1)
);
GO

-- Bảng 2: Lưu trạng thái ván game đang chơi dở (Save Game)
CREATE TABLE SaveGame (
    ID NVARCHAR(50) PRIMARY KEY,     -- Dùng ID để liên kết chính xác với người chơi
    Score INT,                       -- Điểm hiện tại
    TimeLeft INT,                    -- Thời gian còn lại
    CurrentStep INT,                 -- Đang lưu ở Màn mấy
    MatrixData VARCHAR(MAX)          -- Chuỗi nén trạng thái bàn cờ (Trái tim của tính năng Save)
);
GO

-- Bảng 3: Bảng xếp hạng (Leaderboard Top 5)
CREATE TABLE HighScores (
    Id INT IDENTITY(1,1) PRIMARY KEY, -- ID tự tăng
    PlayerName NVARCHAR(100),         -- Tên hiển thị trên bảng xếp hạng
    Score INT,                        -- Điểm số (Ưu tiên 1)
    [Level] INT,                      -- Màn đạt được (Ưu tiên 2)
    TimeLeft INT DEFAULT 0,           -- Thời gian dư (Ưu tiên 3)
    PlayDate DATETIME DEFAULT GETDATE() -- Ngày chơi
);
GO

-- Bảng 4: Cấu hình Ảnh Pokemon
CREATE TABLE PokemonData (
    Id INT PRIMARY KEY,               -- Số thứ tự Pokemon (1 -> 25)
    ImagePath NVARCHAR(255)           -- Tên file ảnh (VD: pieces1.png)
);
GO

-- =========================================================================
-- PHẦN II: KHỞI TẠO DỮ LIỆU MẶC ĐỊNH (SEED DATA)
-- =========================================================================

-- Tự động sinh 25 file ảnh Pokemon bằng vòng lặp (Chống viết code tay mỏi)
DECLARE @i INT = 1;
WHILE @i <= 25
BEGIN
    INSERT INTO PokemonData (Id, ImagePath) 
    VALUES (@i, 'pieces' + CAST(@i AS NVARCHAR) + '.png');
    
    SET @i = @i + 1;
END
GO

PRINT N'✅ Đã khởi tạo thành công PikachuDB cùng toàn bộ dữ liệu!';
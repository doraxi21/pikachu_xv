USE master;
GO

-- 1. XÓA DATABASE CŨ (Nếu đang mở thì tự động đóng kết nối rồi xóa)
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'PikachuDB')
BEGIN
    ALTER DATABASE PikachuDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PikachuDB;
END
GO

-- 2. TẠO DATABASE MỚI
CREATE DATABASE PikachuDB;
GO
USE PikachuDB;
GO

-- =======================================================
-- TẠO CẤU TRÚC BẢNG
-- =======================================================

-- Bảng Users
CREATE TABLE Users (
    ID VARCHAR(50) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL,
    MaxLevel INT DEFAULT 1
);
GO

-- Bảng SaveGame
CREATE TABLE SaveGame (
    ID VARCHAR(50) PRIMARY KEY,
    Score INT,
    TimeLeft INT,
    CurrentStep INT,
    MatrixData VARCHAR(MAX)
);
GO

-- Bảng HighScores (ĐÃ SỬA: Dùng ID làm khóa chính để đồng bộ với Users)
CREATE TABLE HighScores (
    ID VARCHAR(50) PRIMARY KEY,    -- Thẻ căn cước người chơi
    PlayerName NVARCHAR(100),      -- Tên hiển thị trên BXH
    Score INT,
    [Level] INT,
    TimeLeft INT DEFAULT 0,
    PlayDate DATETIME DEFAULT GETDATE()
);
GO

-- Bảng PokemonData
CREATE TABLE PokemonData (
    Id INT PRIMARY KEY,
    ImagePath NVARCHAR(255)
);
GO

-- =======================================================
-- NẠP DỮ LIỆU MẪU (SEED DATA)
-- =======================================================

-- Sinh 25 ảnh Pokemon
DECLARE @i INT = 1;
WHILE @i <= 25
BEGIN
    INSERT INTO PokemonData (Id, ImagePath) 
    VALUES (@i, 'pieces' + CAST(@i AS NVARCHAR) + '.png');
    SET @i = @i + 1;
END
GO

-- Tạo sẵn 2 tài khoản theo yêu cầu
INSERT INTO Users (ID, Username, MaxLevel) VALUES ('admin', 'Admin VIP', 6);
INSERT INTO Users (ID, Username, MaxLevel) VALUES ('player', 'New Player', 1);
GO

PRINT N'✅ Đã khởi tạo hoàn tất PikachuDB với cấu trúc mới và Dữ liệu mẫu!';

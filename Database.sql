USE [master]
GO
CREATE DATABASE [PRN231_FinalProject]
GO
USE [PRN231_FinalProject]
GO
CREATE TABLE [User] (
	ID int IDENTITY(1, 1) primary key,
	FullName nvarchar(255),
	Email varchar(255),
	[Password] varchar(500),
	[Role] varchar(100),
	IsActive bit,
	[TypeAuthentication] varchar(100),
	CreateAt datetime,
	UpdateAt datetime
)
GO
CREATE TABLE [Token] (
	ID INT IDENTITY(1, 1) PRIMARY KEY,
	TokenValue VARCHAR(255),
	IsUse BIT,
	ExpiredTime DATETIME,
	[Type] VARCHAR(100),
	UserID INT,
	CreateAt datetime,
	FOREIGN KEY (UserID) REFERENCES [User](ID)
)
GO
CREATE TABLE Course(
	ID INT IDENTITY (1, 1) PRIMARY KEY,
	[Name] NVARCHAR(500),
	[Description] NVARCHAR(500),
	[Image] VARCHAR(255),
	Href NVARCHAR(500),
	CreateAt DATETIME,
	UpdateAt DATETIME,
)
GO
CREATE TABLE User_Course(
	ID INT IDENTITY(1, 1) PRIMARY KEY,
	UserID INT,
	CourseID INT,
	FOREIGN KEY (UserID) REFERENCES [User](ID),
	FOREIGN KEY (CourseID) REFERENCES [Course](ID)
)
GO
CREATE TABLE Category(
	ID INT IDENTITY(1, 1) PRIMARY KEY,
	[Name] NVARCHAR(255),
	[Description] NVARCHAR(500),
	Href NVARCHAR(255)
)
GO
CREATE TABLE Course_Category(
	ID INT IDENTITY(1, 1) PRIMARY KEY,
	CourseID INT,
	CategoryID INT,
	FOREIGN KEY (CourseID) REFERENCES [Course](ID),
	FOREIGN KEY (CategoryID) REFERENCES [Category](ID)
)
GO
CREATE TABLE Mooc(
	ID INT IDENTITY(1, 1) PRIMARY KEY,
	[Name] NVARCHAR(255),
	[Description] NVARCHAR(255),
	CourseID INT,
	FOREIGN KEY (CourseID) REFERENCES [Course](ID)
)
GO
CREATE TABLE Lesson(
	ID INT IDENTITY(1, 1) PRIMARY KEY,
	[Name] NVARCHAR(255),
	[Description] NVARCHAR(255),
	MoocID INT,
	Href NVARCHAR(255),
	VideoUrl NVARCHAR(500),
	VideoTranscript NTEXT,
	CreateAt DATETIME,
	UpdateAt DATETIME,
	FOREIGN KEY (MoocID) REFERENCES [Mooc](ID)
)
GO
CREATE TABLE Course_Enrolled(
	ID INT IDENTITY(1, 1) PRIMARY KEY,
	UserID INT,
	CourseID INT,
	Href NVARCHAR(500),
	FOREIGN KEY (UserID) REFERENCES [User](ID),
	FOREIGN KEY (CourseID) REFERENCES [Course](ID)
)

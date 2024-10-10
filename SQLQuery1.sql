/*
use master
GO
drop database FirstSQL
GO


CREATE DATABASE FirstSQL
COLLATE Hebrew_CI_AS  
GO

use FirstSQL
GO
*/
--drop table Users
CREATE TABLE Cities(
City_Code INT not null PRIMARY KEY ,
City_Name NVARCHAR(50) null 
)
GO



Create Table Roles(
	Role_code smallint Not null PRIMARY KEY,
	Role_desc Nvarchar(25)
	)
GO

select * from Role_codes

CREATE TABLE Buttons (
  Id INT PRIMARY KEY,
  ButtonName VARCHAR(255) NOT NULL,
  DefaultSize INT NOT NULL DEFAULT 100,
  MaxSize INT NOT NULL DEFAULT 200,
  SizeFactor DECIMAL(4, 2) NOT NULL DEFAULT 0.2
);

delete Users
CREATE TABLE UserButtonClicks (
  UserId INT NOT NULL,
  ButtonId INT NOT NULL,
  ClickCount INT NOT NULL DEFAULT 0,
  LastClickTimestamp DATETIME2 NOT NULL,

  CONSTRAINT PK_UserButtonClicks PRIMARY KEY (UserId, ButtonId),
  FOREIGN KEY (UserId) REFERENCES Users (Id),
  FOREIGN KEY (ButtonId) REFERENCES Buttons (Id)
);

INSERT INTO Buttons (id, ButtonName,  MaxSize, SizeFactor)
VALUES 
    (1,'??????',  200, 5),
    (2,'?????',  200, 5),
    (3,'?????',  250, 5),
    (4,'??? ???',  250, 5),
    (5,'????? ???????',  250, 5),
    (6,'???',  250, 5),
    (7,'????? ????',  250, 5);

delete PasswordResetSys
delete Buttons
delete UserButtonClicks
delete PasswordResetSys

select * from  UserButtonClicks


CREATE TABLE PasswordResetSys(
	Id INT PRIMARY KEY,
	Email NVARCHAR(50) NOT NULL,
	PasswordResetToken NVARCHAR(MAX) NOT NULL,
	PasswordResetTokenExpiration DATETIME2(7) NOT NULL
)
GO

select* from PasswordResetSys
select* from Users

CREATE TABLE Users (
  Id INT PRIMARY KEY,
  Email NVARCHAR(50) NOT NULL UNIQUE,
  PasswordHash VARBINARY(255) NOT NULL,
  UserRole NVARCHAR(20) not null -- 1 Staff, 2 Lecturer, 3 Student
)
GO

select * from Students
select * from Lecturers
delete Lecturers whEre Id = 9
delete Users whEre Id = 9


INSERT INTO Users (Id, Email, PasswordHash, UserRole)
VALUES (1, 'admin@admin.com', 0xC4CA4238A0B923820DCC509A6F75849B, 'Admin');
GO


CREATE TABLE Students (
    Id INT NOT NULL PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    School_Year smallint NOT NULL,
    Phone NVARCHAR(13) NOT NULL UNIQUE,
    Email NVARCHAR(50) NOT NULL UNIQUE,
	Picture_URL NVARCHAR(MAX) NULL ,
	Address NVARCHAR(50) NULL ,
	City_Code int NULL ,
	Enrollment date NULL,
	FOREIGN KEY (City_Code) REFERENCES Cities(City_Code),
	)
GO


CREATE TABLE Lecturers (
    Id INT NOT NULL PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Phone NVARCHAR(13) NOT NULL UNIQUE,
    Email NVARCHAR(50) NOT NULL UNIQUE,
	Academic_Degree NVARCHAR(20) NULL,
	Start_Date date NOT NULL,
	Address NVARCHAR(50) NULL ,
	City_Code INT NULL ,
	FOREIGN KEY (City_Code) REFERENCES Cities(City_Code)
)
GO



Create Table Staff (
	Id int Not Null Primary Key, 
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
	Role_code smallint null,
	Email Nvarchar(50) Not null UNIQUE,
	Phone Nvarchar(13) UNIQUE,
	City_Code INT NULL,
	Foreign Key (Role_code) References Roles(Role_code)
	)
GO


CREATE TABLE Courses (
    Id INT PRIMARY KEY, --IDENTITY(1,1)
    CourseName VARCHAR(255) NOT NULL,
    LecturerId INT NOT NULL,
    Classroom VARCHAR(50) NULL, -- Optional
    FOREIGN KEY (LecturerId) REFERENCES Lecturers(Id) 
);

CREATE TABLE StudentCourses (
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    CONSTRAINT PK_StudentCourses PRIMARY KEY (StudentId, CourseId),
    FOREIGN KEY (StudentId) REFERENCES Students(Id),
    FOREIGN KEY (CourseId) REFERENCES Courses(Id)
);


CREATE TABLE Assignments (
    Id INT PRIMARY KEY, --IDENTITY(1,1)
    CourseId INT NOT NULL,
    Title VARCHAR(255) NOT NULL,
    Description TEXT NULL,
    Deadline DATETIME2 NOT NULL,
    IsVisible BIT NOT NULL DEFAULT 1, -- 1 - visible, 0 - not visible
    FOREIGN KEY (CourseId) REFERENCES Courses(Id)
);



CREATE TABLE StudentAssignments (
    StudentId INT NOT NULL,
    AssignmentId INT NOT NULL,
    SubmissionStatus VARCHAR(50) NULL, 
    SubmissionTimestamp DATETIME2 NULL,
    Grade INT NULL,
    CONSTRAINT PK_StudentAssignments PRIMARY KEY (StudentId, AssignmentId), 
    FOREIGN KEY (StudentId) REFERENCES Students(Id),
    FOREIGN KEY (AssignmentId) REFERENCES Assignments(Id)
);
GO

CREATE TABLE Exams (
    Id INT  PRIMARY KEY, --IDENTITY(1,1)
    CourseId INT NOT NULL,
    ExamDate DATETIME2 NOT NULL,
    Description TEXT NULL,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id)
);

CREATE TABLE StudentExamGrades (
    StudentId INT NOT NULL,
    ExamId INT NOT NULL,
    Grade INT NULL,
    CONSTRAINT PK_StudentExamGrades PRIMARY KEY (StudentId, ExamId),
    FOREIGN KEY (StudentId) REFERENCES Students(Id),
    FOREIGN KEY (ExamId) REFERENCES Exams(Id)
);


CREATE TABLE WeekDays (
    Id INT PRIMARY KEY,
    DayName VARCHAR(10) NOT NULL
);


INSERT INTO WeekDays (Id, DayName) VALUES
    (1, 'Sunday'),
    (2, 'Monday'),
    (3, 'Tuesday'),
    (4, 'Wednesday'),
    (5, 'Thursday'),
    (6, 'Friday'),
    (7, 'Saturday');


CREATE TABLE Lessons (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CourseId INT NOT NULL,
    WeekDayId INT NOT NULL,
    Classroom VARCHAR(50) NULL, 
	StartTime TIME(0) NOT NULL,
    EndTime TIME(0) NOT NULL,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id),
    FOREIGN KEY (WeekDayId) REFERENCES WeekDays(Id)
);

CREATE TABLE StudentLessons (
    StudentId INT NOT NULL,
    LessonId INT NOT NULL,
    Attendance VARCHAR(10) NOT NULL DEFAULT 'Absent',
	LessonDate DATE NOT NULL,
    CONSTRAINT PK_StudentLessons PRIMARY KEY (StudentId, LessonId, LessonDate),
    FOREIGN KEY (StudentId) REFERENCES Students(Id),
    FOREIGN KEY (LessonId) REFERENCES Lessons(Id)
);



INSERT INTO Lessons (CourseId, WeekDayId, Classroom, StartTime, EndTime) 
VALUES (1, 2, 'А-101', '10:00', '11:30');


INSERT INTO StudentLessons (StudentId, LessonId, Attendance, LessonDate)
VALUES (2, 1, 'Present', '2024-02-26');



  
select * from StudentLessons





INSERT INTO StudentExamGrades (StudentId, ExamId, Grade)
VALUES
    (2, 1, 99),
    (2, 2, 98)
GO


INSERT INTO Courses (Id, CourseName, LecturerId, Classroom)
VALUES
    (1, 'SQL', 5, 'А-101'),
    (2, 'C#', 6, 'В-202'),
    (3, 'React', 7, 'С-303');
GO

INSERT INTO StudentCourses (StudentId, CourseId)
VALUES
    (2, 1), -- Студент 2 на Введение в программирование
    (3, 2), -- Студент 3 на Базы данных
    (2, 3), -- Студент 2 на Веб-разработка
    (4, 3); -- Студент 4 на Веб-разработка
GO

INSERT INTO StudentAssignments (StudentId, AssignmentId, SubmissionStatus, SubmissionTimestamp, Grade)
VALUES
    (2, 101, 'Completed', '2024-02-20', 85),
    (2, 102, 'Completed', '2024-03-20', 90),
    (2, 103, 'Pending', NULL, NULL);


INSERT INTO Assignments (Id, CourseId, Title, Description, Deadline, IsVisible)
VALUES
    (101, 1, 'SQL PROJ 1', 'You need to ...','2024-02-20', 1),
    (102, 2, 'SQL PROJ 2','You need to ...' ,'2024-03-20', 1),
    (103, 2, 'C# PROJ 1','You need to ...' ,'2024-01-20', 1);





INSERT INTO Exams (Id, CourseId, ExamDate, Description)
VALUES
    (1, 1, '2024-01-25', 'InnerJoin'),
    (2, 2, '2024-02-25', 'OOP');
GO



INSERT INTO Cities (City_Code, City_Name) VALUES
    (1, 'Jerusalem'),
    (2, 'Tel Aviv'),
    (3, 'Haifa'),
    (4, 'Beer Sheva'),
    (5, 'Rishon LeZion'),
    (6, 'Petah Tikva'),
    (7, 'Ashdod'),
    (8, 'Netanya'),
    (9, 'Holon'),
    (10, 'Bat Yam');








CREATE OR ALTER PROCEDURE AddUser 
    @Username NVARCHAR(50),
	@Email NVARCHAR(50),
    @PasswordHash VARBINARY(255)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username or Email = @Email)
    BEGIN
        RAISERROR('User OR Email Alrady Exists', 16, 1)
        RETURN
    END

    INSERT INTO Users (Username, Email, PasswordHash) 
    VALUES (@Username, @Email, @PasswordHash);

	SELECT * FROM Users WHERE Username = @Username
END
GO

Select * from Users

delete Users where Users.UserID = 3
go

exec AddUser 'asd','asd@asd.com', 0xEBA6
go

--drop table TriggerLogArchive
CREATE TABLE TriggerLog (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    TriggerName VARCHAR(255),
    Action VARCHAR(255),
    ObjectName VARCHAR(255),
	UserEmail NVARCHAR(50) NULL,
	MessageToUser NVARCHAR(500) NULL,
    Timestamp DATETIME2
);

CREATE TABLE TriggerLogArchive (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    TriggerName VARCHAR(255),
    Action VARCHAR(255),
    ObjectName VARCHAR(255),
	UserEmail NVARCHAR(50) NULL,
	MessageToUser NVARCHAR(500) NULL,
    Timestamp DATETIME2
);

CREATE or ALTER TRIGGER UserRegistrationTrigger
ON Users
AFTER INSERT
AS
BEGIN
    -- Получение информации о новом пользователе
    DECLARE @Email NVARCHAR(50);
    SELECT @Email = i.Email
    FROM inserted i;

    -- Запись в журнал
    INSERT INTO TriggerLog (TriggerName, Action, ObjectName, UserEmail , Timestamp)
    VALUES ('UserRegistrationTrigger', 'INSERT', 'Users', @Email , GETDATE());

	INSERT INTO TriggerLogArchive (TriggerName, Action, ObjectName, UserEmail , Timestamp)
    VALUES ('UserRegistrationTrigger', 'INSERT', 'Users', @Email , GETDATE());

    -- Дополнительные действия, например, отправка уведомления администратору 
    -- (например, с помощью процедуры send_email)
    -- ...
END
GO


select * from TriggerLog

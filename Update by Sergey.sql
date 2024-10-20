CREATE TABLE Notices (
    NoticeID INT PRIMARY KEY IDENTITY(1,1), -- Auto-incrementing ID for unique identification
    Title NVARCHAR(255) NOT NULL, -- Title of the notice
    Content TEXT NOT NULL, -- Full content of the notice
    Preview NVARCHAR(255) NOT NULL, -- Short preview or summary of the notice
    PublishDate DATE DEFAULT GETDATE(), -- Date the notice is published, default is current date
    Author NVARCHAR(100), -- Optional field to store the author or publisher of the notice
    ExpiryDate DATE, -- Optional expiry date for time-sensitive notices
    Category NVARCHAR(100) -- Optional category for grouping notices
);

INSERT INTO Notices (Title, Content, Preview, PublishDate, Author, ExpiryDate, Category)
VALUES
('School Closure Announcement', 'The school will be closed on Friday for maintenance.', 'The school will be closed on Friday...', '2024-10-12', 'Principal', '2024-10-15', 'General'),
('Exam Schedule Update', 'The midterm exam schedule has been updated. Please check the portal.', 'Midterm exam schedule update...', '2024-10-11', 'Exam Committee', NULL, 'Exams'),
('New Cafeteria Menu', 'The cafeteria has introduced new healthy meal options.', 'Cafeteria introduces new meals...', '2024-10-10', 'Admin', NULL, 'Announcements'),
('Holiday Break', 'The school will be closed for the winter holidays from Dec 20 to Jan 5.', 'School closed for winter holidays...', '2024-10-05', 'Principal', '2025-01-05', 'General'),
('Library Extended Hours', 'The library will be open 24/7 during the exam period.', 'Library extended hours during exams...', '2024-10-08', 'Librarian', '2024-11-01', 'Facilities'),
('New Parking Rules', 'New parking rules will be enforced starting next month. Please check your email.', 'New parking rules enforcement...', '2024-10-07', 'Admin', NULL, 'Regulations');
Go 

Select * from Notices
Go

Select * from users
go

CREATE DATABASE LeaveManagementDB;
GO

USE LeaveManagementDB;
GO



CREATE TABLE Roles (
    RoleID   INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);
GO

INSERT INTO Roles (RoleName) VALUES ('Admin'), ('Manager'), ('Employee');
GO

CREATE TABLE Departments (
    DepartmentID   INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL UNIQUE,
    CreatedAt      DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Users (
    UserID         INT IDENTITY(1,1) PRIMARY KEY,
    FirstName      NVARCHAR(50)  NOT NULL,
    LastName       NVARCHAR(50)  NOT NULL,
    Email          NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash   NVARCHAR(255) NOT NULL,
    RoleID         INT           NOT NULL,
    DepartmentID   INT,
    ManagerID      INT,
    IsActive       BIT           DEFAULT 1,
    CreatedAt      DATETIME      DEFAULT GETDATE(),
    CONSTRAINT FK_Users_Role       FOREIGN KEY (RoleID)       REFERENCES Roles(RoleID),
    CONSTRAINT FK_Users_Department FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID),
    CONSTRAINT FK_Users_Manager    FOREIGN KEY (ManagerID)    REFERENCES Users(UserID)
);
GO

CREATE TABLE LeaveTypes (
    LeaveTypeID  INT IDENTITY(1,1) PRIMARY KEY,
    TypeName     NVARCHAR(100) NOT NULL UNIQUE,
    MaxDaysPerYear INT         NOT NULL DEFAULT 0,
    IsPaid       BIT           DEFAULT 1,
    Description  NVARCHAR(255)
);
GO

INSERT INTO LeaveTypes (TypeName, MaxDaysPerYear, IsPaid, Description) VALUES
('Annual Leave',    21,  1, 'Yearly paid leave entitlement'),
('Sick Leave',      14,  1, 'Medical or illness related leave'),
('Maternity Leave', 90,  1, 'Leave for new mothers'),
('Paternity Leave', 14,  1, 'Leave for new fathers'),
('Unpaid Leave',    30,  0, 'Leave without pay'),
('Emergency Leave',  3,  1, 'Short notice emergency leave');
GO

CREATE TABLE LeaveBalances (
    BalanceID   INT IDENTITY(1,1) PRIMARY KEY,
    UserID      INT NOT NULL,
    LeaveTypeID INT NOT NULL,
    Year        INT NOT NULL,
    TotalDays   INT NOT NULL DEFAULT 0,
    UsedDays    INT NOT NULL DEFAULT 0,
    RemainingDays AS (TotalDays - UsedDays) PERSISTED,
    CONSTRAINT FK_Balance_User      FOREIGN KEY (UserID)      REFERENCES Users(UserID),
    CONSTRAINT FK_Balance_LeaveType FOREIGN KEY (LeaveTypeID) REFERENCES LeaveTypes(LeaveTypeID),
    CONSTRAINT UQ_Balance UNIQUE (UserID, LeaveTypeID, Year)
);
GO

CREATE TABLE LeaveRequests (
    RequestID    INT IDENTITY(1,1) PRIMARY KEY,
    UserID       INT           NOT NULL,
    LeaveTypeID  INT           NOT NULL,
    StartDate    DATE          NOT NULL,
    EndDate      DATE          NOT NULL,
    TotalDays    INT           NOT NULL,
    Reason       NVARCHAR(500) NOT NULL,
    Status       NVARCHAR(20)  CHECK (Status IN ('Pending','Approved','Rejected','Cancelled')) DEFAULT 'Pending',
    ReviewedBy   INT,
    ReviewedAt   DATETIME,
    ReviewNote   NVARCHAR(500),
    AppliedAt    DATETIME      DEFAULT GETDATE(),
    UpdatedAt    DATETIME      DEFAULT GETDATE(),
    CONSTRAINT FK_Request_User      FOREIGN KEY (UserID)      REFERENCES Users(UserID),
    CONSTRAINT FK_Request_LeaveType FOREIGN KEY (LeaveTypeID) REFERENCES LeaveTypes(LeaveTypeID),
    CONSTRAINT FK_Request_Reviewer  FOREIGN KEY (ReviewedBy)  REFERENCES Users(UserID)
);
GO

CREATE TABLE Notifications (
    NotificationID INT IDENTITY(1,1) PRIMARY KEY,
    UserID         INT           NOT NULL,
    Title          NVARCHAR(150) NOT NULL,
    Message        NVARCHAR(500) NOT NULL,
    IsRead         BIT           DEFAULT 0,
    CreatedAt      DATETIME      DEFAULT GETDATE(),
    CONSTRAINT FK_Notif_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO


-- Trigger: Deduct leave balance on approval
CREATE OR ALTER TRIGGER trg_DeductLeaveBalance
ON LeaveRequests
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE Status = 'Approved')
       AND EXISTS (SELECT 1 FROM deleted WHERE Status = 'Pending')
    BEGIN
        UPDATE lb
        SET lb.UsedDays = lb.UsedDays + i.TotalDays
        FROM LeaveBalances lb
        INNER JOIN inserted i ON lb.UserID = i.UserID AND lb.LeaveTypeID = i.LeaveTypeID
            AND lb.Year = YEAR(i.StartDate)
        WHERE i.Status = 'Approved';

        INSERT INTO Notifications (UserID, Title, Message)
        SELECT i.UserID,
               'Leave Request Approved',
               CONCAT('Your leave request from ', i.StartDate, ' to ', i.EndDate, ' has been approved')
        FROM inserted i
        WHERE i.Status = 'Approved';
    END

    IF EXISTS (SELECT 1 FROM inserted WHERE Status = 'Rejected')
       AND EXISTS (SELECT 1 FROM deleted WHERE Status = 'Pending')
    BEGIN
        INSERT INTO Notifications (UserID, Title, Message)
        SELECT i.UserID,
               'Leave Request Rejected',
               CONCAT('Your leave request from ', i.StartDate, ' to ', i.EndDate, ' has been rejected')
        FROM inserted i
        WHERE i.Status = 'Rejected';
    END
END;
GO

CREATE OR ALTER TRIGGER trg_RestoreLeaveBalance
ON LeaveRequests
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE Status = 'Cancelled')
       AND EXISTS (SELECT 1 FROM deleted WHERE Status = 'Approved')
    BEGIN
        UPDATE lb
        SET lb.UsedDays = lb.UsedDays - i.TotalDays
        FROM LeaveBalances lb
        INNER JOIN inserted i ON lb.UserID = i.UserID AND lb.LeaveTypeID = i.LeaveTypeID
            AND lb.Year = YEAR(i.StartDate)
        WHERE i.Status = 'Cancelled';
    END
END;
GO

CREATE OR ALTER PROCEDURE sp_InitialiseLeaveBalances
    @UserID INT,
    @Year   INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO LeaveBalances (UserID, LeaveTypeID, Year, TotalDays)
    SELECT @UserID, LeaveTypeID, @Year, MaxDaysPerYear
    FROM LeaveTypes
    WHERE NOT EXISTS (
        SELECT 1 FROM LeaveBalances
        WHERE UserID = @UserID AND LeaveTypeID = LeaveTypes.LeaveTypeID AND Year = @Year
    );
END;
GO

CREATE OR ALTER PROCEDURE sp_GetTeamLeaveStatus
    @ManagerID INT,
    @Month     INT,
    @Year      INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.UserID,
        CONCAT(u.FirstName, ' ', u.LastName) AS EmployeeName,
        lt.TypeName AS LeaveType,
        lr.StartDate,
        lr.EndDate,
        lr.TotalDays,
        lr.Status,
        lr.AppliedAt
    FROM LeaveRequests lr
    INNER JOIN Users     u  ON lr.UserID      = u.UserID
    INNER JOIN LeaveTypes lt ON lr.LeaveTypeID = lt.LeaveTypeID
    WHERE u.ManagerID = @ManagerID
      AND MONTH(lr.StartDate) = @Month
      AND YEAR(lr.StartDate)  = @Year
    ORDER BY lr.StartDate;
END;
GO

-- Sample data
INSERT INTO Departments (DepartmentName) VALUES
('Engineering'), ('Human Resources'), ('Finance'), ('Marketing'), ('Operations');
GO

INSERT INTO Users (FirstName, LastName, Email, PasswordHash, RoleID, DepartmentID) VALUES
('Admin',   'User',    'admin@company.com',   'hashed_password_here', 1, 2),
('Jane',    'Manager', 'jane@company.com',    'hashed_password_here', 2, 1),
('John',    'Doe',     'john@company.com',    'hashed_password_here', 3, 1),
('Sarah',   'Smith',   'sarah@company.com',   'hashed_password_here', 3, 1),
('Michael', 'Brown',   'michael@company.com', 'hashed_password_here', 3, 3);
GO

UPDATE Users SET ManagerID = 2 WHERE UserID IN (3, 4, 5);
GO

EXEC sp_InitialiseLeaveBalances @UserID = 3, @Year = 2025;
EXEC sp_InitialiseLeaveBalances @UserID = 4, @Year = 2025;
EXEC sp_InitialiseLeaveBalances @UserID = 5, @Year = 2025;
GO

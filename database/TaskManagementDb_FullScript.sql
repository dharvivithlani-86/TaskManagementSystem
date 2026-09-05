-- ============================================================================
-- TEAM TASK MANAGEMENT SYSTEM - FULL DATABASE SCHEMA & SEED DATA SCRIPT
-- Target Engine: Microsoft SQL Server 2016+ / Azure SQL / LocalDB
-- Database Name: TaskManagementDb
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'TaskManagementDb')
BEGIN
    CREATE DATABASE [TaskManagementDb];
END
GO

USE [TaskManagementDb];
GO

-- 1. Create Migration History Table
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

-- 2. Create Users Table
IF OBJECT_ID(N'[Users]') IS NULL
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY(1,1),
        [FullName] nvarchar(100) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;
GO

-- 3. Create Teams Table
IF OBJECT_ID(N'[Teams]') IS NULL
BEGIN
    CREATE TABLE [Teams] (
        [Id] int NOT NULL IDENTITY(1,1),
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [LeadManagerId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Teams] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Teams_Users_LeadManagerId] FOREIGN KEY ([LeadManagerId]) REFERENCES [Users] ([Id])
    );
    CREATE INDEX [IX_Teams_LeadManagerId] ON [Teams] ([LeadManagerId]);
END;
GO

-- 4. Create TaskItems Table
IF OBJECT_ID(N'[TaskItems]') IS NULL
BEGIN
    CREATE TABLE [TaskItems] (
        [Id] int NOT NULL IDENTITY(1,1),
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Priority] nvarchar(20) NOT NULL,
        [DueDate] datetime2 NOT NULL,
        [TeamId] int NULL,
        [CreatedByUserId] int NOT NULL,
        [AssignedToUserId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_TaskItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TaskItems_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_TaskItems_Users_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users] ([Id]),
        CONSTRAINT [FK_TaskItems_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id])
    );
    CREATE INDEX [IX_TaskItems_AssignedToUserId] ON [TaskItems] ([AssignedToUserId]);
    CREATE INDEX [IX_TaskItems_CreatedByUserId] ON [TaskItems] ([CreatedByUserId]);
    CREATE INDEX [IX_TaskItems_TeamId] ON [TaskItems] ([TeamId]);
END;
GO

-- 5. Create TeamMembers Table
IF OBJECT_ID(N'[TeamMembers]') IS NULL
BEGIN
    CREATE TABLE [TeamMembers] (
        [Id] int NOT NULL IDENTITY(1,1),
        [TeamId] int NOT NULL,
        [UserId] int NOT NULL,
        [JoinedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_TeamMembers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TeamMembers_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TeamMembers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
    CREATE INDEX [IX_TeamMembers_TeamId_UserId_IsDeleted] ON [TeamMembers] ([TeamId], [UserId], [IsDeleted]);
    CREATE INDEX [IX_TeamMembers_UserId] ON [TeamMembers] ([UserId]);
END;
GO

-- 6. Create Comments Table
IF OBJECT_ID(N'[Comments]') IS NULL
BEGIN
    CREATE TABLE [Comments] (
        [Id] int NOT NULL IDENTITY(1,1),
        [TaskItemId] int NOT NULL,
        [UserId] int NOT NULL,
        [Content] nvarchar(2000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comments_TaskItems_TaskItemId] FOREIGN KEY ([TaskItemId]) REFERENCES [TaskItems] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Comments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
    CREATE INDEX [IX_Comments_TaskItemId] ON [Comments] ([TaskItemId]);
    CREATE INDEX [IX_Comments_UserId] ON [Comments] ([UserId]);
END;
GO

-- 7. Create NotificationLogs Table
IF OBJECT_ID(N'[NotificationLogs]') IS NULL
BEGIN
    CREATE TABLE [NotificationLogs] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] int NOT NULL,
        [TaskItemId] int NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [Type] nvarchar(30) NOT NULL,
        [IsRead] bit NOT NULL DEFAULT 0,
        [EmailSent] bit NOT NULL DEFAULT 0,
        [EmailSentAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_NotificationLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NotificationLogs_TaskItems_TaskItemId] FOREIGN KEY ([TaskItemId]) REFERENCES [TaskItems] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_NotificationLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_NotificationLogs_TaskItemId] ON [NotificationLogs] ([TaskItemId]);
    CREATE INDEX [IX_NotificationLogs_UserId] ON [NotificationLogs] ([UserId]);
END;
GO

-- 8. Record Migration in EF History
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260905060725_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905060725_InitialCreate', N'10.0.11');
END;
GO

-- ============================================================================
-- SEED DATA INSERTION
-- Password for all seed users is BCrypt hash of: Admin@12345, Manager@12345, User@12345
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM [Users])
BEGIN
    SET IDENTITY_INSERT [Users] ON;
    INSERT INTO [Users] ([Id], [FullName], [Email], [PasswordHash], [Role], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES 
    (1, 'System Administrator', 'admin@tms.com', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'Admin', GETUTCDATE(), 'System', 0),
    (2, 'Sarah Jenkins (Lead Manager)', 'manager@tms.com', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'Manager', GETUTCDATE(), 'System', 0),
    (3, 'Alex Rivera (Developer)', 'user@tms.com', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'User', GETUTCDATE(), 'System', 0),
    (4, 'Elena Rostova (Frontend Engineer)', 'elena@tms.com', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'User', GETUTCDATE(), 'System', 0);
    SET IDENTITY_INSERT [Users] OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM [Teams])
BEGIN
    SET IDENTITY_INSERT [Teams] ON;
    INSERT INTO [Teams] ([Id], [Name], [Description], [LeadManagerId], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
    (1, 'Core Engineering Team', 'Cross-functional squad focused on API architecture, microservices, and client applications.', 2, GETUTCDATE(), 'admin@tms.com', 0),
    (2, 'Product & UX Team', 'UI design system, accessibility compliance, and product discovery.', 2, GETUTCDATE(), 'admin@tms.com', 0);
    SET IDENTITY_INSERT [Teams] OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM [TeamMembers])
BEGIN
    SET IDENTITY_INSERT [TeamMembers] ON;
    INSERT INTO [TeamMembers] ([Id], [TeamId], [UserId], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
    (1, 1, 3, GETUTCDATE(), GETUTCDATE(), 'manager@tms.com', 0),
    (2, 1, 4, GETUTCDATE(), GETUTCDATE(), 'manager@tms.com', 0),
    (3, 2, 4, GETUTCDATE(), GETUTCDATE(), 'manager@tms.com', 0);
    SET IDENTITY_INSERT [TeamMembers] OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM [TaskItems])
BEGIN
    SET IDENTITY_INSERT [TaskItems] ON;
    INSERT INTO [TaskItems] ([Id], [Title], [Description], [Status], [Priority], [DueDate], [TeamId], [CreatedByUserId], [AssignedToUserId], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
    (1, 'Implement JWT Authentication & RBAC Filters', 'Establish secure JSON Web Token authentication with claims for Admin, Manager, and User roles.', 'Done', 'High', DATEADD(day, 2, GETUTCDATE()), 1, 2, 3, GETUTCDATE(), 'manager@tms.com', 0),
    (2, 'Build Interactive Kanban Board & Task Filter Grid', 'Deliver responsive drag-or-click Kanban board and table filterable by status, priority, and deadline.', 'InProgress', 'Urgent', DATEADD(day, 5, GETUTCDATE()), 1, 2, 3, GETUTCDATE(), 'manager@tms.com', 0),
    (3, 'Develop In-App Notification Center and Mock Email Service', 'Trigger notifications on task assignment and status updates with real-time UI indicator and email logging.', 'ToDo', 'Medium', DATEADD(day, 7, GETUTCDATE()), 1, 2, 4, GETUTCDATE(), 'manager@tms.com', 0),
    (4, 'Audit Mobile Responsive Breakpoints & Accessibility', 'Validate WCAG 2.1 AA accessibility contrast, keyboard navigation, and responsive drawer layouts.', 'ToDo', 'Low', DATEADD(day, 10, GETUTCDATE()), 2, 1, 4, GETUTCDATE(), 'admin@tms.com', 0);
    SET IDENTITY_INSERT [TaskItems] OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM [Comments])
BEGIN
    SET IDENTITY_INSERT [Comments] ON;
    INSERT INTO [Comments] ([Id], [TaskItemId], [UserId], [Content], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
    (1, 2, 2, 'Great progress on the board layouts! Lets ensure columns reflect To Do, In Progress, and Done clearly.', DATEADD(hour, -6, GETUTCDATE()), 'manager@tms.com', 0),
    (2, 2, 3, 'Working on the status transition endpoints and optimistic UI updates now.', DATEADD(hour, -3, GETUTCDATE()), 'user@tms.com', 0);
    SET IDENTITY_INSERT [Comments] OFF;
END
GO

PRINT '========================================================================';
PRINT 'TaskManagementDb Schema & Seed Data successfully created!';
PRINT '========================================================================';
GO

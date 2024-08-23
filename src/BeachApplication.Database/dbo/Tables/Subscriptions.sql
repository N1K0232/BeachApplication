CREATE TABLE [dbo].[Subscriptions]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATE NOT NULL,
    [FinishDate] DATE NOT NULL,
    [Status] NVARCHAR(50) NOT NULL,
    [Notes] NVARCHAR(MAX) NULL,
    [Price] DECIMAL(8, 2) NOT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModifiedAt] DATETIME NULL,
    [IsDeleted] BIT NOT NULL DEFAULT (0),
    [DeletedAt] DATETIME NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
)
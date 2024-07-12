CREATE TABLE [dbo].[Orders]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Status] NVARCHAR(50) NOT NULL,
    [OrderDate] DATE NOT NULL,
    [OrderTime] TIME(7) NOT NULL,
    [SecurityStamp] NVARCHAR(MAX) NOT NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NOT NULL,
    [CreationDate] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModificationDate] DATETIME NULL,
    [IsDeleted] BIT NOT NULL DEFAULT (0),
    [DeletedDate] DATETIME NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
)
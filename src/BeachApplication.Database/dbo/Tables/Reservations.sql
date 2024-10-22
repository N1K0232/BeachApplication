CREATE TABLE [dbo].[Reservations]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [UmbrellaId] UNIQUEIDENTIFIER NOT NULL,
    [StartOn] DATE NOT NULL,
    [StartAt] TIME(7) NOT NULL,
    [EndsOn] DATE NOT NULL,
    [EndsAt] TIME(7) NOT NULL,
    [Notes] NVARCHAR(MAX) NULL,
    [TotalPrice] DECIMAL(8, 2) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModifiedAt] DATETIME NULL,
    [IsDeleted] BIT NOT NULL DEFAULT (0),
    [DeletedAt] DATETIME NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]),
    FOREIGN KEY([UmbrellaId]) REFERENCES [dbo].[Umbrellas]([Id])
)
CREATE TABLE [dbo].[Products]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(256) NOT NULL,
    [Description] NVARCHAR(4000) NOT NULL,
    [Quantity] INTEGER NULL,
    [Price] DECIMAL(8, 2) NOT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModifiedAt] DATETIME NULL,
    [IsDeleted] BIT NOT NULL DEFAULT (0),
    [DeletedAt] DATETIME NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([CategoryId]) REFERENCES [dbo].[Categories]([Id])
)
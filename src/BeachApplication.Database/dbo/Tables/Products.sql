CREATE TABLE [dbo].[Products]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(256) NOT NULL,
    [Description] NVARCHAR(4000) NOT NULL,
    [Quantity] INTEGER NULL,
    [Price] DECIMAL(8, 2) NOT NULL,
    [CreationDate] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModificationDate] DATETIME NULL,
    [IsDeleted] BIT NOT NULL DEFAULT (0),
    [DeletedDate] DATETIME NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([CategoryId]) REFERENCES [dbo].[Categories]([Id])
)
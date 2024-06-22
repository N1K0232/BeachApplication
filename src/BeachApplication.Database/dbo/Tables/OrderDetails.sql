CREATE TABLE [dbo].[OrderDetails]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [OrderId] UNIQUEIDENTIFIER NOT NULL,
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [Quantity] INTEGER NULL,
    [Price] DECIMAL(8, 2) NOT NULL,
    [Annotations] NVARCHAR(MAX) NULL,
    [CreationDate] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModificationDate] DATETIME NULL,
    [IsDeleted] BIT NOT NULL DEFAULT (0),
    [DeletedDate] DATETIME NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([OrderId]) REFERENCES [dbo].[Orders]([Id]),
    FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products]([Id])
)
CREATE TABLE [dbo].[OrderDetails]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [OrderId] UNIQUEIDENTIFIER NOT NULL,
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [Quantity] INTEGER NULL,
    [Price] DECIMAL(8, 2) NOT NULL,
    [Notes] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModifiedAt] DATETIME NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([OrderId]) REFERENCES [dbo].[Orders]([Id]),
    FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products]([Id])
)
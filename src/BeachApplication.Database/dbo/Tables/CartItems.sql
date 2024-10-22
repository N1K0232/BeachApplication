CREATE TABLE [dbo].[CartItems]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [CartId] UNIQUEIDENTIFIER NOT NULL,
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [Quantity] INTEGER NOT NULL,
    [Notes] NVARCHAR(MAX) NULL,
    [Price] DECIMAL(8, 2) NOT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModifiedAt] DATETIME NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([CartId]) REFERENCES [dbo].[Carts]([Id]),
    FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products]([Id])
)
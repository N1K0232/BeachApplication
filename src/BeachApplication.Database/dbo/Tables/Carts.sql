CREATE TABLE [dbo].[Carts]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModifiedAt] DATETIME NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
)
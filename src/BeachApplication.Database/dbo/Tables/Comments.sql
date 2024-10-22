CREATE TABLE [dbo].[Comments]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Score] INTEGER NOT NULL,
    [Title] NVARCHAR(150) NOT NULL,
    [Text] NVARCHAR(MAX) NOT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModifiedAt] DATETIME NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserComment]
ON [dbo].[Comments]([UserId],[Title]);
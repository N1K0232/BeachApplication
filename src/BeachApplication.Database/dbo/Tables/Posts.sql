CREATE TABLE [dbo].[Posts]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [Title] NVARCHAR(256) NOT NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [IsPublished] BIT NOT NULL DEFAULT (1),
    [CreatedAt] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModifiedAt] DATETIME NULL,

    PRIMARY KEY([Id])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Title]
ON [dbo].[Posts]([Title]);
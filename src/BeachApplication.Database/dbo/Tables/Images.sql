CREATE TABLE [dbo].[Images]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [Path] NVARCHAR(512) NOT NULL,
    [Length] BIGINT NOT NULL,
    [ContentType] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [SecurityStamp] NVARCHAR(MAX) NOT NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NOT NULL,
    [CreationDate] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModificationDate] DATETIME NULL,

    PRIMARY KEY([Id])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Path]
ON [dbo].[Images]([Id]);
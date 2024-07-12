CREATE TABLE [dbo].[Categories]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [Name] NVARCHAR(256) NOT NULL,
    [Description] NVARCHAR(512) NOT NULL,
    [SecurityStamp] NVARCHAR(MAX) NOT NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NOT NULL,
    [CreationDate] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModificationDate] DATETIME NULL,

    PRIMARY KEY([Id])
)
CREATE TABLE [dbo].[Categories]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [Name] NVARCHAR(256) NOT NULL,
    [Description] NVARCHAR(512) NOT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModifiedAt] DATETIME NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,

    PRIMARY KEY([Id])
)
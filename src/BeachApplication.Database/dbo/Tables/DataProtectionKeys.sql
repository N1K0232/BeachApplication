CREATE TABLE [dbo].[DataProtectionKeys]
(
	[Id] INT NOT NULL IDENTITY(1, 1),
    [FriendlyName] NVARCHAR(100) NULL,
    [Xml] NVARCHAR(MAX) NULL,

    PRIMARY KEY([Id])
)
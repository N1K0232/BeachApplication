CREATE TABLE [dbo].[Umbrellas]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [Letter] VARCHAR(1) NOT NULL,
    [Number] INTEGER NOT NULL,
    [IsBusy] BIT NOT NULL DEFAULT(0),
    [CreatedAt] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModifiedAt] DATETIME NULL,

    PRIMARY KEY([Id])
)
CREATE TABLE [dbo].[Tenants]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [Name] NVARCHAR(256) NOT NULL,
    [SqlConnectionString] VARCHAR(4000) NOT NULL,
    [AzureStorageConnectionString] VARCHAR(4000) NULL,
    [ContainerName] VARCHAR(256) NULL,

    PRIMARY KEY([Id])
)
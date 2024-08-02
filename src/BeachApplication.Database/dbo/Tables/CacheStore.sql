CREATE TABLE [dbo].[CacheStore]
(
	[Id] NVARCHAR(512) NOT NULL,
    [Value] VARBINARY(MAX) NOT NULL,
    [ExpiresAtTime] DATETIMEOFFSET(7) NOT NULL,
    [SlidingExpirationInSeconds] BIGINT NULL,
    [AbsoluteExpiration] DATETIMEOFFSET(7) NULL,

    PRIMARY KEY([Id])
)
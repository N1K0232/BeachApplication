CREATE TABLE [dbo].[BeachApplicationCache]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [Value] VARBINARY(MAX) NOT NULL,
    [ExpiresAtTime] DATETIMEOFFSET(7) NOT NULL,
    [SlidingExpirationInSeconds] BIGINT NOT NULL,
    [AbsoluteExpiration] DATETIMEOFFSET(7) NOT NULL,
)
CREATE TABLE [dbo].[Region]
(
	[Id] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY, 
    [Name] VARCHAR(100) NOT NULL, 
    [Polygon] [sys].[geometry] NULL, 
    [CountryCode] VARCHAR(2) NOT NULL DEFAULT ('AR'),
	[LowThreshold] INT NOT NULL DEFAULT (50),
	[HighThreshold] INT NOT NULL DEFAULT (90),
	[NormalPoints] INT NOT NULL DEFAULT (100),
	[BonusPoints] INT NOT NULL DEFAULT (125),
	[PenaltyPoints] INT NOT NULL DEFAULT (25),
)

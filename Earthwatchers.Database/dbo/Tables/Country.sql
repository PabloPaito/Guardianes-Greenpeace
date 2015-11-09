CREATE TABLE [dbo].[Country] (
	[CountryCode]	VARCHAR(2)			  NOT NULL	PRIMARY KEY DEFAULT 'AR',
    [Name]  VARCHAR (100) NOT NULL,
    [TlLat] FLOAT (53)    NOT NULL,
    [TlLon] FLOAT (53)    NOT NULL,
    [BrLat] FLOAT (53)    NOT NULL,
    [BrLon] FLOAT (53)    NOT NULL, 
    [Polygon] [sys].[geometry] NULL
);


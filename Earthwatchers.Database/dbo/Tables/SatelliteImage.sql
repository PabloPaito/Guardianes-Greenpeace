CREATE TABLE [dbo].[SatelliteImage] (
    [Id]              INT               IDENTITY (1, 1) NOT NULL,
    [Name]            NVARCHAR (255)    NOT NULL,
    [Extent]          [sys].[geography] NOT NULL,
    [Published]       DATETIME2 (7)     NULL,
    [UrlTileCache]    NVARCHAR (255)    NOT NULL,
    [MinLevel]        INT               DEFAULT ((0)) NULL,
    [MaxLevel]        INT               DEFAULT ((12)) NULL,
    [IsCloudy]        BIT               CONSTRAINT [DF_SatelliteImage_IsCloudy] DEFAULT ((0)) NOT NULL,
    [RegionId]          INT               DEFAULT ((0)) NOT NULL,
    [IsForestLaw] BIT NOT NULL DEFAULT ((0)), 
    CONSTRAINT [PrimaryKeyConstraintSatelliteImage] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
CREATE SPATIAL INDEX [SpatialIndexSatelliteImageExtent]
    ON [dbo].[SatelliteImage] ([Extent])
    USING GEOGRAPHY_GRID
    WITH  (
            GRIDS = (LEVEL_1 = MEDIUM, LEVEL_2 = MEDIUM, LEVEL_3 = MEDIUM, LEVEL_4 = MEDIUM)
          );


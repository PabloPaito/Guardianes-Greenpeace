CREATE PROCEDURE [dbo].[SatelliteImage_Insert] 
@name VARCHAR(255),
@published DATETIME2(7),
@extent VARCHAR(255),
@urltilecache VARCHAR(255),
@minlevel INT,
@maxlevel INT,
@iscloudy BIT = 0,
@isForestLaw BIT = 0,
@regionId INT,
@ID INT output
AS
BEGIN
insert into SatelliteImage(extent,Name,Published, UrlTileCache, MinLevel, MaxLevel, IsCloudy, IsForestLaw, RegionId) values(GEOGRAPHY::STGeomFromText(@extent,4326),@name,@published,@urltilecache,@minlevel,@maxlevel,@iscloudy, @isForestLaw, @regionId)SET @ID = SCOPE_IDENTITY()
END
--TODO: Agregar IsForestLaw EN BACKEND, CODIGO Y SP
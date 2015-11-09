CREATE PROCEDURE [dbo].[SatelliteImage_Update] 
@id INT,
@name VARCHAR(255),
@published DATETIME2(7),
@extent VARCHAR(255),
@minlevel INT,
@maxlevel INT,
@iscloudy BIT = 0,
@isForestLaw BIT = 0,
@regionId INT
AS
BEGIN
UPDATE SatelliteImage set 
	extent = GEOGRAPHY::STGeomFromText(@extent,4326),
	Name = @name,
	Published = @published,
	MinLevel = @minlevel,
	MaxLevel = @maxlevel,
	IsCloudy = @iscloudy,
	IsForestLaw = @isForestLaw,
	RegionId = @regionId
WHERE Id = @id
END

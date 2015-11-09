CREATE PROCEDURE [dbo].[Land_AssignLandToEarthwatcher]
@PlayingRegion INT,
@GeohexKey VARCHAR(11),
@EarthwatcherId INT
AS
BEGIN
update Land set StatusChangedDateTime = GETUTCDATE(), LandStatus = 2 where GeohexKey = @GeohexKey and RegionId = @PlayingRegion
Delete From EarthwatcherLands Where Land in (Select Id From Land Where GeoHexKey = @GeohexKey and RegionId = @PlayingRegion)
INSERT INTO EarthwatcherLands (Land, Earthwatcher) VALUES ((Select Id From Land Where GeoHexKey = @GeohexKey and RegionId = @PlayingRegion), @EarthwatcherId)
END

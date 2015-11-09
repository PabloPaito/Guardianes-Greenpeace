CREATE PROCEDURE [dbo].[SatelliteImage_Get] 
@Id INT
AS
BEGIN
select Id,extent.STAsText() as Wkt,Name,Published, MinLevel, MaxLevel, extent.STStartPoint().Lat ymin, extent.STStartPoint().Long xmin, extent.STPointN(3).Lat ymax, extent.STPointN(3).Long xmax, IsCloudy, RegionId, IsForestLaw from SatelliteImage s where s.Id=@Id
END

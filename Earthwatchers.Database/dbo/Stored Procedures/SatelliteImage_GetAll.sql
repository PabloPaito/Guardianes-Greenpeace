CREATE PROCEDURE [dbo].[SatelliteImage_GetAll] 
@regionId INT
AS
BEGIN
select Id, extent.STAsText() as Wkt, Name, Published, UrlTileCache, MinLevel, MaxLevel, 
	   extent.STStartPoint().Lat ymin, extent.STStartPoint().Long xmin, extent.STPointN(3).Lat ymax, extent.STPointN(3).Long xmax, IsCloudy, RegionId, IsForestLaw
from SatelliteImage 
Where Name = '2008' and RegionId = @regionId
                
UNION 

Select * from (
		select top 1000 Id, extent.STAsText() as Wkt, Name, Published, UrlTileCache, MinLevel, MaxLevel, 
		extent.STStartPoint().Lat ymin, extent.STStartPoint().Long xmin, extent.STPointN(3).Lat ymax, extent.STPointN(3).Long xmax, IsCloudy, RegionId, IsForestLaw
		from SatelliteImage 
		where RegionId = @regionId
		Order By Published DESC) lastImages
END

CREATE PROCEDURE [dbo].[SatelliteImages_BE_GetAll]
AS
BEGIN
Select Id, Published, IsCloudy, Name, 
extent.STStartPoint().Lat ymin, 
extent.STStartPoint().Long xmin, 
extent.STPointN(3).Lat ymax, 
extent.STPointN(3).Long xmax,
MinLevel, MaxLevel, RegionId, IsForestLaw
from SatelliteImage 
		Order By Published DESC
END
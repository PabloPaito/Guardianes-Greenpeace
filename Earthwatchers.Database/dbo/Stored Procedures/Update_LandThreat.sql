
CREATE PROCEDURE [dbo].[Update_LandThreat]
@LayerId int,
@RegionId int
AS
BEGIN
	
	  --reset lands to NO thread
  Update Land
  Set LandThreat = 0
  where RegionId = @RegionId

  --correct geometry type
  UPDATE Polygons
  SET PolygonGeom = PolygonGeom.MakeValid()
  WHERE ZoneId in (select Id from Zones where LayerId = @LayerId);  
  
  UPDATE Polygons
  SET PolygonGeom = PolygonGeom.STUnion(PolygonGeom.STStartPoint());   
  UPDATE Polygons
  SET PolygonGeom = PolygonGeom.MakeValid().STUnion(PolygonGeom.STStartPoint());

--update land thread intersecting with polygons
update Land
set LandThreat = case when t.ZoneName in (select Name from Zones where LayerId = @LayerId) then 5 else 3 end
from Land l, 
		(select GEOGRAPHY::STGeomFromText(p.PolygonGeom.STAsText(),4326) as Polygon,
		z.Name as ZoneName
		from Polygons p
		inner join Zones z on p.ZoneId = z.Id
		where z.Name in (select Name from Zones where LayerId = @LayerId)
		and z.LayerId = @LayerId) as t
where l.Centroid.STIntersects(t.Polygon)=1
and l.RegionId = @RegionId

	
END


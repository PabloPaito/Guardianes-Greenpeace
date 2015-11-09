CREATE PROCEDURE [dbo].[Land_IntersectLandsWithZone]
	@regionId INT,
	@layerName varchar(50)
AS
Update Land
  Set Intersects = 0 where RegionId = @regionId and Intersects is null

  --correct geometry type
  UPDATE Polygons
  SET PolygonGeom = PolygonGeom.MakeValid();  
  UPDATE Polygons
  SET PolygonGeom = PolygonGeom.STUnion(PolygonGeom.STStartPoint());   
  UPDATE Polygons
  SET PolygonGeom = PolygonGeom.MakeValid().STUnion(PolygonGeom.STStartPoint());

--update land intersects, intersecting with polygons
update Land
set Intersects = 1 
from Land l, 
		(select GEOGRAPHY::STGeomFromText(p.PolygonGeom.STAsText(),4326) as Polygon,
		z.Name as ZoneName,
		z.Id 
		from Polygons p
		inner join Zones z on p.ZoneId = z.Id
		where z.Name in (select Name from Zones where LayerId = (select Id from Layers where RegionId = @regionId))) as t
where l.Centroid.STIntersects(t.Polygon)=1 and l.RegionId = @regionId
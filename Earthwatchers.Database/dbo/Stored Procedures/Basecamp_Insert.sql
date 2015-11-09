CREATE PROCEDURE [dbo].[Basecamp_Insert] 
@ID INT output,
@longitude FLOAT(50),
@latitude FLOAT(50),
@hpLongitude FLOAT(50) = 0,
@hpLatitude FLOAT(50) = 0,
@probability INT,
@name VARCHAR(200),
@shortText VARCHAR(MAX),
@region INT,
@show BIT
AS
BEGIN
INSERT INTO BasecampDetails (Location, HotPoint, Probability, Name, ShortText, RegionId, Show) values( geography::STPointFromText('POINT(' + CAST(@longitude AS VARCHAR(20)) + ' ' + CAST(@latitude AS VARCHAR(20)) + ')', 4326), geography::STPointFromText('POINT(' + CAST(@hpLongitude AS VARCHAR(20)) + ' ' + CAST(@hpLatitude AS VARCHAR(20)) + ')', 4326), @probability, @name, @shortText, @region, @show)SET @ID = SCOPE_IDENTITY()

--Luego de agregarlo , calcula las distancias de las lands
insert into BasecampLandDistance
select bc.Id, l.Id, l.Centroid.STDistance(bc.Location)
from Land l, BasecampDetails bc
where l.LandStatus > 0
and l.Landthreat > 0 
and l.IsLocked = 0
and l.DemandAuthorities = 0
and l.Id <> 64207
and bc.Id = (select top 1 Id from BasecampDetails order by Id DESC)
END

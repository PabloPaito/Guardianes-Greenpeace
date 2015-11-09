Create PROCEDURE [dbo].[Basecamp_GetById] 
@id int
AS
BEGIN
Select Name, Location.Lat as Latitude, Location.Long as Longitude, HotPoint.Lat as HotPointLat, HotPoint.Long as HotPointLong, Probability, ShortText, RegionId, Show  
From BasecampDetails 
where Id = @id;
END

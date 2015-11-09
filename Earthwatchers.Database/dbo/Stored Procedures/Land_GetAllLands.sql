CREATE PROCEDURE [dbo].[Land_GetAllLands] 

AS
BEGIN
Select Id, Latitude, Longitude, GeohexKey, Distance, LandThreat, IsTutorLand from Land
END

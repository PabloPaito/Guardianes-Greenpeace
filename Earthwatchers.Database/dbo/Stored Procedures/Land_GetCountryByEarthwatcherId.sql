CREATE PROCEDURE [dbo].[Land_GetPlayingCountryByEarthwatcherId]
	@earthwatcherId int
AS
BEGIN
	SELECT PlayingRegion from Earthwatcher where Id = @earthwatcherId
END
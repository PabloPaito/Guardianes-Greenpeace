CREATE PROCEDURE [dbo].[Earthwatcher_ChangePlayingRegion]
	@playingRegion INT,
	@playingCountry varchar(2),
	@id INT
AS
BEGIN

UPDATE Earthwatcher set PlayingRegion = @playingRegion, PlayingCountry = @playingCountry WHERE Id = @id

END
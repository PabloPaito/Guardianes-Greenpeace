CREATE PROCEDURE [dbo].[Region_GetById]
	@regionId int = 0
AS
BEGIN
	SELECT Id, Name, CountryCode, LowThreshold, HighThreshold, NormalPoints, BonusPoints, PenaltyPoints
	FROM Region
	WHERE Id = @regionId
END
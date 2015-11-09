CREATE PROCEDURE [dbo].[Region_GetAllRegions]
AS
BEGIN
	SELECT Id, Name, CountryCode, LowThreshold, HighThreshold, NormalPoints, BonusPoints, PenaltyPoints
	FROM Region
END

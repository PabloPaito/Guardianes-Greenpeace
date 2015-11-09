CREATE PROCEDURE [dbo].[Region_GetByCountryCode]
	@countryCode varchar(2) = 'AR'
AS
	SELECT Id, Name, CountryCode, LowThreshold, HighThreshold, NormalPoints, BonusPoints, PenaltyPoints
	FROM Region
	WHERE CountryCode = @countryCode

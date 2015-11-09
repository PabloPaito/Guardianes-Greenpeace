CREATE PROCEDURE [dbo].[Region_GetByName]
	@name varchar(100) = ''
AS
BEGIN
	SELECT Id, Name, CountryCode, LowThreshold, HighThreshold, NormalPoints, BonusPoints, PenaltyPoints
	FROM Region
	WHERE Name = @name
END

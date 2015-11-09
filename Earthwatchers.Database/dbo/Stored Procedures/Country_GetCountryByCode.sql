CREATE PROCEDURE [dbo].[Country_GetCountryByCode]
	
	@Code VARCHAR(2)
AS
BEGIN
	SELECT * from Country where CountryCode = @Code
END
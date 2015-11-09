CREATE PROCEDURE [dbo].[Land_GetTutorLand]
	@regionId int = 0
AS
BEGIN
	SELECT TOP 1 * FROM Land WHERE RegionId = @regionId and IsTutorLand = 1
END
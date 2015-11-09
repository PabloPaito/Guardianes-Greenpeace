CREATE PROCEDURE [dbo].[Land_UpdateTutorLand]
	@LandId int = 0,
	@RegionId int = 0
AS
BEGIN
	UPDATE Land set IsTutorLand = 0 where RegionId = @RegionId
	UPDATE Land set IsTutorLand = 1 where Id = @LandId
END
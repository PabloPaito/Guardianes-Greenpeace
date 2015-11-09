CREATE PROCEDURE [dbo].[Score_PostScore] 
@earthwatcherid INT, 
@action VARCHAR(255),
@points INT,
@landId INT = null,
@regionId VARCHAR(2),
@param1 NVARCHAR(50) = null,
@param2 NVARCHAR(50) = null,
@ID int output
AS
BEGIN
insert into scores(EarthwatcherId, action, published, points, LandId, RegionId, Param1, Param2)
                values (@earthwatcherid, @action,GETUTCDATE(),@points, @landId, @regionId, @param1, @param2)set @ID = SCOPE_IDENTITY()
END

USE [Earthwatchers]
GO
/****** Object:  StoredProcedure [dbo].[Score_PostScore]    Script Date: 03/19/2014 16:11:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[Score_PostScore] 
@earthwatcherid INT, 
@action VARCHAR(255),
@points INT,
@landId INT,
@param1 NVARCHAR(50),
@param2 NVARCHAR(50),
@ID int output
AS
BEGIN
insert into scores(EarthwatcherId, action, published, points, LandId, Param1, Param2)
                values (@earthwatcherid, @action,GETUTCDATE(),@points, @landId, @param1, @param2)set @ID = SCOPE_IDENTITY()
END

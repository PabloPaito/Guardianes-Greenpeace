USE [Earthwatchers]
GO
/****** Object:  StoredProcedure [dbo].[Score_GetLeaderBoard]    Script Date: 03/19/2014 16:09:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--Score_GetLeaderBoard 1
ALTER PROCEDURE [dbo].[Score_GetLeaderBoard] 
	@isContest BIT
AS
BEGIN

Select CAST(ROW_NUMBER() OVER(ORDER BY SUM(points) DESC) AS INT) AS Id, s.EarthwatcherId, SUBSTRING(e.Name, 0, CHARINDEX('@', e.Name)) Action, SUM(points) Points, GETDATE() Published, null LandId, null Param1, null Param2 From scores s
Inner Join Earthwatcher e on s.EarthwatcherId = e.Id
Left Join Contest c on GETDATE() BETWEEN c.StartDate and c.EndDate
Where (@isContest = 0 OR (@isContest = 1 AND Published BETWEEN c.StartDate and c.EndDate))
Group by s.EarthwatcherId, e.Name
                                                    
END

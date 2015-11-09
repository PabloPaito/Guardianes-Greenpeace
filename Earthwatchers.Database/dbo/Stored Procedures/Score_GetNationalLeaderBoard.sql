--Score_GetLeaderBoard 1
CREATE PROCEDURE [dbo].[Score_GetNationalLeaderBoard] 
	@isContest BIT,
	@playingRegion INT
AS
BEGIN

Select CAST(ROW_NUMBER() OVER(ORDER BY SUM(points) DESC) AS INT) AS OrderRank, s.EarthwatcherId AS EarthwatcherId, SUBSTRING(e.Name, 0, CHARINDEX('@', e.Name)) AS Name, SUM(points) AS Points, GETDATE() AS Published, e.NickName AS Nick, e.PlayingRegion AS PlayingRegion, e.country AS Country
From scores s
Inner Join Earthwatcher e on s.EarthwatcherId = e.Id
Left Join Contest c on GETDATE() BETWEEN c.StartDate and c.EndDate
Where (@isContest = 0 OR (@isContest = 1 AND Published BETWEEN c.StartDate and c.EndDate))
	   and @playingRegion = e.PlayingRegion
	   and s.RegionId = e.PlayingRegion
Group by s.EarthwatcherId, e.Name, e.PlayingRegion, e.NickName, e.country
END
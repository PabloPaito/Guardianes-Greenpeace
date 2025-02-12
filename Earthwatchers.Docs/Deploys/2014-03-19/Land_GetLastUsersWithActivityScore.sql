USE [Earthwatchers]
GO
/****** Object:  StoredProcedure [dbo].[Land_GetLastUsersWithActivityScore]    Script Date: 03/19/2014 15:46:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[Land_GetLastUsersWithActivityScore] 
@landId INT
AS
BEGIN
Select CAST(ROW_NUMBER() OVER(ORDER BY AddedDate ASC) AS INT) AS Id, [action] = SUBSTRING(Name, 0, CHARINDEX('@', Name)), EarthwatcherId = Earthwatcher, published = AddedDate, 0 points From Verifications d Inner Join Earthwatcher e on d.Earthwatcher = e.Id Where d.Land = @landId and d.IsDeleted = 0 Order by AddedDate ASC
END

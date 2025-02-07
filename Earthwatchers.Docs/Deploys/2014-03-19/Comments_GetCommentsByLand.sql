USE [Earthwatchers]
GO
/****** Object:  StoredProcedure [dbo].[Comments_GetCommentsByLand]    Script Date: 03/19/2014 15:58:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[Comments_GetCommentsByLand] 
@Id INT
AS
BEGIN
select a.Id as Id,a.EarthwatcherId as EarthwatcherId, a.LandId as LandId,a.UserComment as UserComment,a.Published as Published, l.Name as UserName, SUBSTRING(l.Name, 0, CHARINDEX('@', l.Name)) as FullName from Comments a left join Earthwatcher l on a.EarthwatcherId=l.Id where a.LandId=@Id and a.IsDeleted = 0 order by a.Published Desc
END

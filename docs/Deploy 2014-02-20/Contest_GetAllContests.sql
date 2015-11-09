USE [Earthwatchers]
GO
/****** Object:  StoredProcedure [dbo].[Contest_GetAllContests]    Script Date: 02/20/2014 16:39:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[Contest_GetAllContests] 

AS
BEGIN
select Id, ShortTitle, Title, Description, ImageURL,  StartDate , EndDate, WinnerId from Contest
END

/****** Object:  StoredProcedure [dbo].[Contest_Insert]    Script Date: 02/11/2014 19:24:15 ******/
SET ANSI_NULLS ON

USE [Earthwatchers]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[Earthwatcher_UpdateEarthwatcher] 
@id INT,
@role INT,
@country VARCHAR(255) = null, 
@region VARCHAR(255) = null,
@language VARCHAR(255) = null,
@notifyMe BIT,
@nickname VARCHAR(50) = null
AS
BEGIN
update Earthwatcher set role=@role, country=@country, region=@region, language=@language, NotifyMe=@notifyMe, NickName=@nickname where id=@id
END

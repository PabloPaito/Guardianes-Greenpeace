CREATE PROCEDURE [dbo].[Earthwatcher_GetEarthwatcherByGuid] 
@Guid UNIQUEIDENTIFIER
AS
BEGIN
select Id, EarthwatcherGuid as Guid, Name, Country, Role, IsPowerUser, Language, Region, NotifyMe, AllowAutoShare, NickName, ApiEwId, PlayingRegion, PlayingCountry from Earthwatcher where EarthwatcherGuid = @Guid
END

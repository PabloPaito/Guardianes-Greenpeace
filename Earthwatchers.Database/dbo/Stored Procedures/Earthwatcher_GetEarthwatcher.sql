CREATE PROCEDURE [dbo].[Earthwatcher_GetEarthwatcher] 
@Id INT
AS
BEGIN
select Id, EarthwatcherGuid as Guid, Name, Country, Role, IsPowerUser, Language, Region, NotifyMe, AllowAutoShare, NickName, ApiEwId, PlayingRegion, PlayingCountry from Earthwatcher Where Id = @Id
END

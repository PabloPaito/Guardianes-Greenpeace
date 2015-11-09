CREATE PROCEDURE [dbo].[Earthwatcher_GetEarthwatcherByName]
@Name VARCHAR(255)
AS
BEGIN
	select Id, EarthwatcherGuid as Guid, Name, Country, Role, IsPowerUser, Language, Region, NotifyMe, AllowAutoShare, NickName, ApiEwId, PlayingRegion, PlayingCountry from Earthwatcher where Name = @Name
END

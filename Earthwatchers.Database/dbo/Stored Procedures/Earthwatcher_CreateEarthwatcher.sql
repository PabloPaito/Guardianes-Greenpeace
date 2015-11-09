CREATE PROCEDURE [dbo].[Earthwatcher_CreateEarthwatcher] 
@guid UNIQUEIDENTIFIER,
@name VARCHAR(255) = '',
@role INT,
@prefix VARCHAR(255),
@hash VARCHAR(255),
@country VARCHAR(2),
@language VARCHAR(5),
@playingCountry VARCHAR(2),
@nick VARCHAR(255) = null,
@playingRegion INT,
@ID INT output
AS
BEGIN
insert into Earthwatcher(EarthwatcherGuid, Name, Role, PasswordPrefix, HashedPassword, country, language, PlayingCountry, NickName, PlayingRegion) values(@guid,@name,@role,@prefix,@hash,@country,@language,@playingCountry,@nick, @playingRegion)set @ID = SCOPE_IDENTITY()
END

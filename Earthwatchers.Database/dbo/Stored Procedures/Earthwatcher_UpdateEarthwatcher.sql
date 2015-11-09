CREATE PROCEDURE [dbo].[Earthwatcher_UpdateEarthwatcher] --//TODO: revisar que deba pasarle el country y no el regionId
@id INT,
@role INT,
@country VARCHAR(2) = 'AR', 
@region VARCHAR(255) = null,
@language VARCHAR(5) = 'en-CA',
@notifyMe BIT,
@allowAutoShare BIT,
@nickname VARCHAR(50) = null,
@name varchar(255)

AS
BEGIN
update Earthwatcher set role=@role, country=@country, region=@region, language=@language, NotifyMe=@notifyMe, AllowAutoShare=@allowAutoShare, NickName=@nickname, Name = @name where id=@id
END

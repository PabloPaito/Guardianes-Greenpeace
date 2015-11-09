CREATE PROCEDURE [dbo].[Earthwatcher_GetFreeLandByEarthwatcherId]
@BasecampDetailId int,
@EarthwatcherId int,
@PlayingRegion int
AS

Declare @currentLand int = (select top 1 Land from EarthwatcherLands where Earthwatcher = @EarthwatcherId)
Declare @GeoHexKey varchar(max) = (select top 1 GeoHexKey from Land where Id = @currentLand)
Declare @TutorLandId int = (SELECT top 1 Id from Land where RegionId = @PlayingRegion and IsTutorLand = 1)

BEGIN
select top 1
		l.Id, 
		l.BasecampId,
		l.GeohexKey, 
		CASE WHEN l.Id IN (Select Land From EarthwatcherLands where earthwatcher != 17) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS usedLand
	from Land l 
	full join BasecampLandDistance bld on bld.LandId = l.Id and l.BasecampId = bld.BasecampDetailId
	where (l.Id NOT IN (Select Land From EarthwatcherLands where earthwatcher != 17) OR (l.Id IN (Select Land From EarthwatcherLands where earthwatcher != 17) AND l.LandStatus = 2 AND l.StatusChangedDateTime < DATEADD(hour, -1, GETUTCDATE())))
	and l.RegionId = @PlayingRegion
	and (l.LandStatus = 1 or landstatus = 2)
	and l.Landthreat > 0 
	and l.IsLocked = 0
	and l.DemandAuthorities = 0
	and l.Id <> @TutorLandId --Parcela del tutor no se puede reshuffle
	and (@GeoHexKey is null or l.GeohexKey NOT IN (@GeoHexKey))
	order by case when l.BasecampId = @BasecampDetailId then 0 when l.BasecampId IS NULL then 2 when bld.Distance is null then 3 else 1 end, bld.Distance, l.Landthreat desc
		                        
END
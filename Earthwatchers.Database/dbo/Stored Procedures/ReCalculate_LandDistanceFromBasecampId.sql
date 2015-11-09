CREATE PROCEDURE [dbo].[ReCalculate_LandDistanceFromBasecampId]
@basecampId int
AS
BEGIN

declare @regionId int = (select b.RegionId from BasecampDetails b where Id = @basecampId)
declare @TutorLandId int = (SELECT top 1 Id from Land where RegionId = @regionId and IsTutorLand = 1)

delete from BasecampLandDistance where BasecampDetailId = @basecampId
insert into BasecampLandDistance
select bc.Id, l.Id, l.Centroid.STDistance('POINT('+ convert(varchar(20),bc.HotPoint.Long)  +' '+ convert(varchar(20),bc.HotPoint.Lat) +')')
from Land l, BasecampDetails bc
where l.LandStatus > 0
and l.Landthreat > 0 
and l.IsLocked = 0
and l.DemandAuthorities = 0
and l.Id <> @TutorLandId
and bc.Id = @basecampId
END
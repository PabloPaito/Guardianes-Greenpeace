CREATE PROCEDURE [dbo].[Land_PresicionDemand]
	@regionId int,
    @earthwatcherId int

AS
BEGIN
declare @all int = 
(SELECT COUNT(*) 
FROM Verifications v inner join Land l on l.Id = v.Land
WHERE v.Land in (SELECT Id FROM Land WHERE LandStatus > 2 AND RegionId = @regionId AND Confirmed is not null) 
AND v.IsDeleted = 0
AND v.Earthwatcher = @earthwatcherId
)

declare @correct int = 
(SELECT COUNT(*) 
FROM Verifications v inner join Land l on l.Id = v.Land
WHERE v.Land in (SELECT Id FROM Land WHERE LandStatus > 2 AND RegionId = @regionId AND Confirmed is not null) 
AND v.IsDeleted = 0
AND v.Earthwatcher = @earthwatcherId
AND v.IsAlert = l.DemandAuthorities
)

if(@all != 0)
	select CONVERT( DECIMAL(10,2)
			 , ( CONVERT(DECIMAL(10,3), (@correct * 100)) / CONVERT(DECIMAL(10,3), @all) ) 
			 ) AS Total
else
	select null AS Total
END
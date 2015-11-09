CREATE PROCEDURE [dbo].[Land_GetCheckPercentage]
	@regionId int = 0
AS
BEGIN
declare @all int = (select COUNT(*) from Land l where l.LandThreat > 2 and RegionId = @regionId)  --Total de lands asignables
declare @checked int = (select COUNT(*) from Land l where l.LandThreat > 2 and l.LandStatus > 2 and l.RegionId = @regionId) -- Total que se revisaron, Incluye las de demanda (Cambio su status)

select CONVERT( DECIMAL(10,1)
         , ( CONVERT(DECIMAL(10,3), (@checked * 100)) / CONVERT(DECIMAL(10,3), @all) ) 
         ) AS Total
END

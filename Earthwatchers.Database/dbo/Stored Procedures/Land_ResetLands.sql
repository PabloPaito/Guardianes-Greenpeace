CREATE PROCEDURE [dbo].[Land_ResetLands] 
@RegionId int
AS
BEGIN

declare @TutorLandId int = (SELECT top 1 Id from Land where RegionId = @RegionId and IsTutorLand = 1)

Update verifications set IsDeleted = 1 
where Land in (select Id from Land where (LandStatus = 3 or LandStatus = 2 or Landstatus = 1) and RegionId = @RegionId) and IsDeleted = 0 --Borro las verificaciones de las parcelas verdes de el pais seleccionado

Update Comments set IsDeleted = 1 where LandId in (select Id from Land where LandStatus = 3 and RegionId = @RegionId) --Borro los comentarios de las parcelas verdes de el pais seleccionado
Update Land set confirmed = null where LandStatus = 3 and RegionId = @RegionId -- Borro las confirmacion de greenpeace de que era verde de el pais seleccionado
Update Land set IsLocked = 0 where LandStatus = 3 and Id != @TutorLandId and IsTutorLand = 0 and RegionId = @RegionId -- Desbloqueo las parcelas verdes, para que se vuelvan a asignar de el pais seleccionado

Update Land  --Reseteo el estado de las parcelas verdes a 1 "Sin Asignar"
Set LandStatus = CASE WHEN LandStatus = 3 THEN 1 
				 ELSE LandStatus END, LastReset = GETUTCDATE() 
				 Where LandStatus > 1 and Id != @TutorLandId and RegionId = @RegionId
				 
Delete from EarthwatcherLands where Land in (select Id from Land where LandStatus = 1 and RegionId = @RegionId) --Elimino de ewLands las que tienen estado 1(sin asignar) para q no se asigne a un Ew una q pertenece a greenpeace
END
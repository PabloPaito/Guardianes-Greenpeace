CREATE PROCEDURE [dbo].[Layer_GetZones] 
@layerId INT,
@justBasecamps BIT = 0
AS
BEGIN
if(@justBasecamps = 1)
	BEGIN
		select z.Id, z.Description, z.LayerId, z.Name, z.Param1, bd.Show 
		from Zones z inner join BasecampDetails bd on bd.Id = z.Param1
		where LayerId = @layerId
	END
else
	BEGIN
		select * from Zones where LayerId = LayerId
	END
END
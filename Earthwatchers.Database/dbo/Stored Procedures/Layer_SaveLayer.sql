CREATE PROCEDURE [dbo].[Layer_SaveLayer] 
@name VARCHAR(50),
@regionId INT,
@description VARCHAR(200),
@ID INT output
AS
BEGIN
insert into Layers(Name, Description, RegionId) values(@name,@description,@regionId)SET @ID = SCOPE_IDENTITY()
END

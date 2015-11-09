CREATE PROCEDURE [dbo].[CustomShareText_Delete]
	@Id int
AS
BEGIN
	DELETE from CustomShareText where Id = @Id
END
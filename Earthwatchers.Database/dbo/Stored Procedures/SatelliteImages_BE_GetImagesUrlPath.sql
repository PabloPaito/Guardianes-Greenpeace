CREATE PROCEDURE [dbo].[SatelliteImages_BE_GetImagesUrlPath]
AS
BEGIN
SELECT UrlTileCache from SatelliteImage
END
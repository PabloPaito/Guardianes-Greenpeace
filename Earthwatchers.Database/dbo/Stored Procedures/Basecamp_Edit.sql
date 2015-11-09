CREATE PROCEDURE [dbo].[Basecamp_Edit] 
@longitude FLOAT(50),
@latitude FLOAT(50),
@hpLatitude FLOAT(50),
@hpLongitude FLOAT(50),
@probability INT,
@name VARCHAR(200),
@shortText VARCHAR(MAX),
@region INT,
@show BIT,
@id INT
AS
BEGIN

UPDATE BasecampDetails set Location = geography::STPointFromText('POINT(' + CAST(@longitude AS VARCHAR(20)) + ' ' + CAST(@latitude AS VARCHAR(20)) + ')', 4326), HotPoint =  geography::STPointFromText('POINT(' + CAST(@hpLongitude AS VARCHAR(20)) + ' ' + CAST(@hpLatitude AS VARCHAR(20)) + ')', 4326), Probability = @probability, Name = @name, ShortText = @shortText, RegionId = @region, Show = @show where Id = @id

END

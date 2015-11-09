CREATE PROCEDURE [dbo].[Region_Edit]
	@Name varchar(100),
	@CountryCode varchar(2),
	@LowThreshold int,
	@HighThreshold int,
	@NormalPoints int,
	@BonusPoints int,
	@PenaltyPoints int,
	@Id int 
AS
BEGIN
UPDATE Region 
SET Name = @Name, CountryCode = @CountryCode, LowThreshold = @LowThreshold, HighThreshold = @HighThreshold, NormalPoints = @NormalPoints, BonusPoints = @BonusPoints, PenaltyPoints = @PenaltyPoints
WHERE Id = @Id
END

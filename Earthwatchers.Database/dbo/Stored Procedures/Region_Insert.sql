CREATE PROCEDURE [dbo].[Region_Insert]
	@Name varchar,
	@CountryCode varchar(2),
	@LowThreshold int,
	@HighThreshold int,
	@NormalPoints int,
	@BonusPoints int,
	@PenaltyPoints int,
	@Id int  output
AS
BEGIN
INSERT INTO Region(Name, CountryCode, LowThreshold, HighThreshold, NormalPoints, BonusPoints, PenaltyPoints)
	  VALUES  (@Name, @CountryCode, @LowThreshold, @HighThreshold, @NormalPoints, @BonusPoints, @PenaltyPoints) SET @Id = SCOPE_IDENTITY()
END

CREATE PROCEDURE [dbo].[CustomShareText_Insert]
	@Language varchar(6),
	@RegionId int,
	@ShareOk nvarchar(max) = null,
	@ShareAlert nvarchar(max) = null,
	@ShareAlertFinca nvarchar(max) = null,
	@HashTagRegister nvarchar(max) = null,
	@HashTagReportConfirmed nvarchar(max) = null,
	@HashTagRanking nvarchar(max) = null,
	@HashTagCheck nvarchar(max) = null,
	@HashTagDenounce nvarchar(max) = null,
	@HashTagContestWon nvarchar(max) = null, 
	@HashTagTop1 nvarchar(max) = null, 
	@HashTagVerification nvarchar(max) = null,
	@Id int output
	AS
	BEGIN
		INSERT INTO CustomShareText(Language, RegionId, ShareOk, ShareAlert, ShareAlertFinca, 
		HashTagRegister, HashTagReportConfirmed, HashTagRanking, HashTagCheck, HashTagDenounce, HashTagContestWon, HashTagTop1, HashTagVerification) 
		VALUES(@Language, @RegionId, @ShareOk, @ShareAlert, @ShareAlertFinca, 
		@HashTagRegister, @HashTagReportConfirmed, @HashTagRanking, @HashTagCheck, @HashTagDenounce, @HashTagContestWon, @HashTagTop1, @HashTagVerification)
		SET @Id = SCOPE_IDENTITY()
	END
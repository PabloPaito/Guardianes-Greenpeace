CREATE PROCEDURE [dbo].[CustomShareText_Edit]
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
	@Id int
	AS
	BEGIN
		UPDATE CustomShareText 
		SET 
		Language = @Language,
		RegionId = @RegionId,
		ShareOk = @ShareOk, 
		ShareAlert = @ShareAlert, 
		ShareAlertFinca = @ShareAlertFinca,
		HashTagRegister = @HashTagRegister,
		HashTagReportConfirmed = @HashTagReportConfirmed,
		HashTagRanking = @HashTagRanking,
		HashTagCheck = @HashTagCheck,
		HashTagDenounce = @HashTagDenounce,
		HashTagContestWon = @HashTagContestWon,
		HashTagTop1 = @HashTagTop1,
		HashTagVerification = @HashTagVerification

		WHERE Id = @Id
	END


CREATE PROCEDURE [dbo].[CustomShareText_GetById]
	@Id int
	AS
	BEGIN
		SELECT Id, RegionId, Language, ShareOk, ShareAlert, ShareAlertFinca, 
		HashTagRegister, HashTagReportConfirmed, HashTagRanking, HashTagCheck, HashTagDenounce, HashTagContestWon, HashTagTop1, HashTagVerification 
		FROM CustomShareText
		WHERE Id = @Id
	END
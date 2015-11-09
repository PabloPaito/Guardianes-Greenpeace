CREATE PROCEDURE [dbo].[CustomShareText_GetAll]
	AS
	BEGIN
		SELECT Id, RegionId, Language, ShareOk, ShareAlert, ShareAlertFinca, 
		HashTagRegister, HashTagReportConfirmed, HashTagRanking, HashTagCheck, HashTagDenounce, HashTagContestWon, HashTagTop1, HashTagVerification 
		FROM CustomShareText
	END
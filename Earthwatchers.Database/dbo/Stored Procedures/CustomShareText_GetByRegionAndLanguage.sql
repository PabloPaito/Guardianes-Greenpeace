CREATE PROCEDURE [dbo].[CustomShareText_GetByRegionAndLanguage]
	@RegionId int = 1,
	@Language varchar(6) = 'es-AR'
AS
	SELECT top 1 Id, RegionId, Language, ShareOk, ShareAlert, ShareAlertFinca, 
	HashTagRegister, HashTagReportConfirmed, HashTagRanking, HashTagCheck, HashTagDenounce, HashTagContestWon, HashTagTop1, HashTagVerification 
		FROM CustomShareText
	WHERE RegionId = @RegionId and Language = Language

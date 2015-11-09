CREATE TABLE [dbo].[CustomShareText] (
    [Id]                     INT         IDENTITY (1, 1) NOT NULL,
    [RegionId]               INT         CONSTRAINT [DF__CustomSha__Regio__7073AF84] DEFAULT ((1)) NOT NULL,
    [Language]               VARCHAR (6) CONSTRAINT [DF__CustomSha__Langu__7167D3BD] DEFAULT ('es-AR') NOT NULL,
    [ShareOk]                NTEXT       NULL,
    [ShareAlert]             NTEXT       NULL,
    [ShareAlertFinca]        NTEXT       NULL,
    [HashTagRegister]        NTEXT       NULL,
    [HashTagReportConfirmed] NTEXT       NULL,
    [HashTagRanking]         NTEXT       NULL,
    [HashTagCheck]           NTEXT       NULL,
    [HashTagDenounce]        NTEXT       NULL,
    [HashTagContestWon]      NTEXT       NULL,
    [HashTagTop1]            NTEXT       NULL,
    [HashTagVerification]    NTEXT       NULL,
    CONSTRAINT [PK__CustomSh__3214EC0744952D46] PRIMARY KEY CLUSTERED ([Id] ASC)
);



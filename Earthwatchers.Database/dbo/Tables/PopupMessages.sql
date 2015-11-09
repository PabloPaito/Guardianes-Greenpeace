CREATE TABLE [dbo].[PopupMessages] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [ShortTitle]  VARCHAR (50)  NULL,
    [Title]       VARCHAR (100) NOT NULL,
    [Description] VARCHAR (MAX) NOT NULL,
    [ImageURL]    VARCHAR (100) NULL,
    [StartDate]   SMALLDATETIME NOT NULL,
    [EndDate]     SMALLDATETIME NOT NULL,
    [RegionId]      INT           DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_PopupMessages] PRIMARY KEY CLUSTERED ([Id] ASC)
);




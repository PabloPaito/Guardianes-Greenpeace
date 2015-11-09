CREATE TABLE [dbo].[BasecampDetails] (
    [Id]          INT               IDENTITY (1, 1) NOT NULL,
    [Location]    [sys].[geography] NOT NULL,
    [HotPoint]    [sys].[geography] NULL,
    [Probability] INT               DEFAULT ((10)) NOT NULL,
    [Name]        VARCHAR (200)     NOT NULL,
    [ShortText]   VARCHAR (MAX)     DEFAULT ('') NULL,
    [RegionId]      INT               DEFAULT ((0)) NOT NULL,
    [Show] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [PK_BasecampDetails] PRIMARY KEY CLUSTERED ([Id] ASC)
);






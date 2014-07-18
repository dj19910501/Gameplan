-- Run this script on MRPQA


DECLARE @CREATEDBY uniqueidentifier = N'efeb8d52-d7f8-45f4-8297-fe1525afd1f3'
DECLARE @BDSAuth VARCHAR(50) = 'BDSAuthDev'

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'MasterTacticType')
BEGIN
CREATE TABLE [dbo].[MasterTacticType](
	[MasterTacticTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](4000) NULL,
	[ColorCode] [nvarchar](10) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	 CONSTRAINT [PK_MasterTacticType] PRIMARY KEY CLUSTERED 
	(
		[MasterTacticTypeId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Email', N'', N'27a4e5',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Online Banner', N'', N'555305',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Whitepaper', N'', N'452f14',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Webinar', N'', N'520a10',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Video', N'', N'ca3cce',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Tradeshow', N'', N'7c4bbf',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Survey', N'', N'1af3c9',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Social Media', N'', N'f1eb13',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Search Engine Optimization (SEO)', N'', N'c7893b',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Public Relations (PR)', N'', N'e42233',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Pay Per Click', N'', N'a636d6',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Outbound Calling', N'', N'2940e2',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Events (Other)', N'', N'0b3d58',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Newsletter', N'', N'244c0a',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Direct Mail', N'', N'414018',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Data Sheet', N'', N'472519',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Content Syndication', N'', N'4b134d',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Case Study', N'', N'2c1947',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Analyst Briefings', N'', N'055e4d',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Telemarketing', N'', N'6ae11f',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'General', N'', N'bbb748',  0)
INSERT INTO [dbo].[MasterTacticType] ([TITLE], [DESCRIPTION], [COLORCODE],  [isdeleted]) VALUES (N'Other', N'', N'bf6a4b',  0)


CREATE TABLE [dbo].[ClientTacticType](
	[ClientTacticTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](4000) NULL,
	[ColorCode] [nvarchar](10) NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [uniqueidentifier] NULL,
	 CONSTRAINT [PK_ClientTacticType] PRIMARY KEY CLUSTERED 
	(
		[ClientTacticTypeId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

EXECUTE('INSERT INTO [dbo].[ClientTacticType] 
([Title],[Description],[ColorCode],[ClientId],[IsDeleted],[CreatedDate],[CreatedBy])
select t.TITLE,t.[DESCRIPTION],t.COLORCODE,c.ClientId,0 as isdeleted,GETDATE() as createddate,'''+@CREATEDBY+'''
FROM MasterTacticType as t cross join (select ClientId from ['+@BDSAuth+'].[dbo].[Client] ) as c')


delete from TacticType where ModelId is null
ALTER TABLE [TacticType] ALTER COLUMN [ModelId] INTEGER NOT NULL
ALTER TABLE [TacticType] DROP COLUMN [ClientId]


End
GO

ALTER TABLE [dbo].[MasterTacticType] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[ClientTacticType] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'IsDeployedToModel' AND [object_id] = OBJECT_ID(N'TacticType'))
BEGIN
 
	ALTER TABLE TacticType ADD IsDeployedToModel bit NOT NULL DEFAULT(0)

END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'IsDeployedToModel' AND 
			[object_id] = OBJECT_ID(N'TacticType') AND (SELECT COUNT(*) FROM TacticType WHERE IsDeployedToModel = 1) = 0 )
BEGIN
	UPDATE TacticType set IsDeployedToModel = 1 
END
GO

update TacticType set IsDeleted = 0 where IsDeleted is null


;WITH AllCombination AS(
	select distinct 
		mt.Title,
		mt.[Description],
		mt.ColorCode,
		t.ModelId
	from MasterTacticType as mt cross join (select distinct ModelId from TacticType) as t 
)

insert into TacticType (title,[Description],ColorCode,ModelId,CreatedDate,CreatedBy,IsDeleted,IsDeployedToIntegration,IsDeployedToModel)
select distinct ac.Title,ac.Description,ac.ColorCode,ac.ModelId,GETDATE() as CreatedDate,@CREATEDBY as CreatedBy,0 as IsDeleted,0 as  IsDeployedToIntegration,0 as IsDeployedToModel 
from AllCombination ac
inner join TacticType t on t.ModelId = ac.ModelId AND ac.Title NOT IN 
(select Title from TacticType where ModelId= ac.ModelId)


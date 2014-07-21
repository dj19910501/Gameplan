-- Run this script in MRP DB but u require to Add BDS and MRP variable below.


-- ============================================================================
-- Created By :- Sohel Pathan
-- Created Date :- 07/03/2014
-- Description :- Insert default entry of custom restriction for BusinessUnit.

-- NOTE :- Please changes MRP and BDSAuth DB name, respective to deployment. And Run this script on BDSAuth DB.
-- ============================================================================

DECLARE @MRPDB VARCHAR(50) = 'MRPDev'				-- Set MRP DB name here
DECLARE @BDSAuth VARCHAR(50) = 'BDSAuthDev'			-- Set BDSAuth DB name here
DECLARE @ApplicationCode VARCHAR(100) = 'MRP'		-- Application code (Here MRP = Gameplan)
DECLARE @QUERY VARCHAR(MAX)

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomRestriction')
BEGIN

	SET @QUERY = ''
	SET @QUERY +=  'INSERT INTO ' + @BDSAuth + '.[dbo].[CustomRestriction](UserId, CustomField, CustomFieldId, Permission, CreatedDate, CreatedBy, ApplicationId)
					SELECT U.UserId, ''BusinessUnit'', CAST(B.BusinessUnitId as VARCHAR(50)), ''2'', GETDATE(), UA.UserId, A.ApplicationId
					FROM ' + @BDSAuth + '.[dbo].[Application] A 
					INNER JOIN ' + @BDSAuth + '.[dbo].[User_Application] UA ON UA.ApplicationId = A.ApplicationId AND ISNULL(UA.IsDeleted,0 ) = 0
					INNER JOIN ' + @BDSAuth + '.[dbo].[User] U ON U.UserId = UA.UserId AND ISNULL(U.IsDeleted, 0) = 0
					INNER JOIN ' + @BDSAuth + '.[dbo].[Client] C ON C.ClientId = U.ClientId AND ISNULL(C.IsDeleted,0) = 0
					INNER JOIN ' + @MRPDB + '.[dbo].[BusinessUnit] B ON B.ClientId = C.ClientId AND ISNULL(B.IsDeleted, 0) = 0
					WHERE ISNULL(A.IsDeleted, 0) = 0 AND A.Code = ''' + CAST(@ApplicationCode as VARCHAR) + '''
					ORDER BY U.UserId'

	EXEC(@QUERY)
END



-- Run this script on MRPQA


DECLARE @CREATEDBY uniqueidentifier
SET @CREATEDBY = (SELECT TOP 1 CreatedBy FROM Model)-- = N'efeb8d52-d7f8-45f4-8297-fe1525afd1f3'


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

ALTER TABLE [dbo].[MasterTacticType] ADD  DEFAULT ((0)) FOR [IsDeleted]

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

ALTER TABLE [dbo].[ClientTacticType] ADD  DEFAULT ((0)) FOR [IsDeleted]


EXECUTE('INSERT INTO [dbo].[ClientTacticType] 
([Title],[Description],[ColorCode],[ClientId],[IsDeleted],[CreatedDate],[CreatedBy])
select t.TITLE,t.[DESCRIPTION],t.COLORCODE,c.ClientId,0 as isdeleted,GETDATE() as createddate,'''+@CREATEDBY+'''
FROM MasterTacticType as t cross join (select ClientId from ['+@BDSAuth+'].[dbo].[Client] ) as c')

delete from TacticType where ModelId is null
ALTER TABLE [TacticType] ALTER COLUMN [ModelId] INTEGER NOT NULL
ALTER TABLE [TacticType] DROP COLUMN [ClientId]


End
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
	UPDATE TacticType set IsDeleted = 0 where IsDeleted is null
END
GO

--DECLARE @CREATEDBY uniqueidentifier = N'efeb8d52-d7f8-45f4-8297-fe1525afd1f3'
;WITH AllCombination AS(
	select distinct 
		mt.Title,
		mt.[Description],
		mt.ColorCode,
		t.ModelId,
		(SELECT CreatedBy FROM MODEL WHERE ModelId = t.ModelId) AS 'CreatedBy'
	from MasterTacticType as mt cross join (select ModelId from Model where IsDeleted=0) as t 
)
insert into TacticType (title,[Description],ColorCode,ModelId,CreatedDate,CreatedBy,IsDeleted,IsDeployedToIntegration,IsDeployedToModel)
select distinct ac.Title,ac.Description,ac.ColorCode,ac.ModelId,GETDATE() as CreatedDate,ac.CreatedBy as CreatedBy,0 as IsDeleted,0 as  IsDeployedToIntegration,0 as IsDeployedToModel 
from AllCombination ac
inner join TacticType t on t.ModelId = ac.ModelId AND ac.Title NOT IN 
(select Title from TacticType where ModelId= ac.ModelId)





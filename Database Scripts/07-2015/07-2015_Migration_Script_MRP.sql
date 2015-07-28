-- Created By : Brad Gray
-- Created Date : 07/16/2015
-- Description :Database changes to enable WorkFront Integration
-- ======================================================================================
GO

--Add WorkFront entry to IntegrationType
IF NOT EXISTS(SELECT * FROM [dbo].[IntegrationType] WHERE [Title] = 'WorkFront' AND [APIURL] = '.attask-ondemand.com/attask/api')
INSERT INTO [dbo].[IntegrationType] (Title, IsDeleted, APIURL, Code) values ('WorkFront', 0, '.attask-ondemand.com/attask/api', 'WorkFront')
GO

DECLARE @IntegrationTypeID int;

select  @IntegrationTypeID = IntegrationTypeId from [dbo].[IntegrationType]	where Title = 'WorkFront' and [APIURL] = '.attask-ondemand.com/attask/api'

--Insert Datatypes for mapping Gameplan Fields to WorkFront
IF NOT EXISTS(SELECT * FROM [dbo].[GameplanDataType] WHERE [IntegrationTypeId] = @IntegrationTypeID)
begin
	insert into [dbo].[GameplanDataType]
		values(@IntegrationTypeID, 'Global', 'Title', 'Name', 0,0,0) 
	insert into [dbo].[GameplanDataType]
		values(@IntegrationTypeID, 'Global', 'Description', 'Description', 0,0,0) 
	insert into [dbo].[GameplanDataType]
		values(@IntegrationTypeID, 'Global', 'StartDate', 'Start Date', 0,0,0) 
	insert into [dbo].[GameplanDataType]
		values(@IntegrationTypeID, 'Global', 'EndDate', 'End Date', 0,0,0)
	insert into [dbo].[GameplanDataType]
		values(@IntegrationTypeID, 'Global', 'CreatedBy', 'Owner', 0,0,0) 
	insert into [dbo].[GameplanDataType]
		values(@IntegrationTypeID, 'Plan_Campaign_Program_Tactic', 'Cost', 'Cost (Budgeted)', 0,0,0) 
	insert into [dbo].[GameplanDataType]
		values(@IntegrationTypeID, 'Plan_Campaign_Program_Tactic', 'CostActual', 'Cost (Actual)', 0,0,0) 
end
GO
			 
DECLARE @IntegrationTypeID int;

select  @IntegrationTypeID = IntegrationTypeId from [dbo].[IntegrationType]	where Title = 'WorkFront' and [APIURL] = '.attask-ondemand.com/attask/api'
-- Add WorkFront integration attribute to IntegrationTypeAttribute
--WorkFront api requires the company name in front of the uri
IF NOT EXISTS(SELECT * FROM [dbo].[IntegrationTypeAttribute] WHERE [IntegrationTypeID] = @IntegrationTypeID AND [Attribute] = 'Company Name')
	insert into [dbo].[IntegrationTypeAttribute] values(@IntegrationTypeID, 'Company Name', 'textbox', 0)
GO

-- Add new column in Plan_Campaign_Program_Tactic called "IntegrationWorkFrontProjectID"
--This is designed to contain WorkFront Project IDs to link the tactic to the project
IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'IntegrationWorkFrontProjectID') 
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] 	ADD IntegrationWorkFrontProjectID nvarchar(50)
GO

--Start Added 24 July 2015 Brad Gray
-- Add new column in TacticType called "WorkFront Template"
--Tactic Types will can now have WorkFront templates tied to them
IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'WorkFront Template') 
ALTER TABLE [dbo].[TacticType] 	ADD [WorkFront Template] nvarchar(255)
GO

-- Add new column in Model called "IntegrationInstanceIdProjMgmt"
if not exists (select * from sys.columns WHERE [name] = 'IntegrationInstanceIdProjMgmt')
ALTER TABLE [dbo].[Model] ADD [IntegrationInstanceIdProjMgmt] int
go

/****** Object:  Table [dbo].[IntegrationWorkFrontTemplates]    Script Date: 7/24/2015 5:24:25 PM ******/


IF (not EXISTS (SELECT * FROM sys.TABLES WHERE name = 'IntegrationWorkFrontTemplates'))
begin
exec('SET ANSI_NULLS ON')
--exec('if (@tablecreate =1)SET QUOTED_IDENTIFIER ON')
exec('
CREATE TABLE [dbo].[IntegrationWorkFrontTemplates](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[TemplateId] [nvarchar](250) NOT NULL,
	[Template Name] [nvarchar](250) NOT NULL,
	[IsDeleted] [int] NOT NULL,
 CONSTRAINT [PK_IntegrationWorkFrontTemplates] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
')
exec('ALTER TABLE [dbo].[IntegrationWorkFrontTemplates]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceId] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])')
exec('ALTER TABLE [dbo].[IntegrationWorkFrontTemplates] CHECK CONSTRAINT [FK_IntegrationInstanceId]')

end

GO

-- Created By : Brad Gray
-- Created Date : 07/24/2015
-- Description :Database changes to enable WorkFront Integration
-- ======================================================================================

-- Add new column in TacticType called "WorkFront Template"
--Tactic Types will can now have WorkFront templates tied to them
IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'WorkFront Template') 
ALTER TABLE [dbo].[TacticType] 	ADD [WorkFront Template] nvarchar(255)
GO

-- Add new column in Model called "IntegrationInstanceIdProjMgmt"
if not exists (select * from sys.columns WHERE [name] = 'IntegrationInstanceIdProjMgmt')
ALTER TABLE [dbo].[Model] ADD [IntegrationInstanceIdProjMgmt] int
go

/****** Object:  Table [dbo].[IntegrationWorkFrontTemplates]    Script Date: 7/24/2015 5:24:25 PM ******/


IF (not EXISTS (SELECT * FROM sys.TABLES WHERE name = 'IntegrationWorkFrontTemplates'))
begin

exec('SET ANSI_NULLS ON')
--Comment by bhavesh because it give error
--exec('if (@tablecreate =1)SET QUOTED_IDENTIFIER ON')
exec('
CREATE TABLE [dbo].[IntegrationWorkFrontTemplates](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[TemplateId] [nvarchar](250) NOT NULL,
	[Template Name] [nvarchar](250) NOT NULL,
	[IsDeleted] [int] NOT NULL,
 CONSTRAINT [PK_IntegrationWorkFrontTemplates] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
')
exec('ALTER TABLE [dbo].[IntegrationWorkFrontTemplates]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceId] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])')
exec('ALTER TABLE [dbo].[IntegrationWorkFrontTemplates] CHECK CONSTRAINT [FK_IntegrationInstanceId]')

end

GO

--Added by viral
-- Date: 27/7/2015
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IntegrationInstanceLogDetails]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IntegrationInstanceLogDetails](
	[EntityId] [int] NOT NULL,
	[IntegrationInstanceLogId] [int] NULL,
	[LogTime] [datetime] NOT NULL,
	[LogDescription] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO




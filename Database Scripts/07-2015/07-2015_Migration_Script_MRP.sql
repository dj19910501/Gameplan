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
-- Description: Create Table [IntegrationInstanceLogDetails] to insert Integration log records.
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IntegrationInstanceLogDetails]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IntegrationInstanceLogDetails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EntityId] [int] NOT NULL,
	[IntegrationInstanceLogId] [int] NULL,
	[LogTime] [datetime] NOT NULL,
	[LogDescription] [nvarchar](max) NULL,
 CONSTRAINT [PK_IntegrationInstanceLogDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO



-- Created By : Brad Gray
-- Created Date : 07/24/2015
-- Description :Database change to allow CustomFields to be ReadOnly
-- ======================================================================================

-- Add new column in CustomField called "IsGet"

IF NOT EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'IsGet' AND Object_ID = Object_ID(N'CustomField'))
  ALTER TABLE [dbo].[CustomField]   ADD IsGet bit NOT NULL default 0
  Go


GO

/****** Object:  Table [dbo].[IntegrationWorkFrontPortfolios]    Script Date: 7/30/2015 3:55:15 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF (not EXISTS (SELECT * FROM sys.TABLES WHERE name = 'IntegrationWorkFrontPortfolios'))
begin
CREATE TABLE [dbo].[IntegrationWorkFrontPortfolios](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[PortfolioId] [nvarchar](50) NOT NULL,
	[PortfolioName] [nvarchar](250) NOT NULL,
	[PlanProgramId] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_IntegrationWorkFrontPortfolios] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
end
GO

/****** Object:  Table [dbo].[IntegrationWorkFrontPortfolio_Mapping]    Script Date: 7/30/2015 3:55:49 PM ******/
IF (not EXISTS (SELECT * FROM sys.TABLES WHERE name = 'IntegrationWorkFrontPortfolio_Mapping'))
begin
CREATE TABLE [dbo].[IntegrationWorkFrontPortfolio_Mapping](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PortfolioTableId] [int] NOT NULL,
	[ProjectId] [nvarchar](50) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_IntegrationWorkFrontPortfolio_Mapping] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF not exists (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
	 WHERE CONSTRAINT_NAME ='FK_PortfolioTableId' )
begin
ALTER TABLE [dbo].[IntegrationWorkFrontPortfolio_Mapping]  WITH CHECK ADD  CONSTRAINT [FK_PortfolioTableId] FOREIGN KEY([PortfolioTableId])
REFERENCES [dbo].[IntegrationWorkFrontPortfolios] ([Id])
ALTER TABLE [dbo].[IntegrationWorkFrontPortfolio_Mapping] CHECK CONSTRAINT [FK_PortfolioTableId]
end
GO


-- Created By : Brad Gray
-- Created Date : 08/10/2015
-- Description :Add FrontEndURL column to IntegrationType table
-- ======================================================================================


IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'FrontEndUrl') 
ALTER TABLE [dbo].[IntegrationType] ADD FrontEndUrl nvarchar(50)
GO


IF NOT EXISTS(SELECT * FROM [dbo].[IntegrationType] WHERE [Title] = 'Eloqua' AND [FrontEndUrl] = 'https://login.eloqua.com')
  begin
  declare @EloquaInstanceId int
  set  @EloquaInstanceId = (select IntegrationTypeId from [dbo].[IntegrationType] where title = 'Eloqua')
 update [dbo].[IntegrationType]
 set [FrontEndUrl] = 'https://login.eloqua.com' where IntegrationTypeId = @EloquaInstanceId
 end
 go

 IF NOT EXISTS(SELECT * FROM [dbo].[IntegrationType] WHERE [Title] = 'Salesforce-Sandbox' AND [FrontEndUrl] = 'https://test.salesforce.com')
  begin
  declare @SalesforceSandboxIntegrationId int
  set  @SalesforceSandboxIntegrationId = (select IntegrationTypeId from [dbo].[IntegrationType] where title = 'Salesforce-Sandbox')
 update [dbo].[IntegrationType]
 set [FrontEndUrl] = 'https://test.salesforce.com' where IntegrationTypeId = @SalesforceSandboxIntegrationId
 end
 go

  IF NOT EXISTS(SELECT * FROM [dbo].[IntegrationType] WHERE [Title] = 'Salesforce' AND [FrontEndUrl] = 'https://login.salesforce.com')
  begin
  declare @SalesforceIntegrationId int
  set  @SalesforceIntegrationId = (select IntegrationTypeId from [dbo].[IntegrationType] where title = 'Salesforce')
 update [dbo].[IntegrationType]
 set [FrontEndUrl] = 'https://login.salesforce.com' where IntegrationTypeId = @SalesforceIntegrationId
 end
 go

  IF NOT EXISTS(SELECT * FROM [dbo].[IntegrationType] WHERE [Title] = 'WorkFront' AND [FrontEndUrl] = '.attask-ondemand.com')
  begin
  declare @WFIntegrationId int
  set  @WFIntegrationId = (select IntegrationTypeId from [dbo].[IntegrationType] where title = 'WorkFront')
 update [dbo].[IntegrationType]
 set [FrontEndUrl] = '.attask-ondemand.com' where IntegrationTypeId = @WFIntegrationId
 end
 go



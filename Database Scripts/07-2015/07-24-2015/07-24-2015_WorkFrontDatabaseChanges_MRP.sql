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
ALTER TABLE [dbo].[Model] ADD [IntegrationInstanceIdProjMgmt] nvarchar(255)
go

/****** Object:  Table [dbo].[IntegrationWorkFrontTemplates]    Script Date: 7/24/2015 5:24:25 PM ******/

declare @tablecreate int = 0;

IF (not EXISTS (SELECT * FROM sys.TABLES WHERE name = 'IntegrationWorkFrontTemplates'))
begin
set @tablecreate = 1
exec('SET ANSI_NULLS ON')
exec('if (@tablecreate =1)SET QUOTED_IDENTIFIER ON')
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



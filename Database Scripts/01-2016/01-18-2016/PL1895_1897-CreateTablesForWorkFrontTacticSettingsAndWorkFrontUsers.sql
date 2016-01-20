
/* --------- Start Script of PL ticket #1897 & #1895 --------- */
-- Created by : Brad Gray
-- Created On : 1/18/2016
-- Description : Create IntegrationWorkFrontTacticSettings and IntegrationWorkFrontUsers  tables

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

if not exists ((SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'IntegrationWorkFrontTacticSettings'))
begin
CREATE TABLE [dbo].[IntegrationWorkFrontTacticSettings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TacticId] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[TacticApprovalObject] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_IntegrationWorkFrontTacticSettings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
end
GO


IF (OBJECT_ID('FK_IntegrationWorkFrontTacticSettings_TacticId', 'F') IS NULL)
ALTER TABLE [dbo].[IntegrationWorkFrontTacticSettings]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationWorkFrontTacticSettings_TacticId] FOREIGN KEY([TacticId])
REFERENCES [dbo].[Plan_Campaign_Program_Tactic] ([PlanTacticId])
GO

ALTER TABLE [dbo].[IntegrationWorkFrontTacticSettings] CHECK CONSTRAINT [FK_IntegrationWorkFrontTacticSettings_TacticId]
GO

GO


if not exists ((SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'IntegrationWorkFrontUsers'))
begin
CREATE TABLE [dbo].[IntegrationWorkFrontUsers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[WorkFrontUserId] [nvarchar](100) NOT NULL,
	[WorkFrontUserName] [nvarchar](255) NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_IntegrationWorkFrontUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF (OBJECT_ID('FK_IntegrationWorkFrontTacticSettings_IntegrationInstanceId', 'F') IS NULL)
ALTER TABLE [dbo].[IntegrationWorkFrontUsers]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationWorkFrontUsers_IntegrationInstanceId] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])
GO

ALTER TABLE [dbo].[IntegrationWorkFrontUsers] CHECK CONSTRAINT [FK_IntegrationWorkFrontUsers_IntegrationInstanceId]
GO

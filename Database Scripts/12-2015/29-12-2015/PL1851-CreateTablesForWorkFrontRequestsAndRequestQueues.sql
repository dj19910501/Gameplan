
/* --------- Start Script of PL ticket #1851 --------- */
-- Created by : Brad Gray
-- Created On : 12/29/2015
-- Description : Create IntegrationWorkFrontRequestQueues and IntegrationWorkFrontRequests tables

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontRequestQueues') 
Begin
CREATE TABLE [dbo].[IntegrationWorkFrontRequestQueues](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestQueueId] [nvarchar](50) NOT NULL,
	[RequestQueueName] [nvarchar](255) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_IntegrationWorkFrontRequestQueues] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
end
GO


IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontRequests') 
Begin
CREATE TABLE [dbo].[IntegrationWorkFrontRequests](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[RequestId] [nvarchar](50) NOT NULL,
	[QueueId] [int] NOT NULL,
	[PlanTacticId] [int] NOT NULL,
	[AssignedTo] [nvarchar](50) NOT NULL,
	[IsDeletedPlan] [bit] NOT NULL,
	[IsDeletedWorkFront] [bit] NOT NULL,
 CONSTRAINT [PK_IntegrationWorkFrontRequests] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
end
GO

if exists (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontRequests') and 
	exists (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationInstance') and 
	not exists (SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS  WHERE CONSTRAINT_NAME='FK_IntegrationWorkFrontRequests_IntegrationInstanceId')
begin
ALTER TABLE [dbo].[IntegrationWorkFrontRequests]  WITH NOCHECK ADD  CONSTRAINT [FK_IntegrationWorkFrontRequests_IntegrationInstanceId] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])
NOT FOR REPLICATION 
end
GO

if exists (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontRequests')
ALTER TABLE [dbo].[IntegrationWorkFrontRequests] CHECK CONSTRAINT [FK_IntegrationWorkFrontRequests_IntegrationInstanceId]
GO

if exists (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontRequests') and 
	exists (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontRequestQueues') and 
	not exists (SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS  WHERE CONSTRAINT_NAME='FK_IntegrationWorkFrontRequests_QueueId')
ALTER TABLE [dbo].[IntegrationWorkFrontRequests]  WITH NOCHECK ADD  CONSTRAINT [FK_IntegrationWorkFrontRequests_QueueId] FOREIGN KEY([QueueId])
REFERENCES [dbo].[IntegrationWorkFrontRequestQueues] ([Id])
NOT FOR REPLICATION 
GO

if exists (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontRequests')
ALTER TABLE [dbo].[IntegrationWorkFrontRequests] CHECK CONSTRAINT [FK_IntegrationWorkFrontRequests_QueueId]
GO

if exists (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontRequests') and 
	exists (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='Plan_Campaign_Program_Tactic') and 
	not exists (SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS  WHERE CONSTRAINT_NAME='FK_IntegrationWorkFrontRequests_TacticId')
ALTER TABLE [dbo].[IntegrationWorkFrontRequests]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationWorkFrontRequests_TacticId] FOREIGN KEY([PlanTacticId])
REFERENCES [dbo].[Plan_Campaign_Program_Tactic] ([PlanTacticId])
GO

if exists (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontRequests')
ALTER TABLE [dbo].[IntegrationWorkFrontRequests] CHECK CONSTRAINT [FK_IntegrationWorkFrontRequests_TacticId]
GO



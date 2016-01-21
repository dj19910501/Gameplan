/* --------- Start Script of PL ticket #1897 --------- */
-- Created by : Brad Gray
-- Created On : 1/20/2016
-- Description : Drop and recreate the IntegratinWorkFrontRequests table for field updates

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontRequests') 
DROP TABLE [dbo].[IntegrationWorkFrontRequests]
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontRequests') 
Begin
CREATE TABLE [dbo].[IntegrationWorkFrontRequests](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PlanTacticId] [int] NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[RequestId] [nvarchar](50) NULL,
	[RequestName] [nvarchar](255) NOT NULL,
	[QueueId] [int] NOT NULL,
	[AssignedTo] [nvarchar](50) NOT NULL,
	[WorkFrontRequestStatus] [nvarchar](20) NULL,
	[ResolvingObjType] [nvarchar](10) NULL,
	[ResolvingObjId] [nvarchar](50) NULL,
	[IsDeleted] [bit] NOT NULL,
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
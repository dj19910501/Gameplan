/* --------- Start Script of PL ticket #1897 --------- */
-- Created by : Brad Gray
-- Created On : 1/21/2016
-- Description : Add Integration Instance field to IntegerationWorkFrontRequestQueues table. Ensures fk to request table

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF ( EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontRequestQueues') and 
		   not exists (SELECT * FROM sys.columns  WHERE Name = N'IntegrationInstanceId' AND Object_ID = Object_ID(N'IntegrationWorkFrontRequestQueues')) )
Begin
alter TABLE [dbo].[IntegrationWorkFrontRequestQueues]
Add IntegrationInstanceId int not null
end
GO

If not exists (SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS  WHERE CONSTRAINT_NAME='FK_IntegrationWorkFrontRequestQueues_IntegrationInstanceId')
ALTER TABLE [dbo].[IntegrationWorkFrontRequestQueues]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationWorkFrontRequestQueues_IntegrationInstanceId] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontRequestQueues') 
ALTER TABLE [dbo].[IntegrationWorkFrontRequestQueues] CHECK CONSTRAINT [FK_IntegrationWorkFrontRequestQueues_IntegrationInstanceId]
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
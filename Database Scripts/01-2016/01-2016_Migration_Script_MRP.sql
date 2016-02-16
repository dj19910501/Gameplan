/* --------- Start Script of PL ticket #1921 --------- */
-- Created by : Viral Kadiya
-- Created On : 1/22/2016
-- Description : Add column IsSyncSalesForce & IsSyncEloqua column to Tactic table - To save Integration settings from Review tab

IF not exists (SELECT * FROM sys.columns  WHERE Name = N'IsSyncSalesForce' AND Object_ID = Object_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] ADD IsSyncSalesForce bit
END
GO
IF not exists (SELECT * FROM sys.columns  WHERE Name = N'IsSyncEloqua' AND Object_ID = Object_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] ADD IsSyncEloqua bit
END
GO

/* --------- End Script of PL ticket #1921 --------- */

-- Add BY Nishant Sheth
-- Desc :: To GetCollaboratorsId
IF OBJECT_ID(N'[dbo].[GetCollaboratorId]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[GetCollaboratorId]
GO
-- Created By Nishant Sheth
-- EXEC GetCollaboratorId 226
CREATE PROCEDURE GetCollaboratorId
@PlanId int = 0
AS
BEGIN
SELECT tacComment.* From [dbo].[Plan_Campaign_Program_Tactic_Comment] TacComment
CROSS APPLY(Select PlanTacticId,PlanProgramId From [dbo].[Plan_Campaign_Program_Tactic] Tactic WHERE TacComment.PlanTacticId=Tactic.PlanTacticId) Tactic
CROSS APPLY(Select PlanProgramId,PlanCampaignId From  [dbo].[Plan_Campaign_Program] Program WHERE Tactic.PlanProgramId=Program.[PlanProgramId])Program
CROSS APPLY(Select PlanCampaignId,Planid FROM  [dbo].[Plan_Campaign] Camp WHERE Program.[PlanCampaignId]=Camp.[PlanCampaignId])Camp
Where Camp.Planid=@PlanId
END
-- Add By Rahul Shah
-- Desc :: set delete flag when otherLineItem's cost is '0'
Go
IF (EXISTS(SELECT * FROM [Plan_Campaign_Program_Tactic_Lineitem] WHERE LineItemTypeId is Null and Cost = 0 and isDeleted = 0)) 
BEGIN 
	Update [Plan_Campaign_Program_Tactic_Lineitem] Set isDeleted = 1 WHERE LineItemTypeId is Null and Cost = 0 and isDeleted = 0
End

Go


/* --------- Start Script of PL ticket #1943 --------- */
-- Created by : Komal Rawal
-- Created On : 02/02/2016
-- Description : Add column DependencyDate column to Plan table - 

IF not exists (SELECT * FROM sys.columns  WHERE Name = N'DependencyDate' AND Object_ID = Object_ID(N'Plan'))
BEGIN
ALTER TABLE [dbo].[Plan] ADD DependencyDate datetime
END
GO

/* --------- End Script of PL ticket #1943 --------- */

/* --------- Start Script of PL ticket #1943 --------- */
-- Created by : Komal Rawal
-- Created On : 2/3/2016
-- Description : Update dependency Date for Plans of a particular client.
-- Note: Parameters  @ClientId and @DependencyDate needs to be changed as per requirement
Declare @ClientId uniqueidentifier = 'C251AB18-0683-4D1D-9F1E-06709D59FD53'
Declare @DependencyDate datetime = '2016-02-04 00:00:00.000'
Update [Plan] set DependencyDate = @DependencyDate where PlanId In (Select PlanId from [Plan] where ModelId IN ( Select ModelId from [Model] where ClientId = @ClientId and IsDeleted='0') and IsDeleted='0')

/* --------- End Script of PL ticket #1943--------- */

/* --------- Start Script of PL ticket #1856 --------- */
-- Created by : Brad Gray
-- Created On : 1/07/2016
-- Description : Update storage of WorkFront Template Information - create a new column titled WOrkFrontTemplateId to store the ID column from the 
--               IntegrationWorkFrontTemplates table (primary key) instead of storing the Template ID as nvarchar. 

if not exists (SELECT * FROM sys.columns  WHERE Name = N'WorkFrontTemplateId' AND Object_ID = Object_ID(N'TacticType'))
begin
ALTER TABLE [dbo].[TacticType] ADD WorkFrontTemplateId int
end
GO

IF (OBJECT_ID('FK_TacticType_TacticType_WorkFrontTemplateId', 'F') IS NULL)
ALTER TABLE [dbo].[TacticType]  WITH CHECK ADD  CONSTRAINT [FK_TacticType_TacticType_WorkFrontTemplateId] FOREIGN KEY([WorkFrontTemplateId])
REFERENCES [dbo].[IntegrationWorkFrontTemplates] ([ID])
GO


declare @WorkFrontTemplate varchar(22) = '[WorkFront Template]';
declare @WorkFrontTemplateId nvarchar(25) = '[WorkFrontTemplateId]';
DECLARE @sqlText nvarchar(1000); 
if ( exists (SELECT * FROM sys.columns  WHERE Name = N'WorkFront Template' AND Object_ID = Object_ID(N'TacticType'))
and exists (SELECT * FROM sys.columns  WHERE Name = N'WorkFrontTemplateId'  AND Object_ID = Object_ID(N'TacticType')) )
begin
SET @sqlText = 'update a
set a.'+@WorkFrontTemplateId +' = b.ID
from TacticType a
inner join IntegrationworkFrontTemplates b on (a.' + @WorkFrontTemplate +' = b.TemplateId) and b.IntegrationInstanceId = 19'
EXEC (@sqlText)
end

if  exists (SELECT * FROM sys.columns  WHERE Name = N'WorkFront Template' AND Object_ID = Object_ID(N'TacticType'))
begin
alter table [dbo].[TacticType] drop column [WorkFront Template]
end
go

/* --------- End Script of PL ticket #1856--------- */


/* --------- Start Script of PL ticket #1875,1897 --------- */
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

/* --------- End Script of PL ticket #1875,1897 --------- */


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

IF (OBJECT_ID('FK_IntegrationWorkFrontUsers_IntegrationInstanceId', 'F') IS NULL)
ALTER TABLE [dbo].[IntegrationWorkFrontUsers]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationWorkFrontUsers_IntegrationInstanceId] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])
GO

ALTER TABLE [dbo].[IntegrationWorkFrontUsers] CHECK CONSTRAINT [FK_IntegrationWorkFrontUsers_IntegrationInstanceId]
GO

/* --------- End Script of PL ticket #1897 & #1895 --------- */


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
	[WorkFrontUserId] [int] NOT NULL,
	[WorkFrontRequestStatus] [nvarchar](20) NULL,
	[ResolvingObjType] [nvarchar](10) NULL,
	[ResolvingObjId] [nvarchar](50) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_IntegrationWorkFrontRequests] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
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

if exists (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontRequests') and 
	exists (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='IntegrationWorkFrontUsers') and 
	not exists (SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS  WHERE CONSTRAINT_NAME='FK_IntegrationWorkFrontRequests_WorkFrontUser')
ALTER TABLE [dbo].[IntegrationWorkFrontRequests]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationWorkFrontRequests_WorkFrontUser] FOREIGN KEY([WorkFrontUserId])
REFERENCES [dbo].[IntegrationWorkFrontUsers] ([Id])
GO

if exists (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='FK_IntegrationWorkFrontRequests_WorkFrontUser')
ALTER TABLE [dbo].[IntegrationWorkFrontRequests] CHECK CONSTRAINT [FK_IntegrationWorkFrontRequests_WorkFrontUser]
GO

/* --------- End Script of PL ticket #1897 --------- */

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

/* --------- End Script of PL ticket #1897 --------- */

/* --------- Start Script of PL ticket #1922 --------- */
-- Created by : Brad Gray
-- Created On : 1/22/2016
-- Description : Add column IsSyncWorkFront - To save Integration settings from Review tab

IF not exists (SELECT * FROM sys.columns  WHERE Name = N'IsSyncWorkFront' AND Object_ID = Object_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] ADD IsSyncWorkFront bit
END
GO

/* --------- End Script of PL ticket #1922 --------- */

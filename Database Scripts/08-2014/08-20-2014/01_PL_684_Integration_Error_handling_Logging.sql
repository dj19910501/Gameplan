-- db SCRIPT FOR #684 Integration - Error handling & Logging
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceSection')
BEGIN

CREATE TABLE [dbo].[IntegrationInstanceSection](
	[IntegrationInstanceSectionId] [int] IDENTITY(1,1) NOT NULL,
	[IntegartionInstanceLogId] [int] NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[SectionName] [nvarchar](255) NOT NULL,
	[SyncStart] [datetime] NOT NULL,
	[SyncEnd] [datetime] NULL,
	[Status] [nvarchar](50) NULL,
	[Description] [nvarchar](255) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreateBy] [uniqueidentifier] NOT NULL,
	CONSTRAINT [PK_IntegrationInstanceSection] PRIMARY KEY CLUSTERED 
	(
		[IntegrationInstanceSectionId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

ALTER TABLE [dbo].[IntegrationInstanceSection]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceSection_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])

ALTER TABLE [dbo].[IntegrationInstanceSection] CHECK CONSTRAINT [FK_IntegrationInstanceSection_IntegrationInstance]

ALTER TABLE [dbo].[IntegrationInstanceSection]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceSection_IntegrationInstanceLog] FOREIGN KEY([IntegartionInstanceLogId])
REFERENCES [dbo].[IntegrationInstanceLog] ([IntegrationInstanceLogId])

ALTER TABLE [dbo].[IntegrationInstanceSection] CHECK CONSTRAINT [FK_IntegrationInstanceSection_IntegrationInstanceLog]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify IntegrationInstance Section.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'IntegrationInstanceSectionId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated IntegrationInstanceLogId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'IntegartionInstanceLogId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated IntegrationInstanceId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'IntegrationInstanceId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Possible values (PushTacticData,PullResponses,PullQualifiedLeads,PullClosedDeals)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'SectionName'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sync start time for the section' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'SyncStart'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sync end time for the section' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'SyncEnd'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sync status for the section' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'Status'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sync description for the section' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'Description'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'CreatedDate'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceSection', @level2type=N'COLUMN',@level2name=N'CreateBy'

END
GO

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'IntegrationInstancePlanLogEntityId' AND OBJECT_ID = OBJECT_ID(N'IntegrationInstancePlanEntityLog'))
BEGIN
    ALTER TABLE [IntegrationInstancePlanEntityLog] ADD IntegrationInstancePlanLogEntityId BIGINT IDENTITY(1,1) NOT NULL
	ALTER TABLE [IntegrationInstancePlanEntityLog] ADD CONSTRAINT PK_IntegrationInstancePlanEntityLog PRIMARY KEY(IntegrationInstancePlanLogEntityId)
END
GO

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'IntegrationInstanceSectionId' AND OBJECT_ID = OBJECT_ID(N'IntegrationInstancePlanEntityLog'))
BEGIN
	DELETE FROM IntegrationInstanceLog
	DELETE FROM IntegrationInstancePlanEntityLog
    ALTER TABLE [IntegrationInstancePlanEntityLog] ADD IntegrationInstanceSectionId INT NOT NULL
	ALTER TABLE [dbo].[IntegrationInstancePlanEntityLog]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstancePlanEntityLog_IntegrationInstanceSectionId] FOREIGN KEY([IntegrationInstanceSectionId])
	REFERENCES [dbo].[IntegrationInstanceSection] ([IntegrationInstanceSectionId])
	ALTER TABLE [dbo].[IntegrationInstancePlanEntityLog] CHECK CONSTRAINT [FK_IntegrationInstancePlanEntityLog_IntegrationInstanceSectionId]
END
GO

IF EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'IntegrationInstanceLogId' AND OBJECT_ID = OBJECT_ID(N'IntegrationInstancePlanEntityLog'))
BEGIN
    ALTER TABLE [IntegrationInstancePlanEntityLog] DROP COLUMN IntegrationInstanceLogId
END
GO



-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 05/12/2014
-- Description : Custom naming: Campaign name structure
-- ======================================================================================

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CampaignNameConvention')
BEGIN

CREATE TABLE [dbo].[CampaignNameConvention](
	[CampaignNameConventionId] [int] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](255) NOT NULL,
	[FieldName] [nvarchar](255) NULL,
	[CustomFieldId] [int] NULL,
	[Sequence] [int] NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] DEFAULT (0) NOT NULL,
 CONSTRAINT [PK_CampaignNameConvention] PRIMARY KEY CLUSTERED 
(
	[CampaignNameConventionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CampaignNameConvention', @level2type=N'COLUMN',@level2name=N'CampaignNameConventionId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of Table.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CampaignNameConvention', @level2type=N'COLUMN',@level2name=N'TableName'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of Field.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CampaignNameConvention', @level2type=N'COLUMN',@level2name=N'FieldName'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated CustomFieldId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CampaignNameConvention', @level2type=N'COLUMN',@level2name=N'CustomFieldId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Numeric sequence to maintain the logical order.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CampaignNameConvention', @level2type=N'COLUMN',@level2name=N'Sequence'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated ClientId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CampaignNameConvention', @level2type=N'COLUMN',@level2name=N'ClientId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CampaignNameConvention', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CampaignNameConvention', @level2type=N'COLUMN',@level2name=N'CreatedBy'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Flag to identify record is deleted or not.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CampaignNameConvention', @level2type=N'COLUMN',@level2name=N'IsDeleted'


ALTER TABLE [dbo].[CampaignNameConvention]  WITH CHECK ADD  CONSTRAINT [FK_CampaignNameConvention_CustomField] FOREIGN KEY([CustomFieldId])
REFERENCES [dbo].[CustomField] ([CustomFieldId])

ALTER TABLE [dbo].[CampaignNameConvention] CHECK CONSTRAINT [FK_CampaignNameConvention_CustomField]
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Audience')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Abbreviation' AND [object_id] = OBJECT_ID(N'Audience'))
	    BEGIN
		    ALTER TABLE [Audience] ADD Abbreviation NVARCHAR(255) NULL
	    END
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='BusinessUnit')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Abbreviation' AND [object_id] = OBJECT_ID(N'BusinessUnit'))
	    BEGIN
		     ALTER TABLE [BusinessUnit] ADD Abbreviation NVARCHAR(255) NULL
	    END
END
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Geography')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Abbreviation' AND [object_id] = OBJECT_ID(N'Geography'))
	    BEGIN
		     ALTER TABLE [Geography] ADD Abbreviation NVARCHAR(255) NULL
		END
END
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldOption')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Abbreviation' AND [object_id] = OBJECT_ID(N'CustomFieldOption'))
	    BEGIN
		     ALTER TABLE [CustomFieldOption] ADD Abbreviation NVARCHAR(255) NULL
	    END
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Campaign')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Abbreviation' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
	    BEGIN
		     ALTER TABLE [Plan_Campaign] ADD Abbreviation NVARCHAR(255) NULL
	    END
END
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Campaign_Program')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Abbreviation' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program'))
	    BEGIN
		     ALTER TABLE [Plan_Campaign_Program] ADD Abbreviation NVARCHAR(255) NULL
	    END
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Vertical')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Abbreviation' AND [object_id] = OBJECT_ID(N'Vertical'))
	    BEGIN
		     ALTER TABLE [Vertical] ADD Abbreviation NVARCHAR(255) NULL
		END
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Campaign_Program_Tactic')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'TacticCustomName' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
	    BEGIN
		     ALTER TABLE [Plan_Campaign_Program_Tactic] ADD TacticCustomName NVARCHAR(4000) NULL
		END
END
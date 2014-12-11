 ---Execute this script on Gameplan database.
 --01_PL_957_Salesforce_Integration_URLs
BEGIN
		IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationType')
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IntegrationType' AND COLUMN_NAME = 'Code')
		BEGIN
			ALTER TABLE [IntegrationType] ADD Code VARCHAR(255) NULL
		END
	END


IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationType')
BEGIN
	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IntegrationType' AND COLUMN_NAME = 'Code' AND IS_NULLABLE = 'YES')
	BEGIN
		Update IntegrationType SET Code = Title
	END
	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IntegrationType' AND COLUMN_NAME = 'Code' AND IS_NULLABLE = 'YES')
	BEGIN
		ALTER TABLE [IntegrationType] ALTER COLUMN Code VARCHAR(255) NOT NULL
	END
END


IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'IntegrationType' AND TABLE_SCHEMA = 'dbo')
	BEGIN
		IF (SELECT COUNT(*) FROM IntegrationType WHERE (Title = 'Salesforce' OR Title = 'Salesforce-Sandbox')) = 1
		BEGIN
			Update IntegrationType SET Title = 'Salesforce-Sandbox' WHERE Title = 'Salesforce'
			INSERT INTO IntegrationType (Title,IsDeleted,APIURL,Code) VALUES ('Salesforce',0,'https://login.salesforce.com/services/oauth2/token','Salesforce')
		END
	END


IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'IntegrationType' AND TABLE_SCHEMA = 'dbo')
BEGIN
	IF (SELECT COUNT(*) FROM IntegrationType WHERE (Title = 'Salesforce' OR Title = 'Salesforce-Sandbox')) = 2
	BEGIN
		DECLARE @SalesforceSandBoxId int = (SELECT IntegrationTypeId FROM IntegrationType WHERE Title = 'Salesforce-Sandbox')
		DECLARE @SalesforceId int = (SELECT IntegrationTypeId FROM IntegrationType WHERE Title = 'Salesforce')
		IF NOT EXISTS (SELECT 1 FROM IntegrationTypeAttribute WHERE IntegrationTypeId = @SalesforceId)
		BEGIN
			INSERT INTO IntegrationTypeAttribute (IntegrationTypeId, Attribute, AttributeType, IsDeleted)
			SELECT @SalesforceId,Attribute,AttributeType,IsDeleted FROM IntegrationTypeAttribute WHERE IntegrationTypeId = @SalesforceSandBoxId
		END
	END
END
END

--01_PL_978_Add URL to all the notification emails
GO
BEGIN
	IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='TacticApproved')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following tactic has been approved.<br><br><table><tr><td>Tactic Name</td><td>:</td><td>[TacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Approved by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='TacticApproved'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='TacticDeclined')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following tactic has been declined.<br><br><table><tr><td>Tactic Name</td><td>:</td><td>[TacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Declined by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='TacticDeclined'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='TacticSubmitted')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following tactic has been submitted for approval.<br><br><table><tr><td>Tactic Name</td><td>:</td><td>[TacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Submitted by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='TacticSubmitted'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='CampaignCommentAdded')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that comment has been added to the following campaign<br/><br/><table><tr><td>Campaign Name</td><td>:</td><td>[CampaignNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Comment</td><td>:</td><td>[CommentToBeReplaced]</td></tr><tr><td>Comment added by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='CampaignCommentAdded'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='CampaignApproved')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following campaign has been approved.<br><br><table><tr><td>Campaign Name</td><td>:</td><td>[CampaignNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Approved by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='CampaignApproved'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='CampaignDeclined')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following campaign has been declined.<br><br><table><tr><td>Campaign Name</td><td>:</td><td>[CampaignNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Declined by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='CampaignDeclined'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='CampaignSubmitted')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following campaign has been submitted for approval.<br><br><table><tr><td>Campaign Name</td><td>:</td><td>[CampaignNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Submitted by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='CampaignSubmitted'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ProgramCommentAdded')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that comment has been added to the following program<br/><br/><table><tr><td>Program Name</td><td>:</td><td>[ProgramNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Comment</td><td>:</td><td>[CommentToBeReplaced]</td></tr><tr><td>Comment added by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='ProgramCommentAdded'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ProgramApproved')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following program has been approved.<br><br><table><tr><td>Program Name</td><td>:</td><td>[ProgramNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Approved by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='ProgramApproved'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ProgramDeclined')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following program has been declined.<br><br><table><tr><td>Program Name</td><td>:</td><td>[ProgramNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Declined by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='ProgramDeclined'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ProgramSubmitted')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following program has been submitted for approval.<br><br><table><tr><td>Program Name</td><td>:</td><td>[ProgramNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Submitted by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='ProgramSubmitted'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ImprovementTacticApproved')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following improvement tactic has been approved.<br><br><table><tr><td>Improvement Tactic Name</td><td>:</td><td>[ImprovementTacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Approved by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='ImprovementTacticApproved'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ImprovementTacticDeclined')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following improvement tactic has been declined.<br><br><table><tr><td>Improvement Tactic Name</td><td>:</td><td>[ImprovementTacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Declined by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='ImprovementTacticDeclined'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ImprovementTacticSubmitted')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following improvement tactic has been submitted for approval.<br><br><table><tr><td>Improvement Tactic Name</td><td>:</td><td>[ImprovementTacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Submitted by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='ImprovementTacticSubmitted'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='TacticOwnerChanged')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br><br>[ModifierName] has made you the owner of following tactic.<br><br><table><tr><td>Tactic</td><td>:</td><td>[tacticname]</td></tr><tr><td>Program</td><td>:</td><td>[programname]</td></tr><tr><td>Campaign</td><td>:</td><td>[campaignname]</td></tr><tr><td>Plan</td><td>:</td><td>[planname]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='TacticOwnerChanged'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ChangePassword')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>This message is to inform you that your password was recently changed on your Bulldog Gameplan account. If you did not perform this action, please contact you administrator as soon as possible to resolve this issue.<br/><br/>URL:&nbsp;&nbsp;&nbsp;[URL] <br/><br/>Thank You,<br />Bulldog Gameplan Team'
	Where NotificationInternalUseOnly='ChangePassword'
End
END

--01_PL_995_Custom_fields_integration_Tactic_custom_fields
GO
BEGIN
	-- ======================================================================================
-- Created By : Sohel Pathan
-- Created Date : 02/12/2014
-- Description : set global table name for common gameplandatatype fields.
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping') AND
	EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping' AND COLUMN_NAME = 'GameplanDataTypeId' AND IS_NULLABLE = 'NO') 
BEGIN
	ALTER TABLE IntegrationInstanceDataTypeMapping ALTER COLUMN GameplanDataTypeId INT NULL
END



IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping') AND
	NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping' AND COLUMN_NAME = 'CustomFieldId') 
BEGIN
	ALTER TABLE IntegrationInstanceDataTypeMapping ADD CustomFieldId INT NULL
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping') AND
	EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomField') AND
	NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_IntegrationInstanceDataTypeMapping_CustomField_CustomFieldId') 
BEGIN
	ALTER TABLE [dbo].[IntegrationInstanceDataTypeMapping]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceDataTypeMapping_CustomField_CustomFieldId] FOREIGN KEY([CustomFieldId])
	REFERENCES [dbo].[CustomField] ([CustomFieldId])
END



IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType') AND
	NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType' AND COLUMN_NAME = 'IsImprovement') 
BEGIN
	ALTER TABLE GameplanDataType ADD IsImprovement BIT NOT NULL DEFAULT(0)
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Improvement_Campaign') AND
	NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Improvement_Campaign' AND COLUMN_NAME = 'Description') 
BEGIN
	ALTER TABLE Plan_Improvement_Campaign ADD Description VARCHAR(4000) NULL
END


IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Improvement_Campaign_Program') AND
	NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Improvement_Campaign_Program' AND COLUMN_NAME = 'Description') 
BEGIN
	ALTER TABLE Plan_Improvement_Campaign_Program ADD Description VARCHAR(4000) NULL
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping') AND
	EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType') 
BEGIN
	
	IF NOT EXISTS(SELECT 1 FROM GameplanDataType WHERE TableName = 'Global' AND IsDeleted = 0)
	BEGIN
		
		-- ================= Delete mapping for duplicate fields from IntegrationInstanceDataTypeMapping table for global fields ============================
		DELETE from IntegrationInstanceDataTypeMapping WHERE GameplanDataTypeId IN(
		select GameplanDataTypeId from GameplanDataType
		where IsDeleted = 0
		AND TableName In ('Plan_Campaign','Plan_Campaign_Program', 'Plan_Improvement_Campaign', 'Plan_Improvement_Campaign_Program', 'Plan_Improvement_Campaign_Program_Tactic')
		AND ActualFieldName IN ('Description', 'EndDate', 'Title', 'CreatedBy', 'StartDate', 'Status')
		)

		-- ================= Delete datatypes from GameplanDataType table with global fields of all tables except Plan_Campaign_Program_Tactic ============================
		DELETE from GameplanDataType 
		where IsDeleted = 0
		AND TableName In ('Plan_Campaign','Plan_Campaign_Program', 'Plan_Improvement_Campaign', 'Plan_Improvement_Campaign_Program', 'Plan_Improvement_Campaign_Program_Tactic')
		AND ActualFieldName IN ('Description', 'EndDate', 'Title', 'CreatedBy', 'StartDate', 'Status') -- 16
		
		-- ============= Set TableName = Global for Global fields ===========================
		UPDATE GameplanDataType  SET TableName = 'Global'
		from GameplanDataType 
		where IsDeleted = 0
		AND TableName IN ('Plan_Campaign_Program_Tactic')
		AND ActualFieldName IN ('Description', 'EndDate', 'Title', 'CreatedBy', 'StartDate', 'Status') -- 11

		-- ============= Set IsImprovement = 1 for Global fields of Improvement Section ===========================
		UPDATE GameplanDataType  SET IsImprovement = 1
		from GameplanDataType
		where IsDeleted = 0 AND ISNULL(IsImprovement, 0) = 0
		AND TableName = 'Global'
		AND ActualFieldName IN ('Description', 'Title', 'Status')

	END
END

END
--01_PL_995_Plan_Table_Changes_for_Eloqua_Folder_Path
GO
BEGIN
	IF EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'EloquaFolderPath' AND [object_id] = OBJECT_ID(N'Plan'))
BEGIN
  ALTER TABLE dbo.[Plan] DROP COLUMN EloquaFolderPath 
END

BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION

ALTER TABLE dbo.[Plan] ADD
	EloquaFolderPath nvarchar(4000) NULL

ALTER TABLE dbo.[Plan] SET (LOCK_ESCALATION = TABLE)

COMMIT
END
--01_PL_993_Custom_fields_integration_Change_layout_of_existing_UI
GO
BEGIN
	IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType')
BEGIN
	
	IF EXISTS (SELECT 1 FROM GameplanDataType WHERE IsDeleted = 0 AND ActualFieldName = 'Revenue' AND DisplayFieldName = 'Revenue')
	BEGIN
	
		IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping')
		BEGIN
	
			IF EXISTS (SELECT 1 FROM IntegrationInstanceDataTypeMapping WHERE GameplanDataTypeId IN (
						SELECT GameplanDataTypeId FROM GameplanDataType WHERE IsDeleted = 0 AND ActualFieldName = 'Revenue' AND DisplayFieldName = 'Revenue'))
			BEGIN
	
				DELETE FROM IntegrationInstanceDataTypeMapping WHERE GameplanDataTypeId IN (
				SELECT GameplanDataTypeId FROM GameplanDataType WHERE IsDeleted = 0 AND ActualFieldName = 'Revenue' AND DisplayFieldName = 'Revenue')
	
			END
	
		END

		DELETE FROM GameplanDataType WHERE IsDeleted = 0 AND ActualFieldName = 'Revenue' AND DisplayFieldName = 'Revenue'

	END

END
END
--	01_PL_1000_Custom_naming_Campaign_name_structure
GO
BEGIN


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

END
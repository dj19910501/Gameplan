GO

--Run into MRP DB
-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 02/02/2015
-- Description : PL ticket 1215 Model cleanup - Remove unused tables
-- ======================================================================================

Go
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Model_Audience_Event')
BEGIN
DROP TABLE dbo.Model_Audience_Event
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Model_Audience_Inbound')
BEGIN
DROP TABLE dbo.Model_Audience_Inbound
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Model_Audience_Outbound')
BEGIN
DROP TABLE dbo.Model_Audience_Outbound
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='ModelReview')
BEGIN
DROP TABLE dbo.ModelReview
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Model')
BEGIN
        IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'AddressableContacts' AND [object_id] = OBJECT_ID(N'Model'))
	    BEGIN
		    ALTER TABLE dbo.Model DROP COLUMN AddressableContacts 
	    END
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Model')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'AverageDealSize' AND [object_id] = OBJECT_ID(N'Model'))
	    BEGIN
		    ALTER TABLE [Model] ADD AverageDealSize FLOAT NULL
	    END
END
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Model_Funnel_Stage')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ModelId' AND [object_id] = OBJECT_ID(N'Model_Funnel_Stage'))
	    BEGIN
		    ALTER TABLE [Model_Funnel_Stage] ADD ModelId INT NULL
	    END
END

GO
BEGIN TRY
BEGIN TRANSACTION

DECLARE @FunnelIdForMarketing INT=0
DECLARE @MarketingTitle VARCHAR(20)

SELECT @MarketingTitle='Marketing'
	IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Funnel')
	BEGIN
		SELECT @FunnelIdForMarketing=FunnelId FROM dbo.Funnel WHERE Title=@MarketingTitle

		IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'AverageDealSize' AND [object_id] = OBJECT_ID(N'Model') AND @FunnelIdForMarketing>0  AND EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Model_Funnel'))
		BEGIN

            UPDATE dbo.Model
			SET AverageDealSize=0 WHERE AverageDealSize IS NULL

			UPDATE dbo.Model
			SET AverageDealSize=ADS
			FROM
			(SELECT dbo.Model_Funnel.AverageDealSize AS ADS,dbo.Model.ModelId AS Joined_ModelId FROM dbo.Model INNER JOIN dbo.Model_Funnel ON dbo.Model_Funnel.ModelId = dbo.Model.ModelId WHERE dbo.Model_Funnel.FunnelId=@FunnelIdForMarketing)Model_Funnel_Joined
			WHERE dbo.Model.ModelId=Joined_ModelId

		END

	END

	IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ModelId' AND [object_id] = OBJECT_ID(N'Model_Funnel_Stage')) AND EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Model_Funnel')
	BEGIN
		UPDATE dbo.Model_Funnel_Stage
		SET ModelId=Joined_ModelId
		FROM
		(SELECT dbo.Model_Funnel.ModelId AS Joined_ModelId,dbo.Model_Funnel_Stage.ModelFunnelId AS Model_Stage_FunnelId FROM dbo.Model_Funnel INNER JOIN dbo.Model_Funnel_Stage ON dbo.Model_Funnel_Stage.ModelFunnelId = dbo.Model_Funnel.ModelFunnelId)Model_Funnel_Stage_Joined
		WHERE dbo.Model_Funnel_Stage.ModelFunnelId=Model_Stage_FunnelId

	END



IF EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Model_Funnel_Stage_Model_Funnel')
   AND parent_object_id = OBJECT_ID(N'dbo.Model_Funnel_Stage')
)
BEGIN
ALTER TABLE dbo.Model_Funnel_Stage DROP CONSTRAINT [FK_Model_Funnel_Stage_Model_Funnel]
END

COMMIT TRANSACTION

END TRY

BEGIN CATCH
	IF @@TRANCOUNT>0
	BEGIN
	ROLLBACK TRANSACTION
	END
END CATCH

Go
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ModelId' AND [object_id] = OBJECT_ID(N'Model_Funnel_Stage')) AND EXISTS(SELECT * FROM sys.columns WHERE [name] = N'AverageDealSize' AND [object_id] = OBJECT_ID(N'Model'))
BEGIN
IF (SELECT COUNT(ModelId) FROM dbo.Model_Funnel_Stage WHERE ModelId IS NOT NULL) > 0 AND (SELECT COUNT(AverageDealSize) FROM dbo.Model WHERE AverageDealSize IS NOT NULL) > 0
BEGIN
BEGIN TRY
BEGIN TRANSACTION
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Model_Funnel_Stage')
BEGIN
        IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ModelFunnelId' AND [object_id] = OBJECT_ID(N'Model_Funnel_Stage'))
	    BEGIN
		    ALTER TABLE dbo.Model_Funnel_Stage DROP COLUMN ModelFunnelId 
	    END
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Model_Funnel')
BEGIN
DROP TABLE dbo.Model_Funnel
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Funnel_Field')
BEGIN
DROP TABLE dbo.Funnel_Field
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Field')
BEGIN
DROP TABLE dbo.Field
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Funnel')
BEGIN
DROP TABLE dbo.Funnel
END

COMMIT TRANSACTION

END TRY

BEGIN CATCH
IF @@TRANCOUNT>0
BEGIN
ROLLBACK TRANSACTION
END

END CATCH
END
END

Go
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ModelId' AND [object_id] = OBJECT_ID(N'Model_Funnel_Stage')) AND EXISTS(SELECT * FROM sys.columns WHERE [name] = N'AverageDealSize' AND [object_id] = OBJECT_ID(N'Model'))
BEGIN
IF (SELECT COUNT(ModelId) FROM dbo.Model_Funnel_Stage WHERE ModelId IS NOT NULL) > 0 AND (SELECT COUNT(AverageDealSize) FROM dbo.Model WHERE AverageDealSize IS NOT NULL) > 0
BEGIN
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Model')
BEGIN
        IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'AverageDealSize' AND [object_id] = OBJECT_ID(N'Model'))
	    BEGIN
		    ALTER TABLE [Model] ALTER COLUMN AverageDealSize FLOAT NOT NULL
	    END
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Model_Funnel_Stage')
BEGIN
        IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ModelId' AND [object_id] = OBJECT_ID(N'Model_Funnel_Stage'))
	    BEGIN
		    ALTER TABLE [Model_Funnel_Stage] ALTER COLUMN ModelId INT NOT NULL
	    END
END
END
END

Go
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ModelId' AND [object_id] = OBJECT_ID(N'Model_Funnel_Stage')) AND EXISTS(SELECT * FROM sys.columns WHERE [name] = N'AverageDealSize' AND [object_id] = OBJECT_ID(N'Model'))
BEGIN
IF (SELECT COUNT(ModelId) FROM dbo.Model_Funnel_Stage WHERE ModelId IS NOT NULL) > 0 AND (SELECT COUNT(AverageDealSize) FROM dbo.Model WHERE AverageDealSize IS NOT NULL) > 0
BEGIN
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Model_Funnel_Stage')
BEGIN
        IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ModelFunnelStageId' AND [object_id] = OBJECT_ID(N'Model_Funnel_Stage'))
	    BEGIN
		    EXEC sp_rename 'dbo.Model_Funnel_Stage.ModelFunnelStageId','ModelStageId','COLUMN'
	    END
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Model_Funnel_Stage')
BEGIN
EXEC sp_rename 'dbo.Model_Funnel_Stage','Model_Stage'
END
END
END


GO
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ModelId' AND [object_id] = OBJECT_ID(N'Model_Funnel_Stage')) AND EXISTS(SELECT * FROM sys.columns WHERE [name] = N'AverageDealSize' AND [object_id] = OBJECT_ID(N'Model'))
BEGIN
IF (SELECT COUNT(ModelId) FROM dbo.Model_Funnel_Stage WHERE ModelId IS NOT NULL) > 0 AND (SELECT COUNT(AverageDealSize) FROM dbo.Model WHERE AverageDealSize IS NOT NULL) > 0
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Model_Stage_Model')
   AND parent_object_id = OBJECT_ID(N'dbo.Model_Stage')
)
BEGIN
ALTER TABLE [dbo].[Model_Stage]  WITH CHECK ADD  CONSTRAINT [FK_Model_Stage_Model] FOREIGN KEY([ModelId])
REFERENCES [dbo].[Model] ([ModelId])
END
END
END
GO


GO


---- Execute this script on MRP database
Declare @varEloqua varchar(50) ='Eloqua'
Declare @RawCnt int =0
Declare @rawId int =1
Declare @IntegrationTypeId int =0
Declare	@Attribute varchar(50)='Company Name'
Declare @AttributeType varchar(10)='textbox'
Declare @IsDeleted bit = 0
Declare @NewId int = 0

DECLARE @TempIntegrationType Table
(
		ID int IDENTITY(1, 1) primary key,
		IntegrationTypeId int
);

Insert Into @TempIntegrationType Select IntegrationTypeId from IntegrationType where Code =@varEloqua
Select @RawCnt = Count(ID) from @TempIntegrationType
While(@rawId <= @RawCnt)
BEGIN
	Select @IntegrationTypeId = IntegrationTypeId from @TempIntegrationType where ID=@rawId
	IF NOT Exists(Select 1 from IntegrationTypeAttribute where IntegrationTypeId=@IntegrationTypeId and Attribute=@Attribute and [AttributeType] = @AttributeType)
	Begin
		Insert Into IntegrationTypeAttribute(IntegrationTypeId,Attribute,AttributeType,IsDeleted) Values(@IntegrationTypeId,@Attribute,@AttributeType,@IsDeleted)
		Set @NewId = @@IDENTITY
		PRINT @NewId
		
		INSERT INTO IntegrationInstance_Attribute (IntegrationInstanceId,IntegrationTypeAttributeId,Value,CreatedDate,CreatedBy)
		SELECT IntegrationInstanceId,@NewId,Instance,CreatedDate,CreatedBy FROM IntegrationInstance WHERE IntegrationTypeId = @IntegrationTypeId

	End
	Set @rawId = @rawId + 1
END



GO

GO

---- ProgramOwnerChanged 
IF NOT EXISTS (SELECT TOP 1 NotificationInternalUseOnly FROM dbo.Notification WHERE NotificationInternalUseOnly=N'ProgramOwnerChanged')
BEGIN

INSERT dbo.Notification
        ( NotificationInternalUseOnly ,
          Title ,
          Description ,
          NotificationType ,
          EmailContent ,
          IsDeleted ,
          CreatedDate ,
          CreatedBy ,
          ModifiedDate ,
          ModifiedBy ,
          Subject
        )
VALUES  ( 'ProgramOwnerChanged' , -- NotificationInternalUseOnly - varchar(255)
          N'Program Owner Changed' , -- Title - nvarchar(255)
          N'When owner of program changed' , -- Description - nvarchar(4000)
          'CM' , -- NotificationType - char(2)
          N'Dear [NameToBeReplaced],<br><br>[ModifierName] has made you the owner of following program.<br><br><table><tr><td>Program</td><td>:</td><td>[programname]</td></tr><tr><td>Campaign</td><td>:</td><td>[campaignname]</td></tr><tr><td>Plan</td><td>:</td><td>[planname]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br>Thank You,<br>Bulldog Gameplan Admin' , -- EmailContent - nvarchar(4000)
          0 , -- IsDeleted - bit
          GETDATE() , -- CreatedDate - datetime
          '092F54DF-4C71-4F2F-9D21-0AE16155E5C1' , -- CreatedBy - uniqueidentifier,
		  NULL,
		  NULL,
          'Gameplan : Program owner has been changed'  -- Subject - varchar(255)
        )

END

------------ CampaignOwnerChanged
IF NOT EXISTS (SELECT TOP 1 NotificationInternalUseOnly FROM dbo.Notification WHERE NotificationInternalUseOnly=N'CampaignOwnerChanged')
BEGIN

INSERT dbo.Notification
        ( NotificationInternalUseOnly ,
          Title ,
          Description ,
          NotificationType ,
          EmailContent ,
          IsDeleted ,
          CreatedDate ,
          CreatedBy ,
          ModifiedDate ,
          ModifiedBy ,
          Subject
        )
VALUES  ( 'CampaignOwnerChanged' , -- NotificationInternalUseOnly - varchar(255)
          N'Campaign Owner Changed' , -- Title - nvarchar(255)
          N'When owner of Campaign changed' , -- Description - nvarchar(4000)
          'CM' , -- NotificationType - char(2)
          N'Dear [NameToBeReplaced],<br><br>[ModifierName] has made you the owner of following campaign.<br><br><table><tr><td>Campaign</td><td>:</td><td>[campaignname]</td></tr><tr><td>Plan</td><td>:</td><td>[planname]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br>Thank You,<br>Bulldog Gameplan Admin' , -- EmailContent - nvarchar(4000)
          0 , -- IsDeleted - bit
          GETDATE() , -- CreatedDate - datetime
          '092F54DF-4C71-4F2F-9D21-0AE16155E5C1' , -- CreatedBy - uniqueidentifier,
		  NULL,
		  NULL,
          'Gameplan : Campaign owner has been changed'  -- Subject - varchar(255)
        )

END

Go
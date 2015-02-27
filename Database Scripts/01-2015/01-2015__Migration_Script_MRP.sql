--Start 01/01/2015

-- ======================================================================================
-- Created By : Sohel Pathan
-- Created Date : 22/12/2014
-- Description : Make default entries for Gameplan DataType Pull for MQL 
-- ======================================================================================

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GameplanDataTypePull')
BEGIN
	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IntegrationType')
	BEGIN
		IF EXISTS (SELECT 1 FROM IntegrationType WHERE Code = 'Eloqua' AND IsDeleted = 0)
		BEGIN
			DECLARE @IntegrationTypeId INT = (SELECT IntegrationTypeId FROM IntegrationType WHERE Code = 'Eloqua' AND IsDeleted = 0)
			IF (@IntegrationTypeId > 0)
			BEGIN
				
				IF NOT EXISTS(SELECT 1 FROM GameplanDataTypePull WHERE IntegrationTypeId = @IntegrationTypeId AND ActualFieldName = 'MQLDate' AND [Type] = 'MQL' AND IsDeleted = 0)
				BEGIN
					INSERT INTO GameplanDataTypePull (IntegrationTypeId, ActualFieldName, DisplayFieldName, [Type], IsDeleted)
					VALUES(@IntegrationTypeId, 'MQLDate', 'MQL Date', 'MQL', 0)
				END

				IF NOT EXISTS(SELECT 1 FROM GameplanDataTypePull WHERE IntegrationTypeId = @IntegrationTypeId AND ActualFieldName = 'CampaignId' AND [Type] = 'MQL' AND IsDeleted = 0)
				BEGIN
					INSERT INTO GameplanDataTypePull (IntegrationTypeId, ActualFieldName, DisplayFieldName, [Type], IsDeleted)
					VALUES(@IntegrationTypeId, 'CampaignId', 'Last Eloqua Campaign Id', 'MQL', 0)
				END

				IF NOT EXISTS(SELECT 1 FROM GameplanDataTypePull WHERE IntegrationTypeId = @IntegrationTypeId AND ActualFieldName = 'ViewId' AND [Type] = 'MQL' AND IsDeleted = 0)
				BEGIN
					INSERT INTO GameplanDataTypePull (IntegrationTypeId, ActualFieldName, DisplayFieldName, [Type], IsDeleted)
					VALUES(@IntegrationTypeId, 'ViewId', 'View Id', 'MQL', 0)
				END

				IF NOT EXISTS(SELECT 1 FROM GameplanDataTypePull WHERE IntegrationTypeId = @IntegrationTypeId AND ActualFieldName = 'ListId' AND [Type] = 'MQL' AND IsDeleted = 0)
				BEGIN
					INSERT INTO GameplanDataTypePull (IntegrationTypeId, ActualFieldName, DisplayFieldName, [Type], IsDeleted)
					VALUES(@IntegrationTypeId, 'ListId', 'List Id', 'MQL', 0)
				END

			END
		END
	END
END


--End 01/01/2015

GO

--Start 01/02/2015

-- ======================================================================================
-- Created By : Sohel Pathan
-- Created Date : 02/01/2015
-- Description : Add email entry for Sync Notification Email in Notification table.
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Notification')
BEGIN

	IF NOT EXISTS (SELECT 1 FROM [Notification] WHERE NotificationInternalUseOnly = 'SyncIntegrationError')
	BEGIN

		INSERT INTO [Notification](NotificationInternalUseOnly, Title, [Description], NotificationType, EmailContent, IsDeleted, CreatedDate, CreatedBy, [Subject])
		SELECT TOP 1 
			'SyncIntegrationError',
			'Sync integration error email',
			'Sync integration error email',
			'CM',
			'Dear [NameToBeReplaced],
			<br><br>Below is the Sync integration summary for your latest sync:
			<br><br>[ErrorBody]
			<br><br>Thank You,<br>Bulldog Gameplan Admin',
			IsDeleted,
			CreatedDate,
			CreatedBy,
			'Gameplan : Sync Integration Summary'
		FROM [Notification] 
		WHERE IsDeleted = 0

	END

END

--End 01/02/2015

--Start 01/09/2015

/****** Object:  Table [dbo].[Client_Integration_Permission]    Script Date: 01/09/2015 4:36:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Client_Integration_Permission]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Client_Integration_Permission](
	[ClientIntegrationPermissionId] [int] IDENTITY(1,1) NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[IntegrationTypeId] [int] NOT NULL,
	[PermissionCode] [varchar](255) NOT NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_Client_Integration_Permission] PRIMARY KEY CLUSTERED 
(
	[ClientIntegrationPermissionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Client_Integration_Permission_IntegrationType]') AND parent_object_id = OBJECT_ID(N'[dbo].[Client_Integration_Permission]'))
ALTER TABLE [dbo].[Client_Integration_Permission]  WITH CHECK ADD  CONSTRAINT [FK_Client_Integration_Permission_IntegrationType] FOREIGN KEY([IntegrationTypeId])
REFERENCES [dbo].[IntegrationType] ([IntegrationTypeId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Client_Integration_Permission_IntegrationType]') AND parent_object_id = OBJECT_ID(N'[dbo].[Client_Integration_Permission]'))
ALTER TABLE [dbo].[Client_Integration_Permission] CHECK CONSTRAINT [FK_Client_Integration_Permission_IntegrationType]
GO

--End 01/09/2015
GO
-- Start 01/13/2015
-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 05/12/2014
-- Description : Custom naming: Campaign name structure
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='ClientTacticType')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Abbreviation' AND [object_id] = OBJECT_ID(N'ClientTacticType'))
	    BEGIN
		    ALTER TABLE [ClientTacticType] ADD Abbreviation NVARCHAR(255) NULL
	    END
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='MasterTacticType')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Abbreviation' AND [object_id] = OBJECT_ID(N'MasterTacticType'))
	    BEGIN
		    ALTER TABLE [MasterTacticType] ADD Abbreviation NVARCHAR(255) NULL
	    END
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='TacticType')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Abbreviation' AND [object_id] = OBJECT_ID(N'TacticType'))
	    BEGIN
		    ALTER TABLE [TacticType] ADD Abbreviation NVARCHAR(255) NULL
	    END
END
--End 01/13/2015
GO
--Start 01/23/2015

/* Execute this script on MRP database */

--------------- Start: Add Column 'Weightage' to Table [dbo].[CustomField_Entity] --------------- 
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'Weightage' AND [object_id] = OBJECT_ID(N'[dbo].[CustomField_Entity]'))
BEGIN
    ALTER TABLE [dbo].[CustomField_Entity]
	ADD Weightage tinyint NULL
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Attribute wise weight of entity.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'Weightage'
END
GO
IF (SELECT Count(*) FROM CustomField_Entity WHERE Weightage > 0) = 0
BEGIN
	Update [dbo].[CustomField_Entity] Set Weightage = 100
END
GO
--------------- End: Add Column 'Weightage' to Table [dbo].[CustomField_Entity] --------------- 

--------------- Start: Create New Table 'CustomField_Entity_StageWeight' to Database ---------------

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomField_Entity_StageWeight]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CustomField_Entity_StageWeight](
	[StageWeightId] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomFieldEntityId] [int] NOT NULL,
	[StageTitle] [nvarchar](50) NOT NULL,
	[Weightage] [tinyint] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_CustomField_Entity_StageWeight] PRIMARY KEY CLUSTERED 
(
	[StageWeightId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomField_Entity_StageWeight_CustomField_Entity_StageWeight]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomField_Entity_StageWeight]'))
ALTER TABLE [dbo].[CustomField_Entity_StageWeight]  WITH CHECK ADD  CONSTRAINT [FK_CustomField_Entity_StageWeight_CustomField_Entity_StageWeight] FOREIGN KEY([CustomFieldEntityId])
REFERENCES [dbo].[CustomField_Entity] ([CustomFieldEntityId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomField_Entity_StageWeight_CustomField_Entity_StageWeight]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomField_Entity_StageWeight]'))
ALTER TABLE [dbo].[CustomField_Entity_StageWeight] CHECK CONSTRAINT [FK_CustomField_Entity_StageWeight_CustomField_Entity_StageWeight]
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'CustomField_Entity_StageWeight', N'COLUMN',N'StageWeightId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity_StageWeight', @level2type=N'COLUMN',@level2name=N'StageWeightId'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'CustomField_Entity_StageWeight', N'COLUMN',N'CustomFieldEntityId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK- Refers to associated CustomFieldEntityId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity_StageWeight', @level2type=N'COLUMN',@level2name=N'CustomFieldEntityId'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'CustomField_Entity_StageWeight', N'COLUMN',N'StageTitle'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Title of Stage.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity_StageWeight', @level2type=N'COLUMN',@level2name=N'StageTitle'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'CustomField_Entity_StageWeight', N'COLUMN',N'Weightage'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Weight of stage for particular entity.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity_StageWeight', @level2type=N'COLUMN',@level2name=N'Weightage'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'CustomField_Entity_StageWeight', N'COLUMN',N'CreatedDate'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity_StageWeight', @level2type=N'COLUMN',@level2name=N'CreatedDate'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'CustomField_Entity_StageWeight', N'COLUMN',N'CreatedBy'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity_StageWeight', @level2type=N'COLUMN',@level2name=N'CreatedBy'
GO
--------------- End: Create New Table 'CustomField_Entity_StageWeight' to Database --------------- 

--------------- Start: Add Columns 'Description'&'ColorCode' to Table [dbo].[CustomFieldOption] ---------------
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'Description' AND [object_id] = OBJECT_ID(N'[dbo].[CustomFieldOption]'))
BEGIN
    ALTER TABLE [dbo].[CustomFieldOption]
	ADD [Description] nvarchar(4000) NULL
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Description of CustomField.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldOption', @level2type=N'COLUMN',@level2name=N'Description'
END
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'ColorCode' AND [object_id] = OBJECT_ID(N'[dbo].[CustomFieldOption]'))
BEGIN
    ALTER TABLE [dbo].[CustomFieldOption]
	ADD [ColorCode] nvarchar(10) NULL
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Color Code of CustomField.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldOption', @level2type=N'COLUMN',@level2name=N'ColorCode'
END
--------------- End: Add Columns 'Description'&'ColorCode' to Table [dbo].[CustomFieldOption] ---------------

Go
/* Execute this script on MRP database */

/* Create Temporary Client & Attributes table */
IF OBJECT_ID('tempdb..#Client') IS NOT NULL DROP TABLE #Client
GO
Create TABLE #Client 
    (
		ID int IDENTITY(1, 1) primary key,
		ClientId uniqueidentifier
    )

IF OBJECT_ID('tempdb..#Attributes') IS NOT NULL DROP TABLE #Attributes
GO
Create TABLE #Attributes
    (
		ID int IDENTITY(1, 1) primary key,
		Attribute varchar(50)
    )
Go
Declare @lblVertical varchar(50) = 'Vertical'
Declare @lblAudience varchar(50) = 'Audience'
Declare @lblBusinessUnit varchar(50) = 'BusinessUnit'
Declare @lblGeography varchar(50) = 'Geography'

Insert Into #Attributes Values(@lblVertical)
Insert Into #Attributes Values(@lblAudience)
Insert Into #Attributes Values(@lblBusinessUnit)
Insert Into #Attributes Values(@lblGeography)

/* Start - Declare local variables */
Declare @AttrCntr int =1
Declare @Attrtotalrows int = 0
Declare @Cntr int = 1
Declare @totalrows int = 0
Declare @ClientId uniqueidentifier
Declare @CustomFieldID int=0
------ CustomField Table record -----
Declare @CustomFieldName varchar(50) = ''
Declare @CustomFieldType int=0
Declare @Description nvarchar(max)=''
Declare @IsRequired bit='1'
Declare @EntityType varchar(500)='Tactic'
Declare @IsDeleted bit='0'
Declare @CreatedBy uniqueidentifier='A7B9744A-CDC4-4CEA-BF21-2E992EEF5055'
Declare @IsDisplayforFilter bit ='1'
Declare @AbbriviationForMulti varchar(10)='MULTI'
Declare @customIsDefault bit =0
/* End - Declare local variables */

BEGIN
	SELECT Top 1 @CustomFieldType = CustomFieldTypeId FROM CustomFieldType WHERE Name = 'DropDownList'
	/* Insert CustomField & Values to CustomField and CustomFieldOption table respectively based on ClientId */
	Select @Attrtotalrows = COUNT(*) from #Attributes
	While(@AttrCntr <= @Attrtotalrows)
	Begin

	/*Reset Counter variable on each iterate*/
	 Set @Cntr = 1
	 Set @totalrows = 0

	 /* Get Attribute name to insert data dynamically foreach CustomField(i.e Vertical,Audience,BusinessUnit,Geography) */
	 Select @CustomFieldName = Attribute from #Attributes where ID = @AttrCntr
	 Truncate Table #Client

	 /* Insert ClientId data to Temptable based on CustomField */
	 IF(@CustomFieldName = @lblVertical)
	 BEGIN
		/* Insert Distinct ClientId from Vertical table to Temporary table */
		Insert Into #Client(ClientId) Select Distinct ClientId from Vertical Group by ClientId
	 END
	 Else IF(@CustomFieldName = @lblAudience)
	 BEGIN
		/* Insert Distinct ClientId from Vertical table to Temporary table */
		Insert Into #Client(ClientId) Select Distinct ClientId from Audience Group by ClientId
	 END
	 Else IF(@CustomFieldName = @lblBusinessUnit)
	 BEGIN
		/* Insert Distinct ClientId from Vertical table to Temporary table */
		Insert Into #Client(ClientId) Select Distinct ClientId from BusinessUnit Group by ClientId
	 END
	 Else IF(@CustomFieldName = @lblGeography)
	 BEGIN
		/* Insert Distinct ClientId from Vertical table to Temporary table */
		Insert Into #Client(ClientId) Select Distinct ClientId from [Geography] Group by ClientId
	 END
	 
	/* Insert CustomField & Values to CustomFieldOption table based on ClientId */
	Select @totalrows = COUNT(*) from #Client
	While(@Cntr <= @totalrows)
	Begin
		/* Get ClientId from temp table */
		Select @ClientId = ClientId from #Client where ID = @Cntr
		
		/* Check whether Customfield exist or not in CustomField Table */
		Declare @IsCustomFieldExist int = 0
		IF(@CustomFieldName = @lblBusinessUnit)
		BEGIN
			IF Not Exists(Select 1 from CustomField where Name IN (@CustomFieldName,'Business Unit') and CustomFieldTypeId=@CustomFieldType and EntityType=@EntityType and ClientId = @ClientId)
			BEGIN
				SET @IsCustomFieldExist = 1
			END
		END
		ELSE
		BEGIN
			IF Not Exists(Select 1 from CustomField where Name = @CustomFieldName and CustomFieldTypeId=@CustomFieldType and EntityType=@EntityType and ClientId = @ClientId)
			BEGIN
				SET @IsCustomFieldExist = 1
			END
		END	
		/* Add new record to CustomField table If record does not exist in table*/
		IF (@IsCustomFieldExist > 0)
		Begin
			Insert Into CustomField ([Name],[CustomFieldTypeId],[Description],[IsRequired],[EntityType],[ClientId],[IsDeleted],[CreatedDate],[CreatedBy],[ModifiedDate],[ModifiedBy],[IsDisplayForFilter]) values(@CustomFieldName,@CustomFieldType,@Description,@IsRequired,@EntityType,@ClientId,@IsDeleted,GetDate(),CONVERT(uniqueidentifier,@CreatedBy),Null,Null,@IsDisplayforFilter)
			
			/* Retrieve last inserted CustomFieldId on CustomField table*/
			Set @CustomFieldID = @@IDENTITY
			
			IF(@CustomFieldName = @lblVertical)
			BEGIN
				/* Add new record to CustomFieldOption table If record does not exist in table*/
				IF Not Exists(Select 1 from CustomFieldOption where CustomFieldId=@CustomFieldID and Value IN (Select Title from Vertical where ClientId = @ClientId))
				Begin
					Insert Into CustomFieldOption(CustomFieldId,Value,CreatedDate,CreatedBy,Abbreviation,[Description],ColorCode) Select @CustomFieldID,Title,CreatedDate,CreatedBy,Abbreviation,[Description],ColorCode from Vertical where ClientId = @ClientId
				End
			END
			Else IF(@CustomFieldName = @lblAudience)
			BEGIN
				/* Add new record to CustomFieldOption table If record does not exist in table*/
				IF Not Exists(Select 1 from CustomFieldOption where CustomFieldId=@CustomFieldID and Value IN (Select Title from Audience where ClientId = @ClientId))
				Begin
					Insert Into CustomFieldOption(CustomFieldId,Value,CreatedDate,CreatedBy,Abbreviation,[Description],ColorCode) Select @CustomFieldID,Title,CreatedDate,CreatedBy,Abbreviation,[Description],ColorCode from Audience where ClientId = @ClientId
				End
			END
			Else IF(@CustomFieldName = @lblBusinessUnit)
			BEGIN
				/* Add new record to CustomFieldOption table If record does not exist in table*/
				IF Not Exists(Select 1 from CustomFieldOption where CustomFieldId=@CustomFieldID and Value IN (Select Title from BusinessUnit where ClientId = @ClientId))
				Begin
					Insert Into CustomFieldOption(CustomFieldId,Value,CreatedDate,CreatedBy,Abbreviation,[Description],ColorCode) Select @CustomFieldID,Title,CreatedDate,CreatedBy,Abbreviation,[Description],ColorCode from BusinessUnit where ClientId = @ClientId
				End
			END
			Else IF(@CustomFieldName = @lblGeography)
			BEGIN
				/* Add new record to CustomFieldOption table If record does not exist in table*/
				IF Not Exists(Select 1 from CustomFieldOption where CustomFieldId=@CustomFieldID and Value IN (Select Title from [Geography] where ClientId = @ClientId))
				Begin
					Insert Into CustomFieldOption(CustomFieldId,Value,CreatedDate,CreatedBy,Abbreviation,[Description],ColorCode) Select @CustomFieldID,Title,CreatedDate,CreatedBy,Abbreviation,NULL,NULL from [Geography] where ClientId = @ClientId
				End
			END
		End
		Set @Cntr = @Cntr + 1
	End
	Set @AttrCntr = @AttrCntr + 1
	END
END

DROP TABLE #Client
DROP TABLE #Attributes
Go

/* Execute this script on MRP database */

/* Start - Declare local variables */
Declare @PlanTacticId int = 0		
Declare @Tac_VerticalId int = 0
Declare @Tac_AudienceId int = 0
Declare @Tac_GeographyId uniqueidentifier
Declare @Tac_BusinessUnitId uniqueidentifier
Declare @Tac_CreatedDate datetime
Declare @Tac_CreatedBy uniqueidentifier
Declare @Vertical_Title nvarchar(256)=''
Declare @Audience_Title nvarchar(256)=''
Declare @BusinessUnit_Title nvarchar(256)=''
Declare @Geography_Title nvarchar(256)=''
Declare @Ver_ClientID uniqueidentifier
Declare @Aud_ClientID uniqueidentifier
Declare @Bus_ClientID uniqueidentifier
Declare @Geo_ClientID uniqueidentifier
Declare @Ver_CustomFieldID int=0
Declare @Aud_CustomFieldID int=0
Declare @Bus_CustomFieldID int=0
Declare @Geo_CustomFieldID int=0
Declare @Tac_EntityType varchar(50)='Tactic'
Declare @Drpdwn_CustomFieldTypeID int=0
Declare @Ver_CustomFieldOptionID int = 0
Declare @Aud_CustomFieldOptionID int = 0
Declare @Bus_CustomFieldOptionID int = 0
Declare @Geo_CustomFieldOptionID int = 0
Declare @lblVertical varchar(50) = 'Vertical'
Declare @lblAudience varchar(50) = 'Audience'
Declare @lblBusinessUnit varchar(50) = 'BusinessUnit'
Declare @lblGeography varchar(50) = 'Geography'
/* End - Declare local variables */

/* Get CustomfieldTypeId for 'Dropdownlist' name*/
Select Top 1 @Drpdwn_CustomFieldTypeID=CustomFieldTypeId from CustomFieldType where Name='DropDownList'

/* Create cursor for Plan_Campaign_Program_Tactic Table*/
DECLARE CursorTactic CURSOR FOR  
SELECT PlanTacticId,VerticalId,AudienceId,BusinessUnitId,GeographyId,CreatedDate,CreatedBy
FROM dbo.Plan_Campaign_Program_Tactic
WHERE IsDeleted='0'

OPEN CursorTactic 
FETCH NEXT FROM CursorTactic INTO @PlanTacticId,@Tac_VerticalId,@Tac_AudienceId,@Tac_BusinessUnitId,@Tac_GeographyId,@Tac_CreatedDate,@Tac_CreatedBy

WHILE @@FETCH_STATUS = 0   
BEGIN   
		/* Get Title & ClientId from CustomFieldMasterTable(i.e Vertical,Audience,Geography,BusinessUnit) based on Tactic table Customfield(i.e VerticalId,AudienceId,GeographyId,BusinessUnitId)*/
       Select @Vertical_Title=Title,@Ver_ClientID=ClientID from Vertical where VerticalID=@Tac_VerticalId and IsDeleted='0'
	   Select @Audience_Title=Title,@Aud_ClientID=ClientID from Audience where AudienceId=@Tac_AudienceId and IsDeleted='0'
	   Select @BusinessUnit_Title=Title,@Bus_ClientID=ClientID from BusinessUnit where BusinessUnitId=@Tac_BusinessUnitId and IsDeleted='0'
	   Select @Geography_Title=Title,@Geo_ClientID=ClientID from [Geography] where GeographyId=@Tac_GeographyId and IsDeleted='0'
	   
	   
	   /* Get CustomFieldID from CustomField Master table based on ClientId,EntityType,CustomFieldTypeId */
	   Select Top 1 @Ver_CustomFieldID=CustomFieldID from CustomField where Name =@lblVertical and ClientID = @Ver_ClientID and IsDeleted ='0' and EntityType=@Tac_EntityType and CustomFieldTypeId = @Drpdwn_CustomFieldTypeID
	   Select Top 1 @Aud_CustomFieldID=CustomFieldID from CustomField where Name =@lblAudience and ClientID = @Aud_ClientID and IsDeleted ='0' and EntityType=@Tac_EntityType and CustomFieldTypeId = @Drpdwn_CustomFieldTypeID
	   Select Top 1 @Bus_CustomFieldID=CustomFieldID from CustomField where Name =@lblBusinessUnit and ClientID = @Bus_ClientID and IsDeleted ='0' and EntityType=@Tac_EntityType and CustomFieldTypeId = @Drpdwn_CustomFieldTypeID
	   Select Top 1 @Geo_CustomFieldID=CustomFieldID from CustomField where Name =@lblGeography and ClientID = @Geo_ClientID and IsDeleted ='0' and EntityType=@Tac_EntityType and CustomFieldTypeId = @Drpdwn_CustomFieldTypeID
	   
	   /* Get CustomFieldOptionID from CustomFieldOption Master table based on CustomFieldId,Value */
	   Select Top 1 @Ver_CustomFieldOptionID = CustomFieldOptionID from CustomFieldOption where CustomFieldId=@Ver_CustomFieldID and Value=@Vertical_Title
	   Select Top 1 @Aud_CustomFieldOptionID = CustomFieldOptionID from CustomFieldOption where CustomFieldId=@Aud_CustomFieldID and Value=@Audience_Title
	   Select Top 1 @Bus_CustomFieldOptionID = CustomFieldOptionID from CustomFieldOption where CustomFieldId=@Bus_CustomFieldID and Value=@BusinessUnit_Title
	   Select Top 1 @Geo_CustomFieldOptionID = CustomFieldOptionID from CustomFieldOption where CustomFieldId=@Geo_CustomFieldID and Value=@Geography_Title


	   /* If CustomFieldEntity record does not exist then added to table. */
	   IF NOT Exists(Select 1 from CustomField_Entity where EntityId=@PlanTacticId and CustomFieldId=@Ver_CustomFieldID and Value = convert(nvarchar(256),@Ver_CustomFieldOptionID)) AND (@Ver_CustomFieldID > 0)
	   Begin
		Insert Into CustomField_Entity(EntityId,CustomFieldId,Value,CreatedDate,CreatedBy,Weightage) Values(@PlanTacticId,@Ver_CustomFieldID,convert(nvarchar(256),@Ver_CustomFieldOptionID),@Tac_CreatedDate,@Tac_CreatedBy,100)
	   End
	   IF NOT Exists(Select 1 from CustomField_Entity where EntityId=@PlanTacticId and CustomFieldId=@Aud_CustomFieldID and Value = convert(nvarchar(256),@Aud_CustomFieldOptionID)) AND (@Aud_CustomFieldID > 0)
	   Begin
		Insert Into CustomField_Entity(EntityId,CustomFieldId,Value,CreatedDate,CreatedBy,Weightage) Values(@PlanTacticId,@Aud_CustomFieldID,convert(nvarchar(256),@Aud_CustomFieldOptionID),@Tac_CreatedDate,@Tac_CreatedBy,100)
	   End
	   IF NOT Exists(Select 1 from CustomField_Entity where EntityId=@PlanTacticId and CustomFieldId=@Bus_CustomFieldID and Value = convert(nvarchar(256),@Bus_CustomFieldOptionID)) AND (@Bus_CustomFieldID > 0)
	   Begin
		Insert Into CustomField_Entity(EntityId,CustomFieldId,Value,CreatedDate,CreatedBy,Weightage) Values(@PlanTacticId,@Bus_CustomFieldID,convert(nvarchar(256),@Bus_CustomFieldOptionID),@Tac_CreatedDate,@Tac_CreatedBy,100)
	   End
	   IF NOT Exists(Select 1 from CustomField_Entity where EntityId=@PlanTacticId and CustomFieldId=@Geo_CustomFieldID and Value = convert(nvarchar(256),@Geo_CustomFieldOptionID)) AND (@Geo_CustomFieldID > 0)
	   Begin
		Insert Into CustomField_Entity(EntityId,CustomFieldId,Value,CreatedDate,CreatedBy,Weightage) Values(@PlanTacticId,@Geo_CustomFieldID,convert(nvarchar(256),@Geo_CustomFieldOptionID),@Tac_CreatedDate,@Tac_CreatedBy,100)
	   End

       FETCH NEXT FROM CursorTactic INTO  @PlanTacticId,@Tac_VerticalId,@Tac_AudienceId,@Tac_BusinessUnitId,@Tac_GeographyId,@Tac_CreatedDate,@Tac_CreatedBy
END   

CLOSE CursorTactic   
DEALLOCATE CursorTactic

GO


/* Execute this script on MRP database */

/****** Object:  Table [dbo].[CustomRestriction]    Script Date: 01/15/2015 12:56:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomRestriction]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CustomRestriction](
	[CustomRestrictionId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[CustomFieldId] [int] NOT NULL,
	[CustomFieldOptionId] [int] NOT NULL,
	[Permission] [smallint] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_CustomRestriction] PRIMARY KEY CLUSTERED 
(
	[CustomRestrictionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_CustomRestriction_Permission]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[CustomRestriction] ADD  CONSTRAINT [DF_CustomRestriction_Permission]  DEFAULT ((0)) FOR [Permission]
END

GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomRestriction_CustomField]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomRestriction]'))
ALTER TABLE [dbo].[CustomRestriction]  WITH CHECK ADD  CONSTRAINT [FK_CustomRestriction_CustomField] FOREIGN KEY([CustomFieldId])
REFERENCES [dbo].[CustomField] ([CustomFieldId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomRestriction_CustomField]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomRestriction]'))
ALTER TABLE [dbo].[CustomRestriction] CHECK CONSTRAINT [FK_CustomRestriction_CustomField]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomRestriction_CustomFieldOption]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomRestriction]'))
ALTER TABLE [dbo].[CustomRestriction]  WITH CHECK ADD  CONSTRAINT [FK_CustomRestriction_CustomFieldOption] FOREIGN KEY([CustomFieldOptionId])
REFERENCES [dbo].[CustomFieldOption] ([CustomFieldOptionId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomRestriction_CustomFieldOption]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomRestriction]'))
ALTER TABLE [dbo].[CustomRestriction] CHECK CONSTRAINT [FK_CustomRestriction_CustomFieldOption]
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'CustomRestriction', N'COLUMN',N'CustomRestrictionId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomRestriction', @level2type=N'COLUMN',@level2name=N'CustomRestrictionId'
GO

GO

-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 01/23/2015
-- Description : Naming Convention: Multi selection option
-- ======================================================================================


		IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'AbbreviationForMulti' AND [object_id] = OBJECT_ID(N'CustomField'))
	    BEGIN
		    ALTER TABLE [CustomField] ADD AbbreviationForMulti NVARCHAR(255) NOT NULL DEFAULT 'MULTI'
	    END
		

GO

-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 01/23/2015
-- Description : Multi-select : Inspect popup - review tab icons for veritcal, audience etc.
-- ======================================================================================

		IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'IsDefault' AND [object_id] = OBJECT_ID(N'CustomField'))
	    BEGIN
		    ALTER TABLE [CustomField] ADD IsDefault bit NOT NULL DEFAULT 0
	    END

GO

--End 01/23/2015

--Start 01/27/2015


---- set Client Id in Model table and remove BU Id column


IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'ClientId' AND [object_id] = OBJECT_ID(N'Model'))
BEGIN
    -- Column Exists
	ALTER TABLE dbo.Model ADD ClientId UNIQUEIDENTIFIER
END
GO
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'BusinessUnitId' AND [object_id] = OBJECT_ID(N'Model'))
BEGIN
    -- Column Exists
	EXECUTE('UPDATE dbo.Model SET ClientId=bu.ClientId FROM dbo.BusinessUnit AS bu WHERE dbo.Model.BusinessUnitId=bu.BusinessUnitId') 
END
GO
ALTER TABLE dbo.Model ALTER COLUMN ClientId UNIQUEIDENTIFIER NOT NULL
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Model_BusinessUnit]') AND parent_object_id = OBJECT_ID(N'[dbo].[Model]'))
ALTER TABLE [dbo].[Model] DROP CONSTRAINT [FK_Model_BusinessUnit]
GO
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'BusinessUnitId' AND [object_id] = OBJECT_ID(N'Model'))
BEGIN
    -- Column Exists
	ALTER TABLE dbo.Model DROP COLUMN BusinessUnitId
END


Go

----- Remove Fk and allowed NULL values

BEGIN TRANSACTION DeleteVerAudGeoBuIds

--------------Tactic Table
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Tactic_Audience]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic]'))
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] DROP CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Audience]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Tactic_BusinessUnit]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic]'))
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] DROP CONSTRAINT [FK_Plan_Campaign_Program_Tactic_BusinessUnit]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Tactic_Geography]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic]'))
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] DROP CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Geography]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Tactic_Vertical]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic]'))
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] DROP CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Vertical]

------------
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'VerticalId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
	ALTER TABLE dbo.Plan_Campaign_Program_Tactic ALTER COLUMN VerticalId INTEGER NULL
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'BusinessUnitId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
	ALTER TABLE dbo.Plan_Campaign_Program_Tactic ALTER COLUMN BusinessUnitId UNIQUEIDENTIFIER NULL
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'GeographyId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
	ALTER TABLE dbo.Plan_Campaign_Program_Tactic ALTER COLUMN GeographyId UNIQUEIDENTIFIER NULL
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'AudienceId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
	ALTER TABLE dbo.Plan_Campaign_Program_Tactic ALTER COLUMN AudienceId INTEGER NULL
END

------------------ Program Table
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Audience]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program]'))
ALTER TABLE [dbo].[Plan_Campaign_Program] DROP CONSTRAINT [FK_Plan_Campaign_Program_Audience]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Geography]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program]'))
ALTER TABLE [dbo].[Plan_Campaign_Program] DROP CONSTRAINT [FK_Plan_Campaign_Program_Geography]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Vertical]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program]'))
ALTER TABLE [dbo].[Plan_Campaign_Program] DROP CONSTRAINT [FK_Plan_Campaign_Program_Vertical]

------------
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'VerticalId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program'))
BEGIN	
	ALTER TABLE dbo.Plan_Campaign_Program ALTER COLUMN VerticalId INTEGER NULL
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'GeographyId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program'))
BEGIN	
	ALTER TABLE dbo.Plan_Campaign_Program ALTER COLUMN GeographyId UNIQUEIDENTIFIER NULL
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'AudienceId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program'))
BEGIN
	ALTER TABLE dbo.Plan_Campaign_Program ALTER COLUMN AudienceId INTEGER NULL
END


----------------Campaign Table
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Audience]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign]'))
ALTER TABLE [dbo].[Plan_Campaign] DROP CONSTRAINT [FK_Plan_Campaign_Audience]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Geography]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign]'))
ALTER TABLE [dbo].[Plan_Campaign] DROP CONSTRAINT [FK_Plan_Campaign_Geography]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Vertical]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign]'))
ALTER TABLE [dbo].[Plan_Campaign] DROP CONSTRAINT [FK_Plan_Campaign_Vertical]

---------------
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'VerticalId' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN	
	ALTER TABLE dbo.Plan_Campaign ALTER COLUMN VerticalId INTEGER NULL
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'GeographyId' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN	
	ALTER TABLE dbo.Plan_Campaign ALTER COLUMN GeographyId UNIQUEIDENTIFIER NULL
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'AudienceId' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN
	ALTER TABLE dbo.Plan_Campaign ALTER COLUMN AudienceId INTEGER NULL
END


-------- Improvement Tactic table
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Improvement_Campaign_Program_Tactic_BusinessUnit]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Improvement_Campaign_Program_Tactic]'))
ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] DROP CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_BusinessUnit]

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'BusinessUnitId' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program_Tactic'))
BEGIN
	ALTER TABLE dbo.Plan_Improvement_Campaign_Program_Tactic ALTER COLUMN BusinessUnitId UNIQUEIDENTIFIER NULL
END

COMMIT TRANSACTION DeleteVerAudGeoBuIds

GO



--END 01/27/2015
GO
-- Start 01/29/2015

-- ======================================================================================
-- Created By : Sohel Pathan
-- Created Date : 29/01/2015
-- Description : Insert Tactic Type data type entry into GameplanDataType Table
-- ======================================================================================

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GameplanDataType') AND
	EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IntegrationType')
BEGIN

	IF NOT EXISTS (SELECT 1 FROM GameplanDataType WHERE TableName = 'Plan_Campaign_Program_Tactic' AND IsDeleted = 0 AND ActualFieldName = 'TacticType' AND DisplayFieldName = 'Tactic Type')
	BEGIN

		INSERT INTO GameplanDataType(IntegrationTypeId, TableName, ActualFieldName, DisplayFieldName, IsGet, IsDeleted, IsImprovement)
		SELECT
		IntegrationTypeId, 'Plan_Campaign_Program_Tactic', 'TacticType', 'Tactic Type', 0, 0, 0
		FROM IntegrationType WHERE IsDeleted = 0
	
	END

END

GO

-- ======================================================================================
-- Created By : Sohel Pathan
-- Created Date : 29/01/2015
-- Description : Update mapping for PL ticket #1142 - Multi select: Changes to integration logic
-- ======================================================================================

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IntegrationInstanceDataTypeMapping') AND
	EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IntegrationInstance') AND
	EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GameplanDataType')
BEGIN

	--============================================================= Vertical ============================================================================
	BEGIN TRY
	
	BEGIN TRANSACTION Vertical
	IF EXISTS (SELECT GameplanDataTypeId FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('VerticalId'))
	BEGIN
		UPDATE IIM SET IIM.CustomFieldId = C.CustomFieldId, IIM.GameplanDataTypeId = NULL
		--SELECT IIM.*, II.ClientId, C.CustomFieldId 
		FROM IntegrationInstanceDataTypeMapping IIM
		INNER JOIN IntegrationInstance II ON II.IntegrationInstanceId = IIM.IntegrationInstanceId
		INNER JOIN CustomField C ON C.Name = 'Vertical' AND C.ClientId = II.ClientId
		WHERE IIM.GameplanDataTypeId IN (
											SELECT GameplanDataTypeId 
											FROM GameplanDataType 
											WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('VerticalId')
										) 
			AND IIM.GameplanDataTypeId IS NOT NULL AND IIM.CustomFieldId IS NULL

		DELETE FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('VerticalId')
	END
	
	IF(@@ERROR > 0)
	BEGIN
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY()
		ROLLBACK TRANSACTION Vertical
		END
	ELSE
	BEGIN
		COMMIT TRANSACTION Vertical
	END

	END TRY
	BEGIN CATCH
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY(), 'Vertical'
		ROLLBACK TRANSACTION Vertical
	END CATCH

	--============================================================= Audience ============================================================================
	BEGIN TRY
	
	BEGIN TRANSACTION Audience
	IF EXISTS (SELECT GameplanDataTypeId FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('AudienceId'))
	BEGIN
		UPDATE IIM SET IIM.CustomFieldId = C.CustomFieldId, IIM.GameplanDataTypeId = NULL
		--SELECT IIM.*, II.ClientId, C.CustomFieldId 
		FROM IntegrationInstanceDataTypeMapping IIM
		INNER JOIN IntegrationInstance II ON II.IntegrationInstanceId = IIM.IntegrationInstanceId
		INNER JOIN CustomField C ON C.Name = 'Audience' AND C.ClientId = II.ClientId
		WHERE IIM.GameplanDataTypeId IN (
											SELECT GameplanDataTypeId 
											FROM GameplanDataType 
											WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('AudienceId')
										) 
			AND IIM.GameplanDataTypeId IS NOT NULL AND IIM.CustomFieldId IS NULL

		DELETE FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('AudienceId')
	END
	
	IF(@@ERROR > 0)
	BEGIN
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY()
		ROLLBACK TRANSACTION Audience
		END
	ELSE
	BEGIN
		COMMIT TRANSACTION Audience
	END

	END TRY
	BEGIN CATCH
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY(), 'Audience'
		ROLLBACK TRANSACTION Audience
	END CATCH

	--============================================================= Geography ============================================================================
	BEGIN TRY
	
	BEGIN TRANSACTION Geo
	IF EXISTS (SELECT GameplanDataTypeId FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('GeographyId'))
	BEGIN
		UPDATE IIM SET IIM.CustomFieldId = C.CustomFieldId, IIM.GameplanDataTypeId = NULL
		--SELECT IIM.*, II.ClientId, C.CustomFieldId 
		FROM IntegrationInstanceDataTypeMapping IIM
		INNER JOIN IntegrationInstance II ON II.IntegrationInstanceId = IIM.IntegrationInstanceId
		INNER JOIN CustomField C ON C.Name = 'Geography' AND C.ClientId = II.ClientId
		WHERE IIM.GameplanDataTypeId IN (
											SELECT GameplanDataTypeId 
											FROM GameplanDataType 
											WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('GeographyId')
										) 
			AND IIM.GameplanDataTypeId IS NOT NULL AND IIM.CustomFieldId IS NULL

		DELETE FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('GeographyId')
	END
	
	IF(@@ERROR > 0)
	BEGIN
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY()
		ROLLBACK TRANSACTION Geo
		END
	ELSE
	BEGIN
		COMMIT TRANSACTION Geo
	END

	END TRY
	BEGIN CATCH 
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY(), 'Geography'
		ROLLBACK TRANSACTION Geo
	END CATCH

	--============================================================= BusinessUnit ============================================================================
	BEGIN TRY
	
	BEGIN TRANSACTION BusinessUnit
	IF EXISTS (SELECT GameplanDataTypeId FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('BusinessUnitId'))
	BEGIN
		UPDATE IIM SET IIM.CustomFieldId = C.CustomFieldId, IIM.GameplanDataTypeId = NULL
		--SELECT IIM.*, II.ClientId, C.CustomFieldId 
		FROM IntegrationInstanceDataTypeMapping IIM
		INNER JOIN IntegrationInstance II ON II.IntegrationInstanceId = IIM.IntegrationInstanceId
		INNER JOIN CustomField C ON C.Name = 'BusinessUnit' AND C.ClientId = II.ClientId
		WHERE IIM.GameplanDataTypeId IN (
											SELECT GameplanDataTypeId 
											FROM GameplanDataType 
											WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('BusinessUnitId')
										) 
			AND IIM.GameplanDataTypeId IS NOT NULL AND IIM.CustomFieldId IS NULL

		DELETE FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('BusinessUnitId')
	END
	
	IF(@@ERROR > 0)
	BEGIN
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY()
		ROLLBACK TRANSACTION BusinessUnit
		END
	ELSE
	BEGIN
		COMMIT TRANSACTION BusinessUnit
	END

	END TRY
	BEGIN CATCH
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY(), 'BusinessUnit'
		ROLLBACK TRANSACTION BusinessUnit
	END CATCH
END

GO
-- END 01/29/2015
GO

-- Start 02/02/2015

-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 02/02/2015
-- Description : Update CampaignNameConvention table in MRP database as per business unit,verticals,Geography and Audience fields in CustomField table
-- ======================================================================================



BEGIN TRY
BEGIN TRANSACTION 
   DECLARE @clientId UNIQUEIDENTIFIER
   DECLARE @CreatedBy UNIQUEIDENTIFIER
   DECLARE @BusinessUnitCustomFieldId INT
   DECLARE @AudienceCustomFieldId INT
   DECLARE @GeographyUnitCustomFieldId INT
   DECLARE @VerticalCustomFieldId INT
   DECLARE @sequence INT

   DECLARE UniqueClientForCustomNaming CURSOR
   
		STATIC FOR
		SELECT DISTINCT(ClientId) FROM dbo.CampaignNameConvention

 
   OPEN UniqueClientForCustomNaming

	   IF @@CURSOR_ROWS>0
	   BEGIN

		   FETCH NEXT FROM UniqueClientForCustomNaming INTO @clientId
			   WHILE @@FETCH_STATUS=0
			   BEGIN

					 --Business unit section
					 SELECT @BusinessUnitCustomFieldId=CustomFieldId,@CreatedBy=CreatedBy  FROM dbo.CustomField WHERE ClientId=@clientId AND Name IN ('BusinessUnit','Business Unit')
					 SELECT @sequence=Sequence FROM dbo.CampaignNameConvention WHERE ClientId=@clientId AND TableName IN ('BusinessUnit','Business Unit')
					 IF @BusinessUnitCustomFieldId>0 AND @sequence>0
					 BEGIN
					 
					 INSERT INTO dbo.CampaignNameConvention
					         (TableName,  CustomFieldId, Sequence, ClientId, CreatedDate,
					          CreatedBy, IsDeleted)
					 VALUES
					         (N'CustomField', @BusinessUnitCustomFieldId,@sequence,@clientId,GETDATE(), @CreatedBy,0)

					 END	
					 SELECT @sequence=0

					 --Business unit section

					 --Audience unit section
					 SELECT @AudienceCustomFieldId=CustomFieldId,@CreatedBy=CreatedBy  FROM dbo.CustomField WHERE ClientId=@clientId AND Name='Audience'
					 SELECT @sequence=Sequence FROM dbo.CampaignNameConvention WHERE ClientId=@clientId AND TableName='Audience'
					 IF @AudienceCustomFieldId>0 AND @sequence>0
					 BEGIN
					 
					 INSERT INTO dbo.CampaignNameConvention
					         (TableName,  CustomFieldId, Sequence, ClientId, CreatedDate,
					          CreatedBy, IsDeleted)
					 VALUES
					         (N'CustomField', @AudienceCustomFieldId,@sequence,@clientId,GETDATE(), @CreatedBy,0)

					 END	
					 SELECT @sequence=0
					 --Audience unit section

					  --Geography unit section
					 SELECT @GeographyUnitCustomFieldId=CustomFieldId,@CreatedBy=CreatedBy  FROM dbo.CustomField WHERE ClientId=@clientId AND Name='Geography'
					 SELECT @sequence=Sequence FROM dbo.CampaignNameConvention WHERE ClientId=@clientId AND TableName='Geography'
					 IF @GeographyUnitCustomFieldId>0 AND @sequence>0
					 BEGIN
					 
					 INSERT INTO dbo.CampaignNameConvention
					         (TableName,  CustomFieldId, Sequence, ClientId, CreatedDate,
					          CreatedBy, IsDeleted)
					 VALUES
					         (N'CustomField', @GeographyUnitCustomFieldId,@sequence,@clientId,GETDATE(), @CreatedBy,0)

					 END	
					 SELECT @sequence=0
					 --Geography unit section

					 --Vertical unit section
					 SELECT @VerticalCustomFieldId=CustomFieldId,@CreatedBy=CreatedBy  FROM dbo.CustomField WHERE ClientId=@clientId AND Name='Vertical'
					 SELECT @sequence=Sequence FROM dbo.CampaignNameConvention WHERE ClientId=@clientId AND TableName='Vertical'
					 IF @VerticalCustomFieldId>0 AND @sequence>0
					 BEGIN
					 
					 INSERT INTO dbo.CampaignNameConvention
					         (TableName,  CustomFieldId, Sequence, ClientId, CreatedDate,
					          CreatedBy, IsDeleted)
					 VALUES
					         (N'CustomField', @VerticalCustomFieldId,@sequence,@clientId,GETDATE(), @CreatedBy,0)

					 END
					 SELECT @sequence=0	
					 --Vertical unit section

		   FETCH NEXT FROM UniqueClientForCustomNaming INTO @clientId

			   END

	   END

	   CLOSE UniqueClientForCustomNaming
DEALLOCATE UniqueClientForCustomNaming

DELETE FROM dbo.CampaignNameConvention WHERE TableName IN ('Vertical','Geography','Audience','BusinessUnit')

COMMIT
END TRY


BEGIN CATCH
ROLLBACK
END CATCH

GO
-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 02/02/2015
-- Description : update name of CustomField table in MRP database as per custom lable in CustomLabel table
-- ======================================================================================

UPDATE customField_CustomLabel
SET Name=Title
FROM
(SELECT dbo.CustomField.Name,dbo.CustomLabel.Title FROM dbo.CustomField INNER JOIN dbo.CustomLabel ON dbo.CustomField.ClientId=dbo.CustomLabel.ClientId AND dbo.CustomField.Name=dbo.CustomLabel.Code) customField_CustomLabel


-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 02/02/2015
-- Description : Script for Add Space in Custom Field for BU
-- ======================================================================================
UPDATE dbo.CustomField SET Name='Business Unit' WHERE Name='BusinessUnit'

-- END 02/02/2015
GO
-- Start 02/05/2015

--------------- Start: Add Column 'CostWeightage' to Table [dbo].[CustomField_Entity] --------------- 
--- Execute this script in MRP Database.
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'CostWeightage' AND [object_id] = OBJECT_ID(N'[dbo].[CustomField_Entity]'))
BEGIN
    ALTER TABLE [dbo].[CustomField_Entity]
	ADD CostWeightage tinyint NULL
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Attribute wise weight of entity.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'CostWeightage'
END
GO
IF (SELECT Count(*) FROM CustomField_Entity WHERE CostWeightage > 0) = 0
BEGIN
	Update [dbo].[CustomField_Entity] Set CostWeightage = 100
END
GO
--------------- End: Add Column 'CostWeightage' to Table [dbo].[CustomField_Entity] --------------- 

-- END 02/05/2015
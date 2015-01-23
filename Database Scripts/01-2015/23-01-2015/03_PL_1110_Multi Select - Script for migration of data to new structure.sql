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
	   Select Top 1 @Bus_CustomFieldID=CustomFieldID from CustomField where  Name =@lblBusinessUnit and ClientID = @Bus_ClientID and IsDeleted ='0' and EntityType=@Tac_EntityType and CustomFieldTypeId = @Drpdwn_CustomFieldTypeID
	   Select Top 1 @Geo_CustomFieldID=CustomFieldID from CustomField where Name =@lblGeography and ClientID = @Geo_ClientID and IsDeleted ='0' and EntityType=@Tac_EntityType and CustomFieldTypeId = @Drpdwn_CustomFieldTypeID
	   
	   /* Get CustomFieldOptionID from CustomFieldOption Master table based on CustomFieldId,Value */
	   Select Top 1 @Ver_CustomFieldOptionID = CustomFieldOptionID from CustomFieldOption where CustomFieldId=@Ver_CustomFieldID and Value=@Vertical_Title
	   Select Top 1 @Aud_CustomFieldOptionID = CustomFieldOptionID from CustomFieldOption where CustomFieldId=@Aud_CustomFieldID and Value=@Audience_Title
	   Select Top 1 @Bus_CustomFieldOptionID = CustomFieldOptionID from CustomFieldOption where CustomFieldId=@Bus_CustomFieldID and Value=@BusinessUnit_Title
	   Select Top 1 @Geo_CustomFieldOptionID = CustomFieldOptionID from CustomFieldOption where CustomFieldId=@Geo_CustomFieldID and Value=@Geography_Title

	   /* If CustomFieldEntity record does not exist then added to table. */
	   IF NOT Exists(Select 1 from CustomField_Entity where EntityId=@PlanTacticId and CustomFieldId=@Ver_CustomFieldID and Value = convert(nvarchar(256),@Ver_CustomFieldOptionID))
	   Begin
		Insert Into CustomField_Entity(EntityId,CustomFieldId,Value,CreatedDate,CreatedBy,Weightage) Values(@PlanTacticId,@Ver_CustomFieldID,convert(nvarchar(256),@Ver_CustomFieldOptionID),@Tac_CreatedDate,@Tac_CreatedBy,100)
	   End
	   IF NOT Exists(Select 1 from CustomField_Entity where EntityId=@PlanTacticId and CustomFieldId=@Aud_CustomFieldID and Value = convert(nvarchar(256),@Aud_CustomFieldOptionID))
	   Begin
		Insert Into CustomField_Entity(EntityId,CustomFieldId,Value,CreatedDate,CreatedBy,Weightage) Values(@PlanTacticId,@Aud_CustomFieldID,convert(nvarchar(256),@Aud_CustomFieldOptionID),@Tac_CreatedDate,@Tac_CreatedBy,100)
	   End
	   IF NOT Exists(Select 1 from CustomField_Entity where EntityId=@PlanTacticId and CustomFieldId=@Bus_CustomFieldID and Value = convert(nvarchar(256),@Bus_CustomFieldOptionID))
	   Begin
		Insert Into CustomField_Entity(EntityId,CustomFieldId,Value,CreatedDate,CreatedBy,Weightage) Values(@PlanTacticId,@Bus_CustomFieldID,convert(nvarchar(256),@Bus_CustomFieldOptionID),@Tac_CreatedDate,@Tac_CreatedBy,100)
	   End
	   IF NOT Exists(Select 1 from CustomField_Entity where EntityId=@PlanTacticId and CustomFieldId=@Geo_CustomFieldID and Value = convert(nvarchar(256),@Geo_CustomFieldOptionID))
	   Begin
		Insert Into CustomField_Entity(EntityId,CustomFieldId,Value,CreatedDate,CreatedBy,Weightage) Values(@PlanTacticId,@Geo_CustomFieldID,convert(nvarchar(256),@Geo_CustomFieldOptionID),@Tac_CreatedDate,@Tac_CreatedBy,100)
	   End

       FETCH NEXT FROM CursorTactic INTO  @PlanTacticId,@Tac_VerticalId,@Tac_AudienceId,@Tac_BusinessUnitId,@Tac_GeographyId,@Tac_CreatedDate,@Tac_CreatedBy
END   

CLOSE CursorTactic   
DEALLOCATE CursorTactic
/* Execute this script on MRP database */

/* Create Temporary Client & Attributes table */
Create TABLE #Client 
    (
		ID int IDENTITY(1, 1) primary key,
		ClientId uniqueidentifier
    )

Create TABLE #Attributes
    (
		ID int IDENTITY(1, 1) primary key,
		Attribute varchar(50)
    )
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
	
		/* Add new record to CustomField table If record does not exist in table*/
		IF Not Exists(Select 1 from CustomField where Name=@CustomFieldName and CustomFieldTypeId=@CustomFieldType and EntityType=@EntityType and ClientId = @ClientId)
		Begin
			Insert Into CustomField values(@CustomFieldName,@CustomFieldType,@Description,@IsRequired,@EntityType,@ClientId,@IsDeleted,GetDate(),CONVERT(uniqueidentifier,@CreatedBy),Null,Null,@IsDisplayforFilter)
			
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

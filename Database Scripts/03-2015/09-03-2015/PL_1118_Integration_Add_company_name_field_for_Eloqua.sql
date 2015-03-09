
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


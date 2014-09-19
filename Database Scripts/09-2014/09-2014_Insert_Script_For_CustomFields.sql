GO
--Script to add Custom Field i.e. TextBox or DropDownList
DECLARE
	@CustomFieldType VARCHAR(50),  @CustomFieldName varchar(255), @CustomFieldTypeId int, @IsRequired bit, @EntityType varchar(50), @ClientId UNIQUEIDENTIFIER, @UserId uniqueidentifier
	SET @ClientId = '464EB808-AD1F-4481-9365-6AADA15023BD' -- Client id
	SET @UserId = 'F37A855C-9BF4-4A1F-AB7F-B21AF43EB2AF'

	SET @CustomFieldType  = 'TextBox' -- DropDownList
	SET @CustomFieldName = 'Custom Field 1'
	SELECT @CustomFieldTypeId = CustomFieldTypeId FROM CustomFieldType WHERE Name=@CustomFieldType
	SET @IsRequired = 0 -- 1 IS FOR YES AND 0 IS FOR NO
	SET @EntityType = 'Campaign' -- Campaign or Program or Tactic
BEGIN
	--select @CustomFieldName,@CustomFieldTypeId, '', @IsRequired, @EntityType, @ClientId, 0, getdate(), @UserId
	 INSERT INTO [dbo].[CustomField]([Name],[CustomFieldTypeId],[Description],[IsRequired],[EntityType],[ClientId],[IsDeleted],[CreatedDate],[CreatedBy])
	 VALUES(@CustomFieldName,@CustomFieldTypeId, '', @IsRequired, @EntityType, @ClientId, 0, getdate(), @UserId)
END

GO

---------Script to add options/items for custom field of type 'DropDownList'
DECLARE
	@CustomFieldName varchar(255), @CustomFieldId int, @Value varchar(255), @ClientId UNIQUEIDENTIFIER, @UserId uniqueidentifier
	SET @ClientId = '464EB808-AD1F-4481-9365-6AADA15023BD' -- Client id
	SET @UserId = 'F37A855C-9BF4-4A1F-AB7F-B21AF43EB2AF'   --User id for created by

	SET @CustomFieldName = 'Custom Field 1' -- Custom field name
	SELECT @CustomFieldId = CustomFieldId FROM CustomField WHERE Name=@CustomFieldName
	SET @Value = 'Option 1'
BEGIN
	--select @CustomFieldId, @Value, getdate(), @UserId
	INSERT INTO [dbo].[CustomFieldOption]([CustomFieldId],[Value],[CreatedDate],[CreatedBy])VALUES
    (@CustomFieldId, @Value, getdate(), @UserId)
END
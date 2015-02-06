DECLARE @MRPDB VARCHAR(50) = 'MRP'
DECLARE @BDSAuth VARCHAR(50) = 'BDSAuth'


DECLARE @SQLString nvarchar(max)=''
DECLARE @ClientId uniqueidentifier
Declare @IntegrationTypeId int
Declare @PermissionCode varchar(50)='MQL'
Declare @ZebraCode varchar(50) = 'ZBR'
Declare @EloquaCode varchar(50) = 'Eloqua'


		SET @SQLString = N' IF NOT EXISTS(Select ClientIntegrationPermissionId from '+ @MRPDB + '.dbo.[Client_Integration_Permission] Where ClientID = (SElect TOP 1 ClientID from '+ @BDSAuth +'.[dbo].[Client] where Code ='''+@ZebraCode+''') and 
							IntegrationTypeId = (Select TOP 1 IntegrationTypeId from '+ @MRPDB + '.dbo.[IntegrationType] where Code='''+@EloquaCode+''') and
							PermissionCode = '''+@PermissionCode+''')  
							BEGIN '

		SET @SQLString = @SQLString + N' INSERT INTO '+ @MRPDB + '.dbo.[Client_Integration_Permission] Values(
				(SElect TOP 1 ClientID from '+ @BDSAuth +'.[dbo].[Client] where Code ='''+@ZebraCode+'''),
				(Select TOP 1 IntegrationTypeId from '+ @MRPDB + '.dbo.[IntegrationType] where Code='''+@EloquaCode+'''),
				'''+@PermissionCode+''', (SElect TOP 1 ClientID from '+ @BDSAuth +'.[dbo].[Client] where Code ='''+@ZebraCode+'''),GETDATE())
				
				END
				'
Exec(@SQLString)


/* Execute this script on MRP database */

Declare @PlanTacticId int = 0		
Declare @Tac_VerticalId int = 0
Declare @Tac_AudienceId int = 0
Declare @UserId uniqueidentifier
Declare @CustomField nvarchar(100)=''
Declare @CustomFieldId nvarchar(100)=''
Declare @CreatedDate datetime
Declare @CreatedBy uniqueidentifier
Declare @Tac_EntityType varchar(50)='Tactic'
Declare @Ins_CustomFieldId int
Declare @Ins_CustomFieldOptionId int
Declare @CustomFieldName varchar(50)=''
Declare @Ver_CustomField varchar(50)='Verticals'
Declare @Geo_CustomField varchar(50)='Geography'
Declare @Bus_CustomField varchar(50)='BusinessUnit'
Declare @Permission int
Declare @Custm_Title varchar(255)=''
Declare @SQLQuery nvarchar(max)=''
Declare @Cur_Variables varchar(500) ='@UserId,@CustomField,@CustomFieldId,@Permission,@CreatedDate,@CreatedBy'
Declare @Ins_Variables varchar(500)='@UserId,@Ins_CustomFieldId,@Ins_CustomFieldOptionId,@Permission,@CreatedDate,@CreatedBy'
Declare @Parameters nvarchar(max)='@CustomFieldName varchar(50),@Custm_Title varchar(255),@CustomField nvarchar(100),@Ins_CustomFieldId int Output,@Ins_CustomFieldOptionId int Output,@CustomFieldId nvarchar(100),@UserId uniqueidentifier,@Permission int,@CreatedDate datetime,@CreatedBy uniqueidentifier'

SET @SQLQuery = N'DECLARE CursorCustomRestriction CURSOR FOR  
SELECT UserId,CustomField,CustomFieldId,Permission,CreatedDate,CreatedBy
FROM '+ @BDSAuth +'.[dbo].[CustomRestriction]
OPEN CursorCustomRestriction 
FETCH NEXT FROM CursorCustomRestriction INTO ' + @Cur_Variables + '
WHILE @@FETCH_STATUS = 0   
BEGIN   
	SET  @CustomFieldName =''''
	SET  @Custm_Title =''''

	IF (@CustomField  = '''+ @Ver_CustomField +''')
	BEGIN
		SET @CustomFieldName =''Vertical''
	END
	Else IF (@CustomField = '''+ @Geo_CustomField +''')
	BEGIN
		SET  @CustomFieldName=''Geography''
	END
	Else IF (@CustomField = '''+ @Bus_CustomField +''')
	BEGIN
		SET  @CustomFieldName =''BusinessUnit''
	END

	/* Get CustomFieldId from CustomField table */
	Select Top 1 @Ins_CustomFieldId = CustomFieldID from '+ @MRPDB +'.[dbo].CustomField as custmfield
	inner join '+ @BDSAuth +'.[dbo].[User] as usr on custmfield.ClientId = usr.ClientId and usr.UserId = @UserId
	where custmfield.EntityType='''+ @Tac_EntityType +''' and Name= @CustomFieldName
	
	/* Get CustomField Title from specific CustomField table based on respective CustomFieldId*/
	IF ( @CustomField = '''+ @Ver_CustomField +''')
	BEGIN
		Select  @Custm_Title = Title from '+ @MRPDB +'.[dbo].Vertical where VerticalId=Convert(int,@CustomFieldId)
	END
	Else IF ( @CustomField = '''+ @Geo_CustomField +''')
	BEGIN
		Select  @Custm_Title = Title from '+ @MRPDB +'.[dbo].[Geography] where GeographyId=Convert(uniqueidentifier,@CustomFieldId )
	END
	Else IF ( @CustomField = '''+ @Bus_CustomField +''')
	BEGIN
		Select  @Custm_Title = Title from '+ @MRPDB +'.[dbo].BusinessUnit where BusinessUnitId=Convert(uniqueidentifier,@CustomFieldId)
	END
	 
	/* Get CustomFieldOptionId from CustomFieldOption table based on CustomFieldId & Value */
	Select Top 1 @Ins_CustomFieldOptionId = CustomFieldOptionId from '+ @MRPDB+'.[dbo].CustomFieldOption where CustomFieldId = Convert(int,@Ins_CustomFieldId)
				 and Value = @Custm_Title

	IF NOT EXISTS(Select 1 from '+ @MRPDB +'.[dbo].CustomRestriction where UserId=@UserId and CustomFieldId=@Ins_CustomFieldId and CustomFieldOptionId=@Ins_CustomFieldOptionId and Permission=@Permission)
	BEGIN
		Insert Into '+ @MRPDB +'.[dbo].CustomRestriction Values('+ @Ins_Variables +')
	END

 FETCH NEXT FROM CursorCustomRestriction INTO ' + @Cur_Variables + '
END   

CLOSE CursorCustomRestriction   
DEALLOCATE CursorCustomRestriction'

Execute sp_executesql @SQLQuery, @Parameters,@CustomFieldName=@CustomFieldName,@Custm_Title=@Custm_Title,@CustomField=@CustomField,@CustomFieldId=@CustomFieldId,@UserId=@UserId,@Permission=@Permission,@CreatedDate=@CreatedDate,@CreatedBy=@CreatedBy,@Ins_CustomFieldId=@Ins_CustomFieldId,@Ins_CustomFieldOptionId=@Ins_CustomFieldOptionId;
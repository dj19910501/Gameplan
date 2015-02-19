---- Execute this script on MRP database
IF OBJECT_ID('tempdb..#IntegrationType') IS NOT NULL DROP TABLE #IntegrationType
GO
Create TABLE #IntegrationType
    (
		ID int IDENTITY(1, 1) primary key,
		IntegrationTypeId int
    )

Declare @varSalesForce varchar(50) ='Salesforce'
Declare @RawCnt int =0
Declare @rawId int =1
Declare @IntegrationTypeId int =0
Declare	@ActualFieldName varchar(50)='ResponseDate'
Declare	@DisplayFieldName varchar(50)='Response Date'
Declare @Type varchar(10)='CW'
Declare @IsDeleted bit = 0

Insert Into #IntegrationType Select IntegrationTypeId from IntegrationType where Code =@varSalesForce
Select @RawCnt = Count(ID) from #IntegrationType
While(@rawId <= @RawCnt)
BEGIN
	Select @IntegrationTypeId = IntegrationTypeId from #IntegrationType where ID=@rawId
	IF NOT Exists(Select 1 from GameplanDataTypePull where IntegrationTypeId=@IntegrationTypeId and ActualFieldName=@ActualFieldName and DisplayFieldName = @DisplayFieldName and [Type] = @Type)
	Begin
		Insert Into GameplanDataTypePull(IntegrationTypeId,ActualFieldName,DisplayFieldName,[Type],IsDeleted) Values(@IntegrationTypeId,@ActualFieldName,@DisplayFieldName,@Type,@IsDeleted)
	End
	Set @rawId = @rawId + 1
END

DROP TABLE #IntegrationType
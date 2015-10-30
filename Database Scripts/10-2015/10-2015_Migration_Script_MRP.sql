-- Please Add GO statement between script
GO
-- Created By : Dashrath Prajapati
-- Created Date : 20/10/2015
-- Description :Update GamePlan text to Plan from subject Column
-- ======================================================================================
UPDATE [dbo].[Notification] SET [Subject] = REPLACE([Subject], N'GamePlan', 'Plan') 
Go

-- ======================================================================================
-- Created By : Bhavesh Dobariya
-- Created Date : 14/10/2015
-- Description : Add column in Budget Detail table for IsOther
-- ======================================================================================
GO 

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Budget')
BEGIN

		    IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'IsOther' AND [object_id] = OBJECT_ID(N'Budget'))
			 BEGIN
		     ALTER TABLE [dbo].[Budget] ADD IsOther bit Not Null DEFAULT 0
		    END
END

Go

-- Created By Nishant Sheth
-- Created on : 23-Oct-2015
-- Desc :: Delete budget and update line item with other budget item
-- EXEC DeleteBudget 1865,'464eb808-ad1f-4481-9365-6aada15023bd'
GO
IF OBJECT_ID('dbo.DeleteBudget') IS NULL -- Check if SP Exists
 EXEC('CREATE PROCEDURE dbo.DeleteBudget AS SET NOCOUNT ON;') -- Create dummy/empty SP
GO

GO
ALTER PROCEDURE dbo.DeleteBudget
@BudgetDetailId int = 0,
@ClientId varchar(200)= null
AS 
BEGIN
 SET NOCOUNT ON
Declare @ErrorMeesgae varchar(3000) = ''
Declare @OtherBudgetDetailId int = 0
Declare @ParentIdCount int = 0
Declare @RowCount int = 0
BEGIN TRY
IF OBJECT_ID(N'tempdb..#BudgetDetailData') IS NOT NULL
BEGIN
  PRINT 'Table Exists'
  DROP TABLE #BudgetDetailData
END

Select @OtherBudgetDetailId = ChildDetail.Id From Budget_Detail ChildDetail
CROSS APPLY (Select * from Budget ParentDetail where ParentDetail.Id= ChildDetail.BudgetId) ParentDetail
Where (ParentDetail.IsDeleted=0 OR ParentDetail.IsDeleted is null) and (ChildDetail.IsDeleted=0  OR ChildDetail.IsDeleted is null)
and ParentDetail.IsOther=1
and ParentDetail.ClientId=@ClientId

;WITH BudgetCTE AS
( 
SELECT Id
--,Name 
,ParentId
,BudgetId
,IsDeleted
FROM [dbo].[Budget_Detail] 
WHERE ParentId = @BudgetDetailId OR Id = @BudgetDetailId
AND IsDeleted=0 OR IsDeleted is null
UNION ALL

SELECT a.Id
--, a.Name
, a.ParentId
,a.BudgetId
,a.IsDeleted
FROM [dbo].[Budget_Detail] a 
CROSS APPLY (Select * From BudgetCTE s Where a.ParentId=s.Id ) s
where a.IsDeleted=0 OR a.IsDeleted is null and s.IsDeleted=0 and s.IsDeleted is null
)
 
SELECT  Distinct Id,ParentId,BudgetId into #BudgetDetailData FROM BudgetCTE  Order by ID 

Select @RowCount = COUNT(Id) From #BudgetDetailData 
 IF @RowCount > 0 
 BEGIN
-- if parent then delete from budget table
Select @ParentIdCount=Count(*)  From #BudgetDetailData
where ParentId is null

If @ParentIdCount > 0 
BEGIN
	UPDATE [dbo].[Budget] SET IsDeleted=1 Where Id = (Select Top(1) BudgetId From #BudgetDetailData)

END

-- delete budget details
UPDATE [dbo].[Budget_Detail] SET IsDeleted=1 Where Id in (Select Id From #BudgetDetailData)

-- Update Line item details with other budget 
Update [dbo].[LineItem_Budget] SET BudgetDetailId=@OtherBudgetDetailId
	Where Id In(
	Select Id From [dbo].[LineItem_Budget] Where BudgetDetailId in (
		Select Id from #BudgetDetailData))
	END

Select @ErrorMeesgae as 'ErrorMessage'
--Select * From #BudgetDetailData

END TRY
BEGIN CATCH
	SET @ErrorMeesgae= 'Object=DeleteBudget , ErrorMessage='+ERROR_MESSAGE()
	Select @ErrorMeesgae as 'ErrorMessage'
END CATCH
END
GO

--- Created By Nishant Sheth
--- Date: 16-Oct-2015
--- Desc: Increase column size of Budget_Detail column 'Name' size
 IF (EXISTS
          (SELECT *
           FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = 'Budget_Detail')) BEGIN IF EXISTS
  (SELECT *
   FROM sys.columns
   WHERE Name = N'Name'
     AND Object_ID = Object_ID(N'Budget_Detail')) BEGIN
ALTER TABLE [dbo].[Budget_Detail]
ALTER COLUMN [Name] nvarchar(255) END END 
GO


/* --------- Start: Change below variable value as per client --------- */

Declare @displayFieldName varchar(255) = 'Closed Won'

/* --------- End: Change below variable value as per client --------- */

Declare @salesforceCode varchar(50)='Salesforce'
Declare @salesforceIntegrationTypeId int
Declare @actualFieldName varchar(255) = 'CW'
Declare @type varchar(10)='CW'
Declare @isDeleted bit='0'

 
DECLARE cursrDataTypePull CURSOR 

FOR
 
Select IntegrationTypeId FROM  IntegrationType where Code = @salesforceCode
 
OPEN cursrDataTypePull 

FETCH NEXT FROM cursrDataTypePull
 
   INTO @salesforceIntegrationTypeId
 

WHILE @@FETCH_STATUS = 0
 
BEGIN
 
	IF Not Exists(Select * From GameplanDataTypePull where IntegrationTypeId = @salesforceIntegrationTypeId and ActualFieldName=@actualFieldName and DisplayFieldName=@displayFieldName and [Type] = @type and IsDeleted=@isDeleted)
	BEGIN
		Insert Into GameplanDataTypePull Values(@salesforceIntegrationTypeId,@actualFieldName,@displayFieldName,@type,@isDeleted)
	END

   FETCH NEXT FROM cursrDataTypePull
 
   INTO @salesforceIntegrationTypeId
 
END
 
CLOSE cursrDataTypePull 

DEALLOCATE cursrDataTypePull 
GO
/* ===========================================================================  

	Description: This SQL query insert data to IntegrationInstanceDataTypeMappingPull table

   ===========================================================================  */


	/* --------------- Do not make any change to below local variables --------------- */
	
	--------- START: Declare Local Variables -------------
	
	Declare @salesforceCode varchar(50)='Salesforce'	--- Salesforce code for IntegrationType table reference
	Declare @salesforceIntegrationTypeId int
	Declare @actualFieldName varchar(255) = 'CW'		--- ActualFieldName code for GameplanDataTypePull table reference
	Declare @type varchar(10)='CW'						--- Type code for GameplanDataTypePull table reference
	Declare @cwStageCode varchar(10)='CW'				--- CW Stage Code for Stage table reference
	Declare @gpDataTypePullId int = 0
	
	--------- END: Declare Local Variables ------------- 
	
	--------- START: Initialize Integration Type Cursor -------------
	
	DECLARE cursrIntegrationType CURSOR 
	
	FOR
	 
	Select IntegrationTypeId FROM  IntegrationType where Code = @salesforceCode
	 
	OPEN cursrIntegrationType 
	
	FETCH NEXT FROM cursrIntegrationType
	 
	   INTO @salesforceIntegrationTypeId
	 
	
	WHILE @@FETCH_STATUS = 0
	 
	BEGIN
		
		Select @gpDataTypePullId = GameplanDataTypePullId from GameplanDataTypePull where IntegrationTypeId=@salesforceIntegrationTypeId and ActualFieldName=@actualFieldName and [Type]=@type and IsDeleted='0'
		
		--------- START: Initialize Integration Instance Cursor -------------
	
			 DECLARE @instanceId  int = 0 
			 DECLARE @clientId  uniqueidentifier
			 DECLARE @cwStageTitle  varchar(255) = 0 
	
			DECLARE cursrInstance CURSOR 
			
			FOR
			 
			Select IntegrationInstanceId, ClientId FROM  IntegrationInstance where IntegrationTypeId=@salesforceIntegrationTypeId and IsDeleted ='0'
			 
			OPEN cursrInstance 
			
			FETCH NEXT FROM cursrInstance
			 
			   INTO @instanceId,@clientId
			 
			
			WHILE @@FETCH_STATUS = 0
			 
			BEGIN
				
				Select TOP 1 @cwStageTitle = [Title] from Stage as stg where stg.ClientId = @clientId and stg.IsDeleted='0' and stg.Code=@cwStageCode
				
				IF NOT EXISTS(Select 1 from IntegrationInstanceDataTypeMappingPull where IntegrationInstanceId=@instanceId and GameplanDataTypePullId=@gpDataTypePullId and TargetDataType=@cwStageTitle)
				BEGIN
					Insert Into IntegrationInstanceDataTypeMappingPull(IntegrationInstanceId,GameplanDataTypePullId,TargetDataType,CreatedDate,CreatedBy) 
					Values(@instanceId,@gpDataTypePullId,@cwStageTitle,GETDATE(),@clientId)
				END
	
			   FETCH NEXT FROM cursrInstance
			 
			   INTO @instanceId,@clientId
			 
			END
			 
			CLOSE cursrInstance 
			
			DEALLOCATE cursrInstance 
	
		--------- End: Initialize Integration Instance Cursor --------
	
	
	   FETCH NEXT FROM cursrIntegrationType
	 
	   INTO @salesforceIntegrationTypeId
	 
	END
	 
	CLOSE cursrIntegrationType 
	
	DEALLOCATE cursrIntegrationType 
	
	--------- END: Initialize Integration Type Cursor -------------
	GO
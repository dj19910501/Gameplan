/* --------- Start Script of PL ticket #1801 --------- */
-- Created by : Viral Kadiya
-- Created On : 12/17/2015
-- Description : Insert 'Plan Name' field in GameplanDataType table

Declare @displayFieldName varchar(255) = 'Plan Name'
Declare @salesforceCode varchar(50)='Salesforce'
Declare @salesforceIntegrationTypeId int
Declare @tableName varchar(50)='Global'
Declare @actualFieldName varchar(255) = 'PlanName'
Declare @isDeleted bit='0'
Declare @isGet bit='0'
Declare @isImprovement bit='1'

 
DECLARE cursrDataType CURSOR 

FOR
 
Select IntegrationTypeId FROM  IntegrationType where Code = @salesforceCode
 
OPEN cursrDataType 

FETCH NEXT FROM cursrDataType
 
   INTO @salesforceIntegrationTypeId
 

WHILE @@FETCH_STATUS = 0
 
BEGIN
 
	IF Not Exists(Select * From GameplanDataType where IntegrationTypeId = @salesforceIntegrationTypeId and TableName=@tableName and ActualFieldName=@actualFieldName and DisplayFieldName=@displayFieldName and IsGet=@isGet and IsDeleted=@isDeleted and IsImprovement=@isImprovement)
	BEGIN
		Insert Into GameplanDataType Values(@salesforceIntegrationTypeId,@tableName,@actualFieldName,@displayFieldName,@isGet,@isDeleted,@isImprovement)
	END

   FETCH NEXT FROM cursrDataType
 
   INTO @salesforceIntegrationTypeId
 
END
 
CLOSE cursrDataType 

DEALLOCATE cursrDataType 
GO

/* --------- End Script of PL ticket #1801 --------- */

/* --------- Start Script of PL ticket #1749 --------- */
-- Created by : Komal Rawal
-- Created On : 12/28/2015
-- Description : Insert 'IsDefaultPreset' field in Plan_UserSavedViews table to setdefault preset.


GO 

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_UserSavedViews')
BEGIN

		    IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'IsDefaultPreset' AND [object_id] = OBJECT_ID(N'Plan_UserSavedViews'))
			 BEGIN
		     ALTER TABLE [dbo].[Plan_UserSavedViews] ADD IsDefaultPreset bit  Null DEFAULT 0
		    END
END

Go
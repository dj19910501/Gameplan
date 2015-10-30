
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
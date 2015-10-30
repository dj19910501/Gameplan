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
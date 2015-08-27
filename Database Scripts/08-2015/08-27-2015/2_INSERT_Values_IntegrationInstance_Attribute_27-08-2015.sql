/* ----------- STRT:- Insert ClientID & ClientSecret related data to IntegrationInstance_Attribute ----------- */


-- =============================================================================================================================
-- Description: The purpose of this SQL Query to insert ClientID and ClientSecret values to IntegrationInstance_Attribute table.

-- NOTE: Please make change to below variable values as per following comment:
-- 1.) Modify CientID & CientSecret variable value to @ClientID & @ClientSecret respective as per CompanyName in Eloqua.
-- 2.) Don't change @lblClientID & @lblClientSecret variable values.
-- 3.) Modify 'Eloqua' code value to @EloquaCode variable as per defined in IntegrationType table.
-- =============================================================================================================================


Declare @ClientID varchar(MAX) ='745d7e5e-1265-41b3-83ec-d8724a033f98' -- Make change to this value as per CompanyName in Eloqua. 
Declare @ClientSecret varchar(MAX)='1ZXwfhoxJfEq0SlvVuj~0ticvR6lmC74vkg2YaqsOSZJOqcqizH~hcduhCGg6zT9y4VRrSVRnoV3XCW3XWoCFTceDYE~pKn2pBSK' -- Make change to this value as per CompanyName in Eloqua.
Declare @EloquaCode varchar(10)='Eloqua'
Declare @IntegrationTypeID int
DECLARE @intErrorCode INT
DECLARE @lblClientID varchar(50)='ClientId'	-- Don't make any change here.
DECLARE @lblClientSecret varchar(50)='ClientSecret' -- Don't make any change here.

BEGIN TRAN
	
	DECLARE cursr_IntegrationAttribute CURSOR 
	FOR
	SELECT IntegrationTypeID from IntegrationType where Code =@EloquaCode
	OPEN cursr_IntegrationAttribute
	FETCH NEXT FROM cursr_IntegrationAttribute INTO @IntegrationTypeID
	WHILE @@FETCH_STATUS = 0
	BEGIN
	
		/*--------- Start : Loop ClientID from IntegrationTypeAttribute  ---------*/
			Declare @ClientID_Attr int=0

			DECLARE cursr_ClientID_Attribute CURSOR 
			FOR
			Select IntegrationTypeAttributeID from IntegrationTypeAttribute where IntegrationTypeId=@IntegrationTypeID and Attribute=@lblClientID and IsDeleted='0'
			OPEN cursr_ClientID_Attribute
			FETCH NEXT FROM cursr_ClientID_Attribute INTO @ClientID_Attr
			WHILE @@FETCH_STATUS = 0
			BEGIN
				
					/* --------- Start : Insert IntegrationTypeID wise 'ClientID' IntegrationTypeAttributeID  to table IntegrationInstance_Attribute ---------*/

						Declare @clientID_IntegrationInstanceID int
						Declare @clientID_CreatedBy varchar(MAX)
			
						DECLARE cursr_ClientID_IntegrationInstance CURSOR 
						FOR
						Select IntegrationInstanceID,CreatedBy from IntegrationInstance where IntegrationTypeId=@IntegrationTypeID
						OPEN cursr_ClientID_IntegrationInstance
						FETCH NEXT FROM cursr_ClientID_IntegrationInstance INTO @clientID_IntegrationInstanceID,@clientID_CreatedBy
						WHILE @@FETCH_STATUS = 0
						BEGIN
							
							IF NOT EXISTS(Select 1 from IntegrationInstance_Attribute where IntegrationInstanceId=@clientID_IntegrationInstanceID and IntegrationTypeAttributeId=@ClientID_Attr and Value=@ClientID)
							BEGIN	
								INSERT INTO IntegrationInstance_Attribute Values(@clientID_IntegrationInstanceID,@ClientID_Attr,@ClientID,GETDATE(),@clientID_CreatedBy)
							END
	
							SELECT @intErrorCode = @@ERROR
							IF (@intErrorCode <> 0) GOTO ROLLBACK_TRANSACTION

							FETCH NEXT FROM cursr_ClientID_IntegrationInstance INTO @clientID_IntegrationInstanceID,@clientID_CreatedBy

						END   
						CLOSE cursr_ClientID_IntegrationInstance   
						DEALLOCATE cursr_ClientID_IntegrationInstance
					
					/* --------- End : Insert IntegrationTypeID wise 'ClientID' IntegrationTypeAttributeID  to table IntegrationInstance_Attribute ---------*/

				FETCH NEXT FROM cursr_ClientID_Attribute INTO @ClientID_Attr
			END   
			CLOSE cursr_ClientID_Attribute   
			DEALLOCATE cursr_ClientID_Attribute

		/*--------- END : Loop ClientID from IntegrationTypeAttribute  ---------*/

		/*--------- Start : Loop ClientSecret from IntegrationTypeAttribute  ---------*/
			Declare @ClientSecret_Attr int=0

			DECLARE cursr_ClientSecret_Attribute CURSOR 
			FOR
			Select IntegrationTypeAttributeID from IntegrationTypeAttribute where IntegrationTypeId=@IntegrationTypeID and Attribute=@lblClientSecret and IsDeleted='0'
			OPEN cursr_ClientSecret_Attribute
			FETCH NEXT FROM cursr_ClientSecret_Attribute INTO @ClientSecret_Attr
			WHILE @@FETCH_STATUS = 0
			BEGIN
				
					/* --------- Start : Insert IntegrationTypeID wise 'ClientSecret' IntegrationTypeAttributeID value  to table IntegrationInstance_Attribute ---------*/

						Declare @clientsecret_IntegrationInstanceID int
						Declare @clientsecret_CreatedBy varchar(MAX)
			
						DECLARE cursr_ClientSecret_IntegrationInstance CURSOR 
						FOR
						Select IntegrationInstanceID,CreatedBy from IntegrationInstance where IntegrationTypeId=@IntegrationTypeID
						OPEN cursr_ClientSecret_IntegrationInstance
						FETCH NEXT FROM cursr_ClientSecret_IntegrationInstance INTO @ClientSecret_IntegrationInstanceID,@ClientSecret_CreatedBy
						WHILE @@FETCH_STATUS = 0
						BEGIN
							
							IF NOT EXISTS(Select 1 from IntegrationInstance_Attribute where IntegrationInstanceId=@ClientSecret_IntegrationInstanceID and IntegrationTypeAttributeId=@ClientSecret_Attr and Value=@ClientID)
							BEGIN	
								INSERT INTO IntegrationInstance_Attribute Values(@ClientSecret_IntegrationInstanceID,@ClientSecret_Attr,@ClientID,GETDATE(),@ClientSecret_CreatedBy)
							END
	
							SELECT @intErrorCode = @@ERROR
							IF (@intErrorCode <> 0) GOTO ROLLBACK_TRANSACTION

							FETCH NEXT FROM cursr_ClientSecret_IntegrationInstance INTO @ClientSecret_IntegrationInstanceID,@ClientSecret_CreatedBy

						END   
						CLOSE cursr_ClientSecret_IntegrationInstance   
						DEALLOCATE cursr_ClientSecret_IntegrationInstance
					
					/* --------- End : Insert IntegrationTypeID wise 'ClientSecret' IntegrationTypeAttributeID value  to table IntegrationInstance_Attribute ---------*/

				FETCH NEXT FROM cursr_ClientSecret_Attribute INTO @ClientSecret_Attr
			END   
			CLOSE cursr_ClientSecret_Attribute   
			DEALLOCATE cursr_ClientSecret_Attribute

		/*--------- END : Loop ClientSecret from IntegrationTypeAttribute  ---------*/
	
		SELECT @intErrorCode = @@ERROR
		IF (@intErrorCode <> 0) GOTO ROLLBACK_TRANSACTION
		/*-----------END:- Insert Instance wise ClientID & ClientSecret field value to table IntegrationInstance_Attribute-----------*/
	
	FETCH NEXT FROM cursr_IntegrationAttribute INTO @IntegrationTypeID
	END   
	CLOSE cursr_IntegrationAttribute   
	DEALLOCATE cursr_IntegrationAttribute
	
	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0) GOTO ROLLBACK_TRANSACTION

COMMIT TRAN

ROLLBACK_TRANSACTION:
IF (@intErrorCode <> 0) 
BEGIN
PRINT 'Unexpected error occurred!'
    ROLLBACK TRAN
END
/* ----------- END:- Insert ClientID & ClientSecret related data to IntegrationInstance_Attribute ----------- */
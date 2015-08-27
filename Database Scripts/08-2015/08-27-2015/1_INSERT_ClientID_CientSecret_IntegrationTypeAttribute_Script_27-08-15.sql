/* ----------- STRT:- Insert ClientID & ClientSecret field to table IntegrationTypeAttribute ----------- */

-- ========================================================================================================================
-- Description: The purpose of this SQL Query to insert ClientID and ClientSecret fields to IntegrationTypeAttribute table.

-- NOTE: Please make change to below variable values as per following comment:
-- 1.) Don't change @lblClientID & @lblClientSecret variable values.
-- 2.) Modify 'Eloqua' code value to @EloquaCode variable as per defined in IntegrationType table.
-- ========================================================================================================================

Declare @EloquaCode varchar(10)='Eloqua'
Declare @IntegrationTypeID int
DECLARE @intErrorCode INT
DECLARE @lblClientID varchar(50)='ClientId'	-- Don't make any change here.
DECLARE @lblClientSecret varchar(50)='ClientSecret'	-- Don't make any change here.


BEGIN TRAN
	
	DECLARE cursr_IntegrationAttribute CURSOR 
	FOR
	SELECT IntegrationTypeID from IntegrationType where Code =@EloquaCode
	OPEN cursr_IntegrationAttribute
	FETCH NEXT FROM cursr_IntegrationAttribute INTO @IntegrationTypeID
	WHILE @@FETCH_STATUS = 0
	BEGIN
	
		/*-----------START:- Insert ClientID & ClientSecret fields to Table IntegrationTypeAttribute-----------*/
		IF NOT EXISTS(Select 1 from IntegrationTypeAttribute where IntegrationTypeId=@IntegrationTypeID and Attribute=@lblClientID and AttributeType='textbox' and IsDeleted='0')
		BEGIN	
			INSERT INTO IntegrationTypeAttribute Values(@IntegrationTypeID,@lblClientID,'textbox',0)
		END
		
		SELECT @intErrorCode = @@ERROR
	    IF (@intErrorCode <> 0) GOTO ROLLBACK_TRANSACTION
	
		IF NOT EXISTS(Select 1 from IntegrationTypeAttribute where IntegrationTypeId=@IntegrationTypeID and Attribute=@lblClientSecret and AttributeType='textbox' and IsDeleted='0')
		BEGIN	
			INSERT INTO IntegrationTypeAttribute Values(@IntegrationTypeID,@lblClientSecret,'textbox',0)
		END
	
		SELECT @intErrorCode = @@ERROR
	    IF (@intErrorCode <> 0) GOTO ROLLBACK_TRANSACTION
		/*-----------END:- Insert ClientID & ClientSecret fields to Table IntegrationTypeAttribute-----------*/

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
/* ----------- END:- Insert ClientID & ClientSecret field to table IntegrationTypeAttribute ----------- */
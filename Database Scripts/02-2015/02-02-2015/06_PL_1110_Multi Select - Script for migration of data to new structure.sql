-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 02/02/2015
-- Description : Update CampaignNameConvention table in MRP database as per business unit,verticals,Geography and Audience fields in CustomField table
-- ======================================================================================



BEGIN TRY
BEGIN TRANSACTION 
   DECLARE @clientId UNIQUEIDENTIFIER
   DECLARE @CreatedBy UNIQUEIDENTIFIER
   DECLARE @BusinessUnitCustomFieldId INT
   DECLARE @AudienceCustomFieldId INT
   DECLARE @GeographyUnitCustomFieldId INT
   DECLARE @VerticalCustomFieldId INT
   DECLARE @sequence INT

   DECLARE UniqueClientForCustomNaming CURSOR
   
		STATIC FOR
		SELECT DISTINCT(ClientId) FROM dbo.CampaignNameConvention

 
   OPEN UniqueClientForCustomNaming

	   IF @@CURSOR_ROWS>0
	   BEGIN

		   FETCH NEXT FROM UniqueClientForCustomNaming INTO @clientId
			   WHILE @@FETCH_STATUS=0
			   BEGIN

					 --Business unit section
					 SELECT @BusinessUnitCustomFieldId=CustomFieldId,@CreatedBy=CreatedBy  FROM dbo.CustomField WHERE ClientId=@clientId AND Name IN ('BusinessUnit','Business Unit')
					 SELECT @sequence=Sequence FROM dbo.CampaignNameConvention WHERE ClientId=@clientId AND TableName IN ('BusinessUnit','Business Unit')
					 IF @BusinessUnitCustomFieldId>0 AND @sequence>0
					 BEGIN
					 
					 INSERT INTO dbo.CampaignNameConvention
					         (TableName,  CustomFieldId, Sequence, ClientId, CreatedDate,
					          CreatedBy, IsDeleted)
					 VALUES
					         (N'CustomField', @BusinessUnitCustomFieldId,@sequence,@clientId,GETDATE(), @CreatedBy,0)

					 END	
					 SELECT @sequence=0

					 --Business unit section

					 --Audience unit section
					 SELECT @AudienceCustomFieldId=CustomFieldId,@CreatedBy=CreatedBy  FROM dbo.CustomField WHERE ClientId=@clientId AND Name='Audience'
					 SELECT @sequence=Sequence FROM dbo.CampaignNameConvention WHERE ClientId=@clientId AND TableName='Audience'
					 IF @AudienceCustomFieldId>0 AND @sequence>0
					 BEGIN
					 
					 INSERT INTO dbo.CampaignNameConvention
					         (TableName,  CustomFieldId, Sequence, ClientId, CreatedDate,
					          CreatedBy, IsDeleted)
					 VALUES
					         (N'CustomField', @AudienceCustomFieldId,@sequence,@clientId,GETDATE(), @CreatedBy,0)

					 END	
					 SELECT @sequence=0
					 --Audience unit section

					  --Geography unit section
					 SELECT @GeographyUnitCustomFieldId=CustomFieldId,@CreatedBy=CreatedBy  FROM dbo.CustomField WHERE ClientId=@clientId AND Name='Geography'
					 SELECT @sequence=Sequence FROM dbo.CampaignNameConvention WHERE ClientId=@clientId AND TableName='Geography'
					 IF @GeographyUnitCustomFieldId>0 AND @sequence>0
					 BEGIN
					 
					 INSERT INTO dbo.CampaignNameConvention
					         (TableName,  CustomFieldId, Sequence, ClientId, CreatedDate,
					          CreatedBy, IsDeleted)
					 VALUES
					         (N'CustomField', @GeographyUnitCustomFieldId,@sequence,@clientId,GETDATE(), @CreatedBy,0)

					 END	
					 SELECT @sequence=0
					 --Geography unit section

					 --Vertical unit section
					 SELECT @VerticalCustomFieldId=CustomFieldId,@CreatedBy=CreatedBy  FROM dbo.CustomField WHERE ClientId=@clientId AND Name='Vertical'
					 SELECT @sequence=Sequence FROM dbo.CampaignNameConvention WHERE ClientId=@clientId AND TableName='Vertical'
					 IF @VerticalCustomFieldId>0 AND @sequence>0
					 BEGIN
					 
					 INSERT INTO dbo.CampaignNameConvention
					         (TableName,  CustomFieldId, Sequence, ClientId, CreatedDate,
					          CreatedBy, IsDeleted)
					 VALUES
					         (N'CustomField', @VerticalCustomFieldId,@sequence,@clientId,GETDATE(), @CreatedBy,0)

					 END
					 SELECT @sequence=0	
					 --Vertical unit section

		   FETCH NEXT FROM UniqueClientForCustomNaming INTO @clientId

			   END

	   END

	   CLOSE UniqueClientForCustomNaming
DEALLOCATE UniqueClientForCustomNaming

DELETE FROM dbo.CampaignNameConvention WHERE TableName IN ('Vertical','Geography','Audience','BusinessUnit')

COMMIT
END TRY


BEGIN CATCH
ROLLBACK
END CATCH


-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 02/02/2015
-- Description : update name of CustomField table in MRP database as per custom lable in CustomLabel table
-- ======================================================================================

UPDATE customField_CustomLabel
SET Name=Title
FROM
(SELECT dbo.CustomField.Name,dbo.CustomLabel.Title FROM dbo.CustomField INNER JOIN dbo.CustomLabel ON dbo.CustomField.ClientId=dbo.CustomLabel.ClientId AND dbo.CustomField.Name=dbo.CustomLabel.Code) customField_CustomLabel


-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 02/02/2015
-- Description : Script for Add Space in Custom Field for BU
-- ======================================================================================
UPDATE dbo.CustomField SET Name='Business Unit' WHERE Name='BusinessUnit'

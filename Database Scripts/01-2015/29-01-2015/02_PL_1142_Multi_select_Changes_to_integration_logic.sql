-- ======================================================================================
-- Created By : Sohel Pathan
-- Created Date : 29/01/2015
-- Description : Update mapping for PL ticket #1142 - Multi select: Changes to integration logic
-- ======================================================================================

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IntegrationInstanceDataTypeMapping') AND
	EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IntegrationInstance') AND
	EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GameplanDataType')
BEGIN

	--============================================================= Vertical ============================================================================
	BEGIN TRY
	
	BEGIN TRANSACTION Vertical
	PRINT(1)		
	IF EXISTS (SELECT GameplanDataTypeId FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('VerticalId'))
	BEGIN
		PRINT(2)		
		UPDATE IIM SET IIM.CustomFieldId = C.CustomFieldId, IIM.GameplanDataTypeId = NULL
		--SELECT IIM.*, II.ClientId, C.CustomFieldId 
		FROM IntegrationInstanceDataTypeMapping IIM
		INNER JOIN IntegrationInstance II ON II.IntegrationInstanceId = IIM.IntegrationInstanceId
		INNER JOIN CustomField C ON C.Name = 'Vertical' AND C.ClientId = II.ClientId
		WHERE IIM.GameplanDataTypeId IN (
											SELECT GameplanDataTypeId 
											FROM GameplanDataType 
											WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('VerticalId')
										) 
			AND IIM.GameplanDataTypeId IS NOT NULL AND IIM.CustomFieldId IS NULL

		DELETE FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('VerticalId')
	END
	
	IF(@@ERROR > 0)
	BEGIN
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY()
		ROLLBACK TRANSACTION Vertical
		END
	ELSE
	BEGIN
		COMMIT TRANSACTION Vertical
	END

	END TRY
	BEGIN CATCH
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY(), 'Vertical'
		ROLLBACK TRANSACTION Vertical
	END CATCH

	--============================================================= Audience ============================================================================
	BEGIN TRY
	
	BEGIN TRANSACTION Audience
	PRINT(1)		
	IF EXISTS (SELECT GameplanDataTypeId FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('AudienceId'))
	BEGIN
		PRINT(2)		
		UPDATE IIM SET IIM.CustomFieldId = C.CustomFieldId, IIM.GameplanDataTypeId = NULL
		--SELECT IIM.*, II.ClientId, C.CustomFieldId 
		FROM IntegrationInstanceDataTypeMapping IIM
		INNER JOIN IntegrationInstance II ON II.IntegrationInstanceId = IIM.IntegrationInstanceId
		INNER JOIN CustomField C ON C.Name = 'Audience' AND C.ClientId = II.ClientId
		WHERE IIM.GameplanDataTypeId IN (
											SELECT GameplanDataTypeId 
											FROM GameplanDataType 
											WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('AudienceId')
										) 
			AND IIM.GameplanDataTypeId IS NOT NULL AND IIM.CustomFieldId IS NULL

		DELETE FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('AudienceId')
	END
	
	IF(@@ERROR > 0)
	BEGIN
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY()
		ROLLBACK TRANSACTION Audience
		END
	ELSE
	BEGIN
		COMMIT TRANSACTION Audience
	END

	END TRY
	BEGIN CATCH
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY(), 'Audience'
		ROLLBACK TRANSACTION Audience
	END CATCH

	--============================================================= Geography ============================================================================
	BEGIN TRY
	
	BEGIN TRANSACTION Geo
	PRINT(1)		
	IF EXISTS (SELECT GameplanDataTypeId FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('GeographyId'))
	BEGIN
		PRINT(2)		
		UPDATE IIM SET IIM.CustomFieldId = C.CustomFieldId, IIM.GameplanDataTypeId = NULL
		--SELECT IIM.*, II.ClientId, C.CustomFieldId 
		FROM IntegrationInstanceDataTypeMapping IIM
		INNER JOIN IntegrationInstance II ON II.IntegrationInstanceId = IIM.IntegrationInstanceId
		INNER JOIN CustomField C ON C.Name = 'Geography' AND C.ClientId = II.ClientId
		WHERE IIM.GameplanDataTypeId IN (
											SELECT GameplanDataTypeId 
											FROM GameplanDataType 
											WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('GeographyId')
										) 
			AND IIM.GameplanDataTypeId IS NOT NULL AND IIM.CustomFieldId IS NULL

		DELETE FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('GeographyId')
	END
	
	IF(@@ERROR > 0)
	BEGIN
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY()
		ROLLBACK TRANSACTION Geo
		END
	ELSE
	BEGIN
		COMMIT TRANSACTION Geo
	END

	END TRY
	BEGIN CATCH 
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY(), 'Geography'
		ROLLBACK TRANSACTION Geo
	END CATCH

	--============================================================= BusinessUnit ============================================================================
	BEGIN TRY
	
	BEGIN TRANSACTION BusinessUnit
	PRINT(1)		
	IF EXISTS (SELECT GameplanDataTypeId FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('BusinessUnitId'))
	BEGIN
		PRINT(2)		
		UPDATE IIM SET IIM.CustomFieldId = C.CustomFieldId, IIM.GameplanDataTypeId = NULL
		--SELECT IIM.*, II.ClientId, C.CustomFieldId 
		FROM IntegrationInstanceDataTypeMapping IIM
		INNER JOIN IntegrationInstance II ON II.IntegrationInstanceId = IIM.IntegrationInstanceId
		INNER JOIN CustomField C ON C.Name = 'BusinessUnit' AND C.ClientId = II.ClientId
		WHERE IIM.GameplanDataTypeId IN (
											SELECT GameplanDataTypeId 
											FROM GameplanDataType 
											WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('BusinessUnitId')
										) 
			AND IIM.GameplanDataTypeId IS NOT NULL AND IIM.CustomFieldId IS NULL

		DELETE FROM GameplanDataType WHERE IsDeleted = 0 AND TableName = 'Plan_Campaign_Program_Tactic' AND ActualFieldName IN ('BusinessUnitId')
	END
	
	IF(@@ERROR > 0)
	BEGIN
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY()
		ROLLBACK TRANSACTION BusinessUnit
		END
	ELSE
	BEGIN
		COMMIT TRANSACTION BusinessUnit
	END

	END TRY
	BEGIN CATCH
		SELECT ERROR_MESSAGE(), ERROR_SEVERITY(), 'BusinessUnit'
		ROLLBACK TRANSACTION BusinessUnit
	END CATCH
END


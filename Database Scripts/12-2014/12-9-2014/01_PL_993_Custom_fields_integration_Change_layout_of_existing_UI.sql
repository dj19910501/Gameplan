-- ======================================================================================
-- Created By : Sohel Pathan
-- Created Date : 09/12/2014
-- Description : To delete record with ActualFieldName = Revenue
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType')
BEGIN
	
	IF EXISTS (SELECT 1 FROM GameplanDataType WHERE IsDeleted = 0 AND ActualFieldName = 'Revenue' AND DisplayFieldName = 'Revenue')
	BEGIN
	
		IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping')
		BEGIN
	
			IF EXISTS (SELECT 1 FROM IntegrationInstanceDataTypeMapping WHERE GameplanDataTypeId IN (
						SELECT GameplanDataTypeId FROM GameplanDataType WHERE IsDeleted = 0 AND ActualFieldName = 'Revenue' AND DisplayFieldName = 'Revenue'))
			BEGIN
	
				DELETE FROM IntegrationInstanceDataTypeMapping WHERE GameplanDataTypeId IN (
				SELECT GameplanDataTypeId FROM GameplanDataType WHERE IsDeleted = 0 AND ActualFieldName = 'Revenue' AND DisplayFieldName = 'Revenue')
	
			END
	
		END

		DELETE FROM GameplanDataType WHERE IsDeleted = 0 AND ActualFieldName = 'Revenue' AND DisplayFieldName = 'Revenue'

	END

END

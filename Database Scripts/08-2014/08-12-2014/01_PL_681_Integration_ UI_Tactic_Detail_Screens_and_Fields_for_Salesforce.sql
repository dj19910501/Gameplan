-- =======================================================================================
-- Created By :- Sohel Pathan
-- Created Date :- 12/08/2014
-- Description :- Delete record where IsStage = 1 from GameplanDataType table and also remove its reference records. Also remove IsStage field
-- NOTE :- Run this script on 'MRP' DB.
-- =================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping') AND
   EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType')
BEGIN

	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GameplanDataType' AND COLUMN_NAME = 'IsStage')
	BEGIN

		DELETE 
		FROM IntegrationInstanceDataTypeMapping 
		WHERE GameplanDataTypeId IN (
									SELECT GameplanDataTypeId 
									FROM GameplanDataType 
									WHERE IsStage = 1
									)

		DELETE
		FROM GameplanDataType 
		WHERE IsStage = 1							

		IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name = 'DF_GameplanDataType_IsStage' AND [type] = 'D' AND parent_object_id IN 
				(SELECT object_id FROM sys.objects WHERE name = 'GameplanDataType' AND [type] = 'U' AND [schema_id] = '1'))
		BEGIN
				ALTER TABLE GameplanDataType DROP CONSTRAINT DF_GameplanDataType_IsStage
		END

		ALTER TABLE GameplanDataType DROP COLUMN IsStage

	END

END
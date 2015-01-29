-- ======================================================================================
-- Created By : Sohel Pathan
-- Created Date : 29/01/2015
-- Description : Insert Tactic Type data type entry into GameplanDataType Table
-- ======================================================================================

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GameplanDataType') AND
	EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IntegrationType')
BEGIN

	IF NOT EXISTS (SELECT 1 FROM GameplanDataType WHERE TableName = 'Plan_Campaign_Program_Tactic' AND IsDeleted = 0 AND ActualFieldName = 'TacticType' AND DisplayFieldName = 'Tactic Type')
	BEGIN

		INSERT INTO GameplanDataType(IntegrationTypeId, TableName, ActualFieldName, DisplayFieldName, IsGet, IsDeleted, IsImprovement)
		SELECT
		IntegrationTypeId, 'Plan_Campaign_Program_Tactic', 'TacticType', 'Tactic Type', 0, 0, 0
		FROM IntegrationType WHERE IsDeleted = 0
	
	END

END
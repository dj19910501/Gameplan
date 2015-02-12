-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 12/02/2015
-- Description : Remove business unit reference from GameplanDataType and IntegrationInstanceDataTypeMapping table in gameplan database
-- ======================================================================================
BEGIN TRY
		BEGIN TRANSACTION
				DELETE FROM dbo.IntegrationInstanceDataTypeMapping WHERE GameplanDataTypeId IN (  SELECT GameplanDataTypeId FROM dbo.GameplanDataType WHERE TableName='Plan_Improvement_Campaign_Program_Tactic' AND ActualFieldName='BusinessUnitId')
				DELETE FROM dbo.GameplanDataType WHERE TableName='Plan_Improvement_Campaign_Program_Tactic' AND ActualFieldName='BusinessUnitId'
		COMMIT
END TRY

BEGIN CATCH
		IF @@TRANCOUNT>0
		BEGIN
		     ROLLBACK
		END
END CATCH
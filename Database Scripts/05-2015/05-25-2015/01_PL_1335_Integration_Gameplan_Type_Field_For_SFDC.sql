-- Created By : Mitesh Vaishnav
-- Created Date : 05/25/2015
-- Description :Add a mapping field for SFDC - Activity Type
-- ======================================================================================
GO
IF (EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType') AND EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationType'))
BEGIN
	INSERT INTO dbo.GameplanDataType
			(IntegrationTypeId, TableName, ActualFieldName, DisplayFieldName,
			 IsGet, IsDeleted, IsImprovement)
             SELECT IntegrationTypeId,'Global','ActivityType','Activity Type',0,0,0 FROM dbo.IntegrationType WHERE Code='Salesforce' AND IntegrationTypeId NOT IN (SELECT IntegrationTypeId FROM dbo.GameplanDataType WHERE ActualFieldName='ActivityType')
END
GO
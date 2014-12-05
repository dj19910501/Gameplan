-- ======================================================================================
-- Created By : Sohel Pathan
-- Created Date : 02/12/2014
-- Description : set global table name for common gameplandatatype fields.
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping') AND
	EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping' AND COLUMN_NAME = 'GameplanDataTypeId' AND IS_NULLABLE = 'NO') 
BEGIN
	ALTER TABLE IntegrationInstanceDataTypeMapping ALTER COLUMN GameplanDataTypeId INT NULL
END

GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping') AND
	NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping' AND COLUMN_NAME = 'CustomFieldId') 
BEGIN
	ALTER TABLE IntegrationInstanceDataTypeMapping ADD CustomFieldId INT NULL
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping') AND
	EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomField') AND
	NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME = 'FK_IntegrationInstanceDataTypeMapping_CustomField_CustomFieldId') 
BEGIN
	ALTER TABLE [dbo].[IntegrationInstanceDataTypeMapping]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceDataTypeMapping_CustomField_CustomFieldId] FOREIGN KEY([CustomFieldId])
	REFERENCES [dbo].[CustomField] ([CustomFieldId])
END

GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType') AND
	NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType' AND COLUMN_NAME = 'IsImprovement') 
BEGIN
	ALTER TABLE GameplanDataType ADD IsImprovement BIT NOT NULL DEFAULT(0)
END

GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Improvement_Campaign') AND
	NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Improvement_Campaign' AND COLUMN_NAME = 'Description') 
BEGIN
	ALTER TABLE Plan_Improvement_Campaign ADD Description VARCHAR(4000) NULL
END

GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Improvement_Campaign_Program') AND
	NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Improvement_Campaign_Program' AND COLUMN_NAME = 'Description') 
BEGIN
	ALTER TABLE Plan_Improvement_Campaign_Program ADD Description VARCHAR(4000) NULL
END

GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMapping') AND
	EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType') 
BEGIN
	
	IF NOT EXISTS(SELECT 1 FROM GameplanDataType WHERE TableName = 'Global' AND IsDeleted = 0)
	BEGIN
		
		-- ================= Delete mapping for duplicate fields from IntegrationInstanceDataTypeMapping table for global fields ============================
		DELETE from IntegrationInstanceDataTypeMapping WHERE GameplanDataTypeId IN(
		select GameplanDataTypeId from GameplanDataType
		where IsDeleted = 0
		AND TableName In ('Plan_Campaign','Plan_Campaign_Program', 'Plan_Improvement_Campaign', 'Plan_Improvement_Campaign_Program', 'Plan_Improvement_Campaign_Program_Tactic')
		AND ActualFieldName IN ('Description', 'EndDate', 'Title', 'CreatedBy', 'StartDate', 'Status')
		)

		-- ================= Delete datatypes from GameplanDataType table with global fields of all tables except Plan_Campaign_Program_Tactic ============================
		DELETE from GameplanDataType 
		where IsDeleted = 0
		AND TableName In ('Plan_Campaign','Plan_Campaign_Program', 'Plan_Improvement_Campaign', 'Plan_Improvement_Campaign_Program', 'Plan_Improvement_Campaign_Program_Tactic')
		AND ActualFieldName IN ('Description', 'EndDate', 'Title', 'CreatedBy', 'StartDate', 'Status') -- 16
		
		-- ============= Set TableName = Global for Global fields ===========================
		UPDATE GameplanDataType  SET TableName = 'Global'
		from GameplanDataType 
		where IsDeleted = 0
		AND TableName IN ('Plan_Campaign_Program_Tactic')
		AND ActualFieldName IN ('Description', 'EndDate', 'Title', 'CreatedBy', 'StartDate', 'Status') -- 11

		-- ============= Set IsImprovement = 1 for Global fields of Improvement Section ===========================
		UPDATE GameplanDataType  SET IsImprovement = 1
		from GameplanDataType
		where IsDeleted = 0 AND ISNULL(IsImprovement, 0) = 0
		AND TableName = 'Global'
		AND ActualFieldName IN ('Description', 'Title', 'Status')

	END
END

GO


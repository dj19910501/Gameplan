IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'CustomField_Entity_StageWeight')
BEGIN
	IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomField_Entity_StageWeight_CustomField_Entity_StageWeight]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomField_Entity_StageWeight]'))
	BEGIN
		ALTER TABLE [dbo].[CustomField_Entity_StageWeight] DROP CONSTRAINT [FK_CustomField_Entity_StageWeight_CustomField_Entity_StageWeight]
	END
END

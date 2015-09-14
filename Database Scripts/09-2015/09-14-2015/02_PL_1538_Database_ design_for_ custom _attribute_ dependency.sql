-- ======================================================================================
-- Created By : Komal Rawal
-- Created Date : 14/11/2015
-- Description : Add new column to custom field dependency table
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldDependency')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'FieldType' AND [object_id] = OBJECT_ID(N'CustomFieldDependency'))
           BEGIN
               ALTER TABLE CustomFieldDependency Add
               FieldType nvarchar(50) NOT NULL 
           END

		    IF  EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Abbreviation' AND [object_id] = OBJECT_ID(N'ParentCustomFieldId'))
			 BEGIN
		   ALTER TABLE [CustomFieldDependency] ALTER COLUMN [ParentCustomFieldId] INTEGER NULL;
		    END
END


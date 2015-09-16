-- ======================================================================================
-- Created By : Komal Rawal
-- Created Date : 14/11/2015
-- Description : Modify column for custom field dependency table
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldDependency')
BEGIN

		    IF  EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ParentCustomFieldId' AND [object_id] = OBJECT_ID(N'CustomFieldDependency'))
			 BEGIN
		   ALTER TABLE [CustomFieldDependency] ALTER COLUMN [ParentCustomFieldId] INTEGER NULL;
		    END
END


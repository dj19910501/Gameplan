-- ======================================================================================
-- Created By : Komal Rawal
-- Created Date : 24/09/2015
-- Description : Add column and unique key for custom field dependency table
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldDependency')
BEGIN

		    IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'DependencyId' AND [object_id] = OBJECT_ID(N'CustomFieldDependency'))
			 BEGIN
		     ALTER TABLE [dbo].[CustomFieldDependency] ADD  DependencyId int NOT NULL IDENTITY (1, 1)
			 CONSTRAINT [PK_CustomFieldDependency] PRIMARY KEY CLUSTERED (DependencyId)
		    END
END

Go

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldDependency')
BEGIN
  IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = 'IX_CustomFieldDependency')
  BEGIN
ALTER TABLE [dbo].[CustomFieldDependency] ADD  CONSTRAINT [IX_CustomFieldDependency] UNIQUE NONCLUSTERED 
(
	[ChildCustomFieldId] ASC,
	[ChildOptionId] ASC,
	[ParentCustomFieldId] ASC,
	[ParentOptionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
END
END



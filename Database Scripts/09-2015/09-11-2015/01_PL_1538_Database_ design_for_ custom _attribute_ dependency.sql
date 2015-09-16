-- ======================================================================================
-- Created By : Komal Rawal
-- Created Date : 09/11/2015
-- Description : Add new table for custom field dependency feature
-- ======================================================================================

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldDependency')
BEGIN
        CREATE TABLE [dbo].[CustomFieldDependency](
	[ParentCustomFieldId] [int] NOT NULL,
	[ParentOptionId] [int] NOT NULL,
	[ChildCustomFieldId] [int] NOT NULL,
	[ChildOptionId] [int] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL CONSTRAINT [DF_CustomFieldDependency_IsDeleted]  DEFAULT ((0))
) ON [PRIMARY]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N' Refers to associated CustomFieldId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldDependency', @level2type=N'COLUMN',@level2name=N'ParentCustomFieldId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated CustomFieldOptionId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldDependency', @level2type=N'COLUMN',@level2name=N'ParentOptionId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N' Refers to associated CustomFieldId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldDependency', @level2type=N'COLUMN',@level2name=N'ChildCustomFieldId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'If customField will be dropdown then it Refers to associated CustomFieldOptionId else it will be NULL.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldDependency', @level2type=N'COLUMN',@level2name=N'ChildOptionId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldDependency', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldDependency', @level2type=N'COLUMN',@level2name=N'CreatedBy'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Flag to identify record is deleted or not.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldDependency', @level2type=N'COLUMN',@level2name=N'IsDeleted'

END

GO
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_CustomFieldDependency_CustomField')
   AND parent_object_id = OBJECT_ID(N'dbo.CustomFieldDependency')
)
BEGIN

ALTER TABLE [dbo].[CustomFieldDependency]  WITH CHECK ADD  CONSTRAINT [FK_CustomFieldDependency_CustomField] FOREIGN KEY([ParentCustomFieldId])
REFERENCES [dbo].[CustomField] ([CustomFieldId])

END

IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_CustomFieldDependency_CustomField1')
   AND parent_object_id = OBJECT_ID(N'dbo.CustomFieldDependency')
)
BEGIN

ALTER TABLE [dbo].[CustomFieldDependency]  WITH CHECK ADD  CONSTRAINT [FK_CustomFieldDependency_CustomField1] FOREIGN KEY([ChildCustomFieldId])
REFERENCES [dbo].[CustomField] ([CustomFieldId])

END


IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_CustomFieldDependency_CustomFieldOption1')
   AND parent_object_id = OBJECT_ID(N'dbo.CustomFieldDependency')
)
BEGIN

ALTER TABLE [dbo].[CustomFieldDependency]  WITH CHECK ADD  CONSTRAINT [FK_CustomFieldDependency_CustomFieldOption1] FOREIGN KEY([ChildOptionId])
REFERENCES [dbo].[CustomFieldOption] ([CustomFieldOptionId])

END
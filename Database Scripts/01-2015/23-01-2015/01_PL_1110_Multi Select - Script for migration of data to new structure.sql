--------------- Start: Add Column 'Weightage' to Table [dbo].[CustomField_Entity] --------------- 
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'Weightage' AND [object_id] = OBJECT_ID(N'[dbo].[CustomField_Entity]'))
BEGIN
    ALTER TABLE [dbo].[CustomField_Entity]
	ADD Weightage tinyint NULL
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Attribute wise weight of entity.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'Weightage'
END
IF (SELECT Count(*) FROM CustomField_Entity WHERE Weightage > 0) = 0
BEGIN
	Update [dbo].[CustomField_Entity] Set Weightage = 100
END
GO
--------------- End: Add Column 'Weightage' to Table [dbo].[CustomField_Entity] --------------- 

--------------- Start: Create New Table 'CustomField_Entity_StageWeight' to Database ---------------

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomField_Entity_StageWeight]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CustomField_Entity_StageWeight](
	[StageWeightId] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomFieldEntityId] [int] NOT NULL,
	[StageTitle] [nvarchar](50) NOT NULL,
	[Weightage] [tinyint] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_CustomField_Entity_StageWeight] PRIMARY KEY CLUSTERED 
(
	[StageWeightId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomField_Entity_StageWeight_CustomField_Entity_StageWeight]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomField_Entity_StageWeight]'))
ALTER TABLE [dbo].[CustomField_Entity_StageWeight]  WITH CHECK ADD  CONSTRAINT [FK_CustomField_Entity_StageWeight_CustomField_Entity_StageWeight] FOREIGN KEY([CustomFieldEntityId])
REFERENCES [dbo].[CustomField_Entity] ([CustomFieldEntityId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomField_Entity_StageWeight_CustomField_Entity_StageWeight]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomField_Entity_StageWeight]'))
ALTER TABLE [dbo].[CustomField_Entity_StageWeight] CHECK CONSTRAINT [FK_CustomField_Entity_StageWeight_CustomField_Entity_StageWeight]
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'CustomField_Entity_StageWeight', N'COLUMN',N'StageWeightId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity_StageWeight', @level2type=N'COLUMN',@level2name=N'StageWeightId'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'CustomField_Entity_StageWeight', N'COLUMN',N'CustomFieldEntityId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK- Refers to associated CustomFieldEntityId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity_StageWeight', @level2type=N'COLUMN',@level2name=N'CustomFieldEntityId'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'CustomField_Entity_StageWeight', N'COLUMN',N'StageTitle'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Title of Stage.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity_StageWeight', @level2type=N'COLUMN',@level2name=N'StageTitle'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'CustomField_Entity_StageWeight', N'COLUMN',N'Weightage'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Weight of stage for particular entity.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity_StageWeight', @level2type=N'COLUMN',@level2name=N'Weightage'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'CustomField_Entity_StageWeight', N'COLUMN',N'CreatedDate'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity_StageWeight', @level2type=N'COLUMN',@level2name=N'CreatedDate'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'CustomField_Entity_StageWeight', N'COLUMN',N'CreatedBy'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity_StageWeight', @level2type=N'COLUMN',@level2name=N'CreatedBy'
GO
--------------- End: Create New Table 'CustomField_Entity_StageWeight' to Database --------------- 

--------------- Start: Add Columns 'Description'&'ColorCode' to Table [dbo].[CustomFieldOption] ---------------
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'Description' AND [object_id] = OBJECT_ID(N'[dbo].[CustomFieldOption]'))
BEGIN
    ALTER TABLE [dbo].[CustomFieldOption]
	ADD [Description] nvarchar(4000) NULL
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Description of CustomField.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldOption', @level2type=N'COLUMN',@level2name=N'Description'
END
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'ColorCode' AND [object_id] = OBJECT_ID(N'[dbo].[CustomFieldOption]'))
BEGIN
    ALTER TABLE [dbo].[CustomFieldOption]
	ADD [ColorCode] nvarchar(10) NULL
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Color Code of CustomField.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldOption', @level2type=N'COLUMN',@level2name=N'ColorCode'
END
--------------- End: Add Columns 'Description'&'ColorCode' to Table [dbo].[CustomFieldOption] ---------------
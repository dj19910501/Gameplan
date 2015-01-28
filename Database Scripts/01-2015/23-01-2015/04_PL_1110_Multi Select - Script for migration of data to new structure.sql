/* Execute this script on MRP database */

/****** Object:  Table [dbo].[CustomRestriction]    Script Date: 01/15/2015 12:56:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomRestriction]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CustomRestriction](
	[CustomRestrictionId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[CustomFieldId] [int] NOT NULL,
	[CustomFieldOptionId] [int] NOT NULL,
	[Permission] [smallint] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_CustomRestriction] PRIMARY KEY CLUSTERED 
(
	[CustomRestrictionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_CustomRestriction_Permission]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[CustomRestriction] ADD  CONSTRAINT [DF_CustomRestriction_Permission]  DEFAULT ((0)) FOR [Permission]
END

GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomRestriction_CustomField]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomRestriction]'))
ALTER TABLE [dbo].[CustomRestriction]  WITH CHECK ADD  CONSTRAINT [FK_CustomRestriction_CustomField] FOREIGN KEY([CustomFieldId])
REFERENCES [dbo].[CustomField] ([CustomFieldId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomRestriction_CustomField]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomRestriction]'))
ALTER TABLE [dbo].[CustomRestriction] CHECK CONSTRAINT [FK_CustomRestriction_CustomField]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomRestriction_CustomFieldOption]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomRestriction]'))
ALTER TABLE [dbo].[CustomRestriction]  WITH CHECK ADD  CONSTRAINT [FK_CustomRestriction_CustomFieldOption] FOREIGN KEY([CustomFieldOptionId])
REFERENCES [dbo].[CustomFieldOption] ([CustomFieldOptionId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomRestriction_CustomFieldOption]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomRestriction]'))
ALTER TABLE [dbo].[CustomRestriction] CHECK CONSTRAINT [FK_CustomRestriction_CustomFieldOption]
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'CustomRestriction', N'COLUMN',N'CustomRestrictionId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomRestriction', @level2type=N'COLUMN',@level2name=N'CustomRestrictionId'
GO

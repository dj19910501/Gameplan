--- Created By Komal Rawal
--- Date: 24-Nov-2015
--- Desc: To save last accessed views
GO

/****** Object:  Table [dbo].[Plan_UserSavedViews]    Script Date: 11/24/2015 19:43:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_UserSavedViews')
BEGIN
CREATE TABLE [dbo].[Plan_UserSavedViews](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ViewName] [nvarchar](max) NULL,
	[FilterName] [nvarchar](250) NULL,
	[FilterValues] [nvarchar](max) NULL,
	[Userid] [uniqueidentifier] NULL,
	[LastModifiedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Plan_UserSavedViews] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]




EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique identifier' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_UserSavedViews', @level2type=N'COLUMN',@level2name=N'Id'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of the view' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_UserSavedViews', @level2type=N'COLUMN',@level2name=N'ViewName'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of the field ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_UserSavedViews', @level2type=N'COLUMN',@level2name=N'FilterName'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Comma Separated Values selected in each filter' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_UserSavedViews', @level2type=N'COLUMN',@level2name=N'FilterValues'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N' User ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_UserSavedViews', @level2type=N'COLUMN',@level2name=N'Userid'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Modified date' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_UserSavedViews', @level2type=N'COLUMN',@level2name=N'LastModifiedDate'

End
Go


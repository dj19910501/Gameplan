
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomLabel')
BEGIN

CREATE TABLE [dbo].[CustomLabel](
	[CustomLabelId] [int] IDENTITY(1,1) NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[Title] [nvarchar](50) NULL,
	[Code] [nvarchar](50) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [uniqueidentifier] NULL,
 CONSTRAINT [PK_CustomLabel] PRIMARY KEY CLUSTERED 
(
	[CustomLabelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_CustomLabel] UNIQUE NONCLUSTERED 
(
	[ClientId] ASC,
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify Custom Label for a client.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomLabel', @level2type=N'COLUMN',@level2name=N'CustomLabelId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to the associated Client.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomLabel', @level2type=N'COLUMN',@level2name=N'ClientId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Title of custom label.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomLabel', @level2type=N'COLUMN',@level2name=N'Title'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Code of custom label. I.e. Audience,Vertical etc. Here code should be same as table name.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomLabel', @level2type=N'COLUMN',@level2name=N'Code'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomLabel', @level2type=N'COLUMN',@level2name=N'CreatedDate'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomLabel', @level2type=N'COLUMN',@level2name=N'CreatedBy'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was last modified.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomLabel', @level2type=N'COLUMN',@level2name=N'ModifiedDate'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomLabel', @level2type=N'COLUMN',@level2name=N'ModifiedBy'

END



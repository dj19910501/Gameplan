
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataTypePull')
BEGIN

CREATE TABLE [dbo].[GameplanDataTypePull](
	[GameplanDataTypePullId] [int] IDENTITY(1,1) NOT NULL,
	[IntegrationTypeId] [int] NOT NULL,
	[ActualFieldName] [nvarchar](255) NOT NULL,
	[DisplayFieldName] [nvarchar](255) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_GameplanDataTypePull] PRIMARY KEY CLUSTERED 
(
	[GameplanDataTypePullId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[GameplanDataTypePull] ADD  CONSTRAINT [DF_GameplanDataTypePull_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]

ALTER TABLE [dbo].[GameplanDataTypePull]  WITH CHECK ADD  CONSTRAINT [FK_GameplanDataTypePull_IntegrationType] FOREIGN KEY([IntegrationTypeId])
REFERENCES [dbo].[IntegrationType] ([IntegrationTypeId])

ALTER TABLE [dbo].[GameplanDataTypePull] CHECK CONSTRAINT [FK_GameplanDataTypePull_IntegrationType]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify Gameplan Data Type Pull.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GameplanDataTypePull', @level2type=N'COLUMN',@level2name=N'GameplanDataTypePullId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated IntegrationTypeId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GameplanDataTypePull', @level2type=N'COLUMN',@level2name=N'IntegrationTypeId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Actual Field Name.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GameplanDataTypePull', @level2type=N'COLUMN',@level2name=N'ActualFieldName'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Display Field Name.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GameplanDataTypePull', @level2type=N'COLUMN',@level2name=N'DisplayFieldName'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Pull Type. Pull Type can be INQ, MQL, CW.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GameplanDataTypePull', @level2type=N'COLUMN',@level2name=N'Type'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Flag to identify record is deleted or not.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'GameplanDataTypePull', @level2type=N'COLUMN',@level2name=N'IsDeleted'

END
GO


IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceDataTypeMappingPull')
BEGIN

CREATE TABLE [dbo].[IntegrationInstanceDataTypeMappingPull](
	[IntegrationInstanceDataTypeMappingPullId] [int] IDENTITY(1,1) NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[GameplanDataTypePullId] [int] NOT NULL,
	[TargetDataType] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_IntegrationInstanceDataTypeMappingPull] PRIMARY KEY CLUSTERED 
(
	[IntegrationInstanceDataTypeMappingPullId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[IntegrationInstanceDataTypeMappingPull]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceDataTypeMappingPull_GameplanDataTypePull] FOREIGN KEY([GameplanDataTypePullId])
REFERENCES [dbo].[GameplanDataTypePull] ([GameplanDataTypePullId])

ALTER TABLE [dbo].[IntegrationInstanceDataTypeMappingPull] CHECK CONSTRAINT [FK_IntegrationInstanceDataTypeMappingPull_GameplanDataTypePull]

ALTER TABLE [dbo].[IntegrationInstanceDataTypeMappingPull]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceDataTypeMappingPull_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])

ALTER TABLE [dbo].[IntegrationInstanceDataTypeMappingPull] CHECK CONSTRAINT [FK_IntegrationInstanceDataTypeMappingPull_IntegrationInstance]


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify IntegrationInstance DataType Mapping Pull.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceDataTypeMappingPull', @level2type=N'COLUMN',@level2name=N'IntegrationInstanceDataTypeMappingPullId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated IntegrationInstanceId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceDataTypeMappingPull', @level2type=N'COLUMN',@level2name=N'IntegrationInstanceId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated GameplanDataTypePullId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceDataTypeMappingPull', @level2type=N'COLUMN',@level2name=N'GameplanDataTypePullId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Target DataType.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceDataTypeMappingPull', @level2type=N'COLUMN',@level2name=N'TargetDataType'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceDataTypeMappingPull', @level2type=N'COLUMN',@level2name=N'CreatedDate'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceDataTypeMappingPull', @level2type=N'COLUMN',@level2name=N'CreatedBy'

END


IF NOT((SELECT COUNT(*) FROM GameplanDataTypePull) > 0)
BEGIN

	Declare @IntegrationTypeId int
	select TOP 1 @IntegrationTypeId  = IntegrationTypeId from IntegrationType where Title='Salesforce'

	IF (@IntegrationTypeId > 0 AND @IntegrationTypeId IS NOT NULL)
	BEGIN
	INSERT [dbo].[GameplanDataTypePull] ([IntegrationTypeId], [ActualFieldName], [DisplayFieldName], [Type], [IsDeleted]) VALUES (@IntegrationTypeId, N'Stage', N'Stage', N'CW', 0)
	INSERT [dbo].[GameplanDataTypePull] ([IntegrationTypeId], [ActualFieldName], [DisplayFieldName], [Type], [IsDeleted]) VALUES (@IntegrationTypeId, N'Timestamp', N'Timestamp', N'CW', 0)
	INSERT [dbo].[GameplanDataTypePull] ([IntegrationTypeId], [ActualFieldName], [DisplayFieldName], [Type], [IsDeleted]) VALUES (@IntegrationTypeId, N'CampaignID', N'Campaign ID', N'CW', 0)
	INSERT [dbo].[GameplanDataTypePull] ([IntegrationTypeId], [ActualFieldName], [DisplayFieldName], [Type], [IsDeleted]) VALUES (@IntegrationTypeId, N'Amount', N'Revenue Amount', N'CW', 0)
	INSERT [dbo].[GameplanDataTypePull] ([IntegrationTypeId], [ActualFieldName], [DisplayFieldName], [Type], [IsDeleted]) VALUES (@IntegrationTypeId, N'Status', N'Status', N'INQ', 0)
	INSERT [dbo].[GameplanDataTypePull] ([IntegrationTypeId], [ActualFieldName], [DisplayFieldName], [Type], [IsDeleted]) VALUES (@IntegrationTypeId, N'Timestamp', N'Timestamp', N'INQ', 0)
	INSERT [dbo].[GameplanDataTypePull] ([IntegrationTypeId], [ActualFieldName], [DisplayFieldName], [Type], [IsDeleted]) VALUES (@IntegrationTypeId, N'CampaignID', N'CampaignID', N'INQ', 0)
	END
END


-- Below script creates table as given below if they don't exists.
-- IntegrationType, IntegrationInstance, SyncFrequency, IntegrationTypeAttribute, IntegrationInstance_Attribute, IntegrationInstanceLog


-- ================================================ Create 'IntegrationType' table ======================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'IntegrationType')
BEGIN
	CREATE TABLE [dbo].[IntegrationType](
		[IntegrationTypeId] [int] IDENTITY(1,1) NOT NULL,
		[Title] [nvarchar](255) NOT NULL,
		[Description] [nvarchar](4000) NULL,
		[IsDeleted] [bit] NOT NULL,
		[APIVersion] [nvarchar](25) NULL,
		[APIURL] [nvarchar](1000) NULL,
	 CONSTRAINT [PK_IntegrationType] PRIMARY KEY CLUSTERED 
	(
		[IntegrationTypeId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[IntegrationType] ADD  CONSTRAINT [DF_IntegrationType_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
END

-- ================================================ Create 'IntegrationInstance' table ======================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'IntegrationInstance')
BEGIN
	CREATE TABLE [dbo].[IntegrationInstance](
		[IntegrationInstanceId] [int] IDENTITY(1,1) NOT NULL,
		[IntegrationTypeId] [int] NOT NULL,
		[ClientId] [uniqueidentifier] NOT NULL,
		[Instance] [nvarchar](255) NOT NULL,
		[Username] [nvarchar](255) NOT NULL,
		[Password] [nvarchar](255) NOT NULL,
		[IsActive] [bit] NOT NULL,
		[LastSyncDate] [datetime] NULL,
		[LastSyncStatus] [nvarchar](50) NULL,
		[IsImportActuals] [bit] NOT NULL,
		[IsDeleted] [bit] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
		[ModifiedDate] [datetime] NULL,
		[ModifiedBy] [uniqueidentifier] NULL,
	 CONSTRAINT [PK_IntegrationInstance] PRIMARY KEY CLUSTERED 
	(
		[IntegrationInstanceId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[IntegrationInstance] ADD  CONSTRAINT [DF_IntegrationInstance_IsIntegrationActive]  DEFAULT ((1)) FOR [IsActive]
	ALTER TABLE [dbo].[IntegrationInstance] ADD  CONSTRAINT [DF_IntegrationInstance_IsImportActuals]  DEFAULT ((0)) FOR [IsImportActuals]
	ALTER TABLE [dbo].[IntegrationInstance] ADD  CONSTRAINT [DF_IntegrationInstance_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]

	ALTER TABLE [dbo].[IntegrationInstance]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstance_IntegrationType] FOREIGN KEY([IntegrationTypeId])
	REFERENCES [dbo].[IntegrationType] ([IntegrationTypeId])
	
	ALTER TABLE [dbo].[IntegrationInstance] CHECK CONSTRAINT [FK_IntegrationInstance_IntegrationType]
END

-- ================================================ Create 'SyncFrequency' table ======================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'SyncFrequency')
BEGIN
	CREATE TABLE [dbo].[SyncFrequency](
		[IntegrationInstanceId] [int] NOT NULL,
		[Frequency] [nvarchar](50) NOT NULL,
		[Time] [time](7) NULL,
		[DayofWeek] [nvarchar](50) NULL,
		[Day] [nvarchar](255) NULL,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_SyncFrequency] PRIMARY KEY CLUSTERED 
	(
		[IntegrationInstanceId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[SyncFrequency]  WITH CHECK ADD  CONSTRAINT [FK_SyncFrequency_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
	REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])

	ALTER TABLE [dbo].[SyncFrequency] CHECK CONSTRAINT [FK_SyncFrequency_IntegrationInstance]
END 

-- ================================================ Create 'IntegrationTypeAttribute' table ======================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'IntegrationTypeAttribute')
BEGIN
	CREATE TABLE [dbo].[IntegrationTypeAttribute](
		[IntegrationTypeAttributeId] [int] IDENTITY(1,1) NOT NULL,
		[IntegrationTypeId] [int] NOT NULL,
		[Attribute] [nvarchar](255) NOT NULL,
		[AttributeType] [nvarchar](255) NOT NULL,
		[IsDeleted] [bit] NOT NULL,
	 CONSTRAINT [PK_IntegrationTypeAttribute] PRIMARY KEY CLUSTERED 
	(
		[IntegrationTypeAttributeId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[IntegrationTypeAttribute] ADD  CONSTRAINT [DF_IntegrationTypeAttribute_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]

	ALTER TABLE [dbo].[IntegrationTypeAttribute]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationTypeAttribute_IntegrationType] FOREIGN KEY([IntegrationTypeId])
	REFERENCES [dbo].[IntegrationType] ([IntegrationTypeId])

	ALTER TABLE [dbo].[IntegrationTypeAttribute] CHECK CONSTRAINT [FK_IntegrationTypeAttribute_IntegrationType]
END

-- ================================================ Create 'IntegrationInstance_Attribute' table ======================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'IntegrationInstance_Attribute')
BEGIN
	CREATE TABLE [dbo].[IntegrationInstance_Attribute](
		[IntegrationInstanceId] [int] NOT NULL,
		[IntegrationTypeAttributeId] [int] NOT NULL,
		[Value] [nvarchar](255) NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_IntegrationInstance_Attribute] PRIMARY KEY CLUSTERED 
	(
		[IntegrationInstanceId] ASC,
		[IntegrationTypeAttributeId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[IntegrationInstance_Attribute]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstance_Attribute_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
	REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])
	
	ALTER TABLE [dbo].[IntegrationInstance_Attribute] CHECK CONSTRAINT [FK_IntegrationInstance_Attribute_IntegrationInstance]

	ALTER TABLE [dbo].[IntegrationInstance_Attribute]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstance_Attribute_IntegrationTypeAttribute] FOREIGN KEY([IntegrationTypeAttributeId])
	REFERENCES [dbo].[IntegrationTypeAttribute] ([IntegrationTypeAttributeId])

	ALTER TABLE [dbo].[IntegrationInstance_Attribute] CHECK CONSTRAINT [FK_IntegrationInstance_Attribute_IntegrationTypeAttribute]

END

-- ================================================ Create 'IntegrationInstanceLog' table ======================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'IntegrationInstanceLog')
BEGIN
	CREATE TABLE [dbo].[IntegrationInstanceLog](
		[IntegrationInstanceLogId] [int] IDENTITY(1,1) NOT NULL,
		[IntegrationInstanceId] [int] NOT NULL,
		[SyncTimeStamp] [datetime] NOT NULL,
		[Status] [nvarchar](255) NOT NULL,
		[ErrorDescription] [nvarchar](4000) NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NULL,
	 CONSTRAINT [PK_IntegrationInstanceLog] PRIMARY KEY CLUSTERED 
	(
		[IntegrationInstanceLogId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[IntegrationInstanceLog]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceLog_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
	REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])

	ALTER TABLE [dbo].[IntegrationInstanceLog] CHECK CONSTRAINT [FK_IntegrationInstanceLog_IntegrationInstance]

END

-- ================================================ Create 'GameplanDataType' table ======================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'GameplanDataType')
BEGIN
	/****** Object:  Table [dbo].[GameplanDataType]    Script Date: 5/12/2014 7:09:39 PM ******/
	SET ANSI_NULLS ON
	SET QUOTED_IDENTIFIER ON

	CREATE TABLE [dbo].[GameplanDataType](
		[GameplanDataTypeId] [int] IDENTITY(1,1) NOT NULL,
		[IntegrationTypeId] [int] NOT NULL,
		[TableName] [nvarchar](255) NOT NULL,
		[ActualFieldName] [nvarchar](255) NOT NULL,
		[DisplayFieldName] [nvarchar](255) NOT NULL,
		[IsGet] [bit] NOT NULL,
		[IsStage] [bit] NOT NULL,
		[IsDeleted] [bit] NOT NULL,
	 CONSTRAINT [PK_GameplanDataType] PRIMARY KEY CLUSTERED 
	(
		[GameplanDataTypeId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[GameplanDataType] ADD  CONSTRAINT [DF_GameplanDataType_IsGet]  DEFAULT ((0)) FOR [IsGet]

	ALTER TABLE [dbo].[GameplanDataType] ADD  CONSTRAINT [DF_GameplanDataType_IsStage]  DEFAULT ((0)) FOR [IsStage]

	ALTER TABLE [dbo].[GameplanDataType] ADD  CONSTRAINT [DF_GameplanDataType_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]

	ALTER TABLE [dbo].[GameplanDataType]  WITH CHECK ADD  CONSTRAINT [FK_GameplanDataType_IntegrationType] FOREIGN KEY([IntegrationTypeId])
	REFERENCES [dbo].[IntegrationType] ([IntegrationTypeId])

	ALTER TABLE [dbo].[GameplanDataType] CHECK CONSTRAINT [FK_GameplanDataType_IntegrationType]
END

-- ================================================ Create 'IntegrationInstanceDataTypeMapping' table ======================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'IntegrationInstanceDataTypeMapping')
BEGIN
	/****** Object:  Table [dbo].[IntegrationInstanceDataTypeMapping]    Script Date: 5/12/2014 7:09:59 PM ******/
	SET ANSI_NULLS ON
	SET QUOTED_IDENTIFIER ON
	CREATE TABLE [dbo].[IntegrationInstanceDataTypeMapping](
		[IntegrationInstanceDataTypeMappingId] [int] IDENTITY(1,1) NOT NULL,
		[IntegrationInstanceId] [int] NOT NULL,
		[GameplanDataTypeId] [int] NOT NULL,
		[TargetDataType] [nvarchar](255) NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_IntegrationInstanceDataTypeMapping] PRIMARY KEY CLUSTERED 
	(
		[IntegrationInstanceDataTypeMappingId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[IntegrationInstanceDataTypeMapping]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceDataTypeMapping_GameplanDataType] FOREIGN KEY([GameplanDataTypeId])
	REFERENCES [dbo].[GameplanDataType] ([GameplanDataTypeId])

	ALTER TABLE [dbo].[IntegrationInstanceDataTypeMapping] CHECK CONSTRAINT [FK_IntegrationInstanceDataTypeMapping_GameplanDataType]

	ALTER TABLE [dbo].[IntegrationInstanceDataTypeMapping]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceDataTypeMapping_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
	REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])

	ALTER TABLE [dbo].[IntegrationInstanceDataTypeMapping] CHECK CONSTRAINT [FK_IntegrationInstanceDataTypeMapping_IntegrationInstance]
END



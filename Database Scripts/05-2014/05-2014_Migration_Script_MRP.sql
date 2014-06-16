
-- ===========================================_01_PL_430_MAP_CRM_Integration_Setup_Screen.sql===========================================
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

	SET IDENTITY_INSERT [dbo].[IntegrationType] ON 

	INSERT [dbo].[IntegrationType] ([IntegrationTypeId], [Title], [Description], [IsDeleted], [APIVersion], [APIURL]) VALUES (1, N'Eloqua', NULL, 0, N'1.0', N'www.login.eloqua.com')
	INSERT [dbo].[IntegrationType] ([IntegrationTypeId], [Title], [Description], [IsDeleted], [APIVersion], [APIURL]) VALUES (2, N'Salesforce', NULL, 0, NULL, N'https://test.salesforce.com/services/oauth2/token')
	INSERT [dbo].[IntegrationType] ([IntegrationTypeId], [Title], [Description], [IsDeleted], [APIVersion], [APIURL]) VALUES (3, N'Marketo', NULL, 0, NULL, NULL)

	SET IDENTITY_INSERT [dbo].[IntegrationType] OFF
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

	SET IDENTITY_INSERT [dbo].[IntegrationTypeAttribute] ON 
	INSERT [dbo].[IntegrationTypeAttribute] ([IntegrationTypeAttributeId], [IntegrationTypeId], [Attribute], [AttributeType], [IsDeleted]) VALUES (1, 2, N'ConsumerKey', N'textbox', 0)
	INSERT [dbo].[IntegrationTypeAttribute] ([IntegrationTypeAttributeId], [IntegrationTypeId], [Attribute], [AttributeType], [IsDeleted]) VALUES (2, 2, N'ConsumerSecret', N'textbox', 0)
	INSERT [dbo].[IntegrationTypeAttribute] ([IntegrationTypeAttributeId], [IntegrationTypeId], [Attribute], [AttributeType], [IsDeleted]) VALUES (3, 2, N'SecurityToken', N'textbox', 0)
	SET IDENTITY_INSERT [dbo].[IntegrationTypeAttribute] OFF
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
			[SyncStart] [datetime] NOT NULL,
			[SyncEnd] [datetime] NULL,
			[Status] [nvarchar](255) NULL,
			[ErrorDescription] [nvarchar](4000) NULL,
			[CreatedDate] [datetime] NULL,
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

	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'Title', N'Gameplan.Tactic.Name', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'Description', N'Gameplan.Tactic.Description', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'StartDate', N'Gameplan.Tactic.StartDate', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'EndDate', N'Gameplan.Tactic.EndDate', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'VerticalId', N'Gameplan.Tactic.Vertical', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'AudienceId', N'Gameplan.Tactic.Audience', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'GeographyId', N'Gameplan.Tactic.Geography', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'Status', N'Gameplan.Tactic.Status', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'Cost', N'Gameplan.Tactic.Cost(Budgeted)', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'CostActual', N'Gameplan.Tactic.Cost(Actual)', 1, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'CreatedBy', N'Gameplan.Tactic.owner', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'BusinessUnitId', N'Gameplan.Tactic.BusinessUnit', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program', N'Title', N'Gameplan.Program.Name', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program', N'Description', N'Gameplan.Program.Description', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program', N'StartDate', N'Gameplan.Program.StartDate', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program', N'EndDate', N'Gameplan.Program.EndDate', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program', N'Status', N'Gameplan.Program.Status', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program', N'CreatedBy', N'Gameplan.Program.owner', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign', N'Title', N'Gameplan.Campaign.Name', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign', N'Description', N'Gameplan.Campaign.Description', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign', N'StartDate', N'Gameplan.Campaign.StartDate', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign', N'EndDate', N'Gameplan.Campaign.EndDate', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign', N'Status', N'Gameplan.Campaign.Status', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign', N'CreatedBy', N'Gameplan.Campaign.owner', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'SUS', N'Gameplan.Tactic.SUS', 1, 1, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'INQ', N'Gameplan.Tactic.INQ', 1, 1, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'AQL', N'Gameplan.Tactic.AQL', 1, 1, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'MQL', N'Gameplan.Tactic.MQL', 1, 1, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'TQL', N'Gameplan.Tactic.TQL', 1, 1, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'SAL', N'Gameplan.Tactic.SAL', 1, 1, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'SQL', N'Gameplan.Tactic.SQL', 1, 1, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'CW', N'Gameplan.Tactic.CW', 1, 1, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Campaign_Program_Tactic', N'Revenue', N'Gameplan.Tactic.Revenue', 1, 1, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Improvement_Campaign_Program_Tactic', N'Title', N'Gameplan.ImprovementTactic.Name', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Improvement_Campaign_Program_Tactic', N'Description', N'Gameplan.ImprovementTactic.Description', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Improvement_Campaign_Program_Tactic', N'EffectiveDate', N'Gameplan.ImprovementTactic.EffectiveDate', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Improvement_Campaign_Program_Tactic', N'Cost', N'Gameplan.ImprovementTactic.Cost', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Improvement_Campaign_Program_Tactic', N'Status', N'Gameplan.ImprovementTactic.Status', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Improvement_Campaign_Program_Tactic', N'BusinessUnitId', N'Gameplan.ImprovementTactic.BusinessUnit', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Improvement_Campaign_Program_Tactic', N'CreatedBy', N'Gameplan.ImprovementTactic.Owner', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Improvement_Campaign_Program', N'Title', N'Gameplan.ImprovementProgram.Name', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Improvement_Campaign_Program', N'CreatedBy', N'Gameplan.ImprovementProgram.Owner', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Improvement_Campaign', N'Title', N'Gameplan.ImprovementCampaign.Name', 0, 0, 0)
	INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (2, N'Plan_Improvement_Campaign', N'CreatedBy', N'Gameplan.ImprovementCampaign.Owner', 0, 0, 0)
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

-- ================================================ Create 'IntegrationInstancePlanEntityLog' table ======================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'IntegrationInstancePlanEntityLog')
BEGIN
	/****** Object:  Table [dbo].[IntegrationInstancePlanEntityLog]    Script Date: 5/16/2014 7:09:59 PM ******/

			CREATE TABLE [dbo].[IntegrationInstancePlanEntityLog](
			[IntegrationInstanceLogId] [int] NOT NULL,
			[IntegrationInstanceId] [int] NOT NULL,
			[EntityId] [int] NOT NULL,
			[EntityType] [varchar](50) NULL,
			[SyncTimeStamp] [datetime] NOT NULL,
			[Operation] [varchar](50) NULL,
			[Status] [nvarchar](255) NULL,
			[ErrorDescription] [nvarchar](4000) NULL,
			[CreatedDate] [datetime] NULL,
			[CreatedBy] [uniqueidentifier] NULL
		) ON [PRIMARY]

		SET ANSI_PADDING OFF

		ALTER TABLE [dbo].[IntegrationInstancePlanEntityLog]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstancePlanEntityLog_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
		REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])

		ALTER TABLE [dbo].[IntegrationInstancePlanEntityLog] CHECK CONSTRAINT [FK_IntegrationInstancePlanEntityLog_IntegrationInstance]

		ALTER TABLE [dbo].[IntegrationInstancePlanEntityLog]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstancePlanEntityLog_IntegrationInstanceLog] FOREIGN KEY([IntegrationInstanceLogId])
		REFERENCES [dbo].[IntegrationInstanceLog] ([IntegrationInstanceLogId])

		ALTER TABLE [dbo].[IntegrationInstancePlanEntityLog] CHECK CONSTRAINT [FK_IntegrationInstancePlanEntityLog_IntegrationInstanceLog]
END


-- ===========================================01_PL_466_Misspelled Word in Approval Email.sql===========================================


UPDATE Notification SET EmailContent = REPLACE(EmailContent,'apporved','approved') 

UPDATE Notification SET EmailContent = REPLACE(EmailContent,'<br>Gameplan Admin','<br>Bulldog Gameplan Admin')

UPDATE Notification SET EmailContent = REPLACE(EmailContent,'<br/>Gameplan Administrator','<br/>Bulldog Gameplan Admin')


																		
-- ===========================================02_PL_458_Still cant delete a model tactic.sql===========================================

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'IsDeleted' AND OBJECT_ID = OBJECT_ID(N'TacticType'))
BEGIN
    ALTER TABLE TacticType ADD IsDeleted bit
END

-- ===========================================03_PL_433_MAP_CRM_Integration_Model_Screen_Tactic_List.sql===========================================
-- Run below script on MRPQA

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IsDeployedToIntegration' AND [object_id] = OBJECT_ID(N'TacticType'))
BEGIN
    Alter table [TacticType] 
	ADD IsDeployedToIntegration bit NOT NULL
	CONSTRAINT [DF_TacticType_IsDeployedToIntegration] DEFAULT 0
END

-- ===========================================04_PL_435_MAP_CRM_Integration_Tactic_Creation.sql===========================================
-- Run below script on MRPQA

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IsDeployedToIntegration' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN
    Alter table [Plan_Campaign] 
	ADD IsDeployedToIntegration bit NOT NULL
	CONSTRAINT [DF_Plan_Campaign_IsDeployedToIntegration] DEFAULT 0
END

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IntegrationInstanceCampaignId' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN
    Alter table [Plan_Campaign] 
	ADD IntegrationInstanceCampaignId nvarchar(50) NULL
END

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IsDeployedToIntegration' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program'))
BEGIN
    Alter table [Plan_Campaign_Program] 
	ADD IsDeployedToIntegration bit NOT NULL
	CONSTRAINT [DF_Plan_Campaign_Program_IsDeployedToIntegration] DEFAULT 0
END

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IntegrationInstanceProgramId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program'))
BEGIN
    Alter table [Plan_Campaign_Program] 
	ADD IntegrationInstanceProgramId nvarchar(50) NULL
END

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IsDeployedToIntegration' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
    Alter table [Plan_Campaign_Program_Tactic] 
	ADD IsDeployedToIntegration bit NOT NULL
	CONSTRAINT [DF_Plan_Plan_Campaign_Program_Tactic_IsDeployedToIntegration] DEFAULT 0
END

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IntegrationInstanceTacticId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
    Alter table [Plan_Campaign_Program_Tactic] 
	ADD IntegrationInstanceTacticId nvarchar(50) NULL
END

-- ===========================================05_PL_432_MAP_CRM_Integration_Main_Model_Screen.sql===========================================
--Run below script on MRPQA

Go
IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_IntegrationInstance_IntegrationType]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
	  ALTER TABLE [IntegrationInstance] ADD CONSTRAINT FK_IntegrationInstance_IntegrationType FOREIGN KEY (IntegrationTypeId) REFERENCES IntegrationType(IntegrationTypeId)
END


Go
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IntegrationInstanceId' AND [object_id] = OBJECT_ID(N'Model'))
BEGIN
    Alter table [Model] ADD IntegrationInstanceId int NULL
END

Go
IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_Model_IntegrationInstance]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
	  ALTER TABLE [Model] ADD CONSTRAINT FK_Model_IntegrationInstance FOREIGN KEY (IntegrationInstanceId) REFERENCES IntegrationInstance(IntegrationInstanceId)
END


-- ===========================================01_PL_434_MAP_CRM_Integration_Tactic_Modal.sql===========================================

-- Run below script on MRPQA

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'LastSyncDate' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN
    Alter table [Plan_Campaign] 
	ADD LastSyncDate datetime NULL
END

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'LastSyncDate' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program'))
BEGIN
    Alter table [Plan_Campaign_Program] 
	ADD LastSyncDate datetime NULL
END

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'LastSyncDate' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
    Alter table [Plan_Campaign_Program_Tactic] 
	ADD LastSyncDate datetime NULL
END

-- ===========================================02_PL_473_MAP_CRM_Integration_Improvement_Tactic_Type_List.sql===========================================
-- Run below script on MRPQA

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IsDeployedToIntegration' AND [object_id] = OBJECT_ID(N'ImprovementTacticType'))
BEGIN
    Alter table [ImprovementTacticType] 
	ADD IsDeployedToIntegration bit NOT NULL
	CONSTRAINT [DF_ImprovementTacticType_IsDeployedToIntegration] DEFAULT 0
END

-- ===========================================02_PL_473_MAP_CRM_Integration_Improvement_Tactic_Type_List.sql===========================================
-- Run below script on MRPQA

--====================== Plan_Improvement_Campaign ==============================
GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IsDeployedToIntegration' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign'))
BEGIN
    Alter table [Plan_Improvement_Campaign] 
	ADD IsDeployedToIntegration bit NOT NULL
	CONSTRAINT [DF_Plan_Improvement_Campaign_IsDeployedToIntegration] DEFAULT 0
END

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IntegrationInstanceCampaignId' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign'))
BEGIN
    Alter table [Plan_Improvement_Campaign] 
	ADD IntegrationInstanceCampaignId nvarchar(50) NULL
END

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'LastSyncDate' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign'))
BEGIN
    Alter table [Plan_Improvement_Campaign] 
	ADD LastSyncDate datetime NULL
END


--====================== Plan_Improvement_Campaign_Program ==============================


GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IsDeployedToIntegration' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program'))
BEGIN
    Alter table [Plan_Improvement_Campaign_Program] 
	ADD IsDeployedToIntegration bit NOT NULL
	CONSTRAINT [DF_Plan_Improvement_Campaign_Program_IsDeployedToIntegration] DEFAULT 0
END

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IntegrationInstanceProgramId' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program'))
BEGIN
    Alter table [Plan_Improvement_Campaign_Program] 
	ADD IntegrationInstanceProgramId nvarchar(50) NULL
END

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'LastSyncDate' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program'))
BEGIN
    Alter table [Plan_Improvement_Campaign_Program] 
	ADD LastSyncDate datetime NULL
END


--====================== Plan_Improvement_Campaign_Program_Tactic ==============================

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IsDeployedToIntegration' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program_Tactic'))
BEGIN
    Alter table [Plan_Improvement_Campaign_Program_Tactic] 
	ADD IsDeployedToIntegration bit NOT NULL
	CONSTRAINT [DF_Plan_Improvement_Campaign_Program_Tactic_IsDeployedToIntegration] DEFAULT 0
END

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IntegrationInstanceTacticId' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program_Tactic'))
BEGIN
    Alter table [Plan_Improvement_Campaign_Program_Tactic] 
	ADD IntegrationInstanceTacticId nvarchar(50) NULL
END

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'LastSyncDate' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program_Tactic'))
BEGIN
    Alter table [Plan_Improvement_Campaign_Program_Tactic] 
	ADD LastSyncDate datetime NULL
END

-- ===========================================01_PL_457_MAP_CRM_Cannot_Delete_A_Boost_Tactic.sql===========================================
-- Use MRP DB

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'ImprovementTacticType')
BEGIN
	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'ImprovementTacticType' AND COLUMN_NAME = 'ModifiedDate')
	BEGIN
		ALTER TABLE ImprovementTacticType ADD ModifiedDate DATETIME NULL
	END

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'ImprovementTacticType' AND COLUMN_NAME = 'ModifiedBy')
	BEGIN
		ALTER TABLE ImprovementTacticType ADD ModifiedBy uniqueidentifier NULL
	END
END

-- ===========================================01_PL_477_Automated_Scheduled_Synchronization_with_Integration.sql===========================================
-- Run below script on MRPQA

IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'NextSyncDate' AND [object_id] = OBJECT_ID(N'SyncFrequency'))
BEGIN
    Alter table [SyncFrequency] 
	ADD NextSyncDate datetime NULL
END


Go
IF NOT EXISTS(SELECT TOP 1 * FROM Notification WHERE [NotificationInternalUseOnly] = 'IntegrationWindowsService')
BEGIN
insert into Notification 
	(
		NotificationInternalUseOnly,
		Title,
		Description,
		NotificationType,
		EmailContent,
		IsDeleted,
		CreatedDate,
		CreatedBy,
		Subject
	)
values 
	(
		'IntegrationWindowsService',
		'IntegrationWindowsService',
		'When integration windows service stopped working',
		'CM',
		'Scheduled integration windows service has stopped working at [time].<br /><br /> For more detail please look into log file.<br /><br />Thank you,<br/>Bulldog Gameplan Admin',
		0,
		GETDATE(),
		'092f54df-4c71-4f2f-9d21-0ae16155e5c1',
		'Gameplan : Integration windows service'
	)
end


-- ===========================================01_PL_31_Eloqua_API_Integration.sql===========================================
update [dbo].[IntegrationType] set APIURL = 'https://login.eloqua.com'  where APIURL='www.login.eloqua.com'

IF (select count(*) FROM GameplanDataType WHERE integrationtypeid in ( SELECT IntegrationTypeId FROM IntegrationType WHERE title='Eloqua')) != 26 
BEGIN
	DELETE FROM IntegrationInstanceDataTypeMapping WHERE 
	GameplanDataTypeId in ( SELECT GameplanDataTypeId FROM GameplanDataType WHERE integrationtypeid in ( SELECT IntegrationTypeId FROM IntegrationType WHERE title='Eloqua'))

	DELETE FROM GameplanDataType WHERE integrationtypeid in ( SELECT IntegrationTypeId FROM IntegrationType WHERE title='Eloqua')
END

IF NOT EXISTS(SELECT 1 FROM [dbo].[GameplanDataType] WHERE IntegrationTypeId=1)
BEGIN
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'Title', N'Gameplan.Tactic.Name', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'Description', N'Gameplan.Tactic.Description', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'StartDate', N'Gameplan.Tactic.StartDate', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'EndDate', N'Gameplan.Tactic.EndDate', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'VerticalId', N'Gameplan.Tactic.Vertical', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'AudienceId', N'Gameplan.Tactic.Audience', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'GeographyId', N'Gameplan.Tactic.Geography', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'Cost', N'Gameplan.Tactic.Cost(Budgeted)', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'CostActual', N'Gameplan.Tactic.Cost(Actual)', 1, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'CreatedBy', N'Gameplan.Tactic.owner', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'BusinessUnitId', N'Gameplan.Tactic.BusinessUnit', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'SUS', N'Gameplan.Tactic.SUS', 1, 1, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'INQ', N'Gameplan.Tactic.INQ', 1, 1, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'AQL', N'Gameplan.Tactic.AQL', 1, 1, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'MQL', N'Gameplan.Tactic.MQL', 1, 1, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'TQL', N'Gameplan.Tactic.TQL', 1, 1, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'SAL', N'Gameplan.Tactic.SAL', 1, 1, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'SQL', N'Gameplan.Tactic.SQL', 1, 1, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'CW', N'Gameplan.Tactic.CW', 1, 1, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Campaign_Program_Tactic', N'Revenue', N'Gameplan.Tactic.Revenue', 1, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Improvement_Campaign_Program_Tactic', N'Title', N'Gameplan.ImprovementTactic.Name', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Improvement_Campaign_Program_Tactic', N'Description', N'Gameplan.ImprovementTactic.Description', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Improvement_Campaign_Program_Tactic', N'EffectiveDate', N'Gameplan.ImprovementTactic.EffectiveDate', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Improvement_Campaign_Program_Tactic', N'Cost', N'Gameplan.ImprovementTactic.Cost', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Improvement_Campaign_Program_Tactic', N'BusinessUnitId', N'Gameplan.ImprovementTactic.BusinessUnit', 0, 0, 0)
INSERT [dbo].[GameplanDataType] ([IntegrationTypeId], [TableName], [ActualFieldName], [DisplayFieldName], [IsGet], [IsStage], [IsDeleted]) VALUES (1, N'Plan_Improvement_Campaign_Program_Tactic', N'CreatedBy', N'Gameplan.ImprovementTactic.Owner', 0, 0, 0)
END
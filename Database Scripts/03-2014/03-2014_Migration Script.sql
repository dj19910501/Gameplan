/*  01_TFS_300_Unable to enter actuals greater than $9,999,999 */

ALTER TABLE Model ALTER COLUMN AddressableContacts BIGINT NOT NULL
ALTER TABLE Model_Audience_Event ALTER COLUMN NumberofContacts BIGINT NULL
ALTER TABLE Model_Funnel ALTER COLUMN ExpectedLeadCount BIGINT NOT NULL
ALTER TABLE [Plan] ALTER COLUMN MQLs FLOAT NOT NULL
ALTER TABLE Plan_Campaign ALTER COLUMN INQs BIGINT NULL
ALTER TABLE Plan_Campaign ALTER COLUMN MQLs FLOAT NULL
ALTER TABLE Plan_Campaign_Program ALTER COLUMN INQs BIGINT NULL
ALTER TABLE Plan_Campaign_Program ALTER COLUMN MQLs FLOAT NULL
ALTER TABLE Plan_Campaign_Program_Tactic ALTER COLUMN INQs BIGINT NOT NULL
ALTER TABLE Plan_Campaign_Program_Tactic ALTER COLUMN INQsActual BIGINT NULL
ALTER TABLE Plan_Campaign_Program_Tactic ALTER COLUMN MQLs FLOAT NOT NULL
ALTER TABLE Plan_Campaign_Program_Tactic ALTER COLUMN MQLsActual FLOAT NULL
ALTER TABLE Plan_Campaign_Program_Tactic ALTER COLUMN CWs FLOAT NULL
ALTER TABLE Plan_Campaign_Program_Tactic ALTER COLUMN CWsActual FLOAT NULL
ALTER TABLE Plan_Campaign_Program_Tactic_Actual ALTER COLUMN Actualvalue FLOAT NOT NULL
--ALTER TABLE Plan_Improvement_Campaign_Program_Tactic_Actual ALTER COLUMN Actualvalue FLOAT NOT NULL
ALTER TABLE TacticType ALTER COLUMN ProjectedInquiries BIGINT NULL
ALTER TABLE TacticType ALTER COLUMN ProjectedMQLs FLOAT NULL
ALTER TABLE TacticType ALTER COLUMN ProjectedRevenue FLOAT NULL


/* 01_TFS_267_When a tactic is commented on a link to that tactic should be included in e-mail */
IF EXISTS (SELECT * FROM [Notification] WHERE NotificationInternalUseOnly = 'TacticCommentAdded')
BEGIN
	UPDATE [Notification] SET EmailContent ='Dear User,<br/><br/>Please note that comment has been added to the following tactic<br/><br/><table><tr><td>Tactic Name</td><td>:</td><td>[TacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Comment</td><td>:</td><td>[CommentToBeReplaced]</td></tr><tr><td>Comment added by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Gameplan Admin'
	WHERE NotificationInternalUseOnly = 'TacticCommentAdded'
END

/* 01_PL_217_Change language in Contacts and Inquiries section of model screen */
Update Funnel set Description ='I use marketing to generate #MarketingLeads leads annually from my website, blogs and social media efforts with an average deal size of #MarketingDealSize.' where Title='Marketing' 
Update Funnel set Description ='I use teleprospecting to generate #TeleprospectingLeads leads annually from outbound cold calling (TGL) with an average deal size of #TeleprospectingDealSize.' where Title='Teleprospecting' 
Update Funnel set Description ='I use sales to generate #SalesLeads leads annually from outbound cold calling and existing relationships (SGL) with an average deal size of #SalesDealSize.' where Title='Sales' 

/* 02_PL_242_Updating text of e-mail when tactic is declined */
Update [Notification] set [EmailContent]=REPLACE(EmailContent,'Dear User','Dear [NameToBeReplaced]') 
	where  [NotificationInternalUseOnly] In 
			(
				'CampaignApproved','CampaignDeclined','CampaignCommentAdded','CampaignSubmitted',
				'ProgramApproved','ProgramDeclined','ProgramCommentAdded','ProgramSubmitted',
				'TacticApproved','TacticDeclined','TacticCommentAdded','TacticSubmitted'
			 )

		Update [Notification] set [EmailContent]=REPLACE(EmailContent,'Gameplan Admin','Bulldog Gameplan Admin')
		where  [NotificationInternalUseOnly] In 
				(
					'CampaignApproved','CampaignDeclined','CampaignCommentAdded','CampaignSubmitted',
					'ProgramApproved','ProgramDeclined','ProgramCommentAdded','ProgramSubmitted',
					'TacticApproved','TacticDeclined','TacticCommentAdded','TacticSubmitted'
				)and EmailContent like '%<br><br>Thank You,<br>Gameplan Admin'


/* 001_PL_43_Boost_table_creation_and_data_insertion */
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Improvement_Campaign_Program_Tactic_Actual')
BEGIN
    Drop table Plan_Improvement_Campaign_Program_Tactic_Actual
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'ImprovementTacticStageTransitionMap')
BEGIN
    Drop table ImprovementTacticStageTransitionMap
END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Metric')
BEGIN
		CREATE TABLE [dbo].[Metric](
		[MetricId] [int] IDENTITY(1,1) NOT NULL,
		[MetricType] [nvarchar](10) NOT NULL,
		[MetricName] [nvarchar](255) NOT NULL,
		[MetricCode] [nvarchar](50) NOT NULL,
		[ClientId] [uniqueidentifier] NULL,
		[Level] [int] NULL,
		[IsDeleted] [bit] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
		[ModifiedDate] [datetime] NULL,
		[ModifiedBy] [uniqueidentifier] NULL,
	 CONSTRAINT [PK_Metric] PRIMARY KEY CLUSTERED 
	(
		[MetricId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[Metric] ADD  CONSTRAINT [DF_Metric_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]

	SET IDENTITY_INSERT [dbo].[Metric] ON 

	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (1, N'CR', N'SUS->INQ', N'SUS', N'464eb808-ad1f-4481-9365-6aada15023bd', 1, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (2, N'CR', N'INQ->AQL', N'INQ', N'464eb808-ad1f-4481-9365-6aada15023bd', 2, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (3, N'CR', N'AQL->TAL', N'AQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 3, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (4, N'CR', N'TAL->TQL', N'MQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 4, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (5, N'CR', N'TQL->SAL', N'TQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 5, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (6, N'CR', N'SAL->SQL', N'SAL', N'464eb808-ad1f-4481-9365-6aada15023bd', 6, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (7, N'CR', N'SQL->CW', N'SQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 7, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (8, N'SV', N'SUS', N'SUS', N'464eb808-ad1f-4481-9365-6aada15023bd', 1, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (9, N'SV', N'INQ', N'INQ', N'464eb808-ad1f-4481-9365-6aada15023bd', 2, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (10, N'SV', N'AQL', N'AQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 3, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (11, N'SV', N'TAL', N'MQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 4, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (12, N'SV', N'TQL', N'TQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 5, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (13, N'SV', N'SAL', N'SAL', N'464eb808-ad1f-4481-9365-6aada15023bd', 6, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (14, N'SV', N'SQL', N'SQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 7, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (15, N'Size', N'ADS', N'ADS', N'464eb808-ad1f-4481-9365-6aada15023bd', NULL, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	SET IDENTITY_INSERT [dbo].[Metric] OFF
END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'BestInClass')
BEGIN
	 CREATE TABLE [dbo].[BestInClass](
		[BestInClassId] [int] IDENTITY(1,1) NOT NULL,
		[MetricId] [int] NOT NULL,
		[Value] [float] NOT NULL,
		[IsDeleted] [bit] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
		[ModifiedDate] [datetime] NULL,
		[ModifiedBy] [uniqueidentifier] NULL,
	 CONSTRAINT [PK_BestInClass] PRIMARY KEY CLUSTERED 
	(
		[BestInClassId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	 CONSTRAINT [UniqueBICStage] UNIQUE NONCLUSTERED 
	(
		[MetricId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	

	ALTER TABLE [dbo].[BestInClass] ADD  CONSTRAINT [DF_BestInClass_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
	

	ALTER TABLE [dbo].[BestInClass]  WITH CHECK ADD  CONSTRAINT [FK_BestInClass_Metric] FOREIGN KEY([MetricId])
	REFERENCES [dbo].[Metric] ([MetricId])
	

	ALTER TABLE [dbo].[BestInClass] CHECK CONSTRAINT [FK_BestInClass_Metric]
	

	SET IDENTITY_INSERT [dbo].[BestInClass] ON 
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (1, 1, 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (2, 2, 0.6, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (3, 3, 0.65, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (4, 4, 0.7, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (5, 5, 0.75, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (6, 6, 0.7, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (7, 7, 0.65, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (8, 8, 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (9, 9, 15, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (10, 10, 15, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (11, 11, 15, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (12, 12, 15, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (13, 13, 15, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (14, 14, 15, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (15, 15, 10, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	SET IDENTITY_INSERT [dbo].[BestInClass] OFF
	
END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'ImprovementTacticType')
BEGIN
	   CREATE TABLE [dbo].[ImprovementTacticType](
		[ImprovementTacticTypeId] [int] IDENTITY(1,1) NOT NULL,
		[Title] [nvarchar](255) NOT NULL,
		[Description] [nvarchar](4000) NULL,
		[Cost] [float] NOT NULL,
		[ColorCode] [nvarchar](10) NOT NULL,
		[ClientId] [uniqueidentifier] NOT NULL,
		[IsDeployed] [bit] NOT NULL,
		[IsDeleted] [bit] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
		[ModifiedDate] [datetime] NULL,
		[ModifiedBy] [uniqueidentifier] NULL,
	 CONSTRAINT [PK_ImprovementTacticType] PRIMARY KEY CLUSTERED 
	(
		[ImprovementTacticTypeId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	

	ALTER TABLE [dbo].[ImprovementTacticType] ADD  CONSTRAINT [DF_ImprovementTacticType_IsDeployed]  DEFAULT ((0)) FOR [IsDeployed]
	
	SET IDENTITY_INSERT [dbo].[ImprovementTacticType] ON 

	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (1, N'Inphographic Program', NULL, 25000, N'5bae30', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (2, N'Perpetual Nurture Program', NULL, 40000, N'8e2590', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (3, N'Whitepaper Program', NULL, 7500, N'09af8f', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (4, N'Analysis Whitepaper Program', NULL, 10000, N'407b22', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (5, N'Webinar Program', NULL, 10000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (6, N'Reconstituded/Reengage Program', NULL, 40000, N'0071bc', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (7, N'Integrated Inbound and Outbound Media Program', NULL, 25000, N'38bf37', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (8, N'Develop Buyers Journey-Based Messaging', NULL, 25000, N'38bf37', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (9, N'Develop Buyers Personas', NULL, 35000, N'5bae30', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (10, N'Integrated Welcome and Opportunity Accelaration Nurture', NULL, 80000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (11, N'Buyer Insight Assesment for Opportunity Acceleration', NULL, 40000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (12, N'Implement a Active Recycle Nurture Program', NULL, 40000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (13, N'Implement a Passive Recycle Nurture Program', NULL, 150000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (14, N'Implement a Tele-Prospecting Function within Marketing', NULL, 120000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (15, N'Implement Telesales Campaign Playbooks and Scripts', NULL, 20000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (16, N'Tradeshow', NULL, 80000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (17, N'Executive Briefings', NULL, 10000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, N'Sales/Marketing Alignment', NULL, 20000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 1, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, N'Content Nuture', NULL, 10000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 1, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	SET IDENTITY_INSERT [dbo].[ImprovementTacticType] OFF
	
END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'ImprovementTacticType_Metric')
BEGIN
	CREATE TABLE [dbo].[ImprovementTacticType_Metric](
		[ImprovementTacticTypeId] [int] NOT NULL,
		[MetricId] [int] NOT NULL,
		[Weight] [float] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
		[ModifiedDate] [datetime] NULL,
		[ModifiedBy] [uniqueidentifier] NULL,
	 CONSTRAINT [PK_ImprovementTacticType_Metric] PRIMARY KEY CLUSTERED 
	(
		[ImprovementTacticTypeId] ASC,
		[MetricId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	

	ALTER TABLE [dbo].[ImprovementTacticType_Metric]  WITH CHECK ADD  CONSTRAINT [FK_ImprovementTacticType_Metric_ImprovementTacticType] FOREIGN KEY([ImprovementTacticTypeId])
	REFERENCES [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId])
	

	ALTER TABLE [dbo].[ImprovementTacticType_Metric] CHECK CONSTRAINT [FK_ImprovementTacticType_Metric_ImprovementTacticType]
	

	ALTER TABLE [dbo].[ImprovementTacticType_Metric]  WITH CHECK ADD  CONSTRAINT [FK_ImprovementTacticType_Metric_Metric] FOREIGN KEY([MetricId])
	REFERENCES [dbo].[Metric] ([MetricId])
	

	ALTER TABLE [dbo].[ImprovementTacticType_Metric] CHECK CONSTRAINT [FK_ImprovementTacticType_Metric_Metric]
	

	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 2, 5, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 3, 4, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 4, 3, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 5, 2, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 6, 1, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 7, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 9, 5, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 10, 4, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 11, 3, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 12, 2, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 13, 1, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 14, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 15, 5, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 2, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 3, 1, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 4, 2, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 5, 3, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 6, 4, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 7, 5, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 9, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 10, 1, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 11, 2, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 12, 3, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 13, 4, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 14, 5, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
	INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 15, 1, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
	
END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'ImprovementTacticType_Touches')
BEGIN
		CREATE TABLE [dbo].[ImprovementTacticType_Touches](
			[ImprovementTacticTypeId] [int] NOT NULL,
			[MetricId] [int] NOT NULL,
			[CreatedDate] [datetime] NOT NULL,
			[CreatedBy] [uniqueidentifier] NOT NULL,
		 CONSTRAINT [PK_ImprovementTacticType_Touches] PRIMARY KEY CLUSTERED 
		(
			[ImprovementTacticTypeId] ASC,
			[MetricId] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]

		

		ALTER TABLE [dbo].[ImprovementTacticType_Touches]  WITH CHECK ADD  CONSTRAINT [FK_ImprovementTacticType_Touches_ImprovementTacticType] FOREIGN KEY([ImprovementTacticTypeId])
		REFERENCES [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId])
		

		ALTER TABLE [dbo].[ImprovementTacticType_Touches] CHECK CONSTRAINT [FK_ImprovementTacticType_Touches_ImprovementTacticType]
		

		ALTER TABLE [dbo].[ImprovementTacticType_Touches]  WITH CHECK ADD  CONSTRAINT [FK_ImprovementTacticType_Touches_Metric] FOREIGN KEY([MetricId])
		REFERENCES [dbo].[Metric] ([MetricId])
		

		ALTER TABLE [dbo].[ImprovementTacticType_Touches] CHECK CONSTRAINT [FK_ImprovementTacticType_Touches_Metric]
		
		INSERT [dbo].[ImprovementTacticType_Touches] ([ImprovementTacticTypeId], [MetricId], [CreatedDate], [CreatedBy]) VALUES (18, 9, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9')
		
		INSERT [dbo].[ImprovementTacticType_Touches] ([ImprovementTacticTypeId], [MetricId], [CreatedDate], [CreatedBy]) VALUES (18, 10, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9')
		
		INSERT [dbo].[ImprovementTacticType_Touches] ([ImprovementTacticTypeId], [MetricId], [CreatedDate], [CreatedBy]) VALUES (18, 11, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9')
		
		INSERT [dbo].[ImprovementTacticType_Touches] ([ImprovementTacticTypeId], [MetricId], [CreatedDate], [CreatedBy]) VALUES (19, 9, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9')
		
		INSERT [dbo].[ImprovementTacticType_Touches] ([ImprovementTacticTypeId], [MetricId], [CreatedDate], [CreatedBy]) VALUES (19, 10, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9')
		
		INSERT [dbo].[ImprovementTacticType_Touches] ([ImprovementTacticTypeId], [MetricId], [CreatedDate], [CreatedBy]) VALUES (19, 11, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9')
		
END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Improvement_Campaign')
BEGIN
	CREATE TABLE [dbo].[Plan_Improvement_Campaign](
		[ImprovementPlanCampaignId] [int] IDENTITY(1,1) NOT NULL,
		[ImprovePlanId] [int] NOT NULL,
		[Title] [nvarchar](255) NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_Plan_Improvement_Campaign] PRIMARY KEY CLUSTERED 
	(
		[ImprovementPlanCampaignId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	

	ALTER TABLE [dbo].[Plan_Improvement_Campaign]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Plan] FOREIGN KEY([ImprovePlanId])
	REFERENCES [dbo].[Plan] ([PlanId])
	

	ALTER TABLE [dbo].[Plan_Improvement_Campaign] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Plan]
	
END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Improvement_Campaign_Program')
BEGIN
	CREATE TABLE [dbo].[Plan_Improvement_Campaign_Program](
		[ImprovementPlanProgramId] [int] IDENTITY(1,1) NOT NULL,
		[ImprovementPlanCampaignId] [int] NOT NULL,
		[Title] [nvarchar](255) NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_Plan_Improvement_Campaign_Program] PRIMARY KEY CLUSTERED 
	(
		[ImprovementPlanProgramId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	

	ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Plan_Improvement_Campaign] FOREIGN KEY([ImprovementPlanCampaignId])
	REFERENCES [dbo].[Plan_Improvement_Campaign] ([ImprovementPlanCampaignId])
	

	ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Plan_Improvement_Campaign]
	
END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Improvement_Campaign_Program_Tactic')
BEGIN
			CREATE TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic](
			[ImprovementPlanTacticId] [int] IDENTITY(1,1) NOT NULL,
			[ImprovementPlanProgramId] [int] NOT NULL,
			[ImprovementTacticTypeId] [int] NOT NULL,
			[Title] [varchar](255) NOT NULL,
			[Description] [varchar](4000) NULL,
			[VerticalId] [int] NOT NULL,
			[AudienceId] [int] NOT NULL,
			[GeographyId] [uniqueidentifier] NOT NULL,
			[BusinessUnitId] [uniqueidentifier] NOT NULL,
			[EffectiveDate] [datetime] NOT NULL,
			[Cost] [float] NOT NULL,
			[Status] [nvarchar](50) NOT NULL,
			[IsDeleted] [bit] NOT NULL,
			[CreatedDate] [datetime] NOT NULL,
			[CreatedBy] [uniqueidentifier] NOT NULL,
			[ModifiedDate] [datetime] NULL,
			[ModifiedBy] [uniqueidentifier] NULL,
		 CONSTRAINT [PK_Plan_Improvement_Campaign_Program_Tactic] PRIMARY KEY CLUSTERED 
		(
			[ImprovementPlanTacticId] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]

		

		SET ANSI_PADDING OFF
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] ADD  CONSTRAINT [DF_Plan_Improvement_Campaign_Program_Tactic_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Audience] FOREIGN KEY([AudienceId])
		REFERENCES [dbo].[Audience] ([AudienceId])
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Audience]
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_BusinessUnit] FOREIGN KEY([BusinessUnitId])
		REFERENCES [dbo].[BusinessUnit] ([BusinessUnitId])
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_BusinessUnit]
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Geography] FOREIGN KEY([GeographyId])
		REFERENCES [dbo].[Geography] ([GeographyId])
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Geography]
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_ImprovementTacticType] FOREIGN KEY([ImprovementTacticTypeId])
		REFERENCES [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId])
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_ImprovementTacticType]
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Plan_Improvement_Campaign_Program] FOREIGN KEY([ImprovementPlanProgramId])
		REFERENCES [dbo].[Plan_Improvement_Campaign_Program] ([ImprovementPlanProgramId])
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Plan_Improvement_Campaign_Program]
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Vertical] FOREIGN KEY([VerticalId])
		REFERENCES [dbo].[Vertical] ([VerticalId])
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Vertical]
		
END

GO


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Improvement_Campaign_Program_Tactic_Comment')
BEGIN
		CREATE TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic_Comment](
			[ImprovementPlanTacticCommentId] [int] IDENTITY(1,1) NOT NULL,
			[ImprovementPlanTacticId] [int] NOT NULL,
			[Comment] [nvarchar](4000) NOT NULL,
			[CreatedDate] [datetime] NOT NULL,
			[CreatedBy] [uniqueidentifier] NOT NULL,
		 CONSTRAINT [PK_Plan_Improvement_Campaign_Program_Tactic_Comment] PRIMARY KEY CLUSTERED 
		(
			[ImprovementPlanTacticCommentId] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]

		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic_Comment]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Comment_Plan_Improvement_Campaign_Program_Tactic] FOREIGN KEY([ImprovementPlanTacticId])
		REFERENCES [dbo].[Plan_Improvement_Campaign_Program_Tactic] ([ImprovementPlanTacticId])
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic_Comment] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Comment_Plan_Improvement_Campaign_Program_Tactic]
		
END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Improvement_Campaign_Program_Tactic_Share')
BEGIN
		CREATE TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic_Share](
			[ImprovementPlanTacticShareId] [int] IDENTITY(1,1) NOT NULL,
			[ImprovementPlanTacticId] [int] NOT NULL,
			[EmailId] [varchar](255) NOT NULL,
			[EmailBody] [nvarchar](4000) NOT NULL,
			[CreatedDate] [datetime] NOT NULL,
			[CreatedBy] [uniqueidentifier] NOT NULL,
		 CONSTRAINT [PK_Plan_Improvement_Campaign_Program_Tactic_Share] PRIMARY KEY CLUSTERED 
		(
			[ImprovementPlanTacticShareId] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]

		

		SET ANSI_PADDING OFF
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic_Share]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Share_Plan_Improvement_Campaign_Program_Tactic] FOREIGN KEY([ImprovementPlanTacticId])
		REFERENCES [dbo].[Plan_Improvement_Campaign_Program_Tactic] ([ImprovementPlanTacticId])
		

		ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic_Share] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Share_Plan_Improvement_Campaign_Program_Tactic]
		
END

GO

IF NOT EXISTS(SELECT TOP 1 * FROM Notification WHERE [NotificationInternalUseOnly] = 'ShareImprovementTactic')
BEGIN
INSERT [dbo].[Notification] ([NotificationInternalUseOnly], [Title], [Description], [NotificationType], [EmailContent], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy], [Subject]) VALUES (N'ShareImprovementTactic', N'Share Improvement Tactic', N'Share improvement tactic', N'CM', N'Please review plan improvement tactic<br/><strong>Additional Message: </strong>[AdditionalMessage]<br/><strong>URL: </strong>[URL]', 0, CAST(0x0000A29D00000000 AS DateTime), N'092f54df-4c71-4f2f-9d21-0ae16155e5c1', NULL, NULL, N'Gameplan : Improvement Tactic has been shared')
END

IF NOT EXISTS(SELECT TOP 1 * FROM Notification WHERE [NotificationInternalUseOnly] = 'ImprovementTacticCommentAdded')
BEGIN
INSERT [dbo].[Notification] ([NotificationInternalUseOnly], [Title], [Description], [NotificationType], [EmailContent], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy], [Subject]) VALUES (N'ImprovementTacticCommentAdded', N'Improvement Tactic Comment Added', N'When new improvement tactic comment in added', N'CM', N'Dear [NameToBeReplaced],<br/><br/>Please note that comment has been added to the following tactic<br/><br/><table><tr><td>Tactic Name</td><td>:</td><td>[ImprovementTacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Comment</td><td>:</td><td>[CommentToBeReplaced]</td></tr><tr><td>Comment added by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Gameplan Admin', 0, CAST(0x0000A2A400000000 AS DateTime), N'092f54df-4c71-4f2f-9d21-0ae16155e5c1', NULL, NULL, N'Gameplan : Comment added to improvement tactic')
END

IF NOT EXISTS(SELECT TOP 1 * FROM Notification WHERE [NotificationInternalUseOnly] = 'ImprovementTacticApproved')
BEGIN
INSERT [dbo].[Notification] ([NotificationInternalUseOnly], [Title], [Description], [NotificationType], [EmailContent], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy], [Subject]) VALUES (N'ImprovementTacticApproved', N'Improvement Tactic Approved', N'When improvement tactic is approved', N'CM', N'Dear [NameToBeReplaced],<br/><br/>Please note that following improvement tactic has been apporved.<br><br><table><tr><td>Improvement Tactic Name</td><td>:</td><td>[ImprovementTacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Approved by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr></table><br><br>Thank You,<br>Gameplan Admin', 0, CAST(0x0000A2A400000000 AS DateTime), N'092f54df-4c71-4f2f-9d21-0ae16155e5c1', NULL, NULL, N'Gameplan : Improvement Tactic has been approved')
END

IF NOT EXISTS(SELECT TOP 1 * FROM Notification WHERE [NotificationInternalUseOnly] = 'ImprovementTacticDeclined')
BEGIN
INSERT [dbo].[Notification] ([NotificationInternalUseOnly], [Title], [Description], [NotificationType], [EmailContent], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy], [Subject]) VALUES (N'ImprovementTacticDeclined', N'Improvement Tactic Declined', N'When improvement tactic is declined', N'CM', N'Dear [NameToBeReplaced],<br/><br/>Please note that following improvement tactic has been declined.<br><br><table><tr><td>Improvement Tactic Name</td><td>:</td><td>[ImprovementTacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Declined by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr></table><br><br>Thank You,<br>Gameplan Admin', 0, CAST(0x0000A2A400000000 AS DateTime), N'092f54df-4c71-4f2f-9d21-0ae16155e5c1', NULL, NULL, N'Gameplan : Improvement Tactic has been declined')
END

IF NOT EXISTS(SELECT TOP 1 * FROM Notification WHERE [NotificationInternalUseOnly] = 'ImprovementTacticSubmitted')
BEGIN
INSERT [dbo].[Notification] ([NotificationInternalUseOnly], [Title], [Description], [NotificationType], [EmailContent], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy], [Subject]) VALUES (N'ImprovementTacticSubmitted', N'Improvement Tactic Submitted', N'When improvement tactic is submitted', N'CM', N'Dear [NameToBeReplaced],<br/><br/>Please note that following improvement tactic has been submitted for approval.<br><br><table><tr><td>Improvement Tactic Name</td><td>:</td><td>[ImprovementTacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Submitted by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr></table><br><br>Thank You,<br>Gameplan Admin', 0, CAST(0x0000A2A400000000 AS DateTime), N'092f54df-4c71-4f2f-9d21-0ae16155e5c1', NULL, NULL, N'Gameplan : Improvement Tactic has been submitted for approval')
END

/* Added By Bhavesh Dobariya PL Ticket 380*/
Go
IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_Model_BusinessUnit]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
	  ALTER TABLE Model ADD CONSTRAINT FK_Model_BusinessUnit FOREIGN KEY (BusinessUnitId) REFERENCES BusinessUnit(BusinessUnitId)
END

Go
IF EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'BusinessUnitId' AND [object_id] = OBJECT_ID(N'Model'))
BEGIN
    Alter table Model Alter Column BusinessUnitId uniqueidentifier NOT NULL
END

GO
IF EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_Plan_Improvement_Campaign_Program_Tactic_Audience]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
   Alter table Plan_Improvement_Campaign_Program_Tactic drop constraint FK_Plan_Improvement_Campaign_Program_Tactic_Audience
END

GO
IF EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'AudienceId' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program_Tactic'))
BEGIN
    Alter table Plan_Improvement_Campaign_Program_Tactic drop column AudienceId
END

GO
IF EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_Plan_Improvement_Campaign_Program_Tactic_Geography]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
   Alter table Plan_Improvement_Campaign_Program_Tactic drop constraint FK_Plan_Improvement_Campaign_Program_Tactic_Geography
END

GO
IF EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'GeographyId' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program_Tactic'))
BEGIN
    Alter table Plan_Improvement_Campaign_Program_Tactic drop column GeographyId
END

GO
IF EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_Plan_Improvement_Campaign_Program_Tactic_Vertical]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
   Alter table Plan_Improvement_Campaign_Program_Tactic drop constraint FK_Plan_Improvement_Campaign_Program_Tactic_Vertical
END

GO
IF EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'VerticalId' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program_Tactic'))
BEGIN
    Alter table Plan_Improvement_Campaign_Program_Tactic drop column VerticalId
END


/* PL Ticket 395 There is no Parent Campaign field in our system 
Date: 27-03-2014 
Added By Bhavesh Dobariya */
Update Plan_Improvement_Campaign SET Title = 'Improvement Activities'
Update Plan_Improvement_Campaign_Program SET Title = 'Improvement Program'

/* PL Ticket 401 Plan TQL values are including totals from deleted Programs
Date: 31-03-2014 
Added By Bhavesh Dobariya 
Added at last
*/


IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Update_MQL')
	DROP PROCEDURE Update_MQL
GO
/****** Object:  StoredProcedure [dbo].[Update_MQL]    Script Date: 2/28/2014 3:56:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Bhavesh Dobariya>
-- Create date: <28 Feb 2014>
-- Description:	<Update MQL of Tactic Based on Conversion Rate Define in Model.>
-- =============================================
-- EXEC dbo.Update_MQL 342,'464EB808-AD1F-4481-9365-6AADA15023BD','INQ','MQL','CR',0
Create PROCEDURE [dbo].[Update_MQL]
	-- Add the parameters for the stored procedure here
	@ModelId int,
	@ClientId varchar(255),
	@INQ Varchar(10),
	@MQL Varchar(10),
	@StageTypeCR Varchar(10),
	@ReturnValue INT = 0 OUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Declare @INQLevel int
	Declare @MQLLevel int
	SET @INQLevel = (SELECT [Level] FROM Stage WHERE ClientId = @ClientId AND Code = @INQ)
	SET @MQLLevel = (SELECT [Level] FROM Stage WHERE ClientId = @ClientId AND Code = @MQL)
	BEGIN TRANSACTION UpdateMQL 

	;WITH _CTE AS(
		SELECT 
				T.StartDate, 
				T.PlanTacticId,
				ISNULL(T.INQs, 0) AS INQ,
				[Plan].ModelId,
				M.ParentModelId,
				M.EffectiveDate
		FROM Plan_Campaign_Program_Tactic T
			INNER JOIN Plan_Campaign_Program P ON P.PlanProgramId = T.PlanProgramId 
			INNER JOIN Plan_Campaign C ON C.PlanCampaignId = P.PlanCampaignId
			INNER JOIN [Plan] ON [Plan].PlanId = C.PlanId
			INNER JOIN Model M ON M.ModelId = [Plan].ModelId
		WHERE M.ModelId = @ModelId

		UNION ALL
		SELECT 
				C.StartDate, 
				C.PlanTacticId,
				C.INQ,
				M.ModelId,
				M.ParentModelId,
				M.EffectiveDate
		FROM Model M
			INNER JOIN _CTE C ON C.ParentModelId = M.ModelId
		),
		_CTEDetail AS(
			SELECT 
				ROWNUM = ROW_NUMBER() OVER(PARTITION BY C.PlanTacticId ORDER BY C.ModelId DESC),
				C.StartDate, 
				C.PlanTacticId,
				C.INQ,
				C.ModelId,
				C.ParentModelId,
				C.EffectiveDate
			FROM _CTE C
			WHERE C.StartDate >= C.EffectiveDate
		),
		_CTEFinal AS (
		SELECT 
			C.PlanTacticId,
			MFS.Value /100 AS RATE
		FROM _CTEDetail C
			INNER JOIN Model_Funnel MF ON MF.ModelId = C.ModelId
			INNER JOIN Model_Funnel_Stage MFS ON MFS.ModelFunnelId = MF.ModelFunnelId
			INNER JOIN [Stage] on MFS.StageId = [Stage].StageId
		WHERE C.ROWNUM = 1 AND [Stage].[Level] >= @INQLevel AND [Stage].[Level] < @MQLLevel AND [Stage].ClientId = @ClientID AND MFS.StageType = @stageTypeCR
		)
		,
		_CTEUse AS
		(
		SELECT DISTINCT T1.PlanTacticId, CASE WHEN (SELECT COUNT(*) FROM _CTEFinal T2 WHERE T2.RATE = 0 AND T2.PlanTacticId = T1.PlanTacticId) >= 1 THEN 0 ELSE (SELECT EXP(SUM(LOG(ABS(T2.RATE)))) FROM _CTEFinal T2 WHERE T2.PlanTacticId = T1.PlanTacticId GROUP BY T2.PlanTacticId) END  AS Rate FROM _CTEFinal T1  
		)
		Update PT SET PT.MQLs = ROUND(PT.INQs * CF.RATE ,0) FROM Plan_Campaign_Program_Tactic PT
		INNER JOIN _CTEUse CF ON CF.PlanTacticId = PT.PlanTacticId

		IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION UpdateMQL                    
					RETURN                
				END      

		Update P SET P.MQLs = (SELECT SUM(MQLs) FROM Plan_Campaign_Program_Tactic WHERE PlanProgramId = P.PlanProgramId AND IsDeleted = 0) FROM Plan_Campaign_Program P
		INNER JOIN Plan_Campaign ON P.PlanCampaignId = Plan_Campaign.PlanCampaignId
		INNER JOIN [Plan] ON [Plan].PlanId = Plan_Campaign.PlanId
		WHERE ModelId = @ModelId
		
		IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION UpdateMQL                    
					RETURN                
				END   

		Update C SET C.MQLs = (SELECT SUM(MQLs) FROM Plan_Campaign_Program WHERE PlanCampaignId = C.PlanCampaignId AND IsDeleted = 0) FROM Plan_Campaign C
		INNER JOIN [Plan] ON [Plan].PlanId = C.PlanId
		WHERE ModelId = @ModelId

		IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION UpdateMQL                    
					RETURN                
				END
		COMMIT TRANSACTION PlanDuplicate                  
		SET @ReturnValue = 1;        
		RETURN @ReturnValue

END

GO
/*
Update MQL Based on conversion rate
DATE: 2nd April 2014
*/
DECLARE @PlanTacticId int,
@PlanId int,
@ModelId int,
@StartDate datetime

DECLARE @INQ VARCHAR(10) = 'INQ'
DECLARE @MQL VARCHAR(10) = 'MQL'
DECLARE @StageType varchar(10) = 'CR'


	DECLARE innnercursor CURSOR
        FOR
            SELECT  PlanTacticId, Plan_Campaign_Program_Tactic.StartDate, [Plan].PlanId, [Plan].ModelId
            FROM    Plan_Campaign_Program_Tactic
			INNER JOIN Plan_Campaign_Program ON Plan_Campaign_Program.PlanProgramId = Plan_Campaign_Program_Tactic.PlanProgramId
			INNER JOIN Plan_Campaign ON Plan_Campaign.PlanCampaignId = Plan_Campaign_Program.PlanCampaignId
			INNER JOIN [Plan] ON [Plan].PlanId = Plan_Campaign.PlanId
        OPEN innnercursor
					
        FETCH NEXT FROM innnercursor INTO @PlanTacticId, @StartDate, @PlanId, @ModelId
                   
        WHILE ( @@FETCH_STATUS = 0 ) 
            BEGIN
				DECLARE @ParentModelId int
				DECLARE @EffectiveDate datetime
				SET @ParentModelId = @ModelId
				DECLARE @FinalModelId int
				
					WHILE @ParentModelId IS NOT NULL
						BEGIN
						SET @FinalModelId = @ParentModelId
							SET @EffectiveDate = (SELECT EffectiveDate FROM Model WHERE ModelId = @ParentModelId)
							IF(@StartDate >= @EffectiveDate OR @EffectiveDate IS NULL)
								BEGIN
									break;
								END
							ELSE
								BEGIN
									SET @ParentModelId = (SELECT ParentModelId FROM Model WHERE ModelId = @ParentModelId)
								END
							
						END
					Declare @INQLevel int
	Declare @MQLLevel int
	DECLARE @FinalClientId varchar(255)
	SET @FinalClientId = (SELECT ClientId FROM Model
							INNER JOIN BusinessUnit ON Model.BusinessUnitId = BusinessUnit.BusinessUnitId
							 WHERE ModelId = @FinalModelId
							)
	SET @INQLevel = (SELECT [Level] FROM Stage WHERE ClientId = @FinalClientId AND Code = @INQ)
	SET @MQLLevel = (SELECT [Level] FROM Stage WHERE ClientId = @FinalClientId AND Code = @MQL)
	
	;with CTETable AS (SELECT 
			MFS.Value /100 AS RATE
		FROM Model_Funnel_Stage MFS
			INNER JOIN Model_Funnel MF ON MFS.ModelFunnelId = MF.ModelFunnelId
			INNER JOIN [Stage] on MFS.StageId = [Stage].StageId
		WHERE  MF.ModelId = @FinalModelId AND [Stage].[Level] >= @INQLevel AND [Stage].[Level] < @MQLLevel AND [Stage].ClientId = @FinalClientId AND MFS.StageType = @StageType
		),
		_CTEUSE AS
		(SELECT CASE WHEN (SELECT COUNT(*) FROM CTETable T2 WHERE T2.RATE = 0) >= 1 THEN 0 ELSE (SELECT EXP(SUM(LOG(ABS(T2.RATE)))) FROM CTETable T2 ) END  AS Rate 
		)
		--SELECT PlanTacticId  ,MQLs,INQs * (SELECT ISNULL(Rate,0) FROM _CTEUSE) AS CALMQL FROM Plan_Campaign_Program_Tactic WHERE PlanTacticId = @PlanTacticId
		Update Plan_Campaign_Program_Tactic SET MQLs = ROUND(INQs * (SELECT ISNULL(Rate,0) FROM _CTEUSE) ,0) WHERE PlanTacticId = @PlanTacticId
		
			 FETCH NEXT FROM innnercursor INTO @PlanTacticId, @StartDate, @PlanId, @ModelId
            END
        CLOSE innnercursor
        DEALLOCATE innnercursor

GO

WITH Tactics AS (
SELECT DISTINCT
              T.PlanProgramId PlanProgramId1,
              INQSum = (SELECT SUM(T1.INQs) FROM Plan_Campaign_Program_Tactic T1 WHERE T1.PlanProgramId = T.PlanProgramId AND IsDeleted = 0),
              MQLSum = (SELECT SUM(T1.MQLs) FROM Plan_Campaign_Program_Tactic T1 WHERE T1.PlanProgramId = T.PlanProgramId AND IsDeleted = 0),
              CostSum = (SELECT SUM(T1.Cost) FROM Plan_Campaign_Program_Tactic T1 WHERE T1.PlanProgramId = T.PlanProgramId AND IsDeleted = 0)
FROM Plan_Campaign_Program_Tactic T
WHERE T.IsDeleted = 0
),
AllData AS(
SELECT 
              T.PlanProgramId1, 
              P.PlanProgramId, 
              INQSum = CASE WHEN T.PlanProgramId1 IS NOT NULL THEN T.INQSum ELSE 0 END, 
              MQLSum = CASE WHEN T.PlanProgramId1 IS NOT NULL THEN T.MQLSum ELSE 0 END, 
              CostSum = CASE WHEN T.PlanProgramId1 IS NOT NULL THEN T.CostSum ELSE 0 END, 
              P.INQs, 
              P.MQLs, 
              P.Cost 
FROM Tactics T 
RIGHT JOIN Plan_Campaign_Program P ON P.PlanProgramId = T.PlanProgramId1 AND P.IsDeleted = 0
)
UPDATE p  SET p.INQs = T.INQSum, p.MQLs = T.MQLSum, p.Cost = T.CostSum FROM Plan_Campaign_Program p  RIGHT JOIN AllData T ON P.PlanProgramId = T.PlanProgramId AND P.IsDeleted = 0;



WITH Programs AS (
SELECT DISTINCT
              P.PlanCampaignId PlanCampaignId1,
              INQSum = (SELECT SUM(P1.INQs) FROM Plan_Campaign_Program P1 WHERE P1.PlanCampaignId = P.PlanCampaignId AND IsDeleted = 0),
              MQLSum = (SELECT SUM(P1.MQLs) FROM Plan_Campaign_Program P1 WHERE P1.PlanCampaignId = P.PlanCampaignId AND IsDeleted = 0),
              CostSum = (SELECT SUM(P1.Cost) FROM Plan_Campaign_Program P1 WHERE P1.PlanCampaignId = P.PlanCampaignId AND IsDeleted = 0)
FROM Plan_Campaign_Program P
WHERE P.IsDeleted = 0
)
,
AllData AS(
SELECT 
              T.PlanCampaignId1, 
              C.PlanCampaignId, 
              INQSum = CASE WHEN T.PlanCampaignId1 IS NOT NULL THEN T.INQSum ELSE 0 END, 
              MQLSum = CASE WHEN T.PlanCampaignId1 IS NOT NULL THEN T.MQLSum ELSE 0 END, 
              CostSum = CASE WHEN T.PlanCampaignId1 IS NOT NULL THEN T.CostSum ELSE 0 END, 
              C.INQs, 
              C.MQLs, 
              C.Cost 
FROM Programs T 
RIGHT JOIN Plan_Campaign C ON C.PlanCampaignId = T.PlanCampaignId1 AND C.IsDeleted = 0
)
UPDATE p  SET p.INQs = T.INQSum, p.MQLs = T.MQLSum, p.Cost = T.CostSum FROM Plan_Campaign p  RIGHT JOIN AllData T ON P.PlanCampaignId = T.PlanCampaignId AND P.IsDeleted = 0;
GO
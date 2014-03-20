IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Improvement_Campaign_Program_Tactic_Share'))
BEGIN
    Drop table Plan_Improvement_Campaign_Program_Tactic_Share
END

GO

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Improvement_Campaign_Program_Tactic_Comment'))
BEGIN
    Drop table Plan_Improvement_Campaign_Program_Tactic_Comment
END

GO

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Improvement_Campaign_Program_Tactic_Actual'))
BEGIN
    Drop table Plan_Improvement_Campaign_Program_Tactic_Actual
END

GO

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'ImprovementTacticStageTransitionMap'))
BEGIN
    Drop table ImprovementTacticStageTransitionMap
END

GO

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Improvement_Campaign_Program_Tactic'))
BEGIN
    Drop table Plan_Improvement_Campaign_Program_Tactic
END

GO

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Improvement_Campaign_Program'))
BEGIN
    Drop table Plan_Improvement_Campaign_Program
END

GO

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Improvement_Campaign'))
BEGIN
    Drop table Plan_Improvement_Campaign
END

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'ImprovementTacticType_Metric'))
BEGIN
   Drop table ImprovementTacticType_Metric
END

GO

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'ImprovementTacticType_Touches'))
BEGIN
    Drop table ImprovementTacticType_Touches
END

GO

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'ImprovementTacticType'))
BEGIN
    Drop table ImprovementTacticType
END

GO

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'BestInClass'))
BEGIN
    Drop table BestInClass
END

GO

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Metric'))
BEGIN
    Drop table Metric
END

GO

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'MetricType'))
BEGIN
    Drop table MetricType
END

GO

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

GO

ALTER TABLE [dbo].[Metric] ADD  CONSTRAINT [DF_Metric_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO

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

GO

ALTER TABLE [dbo].[BestInClass] ADD  CONSTRAINT [DF_BestInClass_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[BestInClass]  WITH CHECK ADD  CONSTRAINT [FK_BestInClass_Metric] FOREIGN KEY([MetricId])
REFERENCES [dbo].[Metric] ([MetricId])
GO

ALTER TABLE [dbo].[BestInClass] CHECK CONSTRAINT [FK_BestInClass_Metric]
GO

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

GO

ALTER TABLE [dbo].[ImprovementTacticType] ADD  CONSTRAINT [DF_ImprovementTacticType_IsDeployed]  DEFAULT ((0)) FOR [IsDeployed]
GO

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

GO

ALTER TABLE [dbo].[ImprovementTacticType_Metric]  WITH CHECK ADD  CONSTRAINT [FK_ImprovementTacticType_Metric_ImprovementTacticType] FOREIGN KEY([ImprovementTacticTypeId])
REFERENCES [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId])
GO

ALTER TABLE [dbo].[ImprovementTacticType_Metric] CHECK CONSTRAINT [FK_ImprovementTacticType_Metric_ImprovementTacticType]
GO

ALTER TABLE [dbo].[ImprovementTacticType_Metric]  WITH CHECK ADD  CONSTRAINT [FK_ImprovementTacticType_Metric_Metric] FOREIGN KEY([MetricId])
REFERENCES [dbo].[Metric] ([MetricId])
GO

ALTER TABLE [dbo].[ImprovementTacticType_Metric] CHECK CONSTRAINT [FK_ImprovementTacticType_Metric_Metric]
GO

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

GO

ALTER TABLE [dbo].[ImprovementTacticType_Touches]  WITH CHECK ADD  CONSTRAINT [FK_ImprovementTacticType_Touches_ImprovementTacticType] FOREIGN KEY([ImprovementTacticTypeId])
REFERENCES [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId])
GO

ALTER TABLE [dbo].[ImprovementTacticType_Touches] CHECK CONSTRAINT [FK_ImprovementTacticType_Touches_ImprovementTacticType]
GO

ALTER TABLE [dbo].[ImprovementTacticType_Touches]  WITH CHECK ADD  CONSTRAINT [FK_ImprovementTacticType_Touches_Metric] FOREIGN KEY([MetricId])
REFERENCES [dbo].[Metric] ([MetricId])
GO

ALTER TABLE [dbo].[ImprovementTacticType_Touches] CHECK CONSTRAINT [FK_ImprovementTacticType_Touches_Metric]
GO

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

GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Plan] FOREIGN KEY([ImprovePlanId])
REFERENCES [dbo].[Plan] ([PlanId])
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Plan]
GO

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

GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Plan_Improvement_Campaign] FOREIGN KEY([ImprovementPlanCampaignId])
REFERENCES [dbo].[Plan_Improvement_Campaign] ([ImprovementPlanCampaignId])
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Plan_Improvement_Campaign]
GO

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

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] ADD  CONSTRAINT [DF_Plan_Improvement_Campaign_Program_Tactic_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Audience] FOREIGN KEY([AudienceId])
REFERENCES [dbo].[Audience] ([AudienceId])
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Audience]
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_BusinessUnit] FOREIGN KEY([BusinessUnitId])
REFERENCES [dbo].[BusinessUnit] ([BusinessUnitId])
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_BusinessUnit]
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Geography] FOREIGN KEY([GeographyId])
REFERENCES [dbo].[Geography] ([GeographyId])
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Geography]
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_ImprovementTacticType] FOREIGN KEY([ImprovementTacticTypeId])
REFERENCES [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId])
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_ImprovementTacticType]
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Plan_Improvement_Campaign_Program] FOREIGN KEY([ImprovementPlanProgramId])
REFERENCES [dbo].[Plan_Improvement_Campaign_Program] ([ImprovementPlanProgramId])
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Plan_Improvement_Campaign_Program]
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Vertical] FOREIGN KEY([VerticalId])
REFERENCES [dbo].[Vertical] ([VerticalId])
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Vertical]
GO

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

GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic_Comment]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Comment_Plan_Improvement_Campaign_Program_Tactic] FOREIGN KEY([ImprovementPlanTacticId])
REFERENCES [dbo].[Plan_Improvement_Campaign_Program_Tactic] ([ImprovementPlanTacticId])
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic_Comment] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Comment_Plan_Improvement_Campaign_Program_Tactic]
GO

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

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic_Share]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Share_Plan_Improvement_Campaign_Program_Tactic] FOREIGN KEY([ImprovementPlanTacticId])
REFERENCES [dbo].[Plan_Improvement_Campaign_Program_Tactic] ([ImprovementPlanTacticId])
GO

ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic_Share] CHECK CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_Share_Plan_Improvement_Campaign_Program_Tactic]
GO


GO
SET IDENTITY_INSERT [dbo].[Metric] ON 

GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (1, N'CR', N'SUS->INQ', N'SUS', N'464eb808-ad1f-4481-9365-6aada15023bd', 1, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (2, N'CR', N'INQ->AQL', N'INQ', N'464eb808-ad1f-4481-9365-6aada15023bd', 2, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (3, N'CR', N'AQL->TAL', N'AQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 3, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (4, N'CR', N'TAL->TQL', N'MQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 4, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (5, N'CR', N'TQL->SAL', N'TQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 5, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (6, N'CR', N'SAL->SQL', N'SAL', N'464eb808-ad1f-4481-9365-6aada15023bd', 6, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (7, N'CR', N'SQL->CW', N'SQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 7, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (8, N'SV', N'SUS', N'SUS', N'464eb808-ad1f-4481-9365-6aada15023bd', 1, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (9, N'SV', N'INQ', N'INQ', N'464eb808-ad1f-4481-9365-6aada15023bd', 2, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (10, N'SV', N'AQL', N'AQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 3, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (11, N'SV', N'TAL', N'MQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 4, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (12, N'SV', N'TQL', N'TQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 5, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (13, N'SV', N'SAL', N'SAL', N'464eb808-ad1f-4481-9365-6aada15023bd', 6, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (14, N'SV', N'SQL', N'SQL', N'464eb808-ad1f-4481-9365-6aada15023bd', 7, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[Metric] ([MetricId], [MetricType], [MetricName], [MetricCode], [ClientId], [Level], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (15, N'Size', N'ADS', N'ADS', N'464eb808-ad1f-4481-9365-6aada15023bd', NULL, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Metric] OFF
GO
SET IDENTITY_INSERT [dbo].[BestInClass] ON 
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (1, 1, 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (2, 2, 0.6, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (3, 3, 0.65, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (4, 4, 0.7, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (5, 5, 0.75, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (6, 6, 0.7, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (7, 7, 0.65, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (8, 8, 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (9, 9, 15, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (10, 10, 15, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (11, 11, 15, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (12, 12, 15, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (13, 13, 15, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (14, 14, 15, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[BestInClass] ([BestInClassId], [MetricId], [Value], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (15, 15, 10, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[BestInClass] OFF
GO
SET IDENTITY_INSERT [dbo].[ImprovementTacticType] ON 

GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (1, N'Inphographic Program', NULL, 25000, N'5bae30', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (2, N'Perpetual Nurture Program', NULL, 40000, N'8e2590', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (3, N'Whitepaper Program', NULL, 7500, N'09af8f', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (4, N'Analysis Whitepaper Program', NULL, 10000, N'407b22', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (5, N'Webinar Program', NULL, 10000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (6, N'Reconstituded/Reengage Program', NULL, 40000, N'0071bc', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (7, N'Integrated Inbound and Outbound Media Program', NULL, 25000, N'38bf37', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (8, N'Develop Buyers Journey-Based Messaging', NULL, 25000, N'38bf37', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (9, N'Develop Buyers Personas', NULL, 35000, N'5bae30', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (10, N'Integrated Welcome and Opportunity Accelaration Nurture', NULL, 80000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (11, N'Buyer Insight Assesment for Opportunity Acceleration', NULL, 40000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (12, N'Implement a Active Recycle Nurture Program', NULL, 40000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (13, N'Implement a Passive Recycle Nurture Program', NULL, 150000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (14, N'Implement a Tele-Prospecting Function within Marketing', NULL, 120000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (15, N'Implement Telesales Campaign Playbooks and Scripts', NULL, 20000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (16, N'Tradeshow', NULL, 80000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (17, N'Executive Briefings', NULL, 10000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 0, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, N'Sales/Marketing Alignment', NULL, 20000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 1, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType] ([ImprovementTacticTypeId], [Title], [Description], [Cost], [ColorCode], [ClientId], [IsDeployed], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, N'Content Nuture', NULL, 10000, N'a6a10a', N'464eb808-ad1f-4481-9365-6aada15023bd', 1, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[ImprovementTacticType] OFF
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 2, 5, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 3, 4, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 4, 3, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 5, 2, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 6, 1, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 7, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 9, 5, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 10, 4, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 11, 3, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 12, 2, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 13, 1, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 14, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (18, 15, 5, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 2, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 3, 1, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 4, 2, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 5, 3, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 6, 4, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 7, 5, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 9, 0, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 10, 1, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 11, 2, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 12, 3, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 13, 4, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 14, 5, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Metric] ([ImprovementTacticTypeId], [MetricId], [Weight], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy]) VALUES (19, 15, 1, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9', NULL, NULL)
GO
INSERT [dbo].[ImprovementTacticType_Touches] ([ImprovementTacticTypeId], [MetricId], [CreatedDate], [CreatedBy]) VALUES (18, 9, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9')
GO
INSERT [dbo].[ImprovementTacticType_Touches] ([ImprovementTacticTypeId], [MetricId], [CreatedDate], [CreatedBy]) VALUES (18, 10, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9')
GO
INSERT [dbo].[ImprovementTacticType_Touches] ([ImprovementTacticTypeId], [MetricId], [CreatedDate], [CreatedBy]) VALUES (18, 11, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9')
GO
INSERT [dbo].[ImprovementTacticType_Touches] ([ImprovementTacticTypeId], [MetricId], [CreatedDate], [CreatedBy]) VALUES (19, 9, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9')
GO
INSERT [dbo].[ImprovementTacticType_Touches] ([ImprovementTacticTypeId], [MetricId], [CreatedDate], [CreatedBy]) VALUES (19, 10, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9')
GO
INSERT [dbo].[ImprovementTacticType_Touches] ([ImprovementTacticTypeId], [MetricId], [CreatedDate], [CreatedBy]) VALUES (19, 11, CAST(0x0000A2E600000000 AS DateTime), N'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9')
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






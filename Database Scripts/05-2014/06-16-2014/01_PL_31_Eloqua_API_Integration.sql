
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
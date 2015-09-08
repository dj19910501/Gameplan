IF NOT EXISTS (  SELECT *   FROM   sys.columns   WHERE  object_id = OBJECT_ID(N'[dbo].[Model]') AND name = 'IntegrationInstanceEloquaId')
BEGIN
	ALTER TABLE [dbo].[Model]
	ADD IntegrationInstanceEloquaId int NULL
END

IF NOT EXISTS (  SELECT *   FROM   sys.columns   WHERE  object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic]') AND name = 'IntegrationInstanceEloquaId')
BEGIN
	ALTER TABLE [dbo].[Model]
	ADD IntegrationInstanceEloquaId nvarchar(50) NULL
END

IF NOT EXISTS (  SELECT *   FROM   sys.columns   WHERE  object_id = OBJECT_ID(N'[dbo].[Plan_Improvement_Campaign_Program_Tactic]') AND name = 'IntegrationInstanceEloquaId')
BEGIN
	ALTER TABLE [dbo].[Model]
	ADD IntegrationInstanceEloquaId nvarchar(50) NULL
END
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
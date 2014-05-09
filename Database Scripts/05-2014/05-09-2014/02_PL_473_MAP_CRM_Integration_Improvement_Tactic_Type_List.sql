-- Run below script on MRPQA

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IsDeployedToIntegration' AND [object_id] = OBJECT_ID(N'ImprovementTacticType'))
BEGIN
    Alter table [ImprovementTacticType] 
	ADD IsDeployedToIntegration bit NOT NULL
	CONSTRAINT [DF_ImprovementTacticType_IsDeployedToIntegration] DEFAULT 0
END
-- Run below script on MRPQA

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IsDeployedToIntegration' AND [object_id] = OBJECT_ID(N'TacticType'))
BEGIN
    Alter table [TacticType] 
	ADD IsDeployedToIntegration bit NOT NULL
	CONSTRAINT [DF_TacticType_IsDeployedToIntegration] DEFAULT 0
END
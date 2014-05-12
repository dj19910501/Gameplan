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
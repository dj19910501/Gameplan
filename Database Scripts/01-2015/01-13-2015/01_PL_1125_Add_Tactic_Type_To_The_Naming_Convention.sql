-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 05/12/2014
-- Description : Custom naming: Campaign name structure
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='ClientTacticType')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Abbreviation' AND [object_id] = OBJECT_ID(N'ClientTacticType'))
	    BEGIN
		    ALTER TABLE [ClientTacticType] ADD Abbreviation NVARCHAR(255) NULL
	    END
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='MasterTacticType')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Abbreviation' AND [object_id] = OBJECT_ID(N'MasterTacticType'))
	    BEGIN
		    ALTER TABLE [MasterTacticType] ADD Abbreviation NVARCHAR(255) NULL
	    END
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='TacticType')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Abbreviation' AND [object_id] = OBJECT_ID(N'TacticType'))
	    BEGIN
		    ALTER TABLE [TacticType] ADD Abbreviation NVARCHAR(255) NULL
	    END
END
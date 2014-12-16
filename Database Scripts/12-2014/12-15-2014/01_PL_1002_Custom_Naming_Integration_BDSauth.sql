-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 12/12/2014
-- Description : Custom Naming: Integration
-- ======================================================================================



IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ActivityType' AND [object_id] = OBJECT_ID(N'Application_Activity'))
	    BEGIN
		    ALTER TABLE [Application_Activity] ADD ActivityType NVARCHAR(255) NULL
	    END
Go
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ActivityType' AND [object_id] = OBJECT_ID(N'Application_Activity'))
	    BEGIN
		UPDATE dbo.Application_Activity SET ActivityType='User'
		END


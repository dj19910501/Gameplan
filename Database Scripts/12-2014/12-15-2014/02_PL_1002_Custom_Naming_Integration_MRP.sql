-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 12/12/2014
-- Description : Custom Naming: Integration
-- ======================================================================================

IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'CustomNamingPermission' AND [object_id] = OBJECT_ID(N'IntegrationInstance'))
	    BEGIN
		    ALTER TABLE [IntegrationInstance] ADD CustomNamingPermission bit DEFAULT (1) NOT NULL
	    END

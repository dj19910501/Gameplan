-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 01/23/2015
-- Description : Multi-select : Inspect popup - review tab icons for veritcal, audience etc.
-- ======================================================================================

		IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'IsDefault' AND [object_id] = OBJECT_ID(N'CustomField'))
	    BEGIN
		    ALTER TABLE [CustomField] ADD IsDefault bit NOT NULL DEFAULT 0
	    END
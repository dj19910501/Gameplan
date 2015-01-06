-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 22/12/2014
-- Description : Custom fields on reports and Calendar
-- ======================================================================================

IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'IsDisplayForFilter' AND [object_id] = OBJECT_ID(N'CustomField'))
	    BEGIN
		    ALTER TABLE CustomField ADD IsDisplayForFilter bit DEFAULT (1) NOT NULL
	    END

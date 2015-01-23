-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 01/23/2015
-- Description : Naming Convention: Multi selection option
-- ======================================================================================


		IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'AbbreviationForMulti' AND [object_id] = OBJECT_ID(N'CustomField'))
	    BEGIN
		    ALTER TABLE [CustomField] ADD AbbreviationForMulti NVARCHAR(255) NOT NULL DEFAULT 'MULTI'
	    END
		
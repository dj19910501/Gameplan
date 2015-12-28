
/* --------- Start Script of PL ticket #1749 --------- */
-- Created by : Komal Rawal
-- Created On : 12/28/2015
-- Description : Insert 'IsDefaultPreset' field in Plan_UserSavedViews table to setdefault preset.


GO 

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_UserSavedViews')
BEGIN

		    IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'IsDefaultPreset' AND [object_id] = OBJECT_ID(N'Plan_UserSavedViews'))
			 BEGIN
		     ALTER TABLE [dbo].[Plan_UserSavedViews] ADD IsDefaultPreset bit  Null DEFAULT 0
		    END
END

Go
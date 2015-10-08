-- ======================================================================================
-- Created By : Komal Rawal
-- Created Date : 08/10/2015
-- Description : Add column in Budget Detail table
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Budget_Detail')
BEGIN

		    IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'IsDeleted' AND [object_id] = OBJECT_ID(N'Budget_Detail'))
			 BEGIN
		     ALTER TABLE [dbo].[Budget_Detail] ADD  IsDeleted bit  NULL 
		    END
END

-- ======================================================================================
-- Created By : Komal Rawal
-- Created Date : 01/10/2015
-- Description : Add column in LineItem_Budget table
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='LineItem_Budget')
BEGIN

		    IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Weightage' AND [object_id] = OBJECT_ID(N'LineItem_Budget'))
			 BEGIN
		     ALTER TABLE [dbo].[LineItem_Budget] ADD  Weightage tinyint  NULL 
		    END
END

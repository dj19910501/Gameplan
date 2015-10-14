-- ======================================================================================
-- Created By : Bhavesh Dobariya
-- Created Date : 14/10/2015
-- Description : Add column in Budget Detail table for IsForecast
-- ======================================================================================
GO 

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Budget_Detail')
BEGIN

		    IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'IsForecast' AND [object_id] = OBJECT_ID(N'Budget_Detail'))
			 BEGIN
		     ALTER TABLE [dbo].[Budget_Detail] ADD IsForecast bit Not Null DEFAULT 0
		    END
END

Go
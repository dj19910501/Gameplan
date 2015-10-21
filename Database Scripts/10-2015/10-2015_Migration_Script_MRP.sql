-- Please Add GO statement between script
GO
-- Created By : Dashrath Prajapati
-- Created Date : 20/10/2015
-- Description :Update GamePlan text to Plan from subject Column
-- ======================================================================================
UPDATE [dbo].[Notification] SET [Subject] = REPLACE([EmailContent], N'GamePlan', 'Plan') 
Go

-- ======================================================================================
-- Created By : Bhavesh Dobariya
-- Created Date : 14/10/2015
-- Description : Add column in Budget Detail table for IsOther
-- ======================================================================================
GO 

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Budget')
BEGIN

		    IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'IsOther' AND [object_id] = OBJECT_ID(N'Budget'))
			 BEGIN
		     ALTER TABLE [dbo].[Budget] ADD IsOther bit Not Null DEFAULT 0
		    END
END

Go
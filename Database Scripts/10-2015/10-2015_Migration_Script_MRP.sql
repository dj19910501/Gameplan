-- Please Add GO statement between script
GO
-- Created By : Dashrath Prajapati
-- Created Date : 20/10/2015
-- Description :Update GamePlan text to Plan from subject Column
-- ======================================================================================
UPDATE [dbo].[Notification] SET [Subject] = REPLACE([EmailContent], N'GamePlan', 'Plan') 
Go
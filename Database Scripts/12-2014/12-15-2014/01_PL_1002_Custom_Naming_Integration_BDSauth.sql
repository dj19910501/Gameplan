-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 12/12/2014
-- Description : Custom Naming: Integration
-- ======================================================================================



IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ActivityType' AND [object_id] = OBJECT_ID(N'Application_Activity'))
	    BEGIN
		    ALTER TABLE [Application_Activity] ADD ActivityType NVARCHAR(255) NULL
	    END
Go
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ActivityType' AND [object_id] = OBJECT_ID(N'Application_Activity'))
	    BEGIN
		UPDATE dbo.Application_Activity SET ActivityType='User' WHERE ActivityType IS NULL

		IF NOT EXISTS (SELECT * FROM dbo.Application_Activity WHERE Code='CustomCampaignNameConvention')
		BEGIN
		INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId],[ApplicationId],[ActivityTitle],[Code],[CreatedDate],[ActivityType])
        VALUES
           ('22','1c10d4b9-7931-4a7c-99e9-a158ce158951','Generate custom campaign name','CustomCampaignNameConvention',GETDATE(),'Client')
		   END
		END


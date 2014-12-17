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
		IF EXISTS (SELECT * FROM dbo.Application WHERE IsDeleted=0 AND Code='MRP')
		BEGIN
        DECLARE @ApplicationId UNIQUEIDENTIFIER
		DECLARE @count INT 
		SELECT TOP(1) @ApplicationId=ApplicationId FROM dbo.Application WHERE IsDeleted=0 AND Code='MRP'
		SELECT @count=MAX(ApplicationActivityId)  FROM dbo.Application_Activity
		SELECT @count=@count+1
		INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId],[ApplicationId],[ActivityTitle],[Code],[CreatedDate],[ActivityType])
        VALUES
           (@count,@ApplicationId,'Generate custom campaign name','CustomCampaignNameConvention',GETDATE(),'Client')
		   END

		   END
		END

